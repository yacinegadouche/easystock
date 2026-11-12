using EasyStock.Web.Authorization;
using EasyStock.Web.Models;
using EasyStock.Web.Services;
using EasyStock.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EasyStock.Web.Controllers;

[Authorize(Policy = Policies.ViewReports)]
public class HistoryController(HistoryService historyService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] MovementHistoryFilter filter, CancellationToken cancellationToken)
    {
        var movements = ModelState.IsValid
            ? await historyService.GetMovementsAsync(filter, cancellationToken)
            : PagedList<StockMovementListItemViewModel>.Empty(HistoryService.PageSize);

        var items = await historyService.GetItemLookupsAsync(cancellationToken);
        var users = await historyService.GetUserLookupsAsync(cancellationToken);

        return View(new MovementHistoryViewModel
        {
            Filter = filter,
            Movements = movements,
            Items = new SelectList(items, nameof(LookupItem.Id), nameof(LookupItem.Name), filter.ItemId),
            Users = new SelectList(users, nameof(LookupItem.Id), nameof(LookupItem.Name), filter.UserId),
            Types = Enum.GetValues<MovementType>()
                .Select(t => new SelectListItem(t.ToDisplayName(), t.ToString(), t == filter.Type)),
        });
    }

    [HttpGet]
    public async Task<IActionResult> Replenishments([FromQuery] ReplenishmentFilter filter, CancellationToken cancellationToken)
    {
        var (replenishments, totalQuantity) = ModelState.IsValid
            ? await historyService.GetReplenishmentsAsync(filter, cancellationToken)
            : (PagedList<ReplenishmentListItemViewModel>.Empty(HistoryService.PageSize), 0);

        var suppliers = await historyService.GetSupplierLookupsAsync(cancellationToken);

        return View(new ReplenishmentHistoryViewModel
        {
            Filter = filter,
            Replenishments = replenishments,
            TotalQuantity = totalQuantity,
            Suppliers = new SelectList(suppliers, nameof(LookupItem.Id), nameof(LookupItem.Name), filter.SupplierId),
        });
    }
}