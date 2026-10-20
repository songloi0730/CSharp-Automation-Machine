// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Globalization;

namespace TraCuuHam.Mau
{
    internal static class K_3_2a
    {
        internal static void Chay()
        {
            DateTime t = new DateTime(2026, 9, 30, 21, 5, 7, 123);
            string s = t.ToString("yyyy-MM-dd HH:mm:ss");
            if (!(s == "2026-09-30 21:05:07")) throw new Exception("Kết quả sai");
        }
    }
}
