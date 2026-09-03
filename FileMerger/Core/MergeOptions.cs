using System.Text;

namespace FileMerger.Core;

public enum ExcelMergeMode
{
    /// <summary>入力ブックのシートを書式ごとそのまま追加していく</summary>
    SeparateSheets,

    /// <summary>1 枚のシートに縦連結。列は左からの位置どおりに合わせる。</summary>
    SingleSheetByPosition,

    /// <summary>1 枚のシートに縦連結。1 行目の見出し名を突き合わせて列をそろえる。</summary>
    SingleSheetByHeader,

    /// <summary>1 枚のシートに横並び。ファイルごとのブロックを右へ並べる。</summary>
    SingleSheetHorizontal
}

public enum ImagePaperMode
{
    /// <summary>A4 に収まるよう拡大縮小する</summary>
    FitA4,

    /// <summary>画像と同じ寸法のページを作る</summary>
    OriginalSize
}

public sealed class MergeOptions
{
    /// <summary>入力の文字コード。null なら自動判定。</summary>
    public Encoding? InputEncoding { get; set; }

    /// <summary>出力の文字コード。</summary>
    public Encoding OutputEncoding { get; set; } = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

    /// <summary>各ファイルの先頭にファイル名の見出しを入れる（Excel では「元ファイル」列／ブロック見出し）。</summary>
    public bool InsertFileNameHeader { get; set; } = true;

    /// <summary>2 ファイル目以降の見出し行（1 行目）を読み飛ばす。</summary>
    public bool SkipRepeatedHeaderRow { get; set; } = true;

    /// <summary>ファイルの区切りで改ページ・空行・空列を入れる。</summary>
    public bool SplitAtFileBoundary { get; set; } = true;

    public ExcelMergeMode ExcelMode { get; set; } = ExcelMergeMode.SeparateSheets;

    public ImagePaperMode ImagePaper { get; set; } = ImagePaperMode.FitA4;
}
