// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Windows.Forms;
using System.Drawing;

namespace TraCuuHam.Mau
{
    internal class K_14_1b : Form
    {
        Label label1 = new Label(); TextBox textBox1 = new TextBox(); Button button1 = new Button();
        ComboBox comboBox1 = new ComboBox(); DataGridView dataGridView1 = new DataGridView();
        NumericUpDown numericUpDown1 = new NumericUpDown(); CheckBox checkBox1 = new CheckBox();
        ListBox listBox1 = new ListBox();

        internal void Chay()
        {
            this.Invoke((MethodInvoker)delegate { label1.Text = "OK"; });
            this.BeginInvoke((MethodInvoker)(() => textBox1.AppendText("dong\r\n")));
        }
    }
}
