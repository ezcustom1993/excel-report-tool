using ExcelReportTool;

var input = args.Length > 0 ? args[0] : "input";
var output = args.Length > 1 ? args[1] : "report.xlsx";

if (!Directory.Exists(input))
{
    Console.Error.WriteLine($"Input folder not found: {input}");
    Console.Error.WriteLine("Usage: ExcelReportTool <input-folder> [output.xlsx]");
    return 1;
}

var data = SourceReader.LoadFolder(input);
var report = ReportBuilder.Build(data.Rows);
ReportWriter.Write(output, report, data);

Console.WriteLine($"Read {data.Files.Count} file(s): {data.Rows.Count} valid row(s), {data.Skipped.Count} skipped.");
foreach (var store in report.ByStore)
{
    Console.WriteLine($"  {store.Store,-14}{store.Total,16:N2}");
}

Console.WriteLine($"  {"TOTAL",-14}{report.GrandTotal,16:N2}");
foreach (var skip in data.Skipped)
{
    Console.WriteLine($"  skipped {skip.SourceFile} row {skip.Row}: {skip.Reason}");
}

Console.WriteLine($"Report written to {Path.GetFullPath(output)}");
return 0;
