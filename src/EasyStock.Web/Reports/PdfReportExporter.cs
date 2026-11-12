using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace EasyStock.Web.Reports;

public class PdfReportExporter : IReportExporter
{
    public ReportFormat Format => ReportFormat.Pdf;
    public string ContentType => "application/pdf";
    public string FileExtension => "pdf";

    public byte[] Export(ReportDataset dataset) =>
        Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.Letter.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(style => style.FontSize(9));

                page.Header().PaddingBottom(10).Column(column =>
                {
                    column.Item().Text(dataset.Title).FontSize(16).Bold();
                    column.Item().Text(dataset.Description).FontColor(Colors.Grey.Darken1);
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        foreach (var _ in dataset.Headers)
                        {
                            columns.RelativeColumn();
                        }
                    });

                    table.Header(header =>
                    {
                        foreach (var title in dataset.Headers)
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(title).Bold();
                        }
                    });

                    foreach (var row in dataset.Rows)
                    {
                        foreach (var value in row)
                        {
                            var cell = table.Cell()
                                .BorderBottom(0.5f)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(4);

                            var text = ReportValueFormatter.Format(value);

                            if (value is int)
                            {
                                cell.AlignRight().Text(text);
                            }
                            else
                            {
                                cell.Text(text);
                            }
                        }
                    }
                });

                page.Footer().AlignCenter().Text(footer =>
                {
                    footer.Span("Page ");
                    footer.CurrentPageNumber();
                    footer.Span(" of ");
                    footer.TotalPages();
                });
            });
        }).GeneratePdf();
}