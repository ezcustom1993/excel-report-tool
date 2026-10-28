namespace ExcelReportTool;

/// <summary>Finds the Date / Store / Sales columns by header name (English or Vietnamese).</summary>
public sealed record ColumnMap(int Date, int Store, int Amount)
{
    private static readonly string[] DateNames = { "date", "day", "ngay", "ngày" };
    private static readonly string[] StoreNames = { "store", "branch", "shop", "cua hang", "cửa hàng", "chi nhanh", "chi nhánh" };
    private static readonly string[] AmountNames = { "sales", "amount", "revenue", "total", "doanh thu", "so tien", "số tiền" };

    public static ColumnMap? FromHeader(IReadOnlyList<string> header)
    {
        int Find(string[] names)
        {
            for (var i = 0; i < header.Count; i++)
            {
                if (names.Contains(header[i].Trim().ToLowerInvariant()))
                {
                    return i;
                }
            }

            return -1;
        }

        var date = Find(DateNames);
        var store = Find(StoreNames);
        var amount = Find(AmountNames);
        return date < 0 || store < 0 || amount < 0 ? null : new ColumnMap(date, store, amount);
    }
}
