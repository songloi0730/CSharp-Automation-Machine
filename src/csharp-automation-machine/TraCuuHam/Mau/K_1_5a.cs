// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_1_5a
    {
        internal static void Chay()
        {
            string[] mang = { "X", "Y", "Z" };
            string t = string.Join(",", mang);
            if (!(t == "X,Y,Z")) throw new Exception("Kết quả sai");
        }
    }
}
