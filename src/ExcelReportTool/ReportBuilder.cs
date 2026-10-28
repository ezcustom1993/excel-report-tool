namespace ExcelReportTool;

public static class ReportBuilder
{
    public static SalesReport Build(IReadOnlyCollection<SalesRow> rows)
    {
        var byStore = rows
            .GroupBy(r => r.Store)
            .Select(g => new StoreTotal(g.Key, g.Sum(r => r.Amount), g.Count()))
            .OrderByDescending(s => s.Total)
            .ThenBy(s => s.Store, StringComparer.Ordinal)
            .ToList();

        var byDate = rows
            .GroupBy(r => r.Date)
            .Select(g => new DateTotal(g.Key, g.Sum(r => r.Amount), g.Count()))
            .OrderBy(d => d.Date)
            .ToList();

        return new SalesReport(byStore, byDate, rows.Sum(r => r.Amount), rows.Count);
    }
}
