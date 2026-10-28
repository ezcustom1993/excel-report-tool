using ClosedXML.Excel;

namespace ExcelReportTool;

/// <summary>Writes the report workbook: Summary, By date, Data and Skipped sheets.</summary>
public static class ReportWriter
{
    private const string MoneyFormat = "#,##0.00";
    private const string DateFormat = "yyyy-mm-dd";

    public static void Write(string path, SalesReport report, LoadResult data)
    {
        using var workbook = new XLWorkbook();

        var summary = workbook.Worksheets.Add("Summary");
        summary.Cell(1, 1).Value = "Sales report";
        summary.Cell(1, 1).Style.Font.Bold = true;
        summary.Cell(1, 1).Style.Font.FontSize = 14;
        summary.Cell(2, 1).Value = $"Generated {DateTime.Now:yyyy-MM-dd HH:mm} from {data.Files.Count} file(s), {data.Skipped.Count} row(s) skipped";
        Header(summary, 4, "Store", "Rows", "Total");
        var r = 5;
        foreach (var s in report.ByStore)
        {
            summary.Cell(r, 1).Value = s.Store;
            summary.Cell(r, 2).Value = s.Rows;
            summary.Cell(r, 3).Value = s.Total;
            r++;
        }

        summary.Cell(r, 1).Value = "Grand total";
        summary.Cell(r, 2).Value = report.RowCount;
        summary.Cell(r, 3).Value = report.GrandTotal;
        summary.Range(r, 1, r, 3).Style.Font.Bold = true;
        summary.Range(r, 1, r, 3).Style.Border.TopBorder = XLBorderStyleValues.Thin;
        summary.Column(3).Style.NumberFormat.Format = MoneyFormat;
        Widths(summary, 24, 10, 16);

        var byDate = workbook.Worksheets.Add("By date");
        Header(byDate, 1, "Date", "Rows", "Total");
        r = 2;
        foreach (var d in report.ByDate)
        {
            byDate.Cell(r, 1).Value = d.Date.ToDateTime(TimeOnly.MinValue);
            byDate.Cell(r, 2).Value = d.Rows;
            byDate.Cell(r, 3).Value = d.Total;
            r++;
        }

        byDate.Column(1).Style.NumberFormat.Format = DateFormat;
        byDate.Column(3).Style.NumberFormat.Format = MoneyFormat;
        Widths(byDate, 14, 10, 16);

        var rows = workbook.Worksheets.Add("Data");
        Header(rows, 1, "Date", "Store", "Sales", "Source file", "Row");
        r = 2;
        foreach (var row in data.Rows.OrderBy(x => x.Date).ThenBy(x => x.Store, StringComparer.Ordinal))
        {
            rows.Cell(r, 1).Value = row.Date.ToDateTime(TimeOnly.MinValue);
            rows.Cell(r, 2).Value = row.Store;
            rows.Cell(r, 3).Value = row.Amount;
            rows.Cell(r, 4).Value = row.SourceFile;
            rows.Cell(r, 5).Value = row.Row;
            r++;
        }

        rows.Column(1).Style.NumberFormat.Format = DateFormat;
        rows.Column(3).Style.NumberFormat.Format = MoneyFormat;
        Widths(rows, 14, 14, 16, 28, 8);

        var skipped = workbook.Worksheets.Add("Skipped");
        Header(skipped, 1, "Source file", "Row", "Reason");
        r = 2;
        foreach (var s in data.Skipped)
        {
            skipped.Cell(r, 1).Value = s.SourceFile;
            skipped.Cell(r, 2).Value = s.Row;
            skipped.Cell(r, 3).Value = s.Reason;
            r++;
        }

        Widths(skipped, 28, 8, 50);

        var folder = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(folder))
        {
            Directory.CreateDirectory(folder);
        }

        workbook.SaveAs(path);
    }

    private static void Header(IXLWorksheet sheet, int row, params string[] titles)
    {
        for (var i = 0; i < titles.Length; i++)
        {
            var cell = sheet.Cell(row, i + 1);
            cell.Value = titles[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#FFE2A8");
        }
    }

    private static void Widths(IXLWorksheet sheet, params double[] widths)
    {
        for (var i = 0; i < widths.Length; i++)
        {
            sheet.Column(i + 1).Width = widths[i];
        }
    }
}
