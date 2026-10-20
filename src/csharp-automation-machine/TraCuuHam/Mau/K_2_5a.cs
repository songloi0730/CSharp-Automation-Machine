// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_2_5a
    {
        internal static void Chay()
        {
            double r = Math.Round(2.5);
            if (!(r == 2 && Math.Round(3.5) == 4)) throw new Exception("Kết quả sai");
        }
    }
}
