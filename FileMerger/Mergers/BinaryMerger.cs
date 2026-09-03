using FileMerger.Core;

namespace FileMerger.Mergers;

/// <summary>
/// 中身を解釈せず、そのまま先頭から順につなぐ。
/// 分割されたファイル（.001 / .002 …）を元に戻すときなどに使う。
/// </summary>
public sealed class BinaryMerger : IFileMerger
{
    public string Name => "そのまま連結（バイナリ）";
    public string OutputExtension => ".bin";
    public string OutputFilter => "すべてのファイル (*.*)|*.*";

    public IReadOnlyList<string> SupportedExtensions { get; } = Array.Empty<string>();

    public MergerFeatures Features => MergerFeatures.None;

    /// <summary>最後の受け皿なので、ファイルさえあれば受ける。</summary>
    public bool CanMerge(IReadOnlyList<string> files) => files.Count > 0;

    public void Merge(
        IReadOnlyList<string> files,
        string outputPath,
        MergeOptions options,
        IProgress<MergeProgress>? progress,
        CancellationToken token)
    {
        using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20);
        var buffer = new byte[1 << 20];

        for (int i = 0; i < files.Count; i++)
        {
            token.ThrowIfCancellationRequested();
            var file = files[i];
            progress?.Report(new MergeProgress(i, files.Count, $"連結中: {Path.GetFileName(file)}"));

            using var input = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20);
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                token.ThrowIfCancellationRequested();
                output.Write(buffer, 0, read);
            }

            progress?.Report(new MergeProgress(i + 1, files.Count, $"完了: {Path.GetFileName(file)}"));
        }
    }
}
