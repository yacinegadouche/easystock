using System.Net;
using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace EasyStock.Web.Notifications;

public record LowStockAlert(
    int ItemId,
    string ItemName,
    int QuantityInStock,
    int MinimumQuantity,
    string? SupplierName,
    string? SupplierEmail);

public class LowStockAlertQueue
{
    private readonly Channel<LowStockAlert> channel = Channel.CreateBounded<LowStockAlert>(
        new BoundedChannelOptions(capacity: 100) { FullMode = BoundedChannelFullMode.DropOldest });

    public void Enqueue(LowStockAlert alert) => channel.Writer.TryWrite(alert);

    public IAsyncEnumerable<LowStockAlert> ReadAllAsync(CancellationToken cancellationToken) =>
        channel.Reader.ReadAllAsync(cancellationToken);
}

public class LowStockNotificationWorker(
    LowStockAlertQueue queue,
    IEmailSender emailSender,
    IOptionsMonitor<EmailSettings> options,
    ILogger<LowStockNotificationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var alert in queue.ReadAllAsync(stoppingToken))
        {
            var settings = options.CurrentValue;

            if (!settings.Enabled)
            {
                logger.LogInformation("Email disabled; low stock alert for {ItemName} not sent.", alert.ItemName);
                continue;
            }

            try
            {
                await emailSender.SendAsync(
                    settings.RecipientEmail,
                    $"Low stock: {alert.ItemName}",
                    BuildBody(alert),
                    stoppingToken);

                logger.LogInformation("Low stock alert sent for {ItemName}.", alert.ItemName);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Failed to send the low stock alert for {ItemName}.", alert.ItemName);
            }
        }
    }

    private static string BuildBody(LowStockAlert alert)
    {
        var itemName = WebUtility.HtmlEncode(alert.ItemName);
        var supplier = alert.SupplierName is null
            ? "No supplier on file."
            : $"Supplier: {WebUtility.HtmlEncode(alert.SupplierName)}"
              + (alert.SupplierEmail is null ? "" : $" ({WebUtility.HtmlEncode(alert.SupplierEmail)})");

        return $"""
            <p><strong>{itemName}</strong> has reached its minimum stock level.</p>
            <p>In stock: <strong>{alert.QuantityInStock}</strong> &middot; Minimum: {alert.MinimumQuantity}</p>
            <p>{supplier}</p>
            <p style="color:#888">Sent by EasyStock.</p>
            """;
    }
}