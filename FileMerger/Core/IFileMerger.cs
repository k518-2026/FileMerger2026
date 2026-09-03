namespace FileMerger.Core;

/// <summary>進捗報告。Current / Total は処理済みファイル数。</summary>
public readonly record struct MergeProgress(int Current, int Total, string Message);

/// <summary>どの機能でどのオプションが効くかを UI に伝えるためのフラグ。</summary>
[Flags]
public enum MergerFeatures
{
    None = 0,
    Encoding = 1 << 0,
    FileNameHeader = 1 << 1,
    SkipHeaderRow = 1 << 2,
    SplitAtBoundary = 1 << 3,
    ExcelMode = 1 << 4,
    ImagePaper = 1 << 5
}

public interface IFileMerger
{
    /// <summary>コンボボックスに出す名前。</summary>
    string Name { get; }

    /// <summary>出力ファイルの既定の拡張子（".pdf" など）。</summary>
    string OutputExtension { get; }

    /// <summary>SaveFileDialog 用のフィルター文字列。</summary>
    string OutputFilter { get; }

    /// <summary>受け付ける入力拡張子（すべて小文字・ドット付き）。</summary>
    IReadOnlyList<string> SupportedExtensions { get; }

    /// <summary>この機能で有効なオプション。</summary>
    MergerFeatures Features { get; }

    /// <summary>渡されたファイル一式をこの機能で扱えるか。</summary>
    bool CanMerge(IReadOnlyList<string> files);

    /// <summary>統合を実行する。</summary>
    void Merge(
        IReadOnlyList<string> files,
        string outputPath,
        MergeOptions options,
        IProgress<MergeProgress>? progress,
        CancellationToken token);
}
