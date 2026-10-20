// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_12_3c
    {
        internal static void Chay()
        {
            int v = 0x0A;
            v &= ~(1 << 1);
            if (!(v == 0x08)) throw new Exception("Kết quả sai");
        }
    }
}
