namespace FileMerger;

partial class SupportedFilesForm
{
    private System.ComponentModel.IContainer components = null!;

    private Label lblIntro;
    private ListView lvKinds;
    private ColumnHeader colMethod;
    private ColumnHeader colOutput;
    private ColumnHeader colExtensions;
    private Label lblNote;
    private Button btnCopy;
    private Button btnClose;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        lblIntro = new Label();
        lvKinds = new ListView();
        colMethod = new ColumnHeader();
        colOutput = new ColumnHeader();
        colExtensions = new ColumnHeader();
        lblNote = new Label();
        btnCopy = new Button();
        btnClose = new Button();

        SuspendLayout();

        lblIntro.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblIntro.Location = new Point(12, 12);
        lblIntro.Name = "lblIntro";
        lblIntro.Size = new Size(720, 40);
        lblIntro.TabIndex = 0;
        lblIntro.Text =
            "次の種類のファイルを統合できます。一覧に載っていない拡張子でも、"
            + "[そのまま連結（バイナリ）] を選べば中身を解釈せずにつなぐことはできます。";

        lvKinds.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lvKinds.Columns.AddRange(new ColumnHeader[] { colMethod, colOutput, colExtensions });
        lvKinds.FullRowSelect = true;
        lvKinds.GridLines = true;
        lvKinds.HideSelection = false;
        lvKinds.Location = new Point(12, 58);
        lvKinds.MultiSelect = false;
        lvKinds.Name = "lvKinds";
        lvKinds.Size = new Size(720, 322);
        lvKinds.TabIndex = 1;
        lvKinds.View = View.Details;

        colMethod.Text = "統合方法";
        colMethod.Width = 190;
        colOutput.Text = "出力";
        colOutput.Width = 70;
        colExtensions.Text = "扱えるファイル";
        colExtensions.Width = 440;

        lblNote.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lblNote.Location = new Point(12, 388);
        lblNote.Name = "lblNote";
        lblNote.Size = new Size(720, 56);
        lblNote.TabIndex = 2;
        lblNote.Text =
            "[統合方法] を [自動で判定する] にしている場合は、この表の上から順に照合し、"
            + "追加したファイルの拡張子がすべて収まる最初の方法が使われます。"
            + "意図した方法にならないときは、[統合方法] から手動で選んでください。";

        btnCopy.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnCopy.Location = new Point(520, 452);
        btnCopy.Name = "btnCopy";
        btnCopy.Size = new Size(100, 30);
        btnCopy.TabIndex = 3;
        btnCopy.Text = "一覧をコピー";
        btnCopy.UseVisualStyleBackColor = true;
        btnCopy.Click += BtnCopy_Click;

        btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnClose.DialogResult = DialogResult.OK;
        btnClose.Location = new Point(632, 452);
        btnClose.Name = "btnClose";
        btnClose.Size = new Size(100, 30);
        btnClose.TabIndex = 4;
        btnClose.Text = "閉じる";
        btnClose.UseVisualStyleBackColor = true;

        AcceptButton = btnClose;
        CancelButton = btnClose;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(744, 494);
        Controls.Add(btnClose);
        Controls.Add(btnCopy);
        Controls.Add(lblNote);
        Controls.Add(lvKinds);
        Controls.Add(lblIntro);
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(620, 420);
        Name = "SupportedFilesForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "扱えるファイルの一覧";
        Load += SupportedFilesForm_Load;

        ResumeLayout(false);
    }
}
