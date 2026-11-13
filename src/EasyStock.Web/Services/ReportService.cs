using EasyStock.Web.Data;
using EasyStock.Web.Models;
using EasyStock.Web.Reports;
using EasyStock.Web.ViewModels;

using Microsoft.EntityFrameworkCore;

namespace EasyStock.Web.Services;

public class ReportService(ApplicationDbContext db, HistoryService historyService)
{
    public const int MaxRows = 10_000;

    public async Task<ReportDataset> GetInventoryAsync(CancellationToken cancellationToken = default)
    {
        var rows = await db.Items
            .AsNoTracking()
            .OrderBy(i => i.Category.Name)
            .ThenBy(i => i.Name)
            .Select(i => new object?[]
            {
                i.Name,
                i.Category.Name,
                i.Supplier != null ? i.Supplier.Name : null,
                i.QuantityInStock,
                i.MinimumQuantity,
                i.QuantityInStock <= i.MinimumQuantity ? "Low stock" : "OK",
            })
            .ToListAsync(cancellationToken);

        return new ReportDataset(
            "Inventory",
            $"{rows.Count} items.",
            ["Item", "Category", "Supplier", "In stock", "Minimum", "Status"],
            rows,
            DateTime.UtcNow);
    }

    public async Task<ReportDataset> GetLowStockAsync(CancellationToken cancellationToken = default)
    {
        var rows = await db.Items
            .AsNoTracking()
            .Where(i => i.QuantityInStock <= i.MinimumQuantity)
            .OrderBy(i => i.QuantityInStock - i.MinimumQuantity)
            .ThenBy(i => i.Name)
            .Select(i => new object?[]
            {
                i.Name,
                i.Category.Name,
                i.Supplier != null ? i.Supplier.Name : null,
                i.Supplier != null ? i.Supplier.ContactEmail : null,
                i.QuantityInStock,
                i.MinimumQuantity,
                i.MinimumQuantity - i.QuantityInStock,
            })
            .ToListAsync(cancellationToken);

        return new ReportDataset(
            "Low stock",
            $"{rows.Count} items at or below their minimum quantity.",
            ["Item", "Category", "Supplier", "Supplier email", "In stock", "Minimum", "Shortfall"],
            rows,
            DateTime.UtcNow);
    }

    public async Task<ReportDataset> GetMovementsAsync(
        MovementHistoryFilter filter,
        CancellationToken cancellationToken = default)
    {
        var rows = await historyService.QueryMovements(filter)
            .Take(MaxRows)
            .Select(m => new object?[]
            {
                m.OccurredAt,
                m.Type.ToDisplayName(),
                m.Item.Name,
                m.Type.IsIncrease() ? m.Quantity : -m.Quantity,
                m.QuantityAfter,
                m.Reason,
                m.ExternalReference,
                m.User.FullName,
            })
            .ToListAsync(cancellationToken);

        return new ReportDataset(
            "Stock movements",
            $"{DescribePeriod(filter)} {rows.Count} movements{Truncated(rows.Count)}.",
            ["Date", "Type", "Item", "Quantity", "Stock after", "Reason", "Reference", "User"],
            rows,
            DateTime.UtcNow);
    }

    public async Task<ReportDataset> GetReplenishmentsAsync(
        ReplenishmentFilter filter,
        CancellationToken cancellationToken = default)
    {
        var rows = await historyService.QueryReplenishments(filter)
            .Take(MaxRows)
            .Select(m => new object?[]
            {
                m.OccurredAt,
                m.Item.Name,
                m.Item.Supplier != null ? m.Item.Supplier.Name : null,
                m.Quantity,
                m.ExternalReference,
                m.Reason,
                m.User.FullName,
            })
            .ToListAsync(cancellationToken);

        var totalUnits = rows.Sum(row => (int)row[3]!);

        return new ReportDataset(
            "Replenishments",
            $"{DescribePeriod(filter)} {rows.Count} replenishments, {totalUnits} units received{Truncated(rows.Count)}.",
            ["Date", "Item", "Supplier", "Quantity", "Reference", "Reason", "User"],
            rows,
            DateTime.UtcNow);
    }

    private static string DescribePeriod(DateRangeFilter filter) => (filter.From, filter.To) switch
    {
        (null, null) => "All dates.",
        ({ } from, null) => $"From {from:yyyy-MM-dd}.",
        (null, { } to) => $"Until {to:yyyy-MM-dd}.",
        ({ } from, { } to) => $"From {from:yyyy-MM-dd} to {to:yyyy-MM-dd}.",
    };

    private static string Truncated(int rowCount) =>
        rowCount == MaxRows ? $" (limited to the {MaxRows:N0} most recent)" : string.Empty;
}