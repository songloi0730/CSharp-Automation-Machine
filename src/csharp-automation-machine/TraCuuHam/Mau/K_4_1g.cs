// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_4_1g
    {
        internal static void Chay()
        {
            int[] b = { 5, 2, 9 };
            double[,] m = new double[3, 4];
            m[2, 3] = 1.5;
            int hang = m.GetLength(0);
            if (!(hang == 3 && m.GetLength(1) == 4)) throw new Exception("Kết quả sai");
        }
    }
}
