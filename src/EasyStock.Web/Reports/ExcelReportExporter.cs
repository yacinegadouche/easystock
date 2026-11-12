using ClosedXML.Excel;
using EasyStock.Web.Extensions;

namespace EasyStock.Web.Reports;

public class ExcelReportExporter : IReportExporter
{
    private const int HeaderRow = 4;

    public ReportFormat Format => ReportFormat.Excel;
    public string ContentType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public string FileExtension => "xlsx";

    public byte[] Export(ReportDataset dataset)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(dataset.Title.Length <= 31 ? dataset.Title : dataset.Title[..31]);

        sheet.Cell(1, 1).Value = dataset.Title;
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;
        sheet.Cell(2, 1).Value = dataset.Description;
        sheet.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;

        for (var column = 0; column < dataset.Headers.Count; column++)
        {
            var cell = sheet.Cell(HeaderRow, column + 1);
            cell.Value = dataset.Headers[column];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        for (var row = 0; row < dataset.Rows.Count; row++)
        {
            for (var column = 0; column < dataset.Headers.Count; column++)
            {
                SetValue(sheet.Cell(HeaderRow + 1 + row, column + 1), dataset.Rows[row][column]);
            }
        }

        sheet.SheetView.FreezeRows(HeaderRow);

        if (dataset.Rows.Count > 0)
        {
            sheet.Range(HeaderRow, 1, HeaderRow + dataset.Rows.Count, dataset.Headers.Count).SetAutoFilter();
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void SetValue(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null:
                break;
            case int number:
                cell.Value = number;
                break;
            case DateTime date:
                cell.Value = date.ToDisplayTimeZone();
                cell.Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
                break;
            default:
                cell.Value = value.ToString();
                break;
        }
    }
}