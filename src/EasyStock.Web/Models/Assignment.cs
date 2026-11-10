namespace EasyStock.Web.Models;

public class Assignment : AuditableEntity
{
    public int ItemId { get; set; }
    public Item Item { get; set; } = null!;

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public int Quantity { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public string? Notes { get; set; }

    public int AssignedByUserId { get; set; }
    public User AssignedBy { get; set; } = null!;

    public bool IsActive => ReturnedAt is null;
}