using FileMerger.Mergers;

namespace FileMerger.Core;

public static class MergerRegistry
{
    /// <summary>自動判定はこの順で試す。前にあるものほど優先。</summary>
    public static IReadOnlyList<IFileMerger> All { get; } = new IFileMerger[]
    {
        new PdfMerger(),
        new ImageToPdfMerger(),
        new ExcelMerger(),
        new WordMerger(),
        new PowerPointMerger(),
        new CsvMerger(),
        new TextMerger(),
        new BinaryMerger()
    };

    /// <summary>ファイル一式に合う機能を選ぶ。見つからなければ null。</summary>
    public static IFileMerger? Detect(IReadOnlyList<string> files)
    {
        if (files.Count == 0) return null;
        return All.FirstOrDefault(m => m.CanMerge(files));
    }

    /// <summary>「ファイルを開く」ダイアログ用のフィルター。</summary>
    public static string BuildOpenFilter()
    {
        var known = All
            .SelectMany(m => m.SupportedExtensions)
            .Distinct()
            .OrderBy(e => e, StringComparer.Ordinal)
            .Select(e => "*" + e)
            .ToArray();

        var parts = new List<string>
        {
            $"対応ファイル ({string.Join("; ", known)})|{string.Join(";", known)}"
        };

        foreach (var m in All)
        {
            if (m.SupportedExtensions.Count == 0) continue;
            var pattern = string.Join(";", m.SupportedExtensions.Select(e => "*" + e));
            parts.Add($"{m.Name} ({pattern.Replace(";", "; ")})|{pattern}");
        }

        parts.Add("すべてのファイル (*.*)|*.*");
        return string.Join("|", parts);
    }
}
