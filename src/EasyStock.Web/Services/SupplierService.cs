using EasyStock.Web.Data;
using EasyStock.Web.Extensions;
using EasyStock.Web.Models;
using EasyStock.Web.ViewModels;

using Microsoft.EntityFrameworkCore;

namespace EasyStock.Web.Services;

public class SupplierService(ApplicationDbContext db)
{
    public async Task<List<SupplierListItemViewModel>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.Suppliers
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SupplierListItemViewModel(s.Id, s.Name, s.ContactEmail, s.Phone, s.Items.Count))
            .ToListAsync(cancellationToken);

    public async Task<SupplierFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default) =>
        await db.Suppliers
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new SupplierFormViewModel
            {
                Id = s.Id,
                Name = s.Name,
                ContactEmail = s.ContactEmail,
                Phone = s.Phone,
                Address = s.Address,
            })
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<ServiceResult> CreateAsync(SupplierFormViewModel model, CancellationToken cancellationToken = default)
    {
        var name = model.Name.Trim();

        if (await NameExistsAsync(name, excludeId: null, cancellationToken))
        {
            return ServiceResult.Failure($"A supplier named '{name}' already exists.");
        }

        db.Suppliers.Add(new Supplier
        {
            Name = name,
            ContactEmail = model.ContactEmail.TrimToNull(),
            Phone = model.Phone.TrimToNull(),
            Address = model.Address.TrimToNull(),
        });

        await db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(SupplierFormViewModel model, CancellationToken cancellationToken = default)
    {
        var supplier = await db.Suppliers.FindAsync([model.Id], cancellationToken);

        if (supplier is null)
        {
            return ServiceResult.Failure("Supplier not found.");
        }

        var name = model.Name.Trim();

        if (await NameExistsAsync(name, supplier.Id, cancellationToken))
        {
            return ServiceResult.Failure($"A supplier named '{name}' already exists.");
        }

        supplier.Name = name;
        supplier.ContactEmail = model.ContactEmail.TrimToNull();
        supplier.Phone = model.Phone.TrimToNull();
        supplier.Address = model.Address.TrimToNull();

        await db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var supplier = await db.Suppliers.FindAsync([id], cancellationToken);

        if (supplier is null)
        {
            return ServiceResult.Failure("Supplier not found.");
        }

        db.Suppliers.Remove(supplier);
        await db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    private Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken cancellationToken) =>
        db.Suppliers.AnyAsync(
            s => s.Name.ToLower() == name.ToLower() && s.Id != excludeId,
            cancellationToken);
}