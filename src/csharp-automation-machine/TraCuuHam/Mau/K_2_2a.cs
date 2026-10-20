// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Globalization;

namespace TraCuuHam.Mau
{
    internal static class K_2_2a
    {
        internal static void Chay()
        {
            int n;
            bool ok = int.TryParse("abc", out n);
            if (!(!ok && n == 0)) throw new Exception("Kết quả sai");
        }
    }
}
