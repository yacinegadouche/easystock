using System.Globalization;
using EasyStock.Web.Extensions;

namespace EasyStock.Web.Reports;

public enum ReportFormat
{
    Csv,
    Excel,
    Pdf
}

public record ReportDataset(
    string Title,
    string Subtitle,
    IReadOnlyList<string> Headers,
    IReadOnlyList<object?[]> Rows,
    DateTime GeneratedAtUtc)
{
    public string Description => $"{Subtitle} Generated {GeneratedAtUtc.ToDisplay()}.";
}

public interface IReportExporter
{
    ReportFormat Format { get; }
    string ContentType { get; }
    string FileExtension { get; }
    byte[] Export(ReportDataset dataset);
}

public static class ReportValueFormatter
{
    public static string Format(object? value) => value switch
    {
        null => string.Empty,
        DateTime date => date.ToDisplay(),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty,
    };
}