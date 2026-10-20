// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_2_8b
    {
        internal static void Chay()
        {
            double a = 0.1 + 0.2;
            bool b = Math.Abs(a - 0.3) < 1e-9;
            if (!(b)) throw new Exception("Kết quả sai");
        }
    }
}
