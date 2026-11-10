using EasyStock.Web.Services;
using EasyStock.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EasyStock.Web.Controllers;

public class CategoriesController(CategoryService categoryService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await categoryService.GetAllAsync(cancellationToken));

    [HttpGet]
    public IActionResult Create() => View(new CategoryFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await categoryService.CreateAsync(model, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(model.Name), result.Error!);
            return View(model);
        }

        TempData["Success"] = $"Category '{model.Name.Trim()}' created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await categoryService.GetForEditAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await categoryService.UpdateAsync(model, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(model.Name), result.Error!);
            return View(model);
        }

        TempData["Success"] = $"Category '{model.Name.Trim()}' updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var model = await categoryService.GetForEditAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var result = await categoryService.DeleteAsync(id, cancellationToken);

        if (result.Succeeded)
        {
            TempData["Success"] = "Category deleted.";
        }
        else
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Index));
    }
}