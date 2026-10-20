// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_2_7c
    {
        internal static void Chay()
        {
            double rad = 30 * Math.PI / 180;
            double r = Math.Sin(rad);
            if (!(Math.Abs(r - 0.5) < 1e-9)) throw new Exception("Kết quả sai");
        }
    }
}
