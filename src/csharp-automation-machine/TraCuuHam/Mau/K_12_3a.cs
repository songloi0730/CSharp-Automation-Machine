// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_12_3a
    {
        internal static void Chay()
        {
            int v = 0x0A;
            bool bat = (v & (1 << 3)) != 0;
            if (!(bat)) throw new Exception("Kết quả sai");
        }
    }
}
