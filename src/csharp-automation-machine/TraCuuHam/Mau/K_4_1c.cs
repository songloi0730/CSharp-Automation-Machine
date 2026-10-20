// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_4_1c
    {
        internal static void Chay()
        {
            int[] b = { 5, 2, 9 };
            Array.Sort(b);
            if (!(b[0] == 2 && b[2] == 9)) throw new Exception("Kết quả sai");
        }
    }
}
