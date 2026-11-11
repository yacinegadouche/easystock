namespace EasyStock.Web.Models;

public class Item : AuditableEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public int QuantityInStock { get; set; }
    public int MinimumQuantity { get; set; }

    public bool IsLowStock => IsLow(QuantityInStock, MinimumQuantity);

    public static bool IsLow(int quantityInStock, int minimumQuantity) =>
        quantityInStock <= minimumQuantity;

    public ICollection<StockMovement> Movements { get; set; } = new List<StockMovement>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}