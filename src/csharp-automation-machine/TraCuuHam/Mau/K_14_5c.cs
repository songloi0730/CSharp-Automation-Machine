// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Windows.Forms;
using System.Drawing;

namespace TraCuuHam.Mau
{
    internal class K_14_5c : Form
    {
        Label label1 = new Label(); TextBox textBox1 = new TextBox(); Button button1 = new Button();
        ComboBox comboBox1 = new ComboBox(); DataGridView dataGridView1 = new DataGridView();
        NumericUpDown numericUpDown1 = new NumericUpDown(); CheckBox checkBox1 = new CheckBox();
        ListBox listBox1 = new ListBox();

        internal void Chay()
        {
            dataGridView1.Columns.Add("ma", "Mã"); dataGridView1.Rows.Add("SN-42");
            object v = dataGridView1.Rows[0].Cells[0].Value;
            object v2 = dataGridView1.Rows[0].Cells["ma"].Value;
            if (!((string)v == "SN-42" && v2 == v)) throw new Exception("Kết quả sai");
        }
    }
}
