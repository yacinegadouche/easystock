using EasyStock.Web.Data;
using EasyStock.Web.Extensions;
using EasyStock.Web.Models;
using EasyStock.Web.ViewModels;

using Microsoft.EntityFrameworkCore;

namespace EasyStock.Web.Services;

public class EmployeeService(ApplicationDbContext db)
{
    public Task<List<EmployeeListItemViewModel>> GetAllAsync(
        string? search,
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var query = db.Employees.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(e => e.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(term) ||
                e.LastName.ToLower().Contains(term) ||
                e.Email.ToLower().Contains(term) ||
                (e.Department != null && e.Department.ToLower().Contains(term)));
        }

        return query
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .Select(e => new EmployeeListItemViewModel(
                e.Id,
                e.FirstName + " " + e.LastName,
                e.Email,
                e.Department,
                e.IsActive,
                e.Assignments.Count(a => a.ReturnedAt == null)))
            .ToListAsync(cancellationToken);
    }

    public async Task<EmployeeDetailsViewModel?> GetDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await db.Employees
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new { e.Id, FullName = e.FirstName + " " + e.LastName, e.Email, e.Department, e.IsActive })
            .SingleOrDefaultAsync(cancellationToken);

        if (employee is null)
        {
            return null;
        }

        var assignments = await db.Assignments
            .AsNoTracking()
            .Where(a => a.EmployeeId == id)
            .OrderBy(a => a.ReturnedAt != null)
            .ThenByDescending(a => a.AssignedAt)
            .Select(AssignmentListItemViewModel.Projection)
            .ToListAsync(cancellationToken);

        return new EmployeeDetailsViewModel
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Email = employee.Email,
            Department = employee.Department,
            IsActive = employee.IsActive,
            Assignments = assignments,
        };
    }

    public Task<EmployeeFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default) =>
        db.Employees
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new EmployeeFormViewModel
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                Department = e.Department,
                IsActive = e.IsActive,
            })
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<ServiceResult> CreateAsync(EmployeeFormViewModel model, CancellationToken cancellationToken = default)
    {
        var email = model.Email.Trim().ToLowerInvariant();

        if (await db.Employees.AnyAsync(e => e.Email == email, cancellationToken))
        {
            return ServiceResult.Failure($"An employee with the email '{email}' already exists.");
        }

        db.Employees.Add(new Employee
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Email = email,
            Department = model.Department.TrimToNull(),
        });

        await db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(EmployeeFormViewModel model, CancellationToken cancellationToken = default)
    {
        var employee = await db.Employees.FindAsync([model.Id], cancellationToken);

        if (employee is null)
        {
            return ServiceResult.Failure("Employee not found.");
        }

        var email = model.Email.Trim().ToLowerInvariant();

        if (await db.Employees.AnyAsync(e => e.Email == email && e.Id != employee.Id, cancellationToken))
        {
            return ServiceResult.Failure($"An employee with the email '{email}' already exists.");
        }

        var isBeingDeactivated = employee.IsActive && !model.IsActive;

        if (isBeingDeactivated &&
            await db.Assignments.AnyAsync(a => a.EmployeeId == employee.Id && a.ReturnedAt == null, cancellationToken))
        {
            return ServiceResult.Failure(
                $"{employee.FullName} still has equipment assigned. Record the returns before deactivating.");
        }

        employee.FirstName = model.FirstName.Trim();
        employee.LastName = model.LastName.Trim();
        employee.Email = email;
        employee.Department = model.Department.TrimToNull();
        employee.IsActive = model.IsActive;

        await db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }
}