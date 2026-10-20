// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Globalization;

namespace TraCuuHam.Mau
{
    internal static class K_2_2b
    {
        internal static void Chay()
        {
            bool ok = int.TryParse("42", out int n);
            if (!(ok && n == 42)) throw new Exception("Kết quả sai");
        }
    }
}
