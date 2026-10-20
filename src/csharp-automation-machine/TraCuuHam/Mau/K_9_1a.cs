// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Windows.Forms;
using System.Drawing;

namespace TraCuuHam.Mau
{
    internal class K_9_1a : Form
    {
        Label label1 = new Label(); TextBox textBox1 = new TextBox(); Button button1 = new Button();
        ComboBox comboBox1 = new ComboBox(); DataGridView dataGridView1 = new DataGridView();
        NumericUpDown numericUpDown1 = new NumericUpDown(); CheckBox checkBox1 = new CheckBox();
        ListBox listBox1 = new ListBox();

        internal void Chay()
        {
            var t = new System.Windows.Forms.Timer();
            t.Interval = 200;                      // mili-giây, kiểu int
            t.Tick += (s, e) => label1.Text = DateTime.Now.ToString("HH:mm:ss");
            t.Start();                             // t.Stop() để dừng
        }
    }
}
