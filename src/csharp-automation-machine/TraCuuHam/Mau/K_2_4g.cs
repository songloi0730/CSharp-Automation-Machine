// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Globalization;

namespace TraCuuHam.Mau
{
    internal static class K_2_4g
    {
        internal static void Chay()
        {
            double v = 1234.5678; int n = 255;
            string t = v.ToString("F2", new CultureInfo("vi-VN"));
            if (!(t == "1234,57")) throw new Exception("Kết quả sai");
        }
    }
}
