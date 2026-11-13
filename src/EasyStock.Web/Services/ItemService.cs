using EasyStock.Web.Data;
using EasyStock.Web.Extensions;
using EasyStock.Web.Models;
using EasyStock.Web.ViewModels;

using Microsoft.EntityFrameworkCore;

namespace EasyStock.Web.Services;

public class ItemService(ApplicationDbContext db)
{
    public async Task<List<ItemListItemViewModel>> GetListAsync(
        string? search,
        int? categoryId,
        bool lowStockOnly,
        CancellationToken cancellationToken = default)
    {
        var query = db.Items.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(i =>
                i.Name.ToLower().Contains(term) ||
                (i.Description != null && i.Description.ToLower().Contains(term)));
        }

        if (categoryId is not null)
        {
            query = query.Where(i => i.CategoryId == categoryId);
        }

        if (lowStockOnly)
        {
            query = query.Where(i => i.QuantityInStock <= i.MinimumQuantity);
        }

        return await query
            .OrderBy(i => i.Name)
            .Select(i => new ItemListItemViewModel(
                i.Id,
                i.Name,
                i.Category.Name,
                i.Supplier != null ? i.Supplier.Name : null,
                i.QuantityInStock,
                i.MinimumQuantity))
            .ToListAsync(cancellationToken);
    }

    public async Task<ItemFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default) =>
        await db.Items
            .AsNoTracking()
            .Where(i => i.Id == id)
            .Select(i => new ItemFormViewModel
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                CategoryId = i.CategoryId,
                SupplierId = i.SupplierId,
                MinimumQuantity = i.MinimumQuantity,
                QuantityInStock = i.QuantityInStock,
            })
            .SingleOrDefaultAsync(cancellationToken);

    public Task<List<LookupItem>> GetCategoryLookupsAsync(CancellationToken cancellationToken = default) =>
        db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new LookupItem(c.Id, c.Name))
            .ToListAsync(cancellationToken);

    public Task<List<LookupItem>> GetSupplierLookupsAsync(CancellationToken cancellationToken = default) =>
        db.Suppliers
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new LookupItem(s.Id, s.Name))
            .ToListAsync(cancellationToken);

    public async Task<ServiceResult> CreateAsync(
        ItemFormViewModel model,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var validation = await ValidateAsync(model, excludeId: null, cancellationToken);

        if (!validation.Succeeded)
        {
            return validation;
        }

        var item = new Item
        {
            Name = model.Name.Trim(),
            Description = model.Description.TrimToNull(),
            CategoryId = model.CategoryId!.Value,
            SupplierId = model.SupplierId,
            MinimumQuantity = model.MinimumQuantity,
            QuantityInStock = model.InitialQuantity,
        };

        db.Items.Add(item);

        if (model.InitialQuantity > 0)
        {
            db.StockMovements.Add(new StockMovement
            {
                Item = item,
                Type = MovementType.Entry,
                Quantity = model.InitialQuantity,
                QuantityAfter = model.InitialQuantity,
                Reason = "Initial stock",
                OccurredAt = DateTime.UtcNow,
                UserId = userId,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(ItemFormViewModel model, CancellationToken cancellationToken = default)
    {
        var item = await db.Items.FindAsync([model.Id], cancellationToken);

        if (item is null)
        {
            return ServiceResult.Failure("Item not found.");
        }

        var validation = await ValidateAsync(model, item.Id, cancellationToken);

        if (!validation.Succeeded)
        {
            return validation;
        }

        item.Name = model.Name.Trim();
        item.Description = model.Description.TrimToNull();
        item.CategoryId = model.CategoryId!.Value;
        item.SupplierId = model.SupplierId;
        item.MinimumQuantity = model.MinimumQuantity;

        await db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await db.Items.FindAsync([id], cancellationToken);

        if (item is null)
        {
            return ServiceResult.Failure("Item not found.");
        }

        var hasHistory =
            await db.StockMovements.AnyAsync(m => m.ItemId == id, cancellationToken) ||
            await db.Assignments.AnyAsync(a => a.ItemId == id, cancellationToken);

        if (hasHistory)
        {
            return ServiceResult.Failure($"'{item.Name}' has stock history and cannot be deleted.");
        }

        db.Items.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    private async Task<ServiceResult> ValidateAsync(
        ItemFormViewModel model,
        int? excludeId,
        CancellationToken cancellationToken)
    {
        var name = model.Name.Trim();

        if (await db.Items.AnyAsync(i => i.Name.ToLower() == name.ToLower() && i.Id != excludeId, cancellationToken))
        {
            return ServiceResult.Failure($"An item named '{name}' already exists.");
        }

        if (!await db.Categories.AnyAsync(c => c.Id == model.CategoryId, cancellationToken))
        {
            return ServiceResult.Failure("The selected category does not exist.");
        }

        if (model.SupplierId is not null &&
            !await db.Suppliers.AnyAsync(s => s.Id == model.SupplierId, cancellationToken))
        {
            return ServiceResult.Failure("The selected supplier does not exist.");
        }

        return ServiceResult.Success();
    }
}