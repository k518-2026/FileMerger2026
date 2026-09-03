using ClosedXML.Excel;
using FileMerger.Core;
using FileMerger.Utils;

namespace FileMerger.Mergers;

/// <summary>
/// Excel ブック（と CSV）を 1 冊にまとめる。
/// シート別のほか、1 枚のワークシートへ縦・横に連結する方法を選べる。
/// </summary>
public sealed class ExcelMerger : IFileMerger
{
    private const string SourceColumnName = "元ファイル";
    private const string OutputSheetName = "結合結果";

    public string Name => "Excel ブックを結合";
    public string OutputExtension => ".xlsx";
    public string OutputFilter => "Excel ブック (*.xlsx)|*.xlsx";

    public IReadOnlyList<string> SupportedExtensions { get; } = new[] { ".xlsx", ".xlsm", ".csv", ".tsv" };

    public MergerFeatures Features =>
        MergerFeatures.ExcelMode
        | MergerFeatures.SkipHeaderRow
        | MergerFeatures.FileNameHeader
        | MergerFeatures.SplitAtBoundary
        | MergerFeatures.Encoding;

    public bool CanMerge(IReadOnlyList<string> files)
    {
        var exts = files.Select(f => Path.GetExtension(f).ToLowerInvariant()).ToList();
        if (!exts.All(e => SupportedExtensions.Contains(e))) return false;

        // CSV だけのときは CsvMerger に任せたいので、ここでは受けない
        return exts.Any(e => e is ".xlsx" or ".xlsm");
    }

    public void Merge(
        IReadOnlyList<string> files,
        string outputPath,
        MergeOptions options,
        IProgress<MergeProgress>? progress,
        CancellationToken token)
    {
        using var output = new XLWorkbook();

        if (options.ExcelMode == ExcelMergeMode.SeparateSheets)
        {
            MergeAsSheets(files, output, options, progress, token);
        }
        else
        {
            var blocks = LoadBlocks(files, options, progress, token);
            var sheet = output.Worksheets.Add(OutputSheetName);

            switch (options.ExcelMode)
            {
                case ExcelMergeMode.SingleSheetByHeader:
                    WriteVerticalByHeader(sheet, blocks, options, token);
                    break;
                case ExcelMergeMode.SingleSheetHorizontal:
                    WriteHorizontal(sheet, blocks, options, token);
                    break;
                default:
                    WriteVerticalByPosition(sheet, blocks, options, token);
                    break;
            }

            sheet.Columns().AdjustToContents(1, 200);
        }

        if (!output.Worksheets.Any()) output.Worksheets.Add(OutputSheetName);

        output.SaveAs(outputPath);
    }

    // ---------- 読み込み ----------

    /// <summary>1 つのワークシート、または 1 つの CSV 分のデータ。</summary>
    private sealed class Block
    {
        public string SourceFile = "";
        public string Label = "";
        public List<List<XLCellValue>> Rows = new();

        public int ColumnCount => Rows.Count == 0 ? 0 : Rows.Max(r => r.Count);
    }

    private static List<Block> LoadBlocks(
        IReadOnlyList<string> files,
        MergeOptions options,
        IProgress<MergeProgress>? progress,
        CancellationToken token)
    {
        var blocks = new List<Block>();

        for (int i = 0; i < files.Count; i++)
        {
            token.ThrowIfCancellationRequested();
            var file = files[i];
            var stem = Path.GetFileNameWithoutExtension(file);
            progress?.Report(new MergeProgress(i, files.Count, $"読み込み中: {Path.GetFileName(file)}"));

            var ext = Path.GetExtension(file).ToLowerInvariant();

            if (ext is ".csv" or ".tsv")
            {
                blocks.Add(LoadCsvBlock(file, stem, options));
            }
            else
            {
                using var src = new XLWorkbook(file);
                bool single = src.Worksheets.Count == 1;

                foreach (var ws in src.Worksheets)
                {
                    token.ThrowIfCancellationRequested();
                    var block = LoadWorksheetBlock(ws, token);
                    block.SourceFile = Path.GetFileName(file);
                    block.Label = single ? stem : $"{stem} / {ws.Name}";
                    if (block.Rows.Count > 0) blocks.Add(block);
                }
            }

            progress?.Report(new MergeProgress(i + 1, files.Count, $"完了: {Path.GetFileName(file)}"));
        }

        return blocks;
    }

    private static Block LoadWorksheetBlock(IXLWorksheet ws, CancellationToken token)
    {
        var block = new Block();
        var used = ws.RangeUsed();
        if (used is null) return block;

        int firstRow = used.RangeAddress.FirstAddress.RowNumber;
        int lastRow = used.RangeAddress.LastAddress.RowNumber;
        int firstCol = used.RangeAddress.FirstAddress.ColumnNumber;
        int lastCol = used.RangeAddress.LastAddress.ColumnNumber;

        for (int r = firstRow; r <= lastRow; r++)
        {
            token.ThrowIfCancellationRequested();
            var row = new List<XLCellValue>(lastCol - firstCol + 1);
            for (int c = firstCol; c <= lastCol; c++)
                row.Add(ws.Cell(r, c).Value);
            block.Rows.Add(row);
        }

        return block;
    }

    private static Block LoadCsvBlock(string file, string stem, MergeOptions options)
    {
        var text = EncodingUtil.ReadAllText(file, options.InputEncoding);
        var records = CsvUtil.Parse(text, CsvUtil.DelimiterFor(file));

        var block = new Block
        {
            SourceFile = Path.GetFileName(file),
            Label = stem
        };

        foreach (var rec in records)
        {
            var row = new List<XLCellValue>(rec.Count);
            foreach (var field in rec)
            {
                if (field.Length == 0) row.Add(Blank.Value);
                else if (double.TryParse(field, out var num)) row.Add(num);
                else row.Add(field);
            }
            block.Rows.Add(row);
        }

        return block;
    }

    // ---------- 1 枚のシートへ縦連結（列の位置どおり） ----------

    private static void WriteVerticalByPosition(
        IXLWorksheet sheet, List<Block> blocks, MergeOptions options, CancellationToken token)
    {
        if (blocks.Count == 0) return;

        bool hasHeader = options.SkipRepeatedHeaderRow;
        bool addSource = options.InsertFileNameHeader;
        int maxCols = blocks.Max(b => b.ColumnCount);
        int sourceCol = maxCols + 1;

        int outRow = 1;
        bool first = true;

        foreach (var block in blocks)
        {
            token.ThrowIfCancellationRequested();
            if (block.Rows.Count == 0) continue;

            if (!first && options.SplitAtFileBoundary) outRow++;

            int start = 0;

            if (first)
            {
                if (hasHeader)
                {
                    WriteRow(sheet, outRow, 1, block.Rows[0], token);
                    if (addSource) sheet.Cell(outRow, sourceCol).Value = SourceColumnName;
                    outRow++;
                    start = 1;
                }
            }
            else if (hasHeader)
            {
                start = 1;
            }

            for (int r = start; r < block.Rows.Count; r++)
            {
                token.ThrowIfCancellationRequested();
                WriteRow(sheet, outRow, 1, block.Rows[r], token);
                if (addSource) sheet.Cell(outRow, sourceCol).Value = block.SourceFile;
                outRow++;
            }

            first = false;
        }

        if (hasHeader) DecorateHeaderRow(sheet);
    }

    // ---------- 1 枚のシートへ縦連結（見出し名で列をそろえる） ----------

    private static void WriteVerticalByHeader(
        IXLWorksheet sheet, List<Block> blocks, MergeOptions options, CancellationToken token)
    {
        if (blocks.Count == 0) return;

        bool addSource = options.InsertFileNameHeader;

        // 見出し名の一覧を、最初に出てきた順で作る
        var columnOrder = new List<string>();
        var columnIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var maps = new List<int[]>();

        foreach (var block in blocks)
        {
            token.ThrowIfCancellationRequested();

            var headerRow = block.Rows.Count > 0 ? block.Rows[0] : new List<XLCellValue>();
            var map = new int[headerRow.Count];

            for (int c = 0; c < headerRow.Count; c++)
            {
                var name = HeaderName(headerRow[c], c);
                if (!columnIndex.TryGetValue(name, out var idx))
                {
                    idx = columnOrder.Count;
                    columnOrder.Add(name);
                    columnIndex[name] = idx;
                }
                map[c] = idx;
            }

            maps.Add(map);
        }

        // 見出し行
        for (int c = 0; c < columnOrder.Count; c++)
            sheet.Cell(1, c + 1).Value = columnOrder[c];

        int sourceCol = columnOrder.Count + 1;
        if (addSource) sheet.Cell(1, sourceCol).Value = SourceColumnName;

        int outRow = 2;
        bool first = true;

        for (int b = 0; b < blocks.Count; b++)
        {
            token.ThrowIfCancellationRequested();
            var block = blocks[b];
            var map = maps[b];

            if (!first && options.SplitAtFileBoundary) outRow++;

            for (int r = 1; r < block.Rows.Count; r++)
            {
                token.ThrowIfCancellationRequested();
                var row = block.Rows[r];

                for (int c = 0; c < row.Count && c < map.Length; c++)
                {
                    if (row[c].IsBlank) continue;
                    sheet.Cell(outRow, map[c] + 1).Value = row[c];
                }

                if (addSource) sheet.Cell(outRow, sourceCol).Value = block.SourceFile;
                outRow++;
            }

            first = false;
        }

        DecorateHeaderRow(sheet);
    }

    // ---------- 1 枚のシートへ横並び ----------

    private static void WriteHorizontal(
        IXLWorksheet sheet, List<Block> blocks, MergeOptions options, CancellationToken token)
    {
        bool addLabel = options.InsertFileNameHeader;
        int outCol = 1;

        foreach (var block in blocks)
        {
            token.ThrowIfCancellationRequested();
            if (block.Rows.Count == 0) continue;

            int outRow = 1;

            if (addLabel)
            {
                var cell = sheet.Cell(outRow, outCol);
                cell.Value = block.Label;
                cell.Style.Font.Bold = true;
                outRow++;
            }

            for (int r = 0; r < block.Rows.Count; r++)
            {
                token.ThrowIfCancellationRequested();
                WriteRow(sheet, outRow, outCol, block.Rows[r], token);
                outRow++;
            }

            outCol += block.ColumnCount;
            if (options.SplitAtFileBoundary) outCol++;
        }
    }

    // ---------- 共通 ----------

    private static void WriteRow(IXLWorksheet sheet, int row, int startCol, List<XLCellValue> values, CancellationToken token)
    {
        for (int c = 0; c < values.Count; c++)
        {
            token.ThrowIfCancellationRequested();
            if (values[c].IsBlank) continue;
            sheet.Cell(row, startCol + c).Value = values[c];
        }
    }

    private static string HeaderName(XLCellValue value, int columnIndex)
    {
        var name = value.IsBlank ? "" : value.ToString()?.Trim() ?? "";
        return name.Length > 0 ? name : $"列{columnIndex + 1}";
    }

    private static void DecorateHeaderRow(IXLWorksheet sheet)
    {
        sheet.Row(1).Style.Font.Bold = true;
        sheet.SheetView.FreezeRows(1);
    }

    // ---------- シート別（書式を保ったまま） ----------

    private static void MergeAsSheets(
        IReadOnlyList<string> files,
        XLWorkbook output,
        MergeOptions options,
        IProgress<MergeProgress>? progress,
        CancellationToken token)
    {
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < files.Count; i++)
        {
            token.ThrowIfCancellationRequested();
            var file = files[i];
            var stem = Path.GetFileNameWithoutExtension(file);
            progress?.Report(new MergeProgress(i, files.Count, $"読み込み中: {Path.GetFileName(file)}"));

            var ext = Path.GetExtension(file).ToLowerInvariant();

            if (ext is ".csv" or ".tsv")
            {
                var sheet = output.Worksheets.Add(PathUtil.MakeSheetName(stem, used));
                var block = LoadCsvBlock(file, stem, options);
                for (int r = 0; r < block.Rows.Count; r++)
                    WriteRow(sheet, r + 1, 1, block.Rows[r], token);
                sheet.Columns().AdjustToContents(1, 200);
            }
            else
            {
                using var src = new XLWorkbook(file);
                foreach (var ws in src.Worksheets)
                {
                    token.ThrowIfCancellationRequested();
                    var desired = src.Worksheets.Count == 1 ? stem : $"{stem}_{ws.Name}";
                    ws.CopyTo(output, PathUtil.MakeSheetName(desired, used));
                }
            }

            progress?.Report(new MergeProgress(i + 1, files.Count, $"完了: {Path.GetFileName(file)}"));
        }
    }
}
