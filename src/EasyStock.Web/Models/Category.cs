namespace EasyStock.Web.Models;

public class Category : AuditableEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }

    public ICollection<Item> Items { get; set; } = new List<Item>();
}