using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ExcelReportTool;

/// <summary>Turns raw cell text into a validated <see cref="SalesRow"/>.</summary>
public static class RowParser
{
    private static readonly string[] DateFormats =
    {
        "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy", "dd.MM.yyyy"
    };

    public static bool TryParse(
        string? date,
        string? store,
        string? amount,
        string source,
        int row,
        [NotNullWhen(true)] out SalesRow? result,
        out string reason)
    {
        result = null;
        reason = string.Empty;

        if (!TryParseDate(date, out var parsedDate))
        {
            reason = $"invalid date '{date}'";
            return false;
        }

        var storeName = store?.Trim();
        if (string.IsNullOrEmpty(storeName))
        {
            reason = "missing store";
            return false;
        }

        if (!TryParseAmount(amount, out var parsedAmount))
        {
            reason = string.IsNullOrWhiteSpace(amount) || IsDash(amount)
                ? "missing amount"
                : $"invalid amount '{amount}'";
            return false;
        }

        result = new SalesRow(parsedDate, storeName.ToUpperInvariant(), parsedAmount, source, row);
        return true;
    }

    public static bool TryParseDate(string? value, out DateOnly date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var text = value.Trim();
        var space = text.IndexOf(' ');
        if (space > 0)
        {
            text = text[..space]; // drop a time part such as "2026-10-01 00:00:00"
        }

        return DateOnly.TryParseExact(text, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }

    /// <summary>Accepts "1240", "1,240", "1 240" and "980.5" (invariant format).</summary>
    public static bool TryParseAmount(string? value, out decimal amount)
    {
        amount = 0;
        if (string.IsNullOrWhiteSpace(value) || IsDash(value))
        {
            return false;
        }

        var text = value.Trim().Replace(" ", string.Empty);
        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out amount);
    }

    private static bool IsDash(string value) => value.Trim() is "-" or "—" or "–";
}
