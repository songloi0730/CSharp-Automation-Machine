// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_2_7d
    {
        internal static void Chay()
        {
            double doDo = Math.Atan2(1, 1) * 180 / Math.PI;
            if (!(Math.Abs(doDo - 45) < 1e-9)) throw new Exception("Kết quả sai");
        }
    }
}
