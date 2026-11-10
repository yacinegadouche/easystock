using System.ComponentModel.DataAnnotations;

namespace EasyStock.Web.ViewModels;

public record CategoryListItemViewModel(int Id, string Name, string? Description, int ItemCount);

public class CategoryFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}