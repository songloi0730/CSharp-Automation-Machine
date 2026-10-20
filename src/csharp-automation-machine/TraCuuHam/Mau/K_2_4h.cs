// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Globalization;

namespace TraCuuHam.Mau
{
    internal static class K_2_4h
    {
        internal static void Chay()
        {
            double v = 1234.5678; int n = 255;
            string a = 2.5.ToString("F0", CultureInfo.InvariantCulture);
            string b = 1.005.ToString("F2", CultureInfo.InvariantCulture);
            if (!(Environment.Version.Major >= 5 ? (a == "2" && b == "1.00") : (a == "3" && b == "1.01"))) throw new Exception("Kết quả sai");
        }
    }
}
