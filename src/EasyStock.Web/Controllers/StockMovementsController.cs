using EasyStock.Web.Authorization;
using EasyStock.Web.Models;
using EasyStock.Web.Services;
using EasyStock.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EasyStock.Web.Controllers;

public class StockMovementsController(StockService stockService) : Controller
{
    private const int RecentMovementCount = 25;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await stockService.GetRecentAsync(RecentMovementCount, cancellationToken));

    [HttpGet]
    public async Task<IActionResult> Create(
        MovementType type = MovementType.Entry,
        int? itemId = null,
        CancellationToken cancellationToken = default)
    {
        if (!type.IsManual())
        {
            return BadRequest();
        }

        var model = new StockMovementFormViewModel { Type = type, ItemId = itemId };
        await PopulateItemsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StockMovementFormViewModel model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var result = await stockService.RecordManualMovementAsync(
                model.Type,
                model.ItemId!.Value,
                model.Quantity,
                User.GetUserId(),
                model.Reason,
                model.ExternalReference,
                cancellationToken);

            if (result.Succeeded)
            {
                TempData["Success"] = $"{model.Type.ToDisplayName()} of {model.Quantity} recorded.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, result.Error!);
        }

        await PopulateItemsAsync(model, cancellationToken);
        return View(model);
    }

    private async Task PopulateItemsAsync(StockMovementFormViewModel model, CancellationToken cancellationToken)
    {
        var items = await stockService.GetItemLookupsAsync(cancellationToken);
        model.Items = new SelectList(items, nameof(LookupItem.Id), nameof(LookupItem.Name));
    }
}