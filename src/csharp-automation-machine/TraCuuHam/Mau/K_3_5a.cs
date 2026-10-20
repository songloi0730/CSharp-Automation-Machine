// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_3_5a
    {
        internal static void Chay()
        {
            TimeSpan d = new DateTime(2026, 9, 30, 8, 30, 0) - new DateTime(2026, 9, 30, 8, 0, 0);
            if (!(d.TotalMinutes == 30)) throw new Exception("Kết quả sai");
        }
    }
}
