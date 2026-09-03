namespace FileMerger;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;

    private MenuStrip menuStrip;
    private ToolStripMenuItem mnuHelp;
    private ToolStripMenuItem mnuSupportedFiles;
    private ToolStripMenuItem mnuManual;
    private ToolStripSeparator mnuHelpSep;
    private ToolStripMenuItem mnuAbout;

    private ToolStrip toolStrip;
    private ToolStripButton btnAddFiles;
    private ToolStripButton btnAddFolder;
    private ToolStripButton btnRemove;
    private ToolStripButton btnClear;
    private ToolStripSeparator sep1;
    private ToolStripButton btnUp;
    private ToolStripButton btnDown;
    private ToolStripButton btnSortName;

    private ListView lvFiles;
    private ColumnHeader colIndex;
    private ColumnHeader colName;
    private ColumnHeader colKind;
    private ColumnHeader colSize;
    private ColumnHeader colDate;
    private ColumnHeader colPath;

    private GroupBox grpOutput;
    private Label lblType;
    private ComboBox cmbType;
    private Label lblEncIn;
    private ComboBox cmbEncIn;
    private Label lblEncOut;
    private ComboBox cmbEncOut;
    private CheckBox chkFileNameHeader;
    private CheckBox chkSkipHeaderRow;
    private CheckBox chkSplit;
    private Label lblExcelMode;
    private ComboBox cmbExcelMode;
    private Label lblPaper;
    private ComboBox cmbPaper;
    private Label lblOutput;
    private TextBox txtOutput;
    private Button btnBrowse;
    private CheckBox chkOpenFolder;

    private TextBox txtLog;
    private ProgressBar progressBar;
    private Button btnMerge;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        menuStrip = new MenuStrip();
        mnuHelp = new ToolStripMenuItem();
        mnuSupportedFiles = new ToolStripMenuItem();
        mnuManual = new ToolStripMenuItem();
        mnuHelpSep = new ToolStripSeparator();
        mnuAbout = new ToolStripMenuItem();

        toolStrip = new ToolStrip();
        btnAddFiles = new ToolStripButton();
        btnAddFolder = new ToolStripButton();
        btnRemove = new ToolStripButton();
        btnClear = new ToolStripButton();
        sep1 = new ToolStripSeparator();
        btnUp = new ToolStripButton();
        btnDown = new ToolStripButton();
        btnSortName = new ToolStripButton();

        lvFiles = new ListView();
        colIndex = new ColumnHeader();
        colName = new ColumnHeader();
        colKind = new ColumnHeader();
        colSize = new ColumnHeader();
        colDate = new ColumnHeader();
        colPath = new ColumnHeader();

        grpOutput = new GroupBox();
        lblType = new Label();
        cmbType = new ComboBox();
        lblEncIn = new Label();
        cmbEncIn = new ComboBox();
        lblEncOut = new Label();
        cmbEncOut = new ComboBox();
        chkFileNameHeader = new CheckBox();
        chkSkipHeaderRow = new CheckBox();
        chkSplit = new CheckBox();
        lblExcelMode = new Label();
        cmbExcelMode = new ComboBox();
        lblPaper = new Label();
        cmbPaper = new ComboBox();
        lblOutput = new Label();
        txtOutput = new TextBox();
        btnBrowse = new Button();
        chkOpenFolder = new CheckBox();

        txtLog = new TextBox();
        progressBar = new ProgressBar();
        btnMerge = new Button();

        menuStrip.SuspendLayout();
        toolStrip.SuspendLayout();
        grpOutput.SuspendLayout();
        SuspendLayout();

        // menuStrip
        menuStrip.Items.AddRange(new ToolStripItem[] { mnuHelp });
        menuStrip.Location = new Point(0, 0);
        menuStrip.Name = "menuStrip";
        menuStrip.Size = new Size(984, 24);
        menuStrip.TabIndex = 100;

        mnuHelp.DropDownItems.AddRange(new ToolStripItem[] { mnuSupportedFiles, mnuManual, mnuHelpSep, mnuAbout });
        mnuHelp.Name = "mnuHelp";
        mnuHelp.Size = new Size(60, 20);
        mnuHelp.Text = "ヘルプ(&H)";

        mnuSupportedFiles.Name = "mnuSupportedFiles";
        mnuSupportedFiles.ShortcutKeys = Keys.F1;
        mnuSupportedFiles.Size = new Size(220, 22);
        mnuSupportedFiles.Text = "扱えるファイルの一覧(&L)...";
        mnuSupportedFiles.Click += MnuSupportedFiles_Click;

        mnuManual.Name = "mnuManual";
        mnuManual.Size = new Size(220, 22);
        mnuManual.Text = "マニュアルを開く(&M)...";
        mnuManual.Click += MnuManual_Click;

        mnuHelpSep.Name = "mnuHelpSep";

        mnuAbout.Name = "mnuAbout";
        mnuAbout.Size = new Size(220, 22);
        mnuAbout.Text = "バージョン情報(&A)...";
        mnuAbout.Click += MnuAbout_Click;

        // toolStrip
        toolStrip.ImageScalingSize = new Size(20, 20);
        toolStrip.Items.AddRange(new ToolStripItem[]
        {
            btnAddFiles, btnAddFolder, btnRemove, btnClear, sep1, btnUp, btnDown, btnSortName
        });
        toolStrip.Location = new Point(0, 24);
        toolStrip.Name = "toolStrip";
        toolStrip.Size = new Size(984, 27);
        toolStrip.TabIndex = 0;

        btnAddFiles.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnAddFiles.Name = "btnAddFiles";
        btnAddFiles.Text = "ファイルを追加";
        btnAddFiles.ToolTipText = "統合したいファイルを選ぶ（ドラッグ＆ドロップでも追加できます）";
        btnAddFiles.Click += BtnAddFiles_Click;

        btnAddFolder.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnAddFolder.Name = "btnAddFolder";
        btnAddFolder.Text = "フォルダーを追加";
        btnAddFolder.ToolTipText = "フォルダー内のファイルをまとめて追加する";
        btnAddFolder.Click += BtnAddFolder_Click;

        btnRemove.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnRemove.Name = "btnRemove";
        btnRemove.Text = "選択を削除";
        btnRemove.Click += BtnRemove_Click;

        btnClear.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnClear.Name = "btnClear";
        btnClear.Text = "すべて削除";
        btnClear.Click += BtnClear_Click;

        btnUp.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnUp.Name = "btnUp";
        btnUp.Text = "▲ 上へ";
        btnUp.Click += BtnUp_Click;

        btnDown.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnDown.Name = "btnDown";
        btnDown.Text = "▼ 下へ";
        btnDown.Click += BtnDown_Click;

        btnSortName.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnSortName.Name = "btnSortName";
        btnSortName.Text = "名前順に並べ替え";
        btnSortName.Click += BtnSortName_Click;

        // lvFiles
        lvFiles.AllowDrop = true;
        lvFiles.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lvFiles.Columns.AddRange(new ColumnHeader[] { colIndex, colName, colKind, colSize, colDate, colPath });
        lvFiles.FullRowSelect = true;
        lvFiles.GridLines = true;
        lvFiles.HideSelection = false;
        lvFiles.Location = new Point(12, 60);
        lvFiles.Name = "lvFiles";
        lvFiles.Size = new Size(960, 262);
        lvFiles.TabIndex = 1;
        lvFiles.View = View.Details;
        lvFiles.DragEnter += LvFiles_DragEnter;
        lvFiles.DragDrop += LvFiles_DragDrop;
        lvFiles.KeyDown += LvFiles_KeyDown;

        colIndex.Text = "#";
        colIndex.Width = 40;
        colName.Text = "ファイル名";
        colName.Width = 250;
        colKind.Text = "種類";
        colKind.Width = 70;
        colSize.Text = "サイズ";
        colSize.Width = 80;
        colSize.TextAlign = HorizontalAlignment.Right;
        colDate.Text = "更新日時";
        colDate.Width = 130;
        colPath.Text = "場所";
        colPath.Width = 370;

        // grpOutput
        grpOutput.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grpOutput.Controls.Add(lblType);
        grpOutput.Controls.Add(cmbType);
        grpOutput.Controls.Add(lblEncIn);
        grpOutput.Controls.Add(cmbEncIn);
        grpOutput.Controls.Add(lblEncOut);
        grpOutput.Controls.Add(cmbEncOut);
        grpOutput.Controls.Add(chkFileNameHeader);
        grpOutput.Controls.Add(chkSkipHeaderRow);
        grpOutput.Controls.Add(chkSplit);
        grpOutput.Controls.Add(lblExcelMode);
        grpOutput.Controls.Add(cmbExcelMode);
        grpOutput.Controls.Add(lblPaper);
        grpOutput.Controls.Add(cmbPaper);
        grpOutput.Controls.Add(lblOutput);
        grpOutput.Controls.Add(txtOutput);
        grpOutput.Controls.Add(btnBrowse);
        grpOutput.Controls.Add(chkOpenFolder);
        grpOutput.Location = new Point(12, 330);
        grpOutput.Name = "grpOutput";
        grpOutput.Size = new Size(960, 206);
        grpOutput.TabIndex = 2;
        grpOutput.TabStop = false;
        grpOutput.Text = "出力の設定";

        lblType.AutoSize = true;
        lblType.Location = new Point(14, 32);
        lblType.Name = "lblType";
        lblType.Size = new Size(60, 15);
        lblType.Text = "統合方法:";

        cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbType.Location = new Point(96, 28);
        cmbType.Name = "cmbType";
        cmbType.Size = new Size(250, 23);
        cmbType.TabIndex = 3;
        cmbType.SelectedIndexChanged += CmbType_SelectedIndexChanged;

        lblEncIn.AutoSize = true;
        lblEncIn.Location = new Point(366, 32);
        lblEncIn.Name = "lblEncIn";
        lblEncIn.Size = new Size(80, 15);
        lblEncIn.Text = "入力の文字コード:";

        cmbEncIn.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbEncIn.Location = new Point(478, 28);
        cmbEncIn.Name = "cmbEncIn";
        cmbEncIn.Size = new Size(140, 23);
        cmbEncIn.TabIndex = 4;

        lblEncOut.AutoSize = true;
        lblEncOut.Location = new Point(636, 32);
        lblEncOut.Name = "lblEncOut";
        lblEncOut.Size = new Size(80, 15);
        lblEncOut.Text = "出力の文字コード:";

        cmbEncOut.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbEncOut.Location = new Point(748, 28);
        cmbEncOut.Name = "cmbEncOut";
        cmbEncOut.Size = new Size(190, 23);
        cmbEncOut.TabIndex = 5;

        chkFileNameHeader.AutoSize = true;
        chkFileNameHeader.Location = new Point(16, 66);
        chkFileNameHeader.Name = "chkFileNameHeader";
        chkFileNameHeader.Size = new Size(220, 19);
        chkFileNameHeader.TabIndex = 6;
        chkFileNameHeader.Text = "元のファイル名を見出しとして入れる";

        chkSkipHeaderRow.AutoSize = true;
        chkSkipHeaderRow.Location = new Point(300, 66);
        chkSkipHeaderRow.Name = "chkSkipHeaderRow";
        chkSkipHeaderRow.Size = new Size(280, 19);
        chkSkipHeaderRow.TabIndex = 7;
        chkSkipHeaderRow.Text = "2 つ目以降の見出し行を繰り返さない";

        chkSplit.AutoSize = true;
        chkSplit.Location = new Point(636, 66);
        chkSplit.Name = "chkSplit";
        chkSplit.Size = new Size(240, 19);
        chkSplit.TabIndex = 8;
        chkSplit.Text = "ファイルの区切りで改ページ・空行を入れる";

        lblExcelMode.AutoSize = true;
        lblExcelMode.Location = new Point(14, 102);
        lblExcelMode.Name = "lblExcelMode";
        lblExcelMode.Size = new Size(80, 15);
        lblExcelMode.Text = "Excel の並べ方:";

        cmbExcelMode.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbExcelMode.Location = new Point(140, 98);
        cmbExcelMode.Name = "cmbExcelMode";
        cmbExcelMode.Size = new Size(330, 23);
        cmbExcelMode.TabIndex = 9;
        cmbExcelMode.SelectedIndexChanged += CmbExcelMode_SelectedIndexChanged;

        lblPaper.AutoSize = true;
        lblPaper.Location = new Point(496, 102);
        lblPaper.Name = "lblPaper";
        lblPaper.Size = new Size(80, 15);
        lblPaper.Text = "画像のページ:";

        cmbPaper.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbPaper.Location = new Point(610, 98);
        cmbPaper.Name = "cmbPaper";
        cmbPaper.Size = new Size(190, 23);
        cmbPaper.TabIndex = 10;

        lblOutput.AutoSize = true;
        lblOutput.Location = new Point(14, 143);
        lblOutput.Name = "lblOutput";
        lblOutput.Size = new Size(50, 15);
        lblOutput.Text = "保存先:";

        txtOutput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtOutput.Location = new Point(96, 139);
        txtOutput.Name = "txtOutput";
        txtOutput.Size = new Size(730, 23);
        txtOutput.TabIndex = 11;

        btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnBrowse.Location = new Point(836, 138);
        btnBrowse.Name = "btnBrowse";
        btnBrowse.Size = new Size(102, 25);
        btnBrowse.TabIndex = 12;
        btnBrowse.Text = "参照...";
        btnBrowse.UseVisualStyleBackColor = true;
        btnBrowse.Click += BtnBrowse_Click;

        chkOpenFolder.AutoSize = true;
        chkOpenFolder.Checked = true;
        chkOpenFolder.CheckState = CheckState.Checked;
        chkOpenFolder.Location = new Point(96, 173);
        chkOpenFolder.Name = "chkOpenFolder";
        chkOpenFolder.Size = new Size(230, 19);
        chkOpenFolder.TabIndex = 13;
        chkOpenFolder.Text = "統合できたら保存先フォルダーを開く";

        // txtLog
        txtLog.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        txtLog.BackColor = SystemColors.Window;
        txtLog.Location = new Point(12, 544);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = ScrollBars.Vertical;
        txtLog.Size = new Size(960, 100);
        txtLog.TabIndex = 14;

        // progressBar
        progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        progressBar.Location = new Point(12, 656);
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(770, 24);
        progressBar.TabIndex = 15;

        // btnMerge
        btnMerge.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnMerge.Location = new Point(796, 650);
        btnMerge.Name = "btnMerge";
        btnMerge.Size = new Size(176, 36);
        btnMerge.TabIndex = 16;
        btnMerge.Text = "統合する";
        btnMerge.UseVisualStyleBackColor = true;
        btnMerge.Click += BtnMerge_Click;

        // MainForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        AcceptButton = btnMerge;
        AllowDrop = true;
        ClientSize = new Size(984, 698);
        Controls.Add(btnMerge);
        Controls.Add(progressBar);
        Controls.Add(txtLog);
        Controls.Add(grpOutput);
        Controls.Add(lvFiles);
        Controls.Add(toolStrip);
        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;
        MinimumSize = new Size(880, 664);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "ファイル統合ツール";
        DragEnter += LvFiles_DragEnter;
        DragDrop += LvFiles_DragDrop;
        Load += MainForm_Load;

        menuStrip.ResumeLayout(false);
        menuStrip.PerformLayout();
        toolStrip.ResumeLayout(false);
        toolStrip.PerformLayout();
        grpOutput.ResumeLayout(false);
        grpOutput.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
