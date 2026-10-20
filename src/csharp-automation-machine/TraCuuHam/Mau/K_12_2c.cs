// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_12_2c
    {
        internal static void Chay()
        {
            ushort v = (ushort)((0x12 << 8) | 0x34);
            if (!(v == 0x1234)) throw new Exception("Kết quả sai");
        }
    }
}
