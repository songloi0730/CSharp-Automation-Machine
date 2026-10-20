// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_3_4a
    {
        internal static void Chay()
        {
            DateTime t = new DateTime(2026, 9, 30, 23, 0, 0);
            DateTime x = t.AddDays(-30);
            if (!(x.Month == 8 && x.Day == 31)) throw new Exception("Kết quả sai");
        }
    }
}
