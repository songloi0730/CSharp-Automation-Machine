// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_3_4b
    {
        internal static void Chay()
        {
            DateTime t = new DateTime(2026, 9, 30, 23, 0, 0);
            DateTime x = t.AddHours(2);
            if (!(x.Month == 10 && x.Hour == 1)) throw new Exception("Kết quả sai");
        }
    }
}
