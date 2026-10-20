// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Text;

namespace TraCuuHam.Mau
{
    internal static class K_1_10a
    {
        internal static void Chay()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 3; i++)
                sb.Append(i).Append(';');
            sb.AppendLine("xong");
            sb.AppendFormat("{0:F1}", 2.26);
            string t = sb.ToString();     // "0;1;2;xong\r\n2.3"
            sb.Clear();                   // dùng lại, không tạo mới
            if (!(t.StartsWith("0;1;2;xong") && t.EndsWith("2.3"))) throw new Exception("Kết quả sai");
        }
    }
}
