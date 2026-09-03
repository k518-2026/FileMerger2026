using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using FileMerger.Core;

namespace FileMerger.Mergers;

/// <summary>
/// PowerPoint のプレゼンテーションを 1 本にまとめる。
/// 1 つ目を土台にして、2 つ目以降のスライドをスライド マスター・レイアウトごと取り込む。
/// </summary>
public sealed class PowerPointMerger : IFileMerger
{
    /// <summary>スライド マスターの ID はこの値から上を使う慣例。</summary>
    private const uint MasterIdBase = 2147483648u;

    public string Name => "PowerPoint を 1 つに結合";
    public string OutputExtension => ".pptx";
    public string OutputFilter => "PowerPoint プレゼンテーション (*.pptx)|*.pptx";

    public IReadOnlyList<string> SupportedExtensions { get; } = new[] { ".pptx", ".pptm" };

    public MergerFeatures Features => MergerFeatures.None;

    public bool CanMerge(IReadOnlyList<string> files) =>
        files.All(f => SupportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));

    public void Merge(
        IReadOnlyList<string> files,
        string outputPath,
        MergeOptions options,
        IProgress<MergeProgress>? progress,
        CancellationToken token)
    {
        if (files.Count == 0) return;

        progress?.Report(new MergeProgress(0, files.Count, $"土台にするファイル: {Path.GetFileName(files[0])}"));

        // 1 つ目をコピーして土台にする（デザインと画面サイズはこれが基準になる）
        File.Copy(files[0], outputPath, overwrite: true);
        File.SetAttributes(outputPath, FileAttributes.Normal);

        using var dest = PresentationDocument.Open(outputPath, isEditable: true);

        // .pptm を土台にした場合はマクロなし形式へそろえる
        if (dest.DocumentType != DocumentFormat.OpenXml.PresentationDocumentType.Presentation)
            dest.ChangeDocumentType(DocumentFormat.OpenXml.PresentationDocumentType.Presentation);

        var destPart = dest.PresentationPart
                       ?? throw new InvalidOperationException("プレゼンテーションの本体が読み取れませんでした。");

        var slideIdList = destPart.Presentation.SlideIdList
                          ?? throw new InvalidOperationException(
                              $"{Path.GetFileName(files[0])} にスライドがありません。スライドのあるファイルを先頭にしてください。");

        uint nextSlideId = slideIdList.Elements<SlideId>()
            .Select(s => s.Id?.Value ?? 0u)
            .DefaultIfEmpty(255u)
            .Max();

        progress?.Report(new MergeProgress(1, files.Count,
            $"完了: {Path.GetFileName(files[0])}（{slideIdList.Elements<SlideId>().Count()} 枚）"));

        for (int i = 1; i < files.Count; i++)
        {
            token.ThrowIfCancellationRequested();
            var file = files[i];
            progress?.Report(new MergeProgress(i, files.Count, $"取り込み中: {Path.GetFileName(file)}"));

            using var src = PresentationDocument.Open(file, isEditable: false);
            var srcPart = src.PresentationPart;

            if (srcPart?.Presentation.SlideIdList is null)
            {
                progress?.Report(new MergeProgress(i + 1, files.Count,
                    $"スキップ: {Path.GetFileName(file)}（スライドがありません）"));
                continue;
            }

            int copied = 0;

            foreach (var srcSlideId in srcPart.Presentation.SlideIdList.Elements<SlideId>())
            {
                token.ThrowIfCancellationRequested();

                var relId = srcSlideId.RelationshipId?.Value;
                if (relId is null) continue;
                if (srcPart.GetPartById(relId) is not SlidePart srcSlide) continue;

                // AddPart はスライドが参照するレイアウト・画像・埋め込みも一緒に写す
                var newSlide = destPart.AddPart(srcSlide);

                slideIdList.Append(new SlideId
                {
                    Id = ++nextSlideId,
                    RelationshipId = destPart.GetIdOfPart(newSlide)
                });

                copied++;
            }

            progress?.Report(new MergeProgress(i + 1, files.Count,
                $"完了: {Path.GetFileName(file)}（{copied} 枚）"));
        }

        RegisterOrphanMasters(destPart);

        destPart.Presentation.Save();
    }

    /// <summary>
    /// 取り込みで増えたスライド マスターを一覧に登録する。
    /// 登録が漏れていると PowerPoint が修復を促すことがある。
    /// </summary>
    private static void RegisterOrphanMasters(PresentationPart destPart)
    {
        var masterIdList = destPart.Presentation.SlideMasterIdList;
        if (masterIdList is null) return;

        var known = masterIdList.Elements<SlideMasterId>()
            .Select(m => m.RelationshipId?.Value)
            .Where(v => v is not null)
            .ToHashSet(StringComparer.Ordinal);

        uint nextId = masterIdList.Elements<SlideMasterId>()
            .Select(m => m.Id?.Value ?? 0u)
            .DefaultIfEmpty(MasterIdBase)
            .Max();

        if (nextId < MasterIdBase) nextId = MasterIdBase;

        foreach (var masterPart in destPart.SlideMasterParts)
        {
            var relId = destPart.GetIdOfPart(masterPart);
            if (known.Contains(relId)) continue;

            masterIdList.Append(new SlideMasterId
            {
                Id = ++nextId,
                RelationshipId = relId
            });
        }
    }
}
