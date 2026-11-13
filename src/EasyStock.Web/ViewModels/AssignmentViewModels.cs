using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

using EasyStock.Web.Models;

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EasyStock.Web.ViewModels;

public enum AssignmentStatusFilter
{
    Active,
    Returned,
    All
}

public record AssignmentListItemViewModel(
    int Id,
    string ItemName,
    int EmployeeId,
    string EmployeeName,
    int Quantity,
    DateTime AssignedAt,
    DateTime? ReturnedAt,
    string AssignedBy)
{
    public bool IsActive => ReturnedAt is null;

    public static readonly Expression<Func<Assignment, AssignmentListItemViewModel>> Projection = a =>
        new AssignmentListItemViewModel(
            a.Id,
            a.Item.Name,
            a.EmployeeId,
            a.Employee.FirstName + " " + a.Employee.LastName,
            a.Quantity,
            a.AssignedAt,
            a.ReturnedAt,
            a.AssignedBy.FullName);
}

public class AssignmentIndexViewModel
{
    public AssignmentStatusFilter Status { get; init; }
    public required List<AssignmentListItemViewModel> Assignments { get; init; }
}

public class AssignmentCreateViewModel
{
    [Required(ErrorMessage = "Please choose an employee.")]
    [Display(Name = "Employee")]
    public int? EmployeeId { get; set; }

    [Required(ErrorMessage = "Please choose an item.")]
    [Display(Name = "Item")]
    public int? ItemId { get; set; }

    [Range(1, 10_000)]
    public int Quantity { get; set; } = 1;

    [StringLength(500)]
    public string? Notes { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem> Employees { get; set; } = [];

    [ValidateNever]
    public IEnumerable<SelectListItem> Items { get; set; } = [];
}