using System.ComponentModel.DataAnnotations;

namespace EasyStock.Web.ViewModels;

public record SupplierListItemViewModel(int Id, string Name, string? ContactEmail, string? Phone, int ItemCount);

public class SupplierFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(254)]
    [Display(Name = "Contact email")]
    public string? ContactEmail { get; set; }

    [Phone]
    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }
}