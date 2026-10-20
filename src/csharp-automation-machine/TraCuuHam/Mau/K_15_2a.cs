// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Windows;
using System.Windows.Controls;

namespace TraCuuHam.Mau
{
    internal class K_15_2a : Window
    {
        TextBlock text1 = new TextBlock(); TextBox textBox1 = new TextBox(); Button button1 = new Button();
        ListBox list1 = new ListBox(); ComboBox comboBox1 = new ComboBox();

        internal void Chay()
        {
            MessageBoxResult r = MessageBox.Show("Xoá công thức?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (r == MessageBoxResult.Yes) { }
        }
    }
}
