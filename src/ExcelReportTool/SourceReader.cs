using System.Globalization;
using ClosedXML.Excel;

namespace ExcelReportTool;

/// <summary>Reads every .csv and .xlsx file in a folder into one list of rows.</summary>
public static class SourceReader
{
    public static LoadResult LoadFolder(string folder)
    {
        var result = new LoadResult();
        var files = Directory.EnumerateFiles(folder)
            .Where(f => f.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
                     || f.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            .Where(f => !Path.GetFileName(f).StartsWith("~$", StringComparison.Ordinal))
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var file in files)
        {
            var table = file.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
                ? ReadCsv(file)
                : ReadXlsx(file);
            var name = Path.GetFileName(file);
            AddTable(result, name, table);
            result.Files.Add(name);
        }

        return result;
    }

    public static void AddTable(LoadResult result, string source, IReadOnlyList<string[]> table)
    {
        if (table.Count == 0)
        {
            result.Skipped.Add(new SkippedRow(source, 0, "empty file"));
            return;
        }

        var map = ColumnMap.FromHeader(table[0]);
        if (map is null)
        {
            result.Skipped.Add(new SkippedRow(source, 1, "header must contain Date, Store and Sales columns"));
            return;
        }

        for (var i = 1; i < table.Count; i++)
        {
            var cells = table[i];
            string? Cell(int index) => index < cells.Length ? cells[index] : null;

            if (RowParser.TryParse(Cell(map.Date), Cell(map.Store), Cell(map.Amount), source, i + 1, out var row, out var reason))
            {
                result.Rows.Add(row);
            }
            else
            {
                result.Skipped.Add(new SkippedRow(source, i + 1, reason));
            }
        }
    }

    private static List<string[]> ReadCsv(string path)
    {
        var text = File.ReadAllText(path).TrimStart('﻿');
        return CsvParser.Parse(text);
    }

    private static List<string[]> ReadXlsx(string path)
    {
        using var workbook = new XLWorkbook(path);
        var range = workbook.Worksheet(1).RangeUsed();
        var table = new List<string[]>();
        if (range is null)
        {
            return table;
        }

        var columns = range.ColumnCount();
        foreach (var xlRow in range.Rows())
        {
            var cells = new string[columns];
            for (var c = 1; c <= columns; c++)
            {
                cells[c - 1] = CellText(xlRow.Cell(c));
            }

            if (cells.Any(v => !string.IsNullOrWhiteSpace(v)))
            {
                table.Add(cells);
            }
        }

        return table;
    }

    private static string CellText(IXLCell cell) => cell.DataType switch
    {
        XLDataType.DateTime => cell.GetDateTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        XLDataType.Number => cell.GetDouble().ToString(CultureInfo.InvariantCulture),
        _ => cell.GetFormattedString(),
    };
}
