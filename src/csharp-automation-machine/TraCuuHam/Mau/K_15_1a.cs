// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;

namespace TraCuuHam.Mau
{
    internal class K_15_1a : Window
    {
        TextBlock text1 = new TextBlock(); TextBox textBox1 = new TextBox(); Button button1 = new Button();
        ListBox list1 = new ListBox(); ComboBox comboBox1 = new ComboBox();

        internal void Chay()
        {
            if (Dispatcher.CheckAccess())
                text1.Text = "OK";
            else
                Dispatcher.BeginInvoke(new Action(() => text1.Text = "OK"));

            Dispatcher.Invoke(() => text1.Text = "OK");        // chờ làm xong
            Dispatcher.InvokeAsync(() => text1.Text = "OK");   // không chờ, như BeginInvoke (FW 4.5+)
            if (!(text1.Text == "OK")) throw new Exception("Kết quả sai");
        }
    }
}
