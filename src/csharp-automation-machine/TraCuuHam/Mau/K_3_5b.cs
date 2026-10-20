// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_3_5b
    {
        internal static void Chay()
        {
            TimeSpan d = TimeSpan.FromMinutes(90);
            double giay = d.TotalSeconds;
            if (!(giay == 5400 && d.Minutes == 30)) throw new Exception("Kết quả sai");
        }
    }
}
