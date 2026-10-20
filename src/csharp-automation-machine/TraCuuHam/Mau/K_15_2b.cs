// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Windows;
using System.Windows.Controls;

namespace TraCuuHam.Mau
{
    internal class K_15_2b : Window
    {
        TextBlock text1 = new TextBlock(); TextBox textBox1 = new TextBox(); Button button1 = new Button();
        ListBox list1 = new ListBox(); ComboBox comboBox1 = new ComboBox();

        internal void Chay()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "CSV (*.csv)|*.csv";
            if (dlg.ShowDialog() == true)          // kiểu bool?, KHÔNG phải DialogResult.OK
            {
                string duongDan = dlg.FileName;
            }
        }
    }
}
