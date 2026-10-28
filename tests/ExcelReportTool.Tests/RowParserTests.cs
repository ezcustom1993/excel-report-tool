namespace ExcelReportTool.Tests;

public class RowParserTests
{
    [Theory]
    [InlineData("2026-10-01")]
    [InlineData("01/10/2026")]
    [InlineData("1/10/2026")]
    [InlineData("2026-10-01 00:00:00")]
    public void Parses_supported_date_formats(string input)
    {
        Assert.True(RowParser.TryParseDate(input, out var date));
        Assert.Equal(new DateOnly(2026, 10, 1), date);
    }

    [Theory]
    [InlineData("1,240", 1240)]
    [InlineData("980.5", 980.5)]
    [InlineData(" 1 410 ", 1410)]
    public void Parses_amounts(string input, double expected)
    {
        Assert.True(RowParser.TryParseAmount(input, out var amount));
        Assert.Equal((decimal)expected, amount);
    }

    [Theory]
    [InlineData("—", "missing amount")]
    [InlineData("", "missing amount")]
    [InlineData("abc", "invalid amount 'abc'")]
    public void Rejects_bad_amounts_with_a_reason(string input, string expected)
    {
        Assert.False(RowParser.TryParse("2026-10-01", "Q1", input, "f.csv", 2, out _, out var reason));
        Assert.Equal(expected, reason);
    }

    [Fact]
    public void Rejects_impossible_dates()
    {
        Assert.False(RowParser.TryParse("2026-13-01", "Q1", "10", "f.csv", 2, out _, out var reason));
        Assert.Equal("invalid date '2026-13-01'", reason);
    }

    [Fact]
    public void Normalises_store_names()
    {
        Assert.True(RowParser.TryParse("2026-10-01", " q1 ", "10", "f.csv", 2, out var row, out _));
        Assert.Equal("Q1", row.Store);
    }
}
