// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Globalization;

namespace TraCuuHam.Mau
{
    internal static class K_2_3c
    {
        internal static void Chay()
        {
            int n = Convert.ToInt32(12.5);
            if (!(n == 12 && Convert.ToInt32(13.5) == 14)) throw new Exception("Kết quả sai");
        }
    }
}
