using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

using EasyStock.Web.Models;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace EasyStock.Web.ViewModels;

public abstract class DateRangeFilter : IValidatableObject
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public int Page { get; set; } = 1;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (From is not null && To is not null && From > To)
        {
            yield return new ValidationResult("The start date must be before the end date.", [nameof(From)]);
        }
    }
}

public class MovementHistoryFilter : DateRangeFilter
{
    public int? ItemId { get; set; }
    public MovementType? Type { get; set; }
    public int? UserId { get; set; }
}

public class ReplenishmentFilter : DateRangeFilter
{
    public int? SupplierId { get; set; }
}

public class MovementHistoryViewModel
{
    public required MovementHistoryFilter Filter { get; init; }
    public required PagedList<StockMovementListItemViewModel> Movements { get; init; }
    public required SelectList Items { get; init; }
    public required SelectList Users { get; init; }
    public required IEnumerable<SelectListItem> Types { get; init; }
}

public record ReplenishmentListItemViewModel(
    int Id,
    DateTime OccurredAt,
    string ItemName,
    string? SupplierName,
    int Quantity,
    string? ExternalReference,
    string? Reason,
    string UserName)
{
    public static readonly Expression<Func<StockMovement, ReplenishmentListItemViewModel>> Projection = m =>
        new ReplenishmentListItemViewModel(
            m.Id,
            m.OccurredAt,
            m.Item.Name,
            m.Item.Supplier != null ? m.Item.Supplier.Name : null,
            m.Quantity,
            m.ExternalReference,
            m.Reason,
            m.User.FullName);
}

public class ReplenishmentHistoryViewModel
{
    public required ReplenishmentFilter Filter { get; init; }
    public required PagedList<ReplenishmentListItemViewModel> Replenishments { get; init; }
    public required int TotalQuantity { get; init; }
    public required SelectList Suppliers { get; init; }
}