// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_1_7c
    {
        internal static void Chay()
        {
            string a = "OK"; string b = "ok"; string rong = null;
            bool g = string.Compare(a, b, true) == 0;
            if (!(g)) throw new Exception("Kết quả sai");
        }
    }
}
