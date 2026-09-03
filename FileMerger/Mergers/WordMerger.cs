using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FileMerger.Core;

namespace FileMerger.Mergers;

/// <summary>
/// Word 文書を 1 本にまとめる。1 つ目を土台にして、2 つ目以降を AltChunk として差し込む。
/// 書式・見出し・図はそのまま残るが、実際の展開は Word が文書を開いたときに行われる。
/// </summary>
public sealed class WordMerger : IFileMerger
{
    public string Name => "Word 文書を 1 つに結合";
    public string OutputExtension => ".docx";
    public string OutputFilter => "Word 文書 (*.docx)|*.docx";

    public IReadOnlyList<string> SupportedExtensions { get; } = new[] { ".docx" };

    public MergerFeatures Features => MergerFeatures.SplitAtBoundary;

    public bool CanMerge(IReadOnlyList<string> files) =>
        files.All(f => Path.GetExtension(f).Equals(".docx", StringComparison.OrdinalIgnoreCase));

    public void Merge(
        IReadOnlyList<string> files,
        string outputPath,
        MergeOptions options,
        IProgress<MergeProgress>? progress,
        CancellationToken token)
    {
        if (files.Count == 0) return;

        progress?.Report(new MergeProgress(0, files.Count, $"土台にする文書: {Path.GetFileName(files[0])}"));

        // 1 つ目をコピーして土台にする
        File.Copy(files[0], outputPath, overwrite: true);
        File.SetAttributes(outputPath, FileAttributes.Normal);

        progress?.Report(new MergeProgress(1, files.Count, $"完了: {Path.GetFileName(files[0])}"));

        if (files.Count == 1) return;

        using var doc = WordprocessingDocument.Open(outputPath, isEditable: true);
        var main = doc.MainDocumentPart
                   ?? throw new InvalidOperationException("Word 文書の本体が読み取れませんでした。");
        var body = main.Document.Body
                   ?? throw new InvalidOperationException("Word 文書の本文が読み取れませんでした。");

        for (int i = 1; i < files.Count; i++)
        {
            token.ThrowIfCancellationRequested();
            var file = files[i];
            progress?.Report(new MergeProgress(i, files.Count, $"差し込み中: {Path.GetFileName(file)}"));

            var id = $"AltChunkId{i}";
            var chunkPart = main.AddAlternativeFormatImportPart(
                AlternativeFormatImportPartType.WordprocessingML, id);

            using (var stream = File.Open(file, FileMode.Open, FileAccess.Read))
            {
                chunkPart.FeedData(stream);
            }

            if (options.SplitAtFileBoundary)
            {
                body.AppendChild(new Paragraph(new Run(new Break { Type = BreakValues.Page })));
            }

            body.AppendChild(new AltChunk { Id = id });

            progress?.Report(new MergeProgress(i + 1, files.Count, $"完了: {Path.GetFileName(file)}"));
        }

        main.Document.Save();
    }
}
