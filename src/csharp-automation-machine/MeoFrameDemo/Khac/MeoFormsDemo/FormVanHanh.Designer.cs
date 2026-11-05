namespace MeoFormsDemo;

partial class FormVanHanh
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        btnStart = new Button();
        btnStop = new Button();
        lblTrangThaiTieuDe = new Label();
        lblTrangThai = new Label();
        lblTongTieuDe = new Label();
        lblTotalCount = new Label();
        SuspendLayout();
        //
        // btnStart
        //
        btnStart.Location = new Point(24, 120);
        btnStart.Name = "btnStart";
        btnStart.Size = new Size(120, 48);
        btnStart.TabIndex = 0;
        btnStart.Text = "Bắt đầu";
        btnStart.UseVisualStyleBackColor = true;
        btnStart.Click += btnStart_Click;
        //
        // btnStop
        //
        btnStop.Location = new Point(156, 120);
        btnStop.Name = "btnStop";
        btnStop.Size = new Size(120, 48);
        btnStop.TabIndex = 1;
        btnStop.Text = "Dừng";
        btnStop.UseVisualStyleBackColor = true;
        btnStop.Click += btnStop_Click;
        //
        // lblTrangThaiTieuDe
        //
        lblTrangThaiTieuDe.AutoSize = true;
        lblTrangThaiTieuDe.Location = new Point(24, 28);
        lblTrangThaiTieuDe.Name = "lblTrangThaiTieuDe";
        lblTrangThaiTieuDe.Size = new Size(64, 15);
        lblTrangThaiTieuDe.TabIndex = 2;
        lblTrangThaiTieuDe.Text = "Trạng thái:";
        //
        // lblTrangThai
        //
        lblTrangThai.AutoSize = true;
        lblTrangThai.Location = new Point(110, 28);
        lblTrangThai.Name = "lblTrangThai";
        lblTrangThai.Size = new Size(57, 15);
        lblTrangThai.TabIndex = 3;
        lblTrangThai.Text = "Sẵn sàng";
        //
        // lblTongTieuDe
        //
        lblTongTieuDe.AutoSize = true;
        lblTongTieuDe.Location = new Point(24, 64);
        lblTongTieuDe.Name = "lblTongTieuDe";
        lblTongTieuDe.Size = new Size(61, 15);
        lblTongTieuDe.TabIndex = 4;
        lblTongTieuDe.Text = "Sản lượng:";
        //
        // lblTotalCount
        //
        lblTotalCount.AutoSize = true;
        lblTotalCount.Location = new Point(110, 64);
        lblTotalCount.Name = "lblTotalCount";
        lblTotalCount.Size = new Size(13, 15);
        lblTotalCount.TabIndex = 5;
        lblTotalCount.Text = "0";
        //
        // FormVanHanh
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(304, 192);
        Controls.Add(lblTotalCount);
        Controls.Add(lblTongTieuDe);
        Controls.Add(lblTrangThai);
        Controls.Add(lblTrangThaiTieuDe);
        Controls.Add(btnStop);
        Controls.Add(btnStart);
        Name = "FormVanHanh";
        Text = "Màn hình vận hành";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Button btnStart;
    private Button btnStop;
    private Label lblTrangThaiTieuDe;
    private Label lblTrangThai;
    private Label lblTongTieuDe;
    private Label lblTotalCount;
}
