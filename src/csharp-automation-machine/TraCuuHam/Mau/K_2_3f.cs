// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Globalization;

namespace TraCuuHam.Mau
{
    internal static class K_2_3f
    {
        internal static void Chay()
        {
            int n = Convert.ToInt32("FF", 16);
            if (!(n == 255)) throw new Exception("Kết quả sai");
        }
    }
}
