using System.ComponentModel.DataAnnotations;
using MailKit.Security;

namespace EasyStock.Web.Notifications;

public class EmailSettings
{
    public const string SectionName = "Email";

    public bool Enabled { get; set; }

    [Required]
    public string Host { get; set; } = "localhost";

    [Range(1, 65535)]
    public int Port { get; set; } = 1025;

    public SecureSocketOptions Security { get; set; } = SecureSocketOptions.None;

    public string? Username { get; set; }

    public string? Password { get; set; }

    [Required]
    [EmailAddress]
    public string SenderEmail { get; set; } = "easystock@easystock.example";

    [Required]
    public string SenderName { get; set; } = "EasyStock";

    [Required]
    [EmailAddress]
    public string RecipientEmail { get; set; } = "inventory@easystock.example";
}