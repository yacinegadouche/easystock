using EasyStock.Web.Services;
using EasyStock.Web.ViewModels;

using Microsoft.AspNetCore.Mvc;

namespace EasyStock.Web.Controllers;

public class EmployeesController(EmployeeService employeeService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? search, bool includeInactive, CancellationToken cancellationToken)
    {
        var employees = await employeeService.GetAllAsync(search, includeInactive, cancellationToken);

        return View(new EmployeeIndexViewModel
        {
            Search = search,
            IncludeInactive = includeInactive,
            Employees = employees,
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var model = await employeeService.GetDetailsAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpGet]
    public IActionResult Create() => View(new EmployeeFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await employeeService.CreateAsync(model, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(model.Email), result.Error!);
            return View(model);
        }

        TempData["Success"] = $"Employee '{model.FirstName.Trim()} {model.LastName.Trim()}' created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await employeeService.GetForEditAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EmployeeFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await employeeService.UpdateAsync(model, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        TempData["Success"] = "Employee updated.";
        return RedirectToAction(nameof(Details), new { id });
    }
}