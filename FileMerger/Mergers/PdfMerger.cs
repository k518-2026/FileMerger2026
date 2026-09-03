using FileMerger.Core;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace FileMerger.Mergers;

/// <summary>複数の PDF をページ順に 1 冊にまとめる。</summary>
public sealed class PdfMerger : IFileMerger
{
    public string Name => "PDF を 1 つに結合";
    public string OutputExtension => ".pdf";
    public string OutputFilter => "PDF ファイル (*.pdf)|*.pdf";

    public IReadOnlyList<string> SupportedExtensions { get; } = new[] { ".pdf" };

    public MergerFeatures Features => MergerFeatures.None;

    public bool CanMerge(IReadOnlyList<string> files) =>
        files.All(f => Path.GetExtension(f).Equals(".pdf", StringComparison.OrdinalIgnoreCase));

    public void Merge(
        IReadOnlyList<string> files,
        string outputPath,
        MergeOptions options,
        IProgress<MergeProgress>? progress,
        CancellationToken token)
    {
        using var output = new PdfDocument();
        output.Info.Title = Path.GetFileNameWithoutExtension(outputPath);
        output.Info.Creator = "ファイル統合ツール";

        for (int i = 0; i < files.Count; i++)
        {
            token.ThrowIfCancellationRequested();
            var file = files[i];
            progress?.Report(new MergeProgress(i, files.Count, $"読み込み中: {Path.GetFileName(file)}"));

            using var input = PdfReader.Open(file, PdfDocumentOpenMode.Import);
            for (int p = 0; p < input.PageCount; p++)
            {
                token.ThrowIfCancellationRequested();
                output.AddPage(input.Pages[p]);
            }

            progress?.Report(new MergeProgress(i + 1, files.Count,
                $"完了: {Path.GetFileName(file)}（{input.PageCount} ページ）"));
        }

        output.Save(outputPath);
    }
}
