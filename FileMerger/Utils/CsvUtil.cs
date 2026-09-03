using System.Text;

namespace FileMerger.Utils;

/// <summary>
/// 引用符で囲まれた改行やカンマを壊さない、最小限の CSV / TSV 処理。
/// </summary>
public static class CsvUtil
{
    public static char DelimiterFor(string path) =>
        Path.GetExtension(path).Equals(".tsv", StringComparison.OrdinalIgnoreCase) ? '\t' : ',';

    public static List<List<string>> Parse(string text, char delimiter)
    {
        var records = new List<List<string>>();
        var record = new List<string>();
        var field = new StringBuilder();
        bool inQuotes = false;
        bool fieldStarted = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

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
                continue;
            }

            if (c == '"' && field.Length == 0)
            {
                inQuotes = true;
                fieldStarted = true;
            }
            else if (c == delimiter)
            {
                record.Add(field.ToString());
                field.Clear();
                fieldStarted = false;
            }
            else if (c == '\r' || c == '\n')
            {
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;

                if (fieldStarted || field.Length > 0 || record.Count > 0)
                {
                    record.Add(field.ToString());
                    records.Add(record);
                    record = new List<string>();
                    field.Clear();
                    fieldStarted = false;
                }
            }
            else
            {
                field.Append(c);
                fieldStarted = true;
            }
        }

        if (fieldStarted || field.Length > 0 || record.Count > 0)
        {
            record.Add(field.ToString());
            records.Add(record);
        }

        return records;
    }

    public static string Escape(string value, char delimiter)
    {
        bool needsQuote = value.Contains(delimiter)
                          || value.Contains('"')
                          || value.Contains('\n')
                          || value.Contains('\r');

        return needsQuote ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;
    }

    public static string FormatRecord(IEnumerable<string> fields, char delimiter) =>
        string.Join(delimiter.ToString(), fields.Select(f => Escape(f, delimiter)));

    /// <summary>2 つの見出し行が（トリム・大小文字無視で）同じかどうか。</summary>
    public static bool HeadersMatch(IReadOnlyList<string> a, IReadOnlyList<string> b)
    {
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
        {
            if (!a[i].Trim().Equals(b[i].Trim(), StringComparison.OrdinalIgnoreCase))
                return false;
        }
        return true;
    }
}
