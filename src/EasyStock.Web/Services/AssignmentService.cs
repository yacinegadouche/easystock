using EasyStock.Web.Data;
using EasyStock.Web.Extensions;
using EasyStock.Web.Models;
using EasyStock.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EasyStock.Web.Services;

public class AssignmentService(ApplicationDbContext db, StockService stockService)
{
    public Task<List<AssignmentListItemViewModel>> GetListAsync(
        AssignmentStatusFilter status,
        CancellationToken cancellationToken = default)
    {
        var query = db.Assignments.AsNoTracking();

        query = status switch
        {
            AssignmentStatusFilter.Active => query.Where(a => a.ReturnedAt == null),
            AssignmentStatusFilter.Returned => query.Where(a => a.ReturnedAt != null),
            _ => query,
        };

        return query
            .OrderByDescending(a => a.AssignedAt)
            .Select(AssignmentListItemViewModel.Projection)
            .ToListAsync(cancellationToken);
    }

    public Task<AssignmentListItemViewModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Assignments
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(AssignmentListItemViewModel.Projection)
            .SingleOrDefaultAsync(cancellationToken);

    public Task<List<LookupItem>> GetEmployeeLookupsAsync(CancellationToken cancellationToken = default) =>
        db.Employees
            .AsNoTracking()
            .Where(e => e.IsActive)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .Select(e => new LookupItem(e.Id, e.FirstName + " " + e.LastName))
            .ToListAsync(cancellationToken);

    public Task<List<LookupItem>> GetAvailableItemLookupsAsync(CancellationToken cancellationToken = default) =>
        db.Items
            .AsNoTracking()
            .Where(i => i.QuantityInStock > 0)
            .OrderBy(i => i.Name)
            .Select(i => new LookupItem(i.Id, i.Name + " (" + i.QuantityInStock.ToString() + " in stock)"))
            .ToListAsync(cancellationToken);

    public async Task<ServiceResult> CreateAsync(
        AssignmentCreateViewModel model,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var employee = await db.Employees
            .AsNoTracking()
            .Where(e => e.Id == model.EmployeeId && e.IsActive)
            .Select(e => new { e.Id, FullName = e.FirstName + " " + e.LastName })
            .SingleOrDefaultAsync(cancellationToken);

        if (employee is null)
        {
            return ServiceResult.Failure("The selected employee does not exist or is inactive.");
        }

        if (!await db.Items.AnyAsync(i => i.Id == model.ItemId, cancellationToken))
        {
            return ServiceResult.Failure("The selected item does not exist.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var assignment = new Assignment
        {
            ItemId = model.ItemId!.Value,
            EmployeeId = employee.Id,
            Quantity = model.Quantity,
            AssignedAt = DateTime.UtcNow,
            Notes = model.Notes.TrimToNull(),
            AssignedByUserId = userId,
        };

        db.Assignments.Add(assignment);
        await db.SaveChangesAsync(cancellationToken);

        var stockResult = await stockService.RecordMovementAsync(
            assignment.ItemId,
            MovementType.Assignment,
            assignment.Quantity,
            userId,
            reason: $"Assigned to {employee.FullName}",
            assignmentId: assignment.Id,
            cancellationToken: cancellationToken);

        if (!stockResult.Succeeded)
        {
            db.ChangeTracker.Clear();
            return stockResult;
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> ReturnAsync(int id, int userId, CancellationToken cancellationToken = default)
    {
        var assignment = await db.Assignments
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new
            {
                a.ItemId,
                a.Quantity,
                EmployeeName = a.Employee.FirstName + " " + a.Employee.LastName,
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (assignment is null)
        {
            return ServiceResult.Failure("Assignment not found.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var now = DateTime.UtcNow;

        var updatedRows = await db.Assignments
            .Where(a => a.Id == id && a.ReturnedAt == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(a => a.ReturnedAt, (DateTime?)now)
                .SetProperty(a => a.UpdatedAt, now),
                cancellationToken);

        if (updatedRows == 0)
        {
            return ServiceResult.Failure("This assignment has already been returned.");
        }

        var stockResult = await stockService.RecordMovementAsync(
            assignment.ItemId,
            MovementType.AssignmentReturn,
            assignment.Quantity,
            userId,
            reason: $"Returned by {assignment.EmployeeName}",
            assignmentId: id,
            cancellationToken: cancellationToken);

        if (!stockResult.Succeeded)
        {
            return stockResult;
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ServiceResult.Success();
    }
}