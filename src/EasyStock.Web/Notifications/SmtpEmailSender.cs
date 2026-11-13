using MailKit;
using MailKit.Net.Smtp;

using Microsoft.Extensions.Options;

using MimeKit;
using MimeKit.Text;

namespace EasyStock.Web.Notifications;

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}

public class SmtpEmailSender(IOptions<EmailSettings> options) : IEmailSender
{
    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(settings.SenderName, settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(settings.Host, settings.Port, settings.Security, cancellationToken);

        if (!string.IsNullOrEmpty(settings.Username))
        {
            await client.AuthenticateAsync(settings.Username, settings.Password ?? string.Empty, cancellationToken);
        }

        await client.SendAsync(new SendRequest(message), cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);
    }
}