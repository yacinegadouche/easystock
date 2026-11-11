using EasyStock.Web.Authorization;
using EasyStock.Web.Services;
using EasyStock.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EasyStock.Web.Controllers;

public class AssignmentsController(AssignmentService assignmentService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        AssignmentStatusFilter status = AssignmentStatusFilter.Active,
        CancellationToken cancellationToken = default)
    {
        var assignments = await assignmentService.GetListAsync(status, cancellationToken);
        return View(new AssignmentIndexViewModel { Status = status, Assignments = assignments });
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? employeeId, int? itemId, CancellationToken cancellationToken)
    {
        var model = new AssignmentCreateViewModel { EmployeeId = employeeId, ItemId = itemId };
        await PopulateLookupsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssignmentCreateViewModel model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var result = await assignmentService.CreateAsync(model, User.GetUserId(), cancellationToken);

            if (result.Succeeded)
            {
                TempData["Success"] = "Equipment assigned.";
                return RedirectToAction("Details", "Employees", new { id = model.EmployeeId });
            }

            ModelState.AddModelError(string.Empty, result.Error!);
        }

        await PopulateLookupsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Return(int id, CancellationToken cancellationToken)
    {
        var model = await assignmentService.GetByIdAsync(id, cancellationToken);

        if (model is null)
        {
            return NotFound();
        }

        if (!model.IsActive)
        {
            TempData["Error"] = "This assignment has already been returned.";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpPost]
    [ActionName("Return")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnConfirmed(int id, CancellationToken cancellationToken)
    {
        var result = await assignmentService.ReturnAsync(id, User.GetUserId(), cancellationToken);

        if (result.Succeeded)
        {
            TempData["Success"] = "Equipment returned to stock.";
        }
        else
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateLookupsAsync(AssignmentCreateViewModel model, CancellationToken cancellationToken)
    {
        var employees = await assignmentService.GetEmployeeLookupsAsync(cancellationToken);
        var items = await assignmentService.GetAvailableItemLookupsAsync(cancellationToken);

        model.Employees = new SelectList(employees, nameof(LookupItem.Id), nameof(LookupItem.Name));
        model.Items = new SelectList(items, nameof(LookupItem.Id), nameof(LookupItem.Name));
    }
}