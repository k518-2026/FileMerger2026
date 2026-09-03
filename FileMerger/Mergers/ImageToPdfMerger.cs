using System.Drawing.Imaging;
using FileMerger.Core;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace FileMerger.Mergers;

/// <summary>画像を 1 ページ 1 枚で並べた PDF にする。複数ページ TIFF も展開する。</summary>
public sealed class ImageToPdfMerger : IFileMerger
{
    // A4 (ポイント単位)
    private const double A4WidthPt = 595.28;
    private const double A4HeightPt = 841.89;
    private const double MarginPt = 20.0;

    public string Name => "画像を 1 つの PDF にまとめる";
    public string OutputExtension => ".pdf";
    public string OutputFilter => "PDF ファイル (*.pdf)|*.pdf";

    public IReadOnlyList<string> SupportedExtensions { get; } = new[]
    {
        ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tif", ".tiff", ".webp"
    };

    public MergerFeatures Features => MergerFeatures.ImagePaper;

    public bool CanMerge(IReadOnlyList<string> files) =>
        files.All(f => SupportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));

    public void Merge(
        IReadOnlyList<string> files,
        string outputPath,
        MergeOptions options,
        IProgress<MergeProgress>? progress,
        CancellationToken token)
    {
        using var doc = new PdfDocument();
        doc.Info.Title = Path.GetFileNameWithoutExtension(outputPath);
        doc.Info.Creator = "ファイル統合ツール";

        var tempFiles = new List<string>();

        try
        {
            for (int i = 0; i < files.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                var file = files[i];
                progress?.Report(new MergeProgress(i, files.Count, $"読み込み中: {Path.GetFileName(file)}"));

                foreach (var pagePath in ExpandFrames(file, tempFiles))
                {
                    token.ThrowIfCancellationRequested();
                    AddImagePage(doc, pagePath, options.ImagePaper);
                }

                progress?.Report(new MergeProgress(i + 1, files.Count, $"完了: {Path.GetFileName(file)}"));
            }

            doc.Save(outputPath);
        }
        finally
        {
            foreach (var t in tempFiles)
            {
                try { File.Delete(t); } catch { /* 一時ファイルの削除失敗は無視 */ }
            }
        }
    }

    /// <summary>
    /// PdfSharp がそのまま読めない形式や複数ページ画像を、GDI+ で PNG に展開する。
    /// JPEG / PNG はそのまま渡す。
    /// </summary>
    private static IEnumerable<string> ExpandFrames(string path, List<string> tempFiles)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        bool passthrough = ext is ".jpg" or ".jpeg" or ".png";

        using var image = System.Drawing.Image.FromFile(path);

        int frameCount = 1;
        try
        {
            frameCount = image.GetFrameCount(FrameDimension.Page);
        }
        catch (ArgumentException)
        {
            frameCount = 1;
        }

        if (passthrough && frameCount <= 1)
        {
            yield return path;
            yield break;
        }

        for (int f = 0; f < frameCount; f++)
        {
            if (frameCount > 1) image.SelectActiveFrame(FrameDimension.Page, f);

            var temp = Path.Combine(Path.GetTempPath(), $"fm_{Guid.NewGuid():N}.png");
            image.Save(temp, ImageFormat.Png);
            tempFiles.Add(temp);
            yield return temp;
        }
    }

    private static void AddImagePage(PdfDocument doc, string imagePath, ImagePaperMode mode)
    {
        using var ximg = XImage.FromFile(imagePath);

        double imgW = ximg.PointWidth;
        double imgH = ximg.PointHeight;
        if (imgW <= 0 || imgH <= 0) return;

        var page = doc.AddPage();

        double pageW, pageH;

        if (mode == ImagePaperMode.OriginalSize)
        {
            pageW = imgW;
            pageH = imgH;
        }
        else
        {
            bool landscape = imgW > imgH;
            pageW = landscape ? A4HeightPt : A4WidthPt;
            pageH = landscape ? A4WidthPt : A4HeightPt;
        }

        page.Width = XUnit.FromPoint(pageW);
        page.Height = XUnit.FromPoint(pageH);

        using var gfx = XGraphics.FromPdfPage(page);

        if (mode == ImagePaperMode.OriginalSize)
        {
            gfx.DrawImage(ximg, 0, 0, pageW, pageH);
            return;
        }

        double availW = pageW - MarginPt * 2;
        double availH = pageH - MarginPt * 2;
        double scale = Math.Min(availW / imgW, availH / imgH);

        double drawW = imgW * scale;
        double drawH = imgH * scale;
        double x = (pageW - drawW) / 2;
        double y = (pageH - drawH) / 2;

        gfx.DrawImage(ximg, x, y, drawW, drawH);
    }
}
