namespace EasyStock.Web.Models;

public class User : AuditableEntity
{
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required string FullName { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
}