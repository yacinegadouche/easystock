using EasyStock.Web.Services;
using EasyStock.Web.ViewModels;

using Microsoft.AspNetCore.Mvc;

namespace EasyStock.Web.Controllers;

public class SuppliersController(SupplierService supplierService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await supplierService.GetAllAsync(cancellationToken));

    [HttpGet]
    public IActionResult Create() => View(new SupplierFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupplierFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await supplierService.CreateAsync(model, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(model.Name), result.Error!);
            return View(model);
        }

        TempData["Success"] = $"Supplier '{model.Name.Trim()}' created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await supplierService.GetForEditAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SupplierFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await supplierService.UpdateAsync(model, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(model.Name), result.Error!);
            return View(model);
        }

        TempData["Success"] = $"Supplier '{model.Name.Trim()}' updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var model = await supplierService.GetForEditAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var result = await supplierService.DeleteAsync(id, cancellationToken);

        if (result.Succeeded)
        {
            TempData["Success"] = "Supplier deleted.";
        }
        else
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Index));
    }
}