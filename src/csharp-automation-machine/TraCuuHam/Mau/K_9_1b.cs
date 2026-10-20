// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;

namespace TraCuuHam.Mau
{
    internal class K_9_1b : Window
    {
        TextBlock text1 = new TextBlock(); TextBox textBox1 = new TextBox(); Button button1 = new Button();
        ListBox list1 = new ListBox(); ComboBox comboBox1 = new ComboBox();

        internal void Chay()
        {
            var t = new DispatcherTimer();
            t.Interval = TimeSpan.FromMilliseconds(200);   // kiểu TimeSpan, không phải int
            t.Tick += (s, e) => text1.Text = DateTime.Now.ToString("HH:mm:ss");
            t.Start();
        }
    }
}
