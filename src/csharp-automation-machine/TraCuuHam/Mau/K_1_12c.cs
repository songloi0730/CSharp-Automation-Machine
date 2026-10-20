// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_1_12c
    {
        internal static void Chay()
        {
            string s = "A1 b";
            bool b = char.IsWhiteSpace(s[2]);
            if (!(b)) throw new Exception("Kết quả sai");
        }
    }
}
