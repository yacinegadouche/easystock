using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using EasyStock.Web.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EasyStock.Web.ViewModels;

public record StockMovementListItemViewModel(
    int Id,
    DateTime OccurredAt,
    MovementType Type,
    string ItemName,
    int Quantity,
    int QuantityAfter,
    int MinimumQuantity,
    string? Reason,
    string? ExternalReference,
    string UserName)
{
    public bool IsIncrease => Type.IsIncrease();
    public bool LeftLowStock => Item.IsLow(QuantityAfter, MinimumQuantity);

    public static readonly Expression<Func<StockMovement, StockMovementListItemViewModel>> Projection = m =>
        new StockMovementListItemViewModel(
            m.Id,
            m.OccurredAt,
            m.Type,
            m.Item.Name,
            m.Quantity,
            m.QuantityAfter,
            m.Item.MinimumQuantity,
            m.Reason,
            m.ExternalReference,
            m.User.FullName);
}

public class StockMovementFormViewModel : IValidatableObject
{
    [EnumDataType(typeof(MovementType))]
    public MovementType Type { get; set; }

    [Required(ErrorMessage = "Please choose an item.")]
    [Display(Name = "Item")]
    public int? ItemId { get; set; }

    [Range(1, 100_000)]
    public int Quantity { get; set; } = 1;

    [StringLength(500)]
    public string? Reason { get; set; }

    [StringLength(100)]
    [Display(Name = "External reference")]
    public string? ExternalReference { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem> Items { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Type.IsManual())
        {
            yield return new ValidationResult(
                "This movement type cannot be recorded manually.",
                [nameof(Type)]);
        }

        if (Type == MovementType.Exit && string.IsNullOrWhiteSpace(Reason))
        {
            yield return new ValidationResult(
                "A reason is required for stock exits.",
                [nameof(Reason)]);
        }
    }
}