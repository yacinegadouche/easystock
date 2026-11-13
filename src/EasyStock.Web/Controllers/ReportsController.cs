using EasyStock.Web.Authorization;
using EasyStock.Web.Extensions;
using EasyStock.Web.Reports;
using EasyStock.Web.Services;
using EasyStock.Web.ViewModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyStock.Web.Controllers;

[Authorize(Policy = Policies.ViewReports)]
public class ReportsController(ReportService reportService, IEnumerable<IReportExporter> exporters) : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public async Task<IActionResult> Inventory(ReportFormat format, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return Export(await reportService.GetInventoryAsync(cancellationToken), format, "inventory");
    }

    [HttpGet]
    public async Task<IActionResult> LowStock(ReportFormat format, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return Export(await reportService.GetLowStockAsync(cancellationToken), format, "low-stock");
    }

    [HttpGet]
    public async Task<IActionResult> Movements(
        ReportFormat format,
        [FromQuery] MovementHistoryFilter filter,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return Export(await reportService.GetMovementsAsync(filter, cancellationToken), format, "movements");
    }

    [HttpGet]
    public async Task<IActionResult> Replenishments(
        ReportFormat format,
        [FromQuery] ReplenishmentFilter filter,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return Export(await reportService.GetReplenishmentsAsync(filter, cancellationToken), format, "replenishments");
    }

    private IActionResult Export(ReportDataset dataset, ReportFormat format, string reportName)
    {
        var exporter = exporters.SingleOrDefault(e => e.Format == format);

        if (exporter is null)
        {
            return BadRequest();
        }

        var date = DateTime.UtcNow.ToDisplayTimeZone().ToString("yyyy-MM-dd");
        var fileName = $"easystock-{reportName}-{date}.{exporter.FileExtension}";

        return File(exporter.Export(dataset), exporter.ContentType, fileName);
    }
}