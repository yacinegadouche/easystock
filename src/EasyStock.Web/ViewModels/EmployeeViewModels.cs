using System.ComponentModel.DataAnnotations;

namespace EasyStock.Web.ViewModels;

public record EmployeeListItemViewModel(
    int Id,
    string FullName,
    string Email,
    string? Department,
    bool IsActive,
    int ActiveAssignmentCount);

public class EmployeeIndexViewModel
{
    public string? Search { get; init; }
    public bool IncludeInactive { get; init; }
    public required List<EmployeeListItemViewModel> Employees { get; init; }
}

public class EmployeeDetailsViewModel
{
    public required int Id { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public string? Department { get; init; }
    public required bool IsActive { get; init; }
    public required List<AssignmentListItemViewModel> Assignments { get; init; }
}

public class EmployeeFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Department { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}