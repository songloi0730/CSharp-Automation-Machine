// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_4_1f
    {
        internal static void Chay()
        {
            int[] b = { 5, 2, 9 };
            int[] c = new int[3];
            Array.Copy(b, 0, c, 0, 3);
            if (!(c[1] == 2)) throw new Exception("Kết quả sai");
        }
    }
}
