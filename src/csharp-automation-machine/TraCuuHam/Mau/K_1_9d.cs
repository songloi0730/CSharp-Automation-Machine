// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_1_9d
    {
        internal static void Chay()
        {
            string ten = "X"; double viTri = 12.3456;
            string t = string.Format("{0,6}|{1,-6}|", 42, "OK");
            if (!(t == "    42|OK    |")) throw new Exception("Kết quả sai");
        }
    }
}
