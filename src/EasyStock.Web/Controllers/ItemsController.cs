using EasyStock.Web.Authorization;
using EasyStock.Web.Services;
using EasyStock.Web.ViewModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EasyStock.Web.Controllers;

public class ItemsController(ItemService itemService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        string? search,
        int? categoryId,
        bool lowStockOnly,
        CancellationToken cancellationToken)
    {
        var items = await itemService.GetListAsync(search, categoryId, lowStockOnly, cancellationToken);
        var categories = await itemService.GetCategoryLookupsAsync(cancellationToken);

        return View(new ItemIndexViewModel
        {
            Search = search,
            CategoryId = categoryId,
            LowStockOnly = lowStockOnly,
            Items = items,
            Categories = new SelectList(categories, nameof(LookupItem.Id), nameof(LookupItem.Name), categoryId),
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new ItemFormViewModel();
        await PopulateLookupsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ItemFormViewModel model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var result = await itemService.CreateAsync(model, User.GetUserId(), cancellationToken);

            if (result.Succeeded)
            {
                TempData["Success"] = $"Item '{model.Name.Trim()}' created.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, result.Error!);
        }

        await PopulateLookupsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await itemService.GetForEditAsync(id, cancellationToken);

        if (model is null)
        {
            return NotFound();
        }

        await PopulateLookupsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ItemFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            var result = await itemService.UpdateAsync(model, cancellationToken);

            if (result.Succeeded)
            {
                TempData["Success"] = $"Item '{model.Name.Trim()}' updated.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, result.Error!);
        }

        await PopulateLookupsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var model = await itemService.GetForEditAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var result = await itemService.DeleteAsync(id, cancellationToken);

        if (result.Succeeded)
        {
            TempData["Success"] = "Item deleted.";
        }
        else
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateLookupsAsync(ItemFormViewModel model, CancellationToken cancellationToken)
    {
        var categories = await itemService.GetCategoryLookupsAsync(cancellationToken);
        var suppliers = await itemService.GetSupplierLookupsAsync(cancellationToken);

        model.Categories = new SelectList(categories, nameof(LookupItem.Id), nameof(LookupItem.Name));
        model.Suppliers = new SelectList(suppliers, nameof(LookupItem.Id), nameof(LookupItem.Name));
    }
}