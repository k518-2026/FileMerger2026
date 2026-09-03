using System.Text;
using FileMerger.Core;
using FileMerger.Utils;

namespace FileMerger;

/// <summary>
/// 扱えるファイルの一覧。内容は MergerRegistry から組み立てるので、
/// 統合方法を足せば自動でここにも並ぶ。
/// </summary>
public partial class SupportedFilesForm : Form
{
    private const string AnyFileLabel = "上記以外のすべてのファイル";

    public SupportedFilesForm()
    {
        InitializeComponent();
        AppIcon.Apply(this);
    }

    private void SupportedFilesForm_Load(object? sender, EventArgs e)
    {
        lvKinds.BeginUpdate();
        lvKinds.Items.Clear();

        foreach (var merger in MergerRegistry.All)
        {
            var item = new ListViewItem(merger.Name);
            item.SubItems.Add(merger.OutputExtension);
            item.SubItems.Add(DescribeExtensions(merger));
            lvKinds.Items.Add(item);
        }

        lvKinds.EndUpdate();
    }

    private static string DescribeExtensions(IFileMerger merger)
    {
        if (merger.SupportedExtensions.Count == 0) return AnyFileLabel;
        return string.Join("  ", merger.SupportedExtensions);
    }

    private void BtnCopy_Click(object? sender, EventArgs e)
    {
        var sb = new StringBuilder();
        sb.AppendLine("統合方法\t出力\t扱えるファイル");

        foreach (var merger in MergerRegistry.All)
            sb.AppendLine($"{merger.Name}\t{merger.OutputExtension}\t{DescribeExtensions(merger)}");

        try
        {
            Clipboard.SetText(sb.ToString());
            btnCopy.Text = "コピーしました";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"クリップボードにコピーできませんでした。\r\n\r\n{ex.Message}",
                Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
