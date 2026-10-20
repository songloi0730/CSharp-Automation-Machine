// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_2_6d
    {
        internal static void Chay()
        {
            double v = 120;
            double k = Math.Clamp(v, 0, 100);
            if (!(k == 100)) throw new Exception("Kết quả sai");
        }
    }
}
