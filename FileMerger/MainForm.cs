using System.Diagnostics;
using System.Text;
using FileMerger.Core;
using FileMerger.Utils;

namespace FileMerger;

public partial class MainForm : Form
{
    private readonly List<string> _files = new();
    private CancellationTokenSource? _cts;
    private bool _running;

    private const string AutoDetectLabel = "自動で判定する";

    public MainForm()
    {
        InitializeComponent();
        AppIcon.Apply(this);
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        cmbType.Items.Add(AutoDetectLabel);
        foreach (var m in MergerRegistry.All) cmbType.Items.Add(m.Name);
        cmbType.SelectedIndex = 0;

        cmbEncIn.Items.AddRange(new object[] { "自動で判定する", "UTF-8", "Shift_JIS", "UTF-16 LE" });
        cmbEncIn.SelectedIndex = 0;

        foreach (var (label, _) in EncodingUtil.OutputChoices) cmbEncOut.Items.Add(label);
        cmbEncOut.SelectedIndex = 0;

        cmbExcelMode.Items.AddRange(new object[]
        {
            "シートごとに分けて入れる（書式そのまま）",
            "1 枚のシートに縦連結（列の位置どおり）",
            "1 枚のシートに縦連結（見出し名で列をそろえる）",
            "1 枚のシートに横並び"
        });
        cmbExcelMode.SelectedIndex = 0;

        cmbPaper.Items.AddRange(new object[] { "A4 に合わせる", "画像と同じ大きさ" });
        cmbPaper.SelectedIndex = 0;

        chkFileNameHeader.Checked = true;
        chkSkipHeaderRow.Checked = true;
        chkSplit.Checked = true;

        Log("ファイルをドラッグ＆ドロップするか、[ファイルを追加] で選んでください。");
        Log("扱えるファイルの一覧は [ヘルプ] → [扱えるファイルの一覧]（F1）で確認できます。");
        UpdateOptionState();
    }

    // ---------- ファイル一覧の操作 ----------

    private void BtnAddFiles_Click(object? sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Multiselect = true,
            Title = "統合するファイルを選ぶ",
            Filter = MergerRegistry.BuildOpenFilter()
        };

        if (dlg.ShowDialog(this) == DialogResult.OK) AddFiles(dlg.FileNames);
    }

    private void BtnAddFolder_Click(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog { Description = "フォルダー内のファイルを追加します" };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        var files = Directory.EnumerateFiles(dlg.SelectedPath)
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        AddFiles(files);
    }

    private void AddFiles(IEnumerable<string> paths)
    {
        int added = 0, skipped = 0;

        foreach (var p in paths)
        {
            if (Directory.Exists(p))
            {
                AddFiles(Directory.EnumerateFiles(p).OrderBy(f => f, StringComparer.OrdinalIgnoreCase));
                continue;
            }

            if (!File.Exists(p)) continue;

            var full = Path.GetFullPath(p);
            if (_files.Contains(full, StringComparer.OrdinalIgnoreCase))
            {
                skipped++;
                continue;
            }

            _files.Add(full);
            added++;
        }

        RefreshList();
        SuggestOutputPath();

        if (added > 0) Log($"{added} 件を追加しました。");
        if (skipped > 0) Log($"{skipped} 件は既に一覧にあるため追加しませんでした。");
    }

    private void BtnRemove_Click(object? sender, EventArgs e) => RemoveSelected();

    private void RemoveSelected()
    {
        var indices = lvFiles.SelectedIndices.Cast<int>().OrderByDescending(i => i).ToList();
        if (indices.Count == 0) return;

        foreach (var i in indices) _files.RemoveAt(i);
        RefreshList();
    }

    private void BtnClear_Click(object? sender, EventArgs e)
    {
        _files.Clear();
        RefreshList();
        txtOutput.Clear();
    }

    private void BtnUp_Click(object? sender, EventArgs e) => Move(-1);

    private void BtnDown_Click(object? sender, EventArgs e) => Move(1);

    private void Move(int delta)
    {
        var indices = lvFiles.SelectedIndices.Cast<int>().ToList();
        if (indices.Count == 0) return;

        var ordered = delta < 0 ? indices.OrderBy(i => i).ToList() : indices.OrderByDescending(i => i).ToList();

        foreach (var i in ordered)
        {
            int target = i + delta;
            if (target < 0 || target >= _files.Count) return;
            (_files[i], _files[target]) = (_files[target], _files[i]);
        }

        RefreshList();
        foreach (var i in indices)
        {
            int target = i + delta;
            if (target >= 0 && target < lvFiles.Items.Count) lvFiles.Items[target].Selected = true;
        }
        lvFiles.Focus();
    }

    private void BtnSortName_Click(object? sender, EventArgs e)
    {
        _files.Sort((a, b) => string.Compare(Path.GetFileName(a), Path.GetFileName(b), StringComparison.OrdinalIgnoreCase));
        RefreshList();
    }

    private void LvFiles_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete) RemoveSelected();
    }

    private void LvFiles_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            e.Effect = DragDropEffects.Copy;
    }

    private void LvFiles_DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetData(DataFormats.FileDrop) is string[] paths) AddFiles(paths);
    }

    private void RefreshList()
    {
        lvFiles.BeginUpdate();
        lvFiles.Items.Clear();

        for (int i = 0; i < _files.Count; i++)
        {
            var f = _files[i];
            var info = new FileInfo(f);
            var item = new ListViewItem((i + 1).ToString());
            item.SubItems.Add(Path.GetFileName(f));
            item.SubItems.Add(Path.GetExtension(f).TrimStart('.').ToUpperInvariant());
            item.SubItems.Add(info.Exists ? PathUtil.FormatSize(info.Length) : "-");
            item.SubItems.Add(info.Exists ? info.LastWriteTime.ToString("yyyy/MM/dd HH:mm") : "-");
            item.SubItems.Add(Path.GetDirectoryName(f) ?? "");
            lvFiles.Items.Add(item);
        }

        lvFiles.EndUpdate();
        UpdateOptionState();
    }

    // ---------- 出力設定 ----------

    private IFileMerger? ResolveMerger()
    {
        if (cmbType.SelectedIndex <= 0)
            return _files.Count > 0 ? MergerRegistry.Detect(_files) : null;

        return MergerRegistry.All[cmbType.SelectedIndex - 1];
    }

    private void CmbType_SelectedIndexChanged(object? sender, EventArgs e)
    {
        UpdateOptionState();
        SuggestOutputPath();
    }

    private void CmbExcelMode_SelectedIndexChanged(object? sender, EventArgs e) => UpdateOptionState();

    private void UpdateOptionState()
    {
        var merger = ResolveMerger();
        var features = merger?.Features ?? MergerFeatures.None;

        bool enc = features.HasFlag(MergerFeatures.Encoding);
        lblEncIn.Enabled = cmbEncIn.Enabled = enc;
        lblEncOut.Enabled = cmbEncOut.Enabled = enc;

        bool nameHeader = features.HasFlag(MergerFeatures.FileNameHeader);
        bool skipHeader = features.HasFlag(MergerFeatures.SkipHeaderRow);
        bool split = features.HasFlag(MergerFeatures.SplitAtBoundary);

        bool excel = features.HasFlag(MergerFeatures.ExcelMode);
        lblExcelMode.Enabled = cmbExcelMode.Enabled = excel;

        if (excel)
        {
            // シート別は元のシートをそのまま写すので、行レベルの調整は効かない。
            // 見出し名でそろえるモードと横並びは 1 行目＝見出しが前提なので設定不要。
            bool separateSheets = cmbExcelMode.SelectedIndex <= 0;
            nameHeader = !separateSheets;
            skipHeader = cmbExcelMode.SelectedIndex == 1;
            split = !separateSheets;

            chkFileNameHeader.Text = cmbExcelMode.SelectedIndex == 3
                ? "ブロックの上に元のファイル名を入れる"
                : "「元ファイル」列を追加する";
            chkSplit.Text = cmbExcelMode.SelectedIndex == 3
                ? "ファイルの区切りに空列を入れる"
                : "ファイルの区切りに空行を入れる";
        }
        else
        {
            chkFileNameHeader.Text = "元のファイル名を見出しとして入れる";
            chkSplit.Text = "ファイルの区切りで改ページ・空行を入れる";
        }

        chkFileNameHeader.Enabled = nameHeader;
        chkSkipHeaderRow.Enabled = skipHeader;
        chkSplit.Enabled = split;

        bool image = features.HasFlag(MergerFeatures.ImagePaper);
        lblPaper.Enabled = cmbPaper.Enabled = image;

        if (cmbType.SelectedIndex <= 0)
        {
            grpOutput.Text = merger is null
                ? "出力の設定"
                : $"出力の設定 － 判定結果: {merger.Name}";
        }
        else
        {
            grpOutput.Text = "出力の設定";
        }

        btnMerge.Enabled = !_running && _files.Count > 0;
    }

    private void SuggestOutputPath()
    {
        if (_files.Count == 0) return;

        var merger = ResolveMerger();
        if (merger is null) return;

        var ext = merger is Mergers.BinaryMerger
            ? Path.GetExtension(_files[0])
            : merger.OutputExtension;

        if (string.IsNullOrWhiteSpace(txtOutput.Text))
        {
            txtOutput.Text = PathUtil.SuggestOutputPath(_files[0], ext);
            return;
        }

        // 既定名のままなら拡張子だけ差し替える
        var current = txtOutput.Text;
        if (Path.GetFileNameWithoutExtension(current).StartsWith("統合結果", StringComparison.Ordinal))
            txtOutput.Text = Path.ChangeExtension(current, ext);
    }

    private void BtnBrowse_Click(object? sender, EventArgs e)
    {
        var merger = ResolveMerger();

        using var dlg = new SaveFileDialog
        {
            Title = "保存先を選ぶ",
            Filter = merger?.OutputFilter ?? "すべてのファイル (*.*)|*.*",
            OverwritePrompt = true
        };

        if (!string.IsNullOrWhiteSpace(txtOutput.Text))
        {
            var dir = Path.GetDirectoryName(txtOutput.Text);
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir)) dlg.InitialDirectory = dir;
            dlg.FileName = Path.GetFileName(txtOutput.Text);
        }

        if (dlg.ShowDialog(this) == DialogResult.OK) txtOutput.Text = dlg.FileName;
    }

    private MergeOptions BuildOptions() => new()
    {
        InputEncoding = cmbEncIn.SelectedIndex switch
        {
            1 => new UTF8Encoding(false),
            2 => EncodingUtil.ShiftJis,
            3 => Encoding.Unicode,
            _ => null
        },
        OutputEncoding = EncodingUtil.OutputChoices[Math.Max(0, cmbEncOut.SelectedIndex)].Encoding,
        InsertFileNameHeader = chkFileNameHeader.Checked,
        SkipRepeatedHeaderRow = chkSkipHeaderRow.Checked,
        SplitAtFileBoundary = chkSplit.Checked,
        ExcelMode = cmbExcelMode.SelectedIndex switch
        {
            1 => ExcelMergeMode.SingleSheetByPosition,
            2 => ExcelMergeMode.SingleSheetByHeader,
            3 => ExcelMergeMode.SingleSheetHorizontal,
            _ => ExcelMergeMode.SeparateSheets
        },
        ImagePaper = cmbPaper.SelectedIndex == 1 ? ImagePaperMode.OriginalSize : ImagePaperMode.FitA4
    };

    // ---------- 統合の実行 ----------

    private async void BtnMerge_Click(object? sender, EventArgs e)
    {
        if (_running)
        {
            _cts?.Cancel();
            Log("中止しています...");
            return;
        }

        if (_files.Count == 0)
        {
            MessageBox.Show(this, "統合するファイルがありません。ファイルを追加してください。",
                "ファイル統合ツール", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var missing = _files.Where(f => !File.Exists(f)).ToList();
        if (missing.Count > 0)
        {
            MessageBox.Show(this,
                "次のファイルが見つかりません。一覧から削除してからやり直してください。\r\n\r\n"
                + string.Join("\r\n", missing.Take(10).Select(Path.GetFileName)),
                "ファイル統合ツール", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var merger = ResolveMerger();
        if (merger is null)
        {
            MessageBox.Show(this,
                "この組み合わせに合う統合方法が見つかりませんでした。[統合方法] から手動で選んでください。",
                "ファイル統合ツール", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(txtOutput.Text))
        {
            BtnBrowse_Click(sender, e);
            if (string.IsNullOrWhiteSpace(txtOutput.Text)) return;
        }

        var outputPath = Path.GetFullPath(txtOutput.Text);
        var outputDir = Path.GetDirectoryName(outputPath);

        if (string.IsNullOrEmpty(outputDir))
        {
            MessageBox.Show(this, "保存先のフォルダーを正しく指定してください。",
                "ファイル統合ツール", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Directory.CreateDirectory(outputDir);

        if (_files.Any(f => string.Equals(Path.GetFullPath(f), outputPath, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show(this, "入力の 1 つと保存先が同じです。別の名前を指定してください。",
                "ファイル統合ツール", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (File.Exists(outputPath))
        {
            var answer = MessageBox.Show(this,
                $"{Path.GetFileName(outputPath)} は既にあります。上書きしますか？",
                "ファイル統合ツール", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes) return;
        }

        var options = BuildOptions();
        var inputs = _files.ToList();

        SetRunning(true);
        progressBar.Value = 0;
        progressBar.Maximum = Math.Max(1, inputs.Count);
        Log($"――― {merger.Name}：{inputs.Count} 件を統合します ―――");

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        var progress = new Progress<MergeProgress>(p =>
        {
            progressBar.Value = Math.Min(progressBar.Maximum, Math.Max(0, p.Current));
            Log(p.Message);
        });

        var sw = Stopwatch.StartNew();

        try
        {
            await Task.Run(() => merger.Merge(inputs, outputPath, options, progress, token), token);

            sw.Stop();
            progressBar.Value = progressBar.Maximum;
            Log($"統合しました（{sw.Elapsed.TotalSeconds:0.0} 秒）→ {outputPath}");

            if (chkOpenFolder.Checked) RevealInExplorer(outputPath);
        }
        catch (OperationCanceledException)
        {
            Log("中止しました。作りかけのファイルが残っている場合があります。");
        }
        catch (Exception ex)
        {
            Log($"エラー: {ex.Message}");
            MessageBox.Show(this,
                $"統合できませんでした。\r\n\r\n{ex.Message}",
                "ファイル統合ツール", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _cts.Dispose();
            _cts = null;
            SetRunning(false);
        }
    }

    private void SetRunning(bool running)
    {
        _running = running;
        btnMerge.Text = running ? "中止" : "統合する";
        toolStrip.Enabled = !running;
        grpOutput.Enabled = !running;
        lvFiles.Enabled = !running;
        btnMerge.Enabled = running || _files.Count > 0;
        UseWaitCursor = running;
    }

    private static void RevealInExplorer(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"") { UseShellExecute = true });
        }
        catch
        {
            // エクスプローラーを開けなくても統合自体は済んでいるので何もしない
        }
    }

    private void Log(string message)
    {
        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
    }

    // ---------- ヘルプ ----------

    private void MnuSupportedFiles_Click(object? sender, EventArgs e)
    {
        using var dlg = new SupportedFilesForm();
        dlg.ShowDialog(this);
    }

    private void MnuManual_Click(object? sender, EventArgs e)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "docs", "manual.html");

        if (!File.Exists(path))
        {
            MessageBox.Show(this,
                $"マニュアルが見つかりませんでした。\r\n\r\n{path}",
                "ファイル統合ツール", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                $"マニュアルを開けませんでした。\r\n\r\n{ex.Message}",
                "ファイル統合ツール", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void MnuAbout_Click(object? sender, EventArgs e)
    {
        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

        MessageBox.Show(this,
            "ファイル統合ツール\r\n"
            + $"バージョン {version?.ToString(3) ?? "1.0.0"}\r\n\r\n"
            + "複数のファイルを 1 つにまとめるツールです。\r\n"
            + "使い方は [ヘルプ] → [マニュアルを開く]、\r\n"
            + "扱えるファイルは [扱えるファイルの一覧] (F1) で確認できます。",
            "バージョン情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
