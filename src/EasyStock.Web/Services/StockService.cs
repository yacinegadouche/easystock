using EasyStock.Web.Data;
using EasyStock.Web.Extensions;
using EasyStock.Web.Models;
using EasyStock.Web.Notifications;
using EasyStock.Web.ViewModels;

using Microsoft.EntityFrameworkCore;

namespace EasyStock.Web.Services;

public class StockService(ApplicationDbContext db, LowStockAlertQueue alertQueue)
{
    private readonly List<LowStockAlert> pendingAlerts = [];

    public Task<List<StockMovementListItemViewModel>> GetRecentAsync(
    int count,
    CancellationToken cancellationToken = default) =>
    db.StockMovements
        .AsNoTracking()
        .OrderByDescending(m => m.OccurredAt)
        .ThenByDescending(m => m.Id)
        .Take(count)
        .Select(StockMovementListItemViewModel.Projection)
        .ToListAsync(cancellationToken);

    public Task<List<LookupItem>> GetItemLookupsAsync(CancellationToken cancellationToken = default) =>
        db.Items
            .AsNoTracking()
            .OrderBy(i => i.Name)
            .Select(i => new LookupItem(i.Id, i.Name + " (" + i.QuantityInStock.ToString() + " in stock)"))
            .ToListAsync(cancellationToken);

    public async Task<ServiceResult> RecordManualMovementAsync(
        MovementType type,
        int itemId,
        int quantity,
        int userId,
        string? reason,
        string? externalReference,
        CancellationToken cancellationToken = default)
    {
        if (!type.IsManual())
        {
            return ServiceResult.Failure("This movement type cannot be recorded manually.");
        }

        if (type == MovementType.Exit && string.IsNullOrWhiteSpace(reason))
        {
            return ServiceResult.Failure("A reason is required for stock exits.");
        }

        return await ExecuteInTransactionAsync(
            token => RecordMovementAsync(
                itemId,
                type,
                quantity,
                userId,
                reason.TrimToNull(),
                externalReference.TrimToNull(),
                cancellationToken: token),
            cancellationToken);
    }

    public async Task<ServiceResult> ExecuteInTransactionAsync(
        Func<CancellationToken, Task<ServiceResult>> work,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var result = await work(cancellationToken);

        if (!result.Succeeded)
        {
            db.ChangeTracker.Clear();
            pendingAlerts.Clear();
            return result;
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        foreach (var alert in pendingAlerts)
        {
            alertQueue.Enqueue(alert);
        }

        pendingAlerts.Clear();
        return result;
    }

    /// <summary>
    /// Atomically changes an item's stock and adds the matching movement to the change tracker.
    /// Must be called inside a transaction; the caller saves and commits.
    /// </summary>
    public async Task<ServiceResult> RecordMovementAsync(
        int itemId,
        MovementType type,
        int quantity,
        int userId,
        string? reason = null,
        string? externalReference = null,
        int? assignmentId = null,
        CancellationToken cancellationToken = default)
    {
        if (db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("Stock movements must be recorded inside a transaction.");
        }

        if (quantity <= 0)
        {
            return ServiceResult.Failure("Quantity must be greater than zero.");
        }

        var delta = type.IsIncrease() ? quantity : -quantity;
        var now = DateTime.UtcNow;

        var updatedRows = await db.Items
            .Where(i => i.Id == itemId && i.QuantityInStock + delta >= 0)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(i => i.QuantityInStock, i => i.QuantityInStock + delta)
                .SetProperty(i => i.UpdatedAt, now),
                cancellationToken);

        if (updatedRows == 0)
        {
            var itemExists = await db.Items.AnyAsync(i => i.Id == itemId, cancellationToken);
            return ServiceResult.Failure(itemExists ? "Not enough stock available." : "Item not found.");
        }

        var stock = await db.Items
            .Where(i => i.Id == itemId)
            .Select(i => new
            {
                i.Name,
                i.QuantityInStock,
                i.MinimumQuantity,
                SupplierName = i.Supplier != null ? i.Supplier.Name : null,
                SupplierEmail = i.Supplier != null ? i.Supplier.ContactEmail : null,
            })
            .SingleAsync(cancellationToken);

        var quantityBefore = stock.QuantityInStock - delta;

        if (!Item.IsLow(quantityBefore, stock.MinimumQuantity) && Item.IsLow(stock.QuantityInStock, stock.MinimumQuantity))
        {
            pendingAlerts.Add(new LowStockAlert(
                itemId,
                stock.Name,
                stock.QuantityInStock,
                stock.MinimumQuantity,
                stock.SupplierName,
                stock.SupplierEmail));
        }

        db.StockMovements.Add(new StockMovement
        {
            ItemId = itemId,
            Type = type,
            Quantity = quantity,
            QuantityAfter = stock.QuantityInStock,
            Reason = reason,
            ExternalReference = externalReference,
            OccurredAt = now,
            UserId = userId,
            AssignmentId = assignmentId,
        });

        return ServiceResult.Success();
    }
}