// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace TraCuuHam.Mau
{
    internal class K_15_3a : Window
    {
        TextBlock text1 = new TextBlock(); TextBox textBox1 = new TextBox(); Button button1 = new Button();
        ListBox list1 = new ListBox(); ComboBox comboBox1 = new ComboBox();

        internal void Chay()
        {
            var canhBao = new ObservableCollection<string>();
            list1.ItemsSource = canhBao;       // thêm/xoá trong canhBao là danh sách tự cập nhật
            canhBao.Insert(0, "Trục X quá giờ");
            if (canhBao.Count > 200) canhBao.RemoveAt(canhBao.Count - 1);
            if (!(list1.Items.Count == 1)) throw new Exception("Kết quả sai");
        }
    }
}
