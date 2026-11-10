namespace EasyStock.Web.Models;

public class Employee : AuditableEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? Department { get; set; }
    public bool IsActive { get; set; } = true;

    public string FullName => $"{FirstName} {LastName}";

    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}