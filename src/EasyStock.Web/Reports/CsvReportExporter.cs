using System.Text;

namespace EasyStock.Web.Reports;

public class CsvReportExporter : IReportExporter
{
    private static readonly char[] CharactersRequiringQuotes = [',', '"', '\r', '\n'];
    private const string FormulaTriggers = "=+-@\t\r";

    public ReportFormat Format => ReportFormat.Csv;
    public string ContentType => "text/csv";
    public string FileExtension => "csv";

    public byte[] Export(ReportDataset dataset)
    {
        var builder = new StringBuilder();

        AppendLine(builder, dataset.Headers);

        foreach (var row in dataset.Rows)
        {
            AppendLine(builder, row.Select(value =>
                value is string text ? NeutralizeFormula(text) : ReportValueFormatter.Format(value)));
        }

        byte[] bom = Encoding.UTF8.GetPreamble();
        byte[] content = Encoding.UTF8.GetBytes(builder.ToString());
        return [.. bom, .. content];
    }

    private static void AppendLine(StringBuilder builder, IEnumerable<string> values)
    {
        builder.AppendJoin(',', values.Select(Quote));
        builder.Append("\r\n");
    }

    private static string Quote(string value) =>
        value.IndexOfAny(CharactersRequiringQuotes) >= 0
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;

    private static string NeutralizeFormula(string value) =>
        value.Length > 0 && FormulaTriggers.Contains(value[0]) ? "'" + value : value;
}