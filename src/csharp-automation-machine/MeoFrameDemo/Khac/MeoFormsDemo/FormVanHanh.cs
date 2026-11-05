namespace MeoFormsDemo;

// Mục 8.1.1: form tối giản để nhìn ba cửa sổ của trình thiết kế. Tên điều khiển theo quy ước ở mục 10.1.8.
public partial class FormVanHanh : Form
{
    public FormVanHanh()
    {
        InitializeComponent();
    }

    private void btnStart_Click(object sender, EventArgs e)
    {
        lblTrangThai.Text = "Đang chạy";
    }

    private void btnStop_Click(object sender, EventArgs e)
    {
        lblTrangThai.Text = "Đã dừng";
    }
}
