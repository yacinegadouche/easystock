using EasyStock.Web.Data;
using EasyStock.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyStock.Web.Services;

public class StockService(ApplicationDbContext db)
{
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

        var delta = IsIncrease(type) ? quantity : -quantity;
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

        var quantityAfter = await db.Items
            .Where(i => i.Id == itemId)
            .Select(i => i.QuantityInStock)
            .SingleAsync(cancellationToken);

        db.StockMovements.Add(new StockMovement
        {
            ItemId = itemId,
            Type = type,
            Quantity = quantity,
            QuantityAfter = quantityAfter,
            Reason = reason,
            ExternalReference = externalReference,
            OccurredAt = now,
            UserId = userId,
            AssignmentId = assignmentId,
        });

        return ServiceResult.Success();
    }

    private static bool IsIncrease(MovementType type) =>
        type is MovementType.Entry or MovementType.Return or MovementType.AssignmentReturn;
}