using System.ComponentModel.DataAnnotations;

using EasyStock.Web.Models;

namespace EasyStock.Web.ViewModels;

public record UserListItemViewModel(int Id, string Username, string FullName, UserRole Role, bool IsActive);

public class UserCreateViewModel
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression("^[a-zA-Z0-9._-]+$", ErrorMessage = "Use letters, digits, dots, dashes or underscores only.")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [EnumDataType(typeof(UserRole))]
    public UserRole Role { get; set; } = UserRole.Operator;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class UserEditViewModel
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [EnumDataType(typeof(UserRole))]
    public UserRole Role { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; }
}

public class UserResetPasswordViewModel
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}