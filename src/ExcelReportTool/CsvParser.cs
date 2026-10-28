using System.Text;

namespace ExcelReportTool;

/// <summary>Small RFC 4180 style parser: quoted fields, escaped quotes, comma or semicolon delimiter.</summary>
public static class CsvParser
{
    public static List<string[]> Parse(string text)
    {
        var rows = new List<string[]>();
        var fields = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;
        var delimiter = DetectDelimiter(text);

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    field.Append(c);
                }
            }
            else if (c == '"')
            {
                inQuotes = true;
            }
            else if (c == delimiter)
            {
                fields.Add(field.ToString());
                field.Clear();
            }
            else if (c == '\n' || c == '\r')
            {
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                {
                    i++;
                }

                fields.Add(field.ToString());
                field.Clear();
                rows.Add(fields.ToArray());
                fields.Clear();
            }
            else
            {
                field.Append(c);
            }
        }

        if (field.Length > 0 || fields.Count > 0)
        {
            fields.Add(field.ToString());
            rows.Add(fields.ToArray());
        }

        rows.RemoveAll(r => r.All(string.IsNullOrWhiteSpace));
        return rows;
    }

    private static char DetectDelimiter(string text)
    {
        var end = text.IndexOfAny(new[] { '\r', '\n' });
        var header = end < 0 ? text : text[..end];
        return header.Count(ch => ch == ';') > header.Count(ch => ch == ',') ? ';' : ',';
    }
}
