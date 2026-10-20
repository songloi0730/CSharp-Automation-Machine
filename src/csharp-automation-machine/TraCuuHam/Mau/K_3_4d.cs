// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_3_4d
    {
        internal static void Chay()
        {
            DateTime t = new DateTime(2026, 9, 30, 23, 0, 0);
            int h = t.Hour; int thang = t.Month;
            if (!(h == 23 && thang == 9)) throw new Exception("Kết quả sai");
        }
    }
}
