using System.Text;
using EasyStock.Web.Reports;

namespace EasyStock.Tests.Unit;

public class CsvReportExporterTests
{
    private static byte[] ExportBytes(params object?[][] rows) =>
        new CsvReportExporter().Export(new ReportDataset("Test", "Subtitle.", ["A", "B"], rows, DateTime.UtcNow));

    private static string Export(params object?[][] rows) => Encoding.UTF8.GetString(ExportBytes(rows));

    [Fact]
    public void Export_StartsWithUtf8ByteOrderMark()
    {
        var bytes = ExportBytes();

        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bytes[..3]);
    }

    [Fact]
    public void Export_UsesCrlfLineEndings()
    {
        var csv = Export(new object?[] { "x", 1 });

        Assert.Equal("\uFEFFA,B\r\nx,1\r\n", csv);
    }

    [Fact]
    public void Export_QuotesValuesContainingCommasAndDoublesInnerQuotes()
    {
        var csv = Export(new object?[] { "i7, 16 GB", "Say \"hi\"" });

        Assert.EndsWith("\"i7, 16 GB\",\"Say \"\"hi\"\"\"\r\n", csv);
    }

    [Fact]
    public void Export_NeutralizesFormulasButKeepsNegativeNumbers()
    {
        var csv = Export(new object?[] { "=1+1", -5 });

        Assert.EndsWith("'=1+1,-5\r\n", csv);
    }

    [Fact]
    public void Export_WritesNullAsEmptyCell()
    {
        var csv = Export(new object?[] { null, "x" });

        Assert.EndsWith(",x\r\n", csv);
    }
}