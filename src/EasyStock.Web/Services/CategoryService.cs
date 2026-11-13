using EasyStock.Web.Data;
using EasyStock.Web.Extensions;
using EasyStock.Web.Models;
using EasyStock.Web.ViewModels;

using Microsoft.EntityFrameworkCore;

namespace EasyStock.Web.Services;

public class CategoryService(ApplicationDbContext db)
{
    public async Task<List<CategoryListItemViewModel>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryListItemViewModel(c.Id, c.Name, c.Description, c.Items.Count))
            .ToListAsync(cancellationToken);

    public async Task<CategoryFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default) =>
        await db.Categories
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoryFormViewModel { Id = c.Id, Name = c.Name, Description = c.Description })
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<ServiceResult> CreateAsync(CategoryFormViewModel model, CancellationToken cancellationToken = default)
    {
        var name = model.Name.Trim();

        if (await NameExistsAsync(name, excludeId: null, cancellationToken))
        {
            return ServiceResult.Failure($"A category named '{name}' already exists.");
        }

        db.Categories.Add(new Category { Name = name, Description = model.Description.TrimToNull() });
        await db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(CategoryFormViewModel model, CancellationToken cancellationToken = default)
    {
        var category = await db.Categories.FindAsync([model.Id], cancellationToken);

        if (category is null)
        {
            return ServiceResult.Failure("Category not found.");
        }

        var name = model.Name.Trim();

        if (await NameExistsAsync(name, category.Id, cancellationToken))
        {
            return ServiceResult.Failure($"A category named '{name}' already exists.");
        }

        category.Name = name;
        category.Description = model.Description.TrimToNull();
        await db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await db.Categories.FindAsync([id], cancellationToken);

        if (category is null)
        {
            return ServiceResult.Failure("Category not found.");
        }

        if (await db.Items.AnyAsync(i => i.CategoryId == id, cancellationToken))
        {
            return ServiceResult.Failure($"'{category.Name}' still contains items and cannot be deleted.");
        }

        db.Categories.Remove(category);
        await db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    private Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken cancellationToken) =>
        db.Categories.AnyAsync(
            c => c.Name.ToLower() == name.ToLower() && c.Id != excludeId,
            cancellationToken);
}