namespace ExcelReportTool.Tests;

public class CsvParserTests
{
    [Fact]
    public void Handles_quotes_and_embedded_commas()
    {
        var rows = CsvParser.Parse("Date,Store,Sales\r\n2026-10-01,\"Q1, Main\",\"1,240\"\r\n");

        Assert.Equal(2, rows.Count);
        Assert.Equal(new[] { "2026-10-01", "Q1, Main", "1,240" }, rows[1]);
    }

    [Fact]
    public void Handles_escaped_quotes()
    {
        var rows = CsvParser.Parse("a,b\n\"say \"\"hi\"\"\",2\n");

        Assert.Equal("say \"hi\"", rows[1][0]);
    }

    [Fact]
    public void Detects_semicolon_delimiter()
    {
        var rows = CsvParser.Parse("Date;Store;Sales\n01/10/2026;Q3;980\n");

        Assert.Equal(new[] { "01/10/2026", "Q3", "980" }, rows[1]);
    }

    [Fact]
    public void Skips_blank_lines()
    {
        var rows = CsvParser.Parse("Date,Store,Sales\n\n2026-10-01,Q1,10\n\n");

        Assert.Equal(2, rows.Count);
    }

    [Fact]
    public void Finds_vietnamese_headers()
    {
        var map = ColumnMap.FromHeader(new[] { "Ngày", "Cửa hàng", "Doanh thu" });

        Assert.Equal(new ColumnMap(0, 1, 2), map);
    }
}
