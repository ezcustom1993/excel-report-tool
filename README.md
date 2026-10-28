# Excel Report Tool

![build](https://github.com/ezcustom1993/excel-report-tool/actions/workflows/build.yml/badge.svg)

A small C# (.NET 8) console tool that turns a folder of sales files into one clean Excel report.

Many small shops send daily sales as separate Excel or CSV files, one per branch. Merging them by hand takes hours and mistakes slip in. This tool does it in seconds:

- reads every `.xlsx` and `.csv` file in a folder (comma or semicolon CSV, UTF-8)
- finds the **Date**, **Store** and **Sales** columns by header name, in English or Vietnamese (`Ngày`, `Cửa hàng`, `Doanh thu`)
- accepts common formats: `2026-10-01`, `01/10/2026`, `1,240`, `1 240`, `980.50`
- cleans the data: trims and normalises store names, skips empty or invalid rows **and lists why** each one was skipped
- writes `report.xlsx` with four sheets: **Summary** (total per store + grand total), **By date**, **Data** (every valid row with its source file) and **Skipped**

## Run it

```bash
dotnet run --project src/ExcelReportTool -- samples report.xlsx
```

Output for the sample files:

```
Read 3 file(s): 9 valid row(s), 2 skipped.
  Q1                    3,800.00
  Q3                    2,975.50
  Q7                    2,175.00
  TOTAL                 8,950.50
  skipped store-q1.csv row 4: missing amount
  skipped store-q3.csv row 4: invalid date '2026-13-01'
Report written to /path/to/report.xlsx
```

To get a single `.exe` for Windows users who do not have .NET installed:

```bash
dotnet publish src/ExcelReportTool -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

## Tests

```bash
dotnet test tests/ExcelReportTool.Tests
```

The tests cover date and number parsing, the CSV parser (quotes, escaped quotes, delimiters), Vietnamese headers, and an end-to-end run that merges a CSV and an XLSX file and checks the totals in the generated workbook. GitHub Actions runs the build, the tests and the sample run on every push and keeps the generated report as a downloadable artifact.

## Project layout

```
src/ExcelReportTool/
  Program.cs        command line entry point
  SourceReader.cs   loads CSV and XLSX files
  CsvParser.cs      quoted CSV parsing
  ColumnMap.cs      finds columns by header name
  RowParser.cs      validates and normalises one row
  ReportBuilder.cs  totals per store and per date
  ReportWriter.cs   writes the Excel report (ClosedXML)
tests/ExcelReportTool.Tests/
samples/            three example input files
```

Built by **Kien Code**: small C# tools that save you time.
