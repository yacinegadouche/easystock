using System.ComponentModel.DataAnnotations;
using EasyStock.Web.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EasyStock.Web.ViewModels;

public record LookupItem(int Id, string Name);

public record ItemListItemViewModel(
    int Id,
    string Name,
    string CategoryName,
    string? SupplierName,
    int QuantityInStock,
    int MinimumQuantity)
{
    public bool IsLowStock => Item.IsLow(QuantityInStock, MinimumQuantity);
}

public class ItemIndexViewModel
{
    public string? Search { get; init; }
    public int? CategoryId { get; init; }
    public bool LowStockOnly { get; init; }
    public required List<ItemListItemViewModel> Items { get; init; }
    public required SelectList Categories { get; init; }
}

public class ItemFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Please choose a category.")]
    [Display(Name = "Category")]
    public int? CategoryId { get; set; }

    [Display(Name = "Supplier")]
    public int? SupplierId { get; set; }

    [Range(0, 100_000)]
    [Display(Name = "Minimum quantity")]
    public int MinimumQuantity { get; set; }

    [Range(0, 100_000)]
    [Display(Name = "Initial quantity")]
    public int InitialQuantity { get; set; }

    [Display(Name = "Quantity in stock")]
    public int QuantityInStock { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem> Categories { get; set; } = [];

    [ValidateNever]
    public IEnumerable<SelectListItem> Suppliers { get; set; } = [];
}