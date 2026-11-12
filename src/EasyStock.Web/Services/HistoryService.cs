using EasyStock.Web.Data;
using EasyStock.Web.Extensions;
using EasyStock.Web.Models;
using EasyStock.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EasyStock.Web.Services;

public class HistoryService(ApplicationDbContext db)
{
    public const int PageSize = 25;

    public IQueryable<StockMovement> QueryMovements(MovementHistoryFilter filter)
    {
        var query = ApplyDateRange(db.StockMovements.AsNoTracking(), filter);

        if (filter.ItemId is not null)
        {
            query = query.Where(m => m.ItemId == filter.ItemId);
        }

        if (filter.Type is not null)
        {
            query = query.Where(m => m.Type == filter.Type);
        }

        if (filter.UserId is not null)
        {
            query = query.Where(m => m.UserId == filter.UserId);
        }

        return query.OrderByDescending(m => m.OccurredAt).ThenByDescending(m => m.Id);
    }

    public IQueryable<StockMovement> QueryReplenishments(ReplenishmentFilter filter)
    {
        var query = ApplyDateRange(
            db.StockMovements.AsNoTracking().Where(m => m.Type == MovementType.Entry),
            filter);

        if (filter.SupplierId is not null)
        {
            query = query.Where(m => m.Item.SupplierId == filter.SupplierId);
        }

        return query.OrderByDescending(m => m.OccurredAt).ThenByDescending(m => m.Id);
    }

    public Task<PagedList<StockMovementListItemViewModel>> GetMovementsAsync(
        MovementHistoryFilter filter,
        CancellationToken cancellationToken = default) =>
        QueryMovements(filter)
            .Select(StockMovementListItemViewModel.Projection)
            .ToPagedListAsync(filter.Page, PageSize, cancellationToken);

    public async Task<(PagedList<ReplenishmentListItemViewModel> Replenishments, int TotalQuantity)> GetReplenishmentsAsync(
        ReplenishmentFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = QueryReplenishments(filter);

        var totalQuantity = await query.SumAsync(m => m.Quantity, cancellationToken);
        var replenishments = await query
            .Select(ReplenishmentListItemViewModel.Projection)
            .ToPagedListAsync(filter.Page, PageSize, cancellationToken);

        return (replenishments, totalQuantity);
    }

    public Task<List<LookupItem>> GetItemLookupsAsync(CancellationToken cancellationToken = default) =>
        db.Items.AsNoTracking()
            .OrderBy(i => i.Name)
            .Select(i => new LookupItem(i.Id, i.Name))
            .ToListAsync(cancellationToken);

    public Task<List<LookupItem>> GetUserLookupsAsync(CancellationToken cancellationToken = default) =>
        db.Users.AsNoTracking()
            .OrderBy(u => u.FullName)
            .Select(u => new LookupItem(u.Id, u.FullName))
            .ToListAsync(cancellationToken);

    public Task<List<LookupItem>> GetSupplierLookupsAsync(CancellationToken cancellationToken = default) =>
        db.Suppliers.AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new LookupItem(s.Id, s.Name))
            .ToListAsync(cancellationToken);

    private static IQueryable<StockMovement> ApplyDateRange(IQueryable<StockMovement> query, DateRangeFilter filter)
    {
        if (filter.From is { } from)
        {
            var fromUtc = from.StartOfDayUtc();
            query = query.Where(m => m.OccurredAt >= fromUtc);
        }

        if (filter.To is { } to)
        {
            var toExclusiveUtc = to.AddDays(1).StartOfDayUtc();
            query = query.Where(m => m.OccurredAt < toExclusiveUtc);
        }

        return query;
    }
}