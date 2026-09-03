namespace FileMerger.Utils;

public static class PathUtil
{
    /// <summary>入力の 1 つ目と同じフォルダーに「統合結果」の候補パスを作る。既存なら連番を付ける。</summary>
    public static string SuggestOutputPath(string firstInput, string extension)
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(firstInput)) ?? Environment.CurrentDirectory;
        var baseName = "統合結果";
        var candidate = Path.Combine(dir, baseName + extension);

        int n = 2;
        while (File.Exists(candidate))
        {
            candidate = Path.Combine(dir, $"{baseName}_{n}{extension}");
            n++;
        }
        return candidate;
    }

    public static string FormatSize(long bytes)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        double v = bytes;
        int i = 0;
        while (v >= 1024 && i < units.Length - 1)
        {
            v /= 1024;
            i++;
        }
        return i == 0 ? $"{bytes} B" : $"{v:0.#} {units[i]}";
    }

    /// <summary>Excel のシート名として使える形に整える（31 文字・禁止文字・重複を処理）。</summary>
    public static string MakeSheetName(string desired, ICollection<string> used)
    {
        var invalid = new[] { ':', '\\', '/', '?', '*', '[', ']' };
        var name = new string(desired.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim();
        if (name.Length == 0) name = "Sheet";
        if (name.Length > 31) name = name[..31];

        var result = name;
        int n = 2;
        while (used.Contains(result))
        {
            var suffix = $"_{n}";
            var head = name.Length + suffix.Length > 31 ? name[..(31 - suffix.Length)] : name;
            result = head + suffix;
            n++;
        }

        used.Add(result);
        return result;
    }
}
