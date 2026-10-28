using ClosedXML.Excel;

namespace ExcelReportTool.Tests;

public sealed class PipelineTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "ert-" + Guid.NewGuid().ToString("N"));

    public PipelineTests() => Directory.CreateDirectory(_dir);

    public void Dispose() => Directory.Delete(_dir, recursive: true);

    [Fact]
    public void Merges_csv_and_xlsx_into_one_report()
    {
        File.WriteAllText(
            Path.Combine(_dir, "a.csv"),
            "Date,Store,Sales\n2026-10-01,Q1,\"1,240\"\n2026-10-02,Q1,—\n2026-10-02,Q3,980\n");

        using (var wb = new XLWorkbook())
        {
            var ws = wb.Worksheets.Add("Data");
            ws.Cell(1, 1).Value = "Date";
            ws.Cell(1, 2).Value = "Store";
            ws.Cell(1, 3).Value = "Sales";
            ws.Cell(2, 1).Value = new DateTime(2026, 10, 1);
            ws.Cell(2, 2).Value = "q3";
            ws.Cell(2, 3).Value = 20;
            wb.SaveAs(Path.Combine(_dir, "b.xlsx"));
        }

        var data = SourceReader.LoadFolder(_dir);
        var report = ReportBuilder.Build(data.Rows);

        Assert.Equal(new[] { "a.csv", "b.xlsx" }, data.Files);
        Assert.Equal(3, data.Rows.Count);
        var skipped = Assert.Single(data.Skipped);
        Assert.Equal("missing amount", skipped.Reason);
        Assert.Equal(2240m, report.GrandTotal);
        Assert.Equal("Q1", report.ByStore[0].Store);
        Assert.Equal(1000m, report.ByStore.Single(s => s.Store == "Q3").Total);
        Assert.Equal(1260m, report.ByDate.Single(d => d.Date == new DateOnly(2026, 10, 1)).Total);

        var output = Path.Combine(_dir, "out", "report.xlsx");
        ReportWriter.Write(output, report, data);

        using var check = new XLWorkbook(output);
        var summary = check.Worksheet("Summary");
        Assert.Equal("Grand total", summary.Cell(7, 1).GetText());
        Assert.Equal(2240m, summary.Cell(7, 3).GetValue<decimal>());
        Assert.Equal(3, check.Worksheet("Data").RangeUsed()!.RowCount() - 1);
        Assert.Equal("missing amount", check.Worksheet("Skipped").Cell(2, 3).GetText());
    }

    [Fact]
    public void Reports_files_with_a_wrong_header()
    {
        File.WriteAllText(Path.Combine(_dir, "bad.csv"), "Foo,Bar\n1,2\n");

        var data = SourceReader.LoadFolder(_dir);

        Assert.Empty(data.Rows);
        Assert.Contains("header", Assert.Single(data.Skipped).Reason);
    }
}
