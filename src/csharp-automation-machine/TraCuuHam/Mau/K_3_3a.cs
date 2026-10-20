// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Globalization;

namespace TraCuuHam.Mau
{
    internal static class K_3_3a
    {
        internal static void Chay()
        {
            DateTime t = DateTime.ParseExact("2026-09-30 21:05", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            if (!(t.Hour == 21 && t.Minute == 5)) throw new Exception("Kết quả sai");
        }
    }
}
