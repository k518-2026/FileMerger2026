using System.Text;
using FileMerger.Core;
using FileMerger.Utils;

namespace FileMerger.Mergers;

/// <summary>テキスト系のファイルを 1 本のテキストにつなぐ。</summary>
public sealed class TextMerger : IFileMerger
{
    public string Name => "テキストとして結合";
    public string OutputExtension => ".txt";
    public string OutputFilter => "テキスト ファイル (*.txt)|*.txt|すべてのファイル (*.*)|*.*";

    public IReadOnlyList<string> SupportedExtensions { get; } = new[]
    {
        ".txt", ".text", ".log", ".md", ".markdown", ".csv", ".tsv",
        ".json", ".xml", ".html", ".htm", ".css", ".js", ".ts",
        ".sql", ".ini", ".cfg", ".conf", ".yaml", ".yml", ".srt", ".vtt"
    };

    public MergerFeatures Features =>
        MergerFeatures.Encoding | MergerFeatures.FileNameHeader | MergerFeatures.SplitAtBoundary;

    public bool CanMerge(IReadOnlyList<string> files) =>
        files.All(f => SupportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));

    public void Merge(
        IReadOnlyList<string> files,
        string outputPath,
        MergeOptions options,
        IProgress<MergeProgress>? progress,
        CancellationToken token)
    {
        using var writer = new StreamWriter(outputPath, false, options.OutputEncoding);

        for (int i = 0; i < files.Count; i++)
        {
            token.ThrowIfCancellationRequested();
            var file = files[i];
            progress?.Report(new MergeProgress(i, files.Count, $"読み込み中: {Path.GetFileName(file)}"));

            if (i > 0 && options.SplitAtFileBoundary)
                writer.WriteLine();

            if (options.InsertFileNameHeader)
            {
                writer.WriteLine("==================================================");
                writer.WriteLine($"  {Path.GetFileName(file)}");
                writer.WriteLine("==================================================");
            }

            var text = EncodingUtil.ReadAllText(file, options.InputEncoding);

            // 末尾の改行はこちらで整えるので一度落とす
            writer.Write(text.TrimEnd('\r', '\n'));
            writer.WriteLine();

            progress?.Report(new MergeProgress(i + 1, files.Count, $"完了: {Path.GetFileName(file)}"));
        }
    }
}
