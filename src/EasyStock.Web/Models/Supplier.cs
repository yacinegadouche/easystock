namespace EasyStock.Web.Models;

public class Supplier : AuditableEntity
{
    public required string Name { get; set; }
    public string? ContactEmail { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }

    public ICollection<Item> Items { get; set; } = new List<Item>();
}