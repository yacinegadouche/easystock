namespace EasyStock.Web.Models;

public class StockMovement
{
    public int Id { get; set; }

    public int ItemId { get; set; }
    public Item Item { get; set; } = null!;

    public MovementType Type { get; set; }
    public int Quantity { get; set; }
    public int QuantityAfter { get; set; }
    public string? Reason { get; set; }
    public string? ExternalReference { get; set; }
    public DateTime OccurredAt { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int? AssignmentId { get; set; }
    public Assignment? Assignment { get; set; }
}