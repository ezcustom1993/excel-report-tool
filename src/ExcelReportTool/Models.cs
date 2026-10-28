namespace ExcelReportTool;

/// <summary>One valid sales line read from an input file.</summary>
public sealed record SalesRow(DateOnly Date, string Store, decimal Amount, string SourceFile, int Row);

/// <summary>A line that could not be used, with the reason shown in the report.</summary>
public sealed record SkippedRow(string SourceFile, int Row, string Reason);

public sealed record StoreTotal(string Store, decimal Total, int Rows);

public sealed record DateTotal(DateOnly Date, decimal Total, int Rows);

public sealed record SalesReport(
    IReadOnlyList<StoreTotal> ByStore,
    IReadOnlyList<DateTotal> ByDate,
    decimal GrandTotal,
    int RowCount);

public sealed class LoadResult
{
    public List<SalesRow> Rows { get; } = new();
    public List<SkippedRow> Skipped { get; } = new();
    public List<string> Files { get; } = new();
}
