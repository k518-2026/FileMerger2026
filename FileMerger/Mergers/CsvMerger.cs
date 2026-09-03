using FileMerger.Core;
using FileMerger.Utils;

namespace FileMerger.Mergers;

/// <summary>CSV / TSV を 1 枚の表につなぐ。見出し行の重複を落とせる。</summary>
public sealed class CsvMerger : IFileMerger
{
    public string Name => "CSV / TSV を 1 つの表に結合";
    public string OutputExtension => ".csv";
    public string OutputFilter => "CSV ファイル (*.csv)|*.csv|タブ区切り (*.tsv)|*.tsv|すべてのファイル (*.*)|*.*";

    public IReadOnlyList<string> SupportedExtensions { get; } = new[] { ".csv", ".tsv" };

    public MergerFeatures Features =>
        MergerFeatures.Encoding | MergerFeatures.SkipHeaderRow | MergerFeatures.FileNameHeader;

    public bool CanMerge(IReadOnlyList<string> files) =>
        files.All(f => SupportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));

    public void Merge(
        IReadOnlyList<string> files,
        string outputPath,
        MergeOptions options,
        IProgress<MergeProgress>? progress,
        CancellationToken token)
    {
        char outDelimiter = CsvUtil.DelimiterFor(outputPath);

        using var writer = new StreamWriter(outputPath, false, options.OutputEncoding);

        List<string>? firstHeader = null;
        bool wroteAnything = false;

        for (int i = 0; i < files.Count; i++)
        {
            token.ThrowIfCancellationRequested();
            var file = files[i];
            progress?.Report(new MergeProgress(i, files.Count, $"読み込み中: {Path.GetFileName(file)}"));

            var text = EncodingUtil.ReadAllText(file, options.InputEncoding);
            var records = CsvUtil.Parse(text, CsvUtil.DelimiterFor(file));
            if (records.Count == 0) continue;

            int startRow = 0;

            if (options.SkipRepeatedHeaderRow)
            {
                if (firstHeader is null)
                {
                    firstHeader = records[0];
                }
                else if (CsvUtil.HeadersMatch(firstHeader, records[0]))
                {
                    // 同じ見出しなので 2 回目以降は落とす
                    startRow = 1;
                }
            }

            // 出所を残したい場合は最終列にファイル名を足す
            bool addSource = options.InsertFileNameHeader;
            string sourceName = Path.GetFileName(file);

            if (addSource && !wroteAnything && records.Count > 0)
            {
                // 見出し行にも列名を足す
                var header = new List<string>(records[0]) { "元ファイル" };
                writer.WriteLine(CsvUtil.FormatRecord(header, outDelimiter));
                startRow = Math.Max(startRow, 1);
                wroteAnything = true;
            }

            for (int r = startRow; r < records.Count; r++)
            {
                token.ThrowIfCancellationRequested();
                var row = records[r];
                if (addSource)
                {
                    var copy = new List<string>(row) { sourceName };
                    writer.WriteLine(CsvUtil.FormatRecord(copy, outDelimiter));
                }
                else
                {
                    writer.WriteLine(CsvUtil.FormatRecord(row, outDelimiter));
                }
                wroteAnything = true;
            }

            progress?.Report(new MergeProgress(i + 1, files.Count, $"完了: {Path.GetFileName(file)}（{records.Count} 行）"));
        }
    }
}
