// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_1_9c
    {
        internal static void Chay()
        {
            string ten = "X"; double viTri = 12.3456;
            string t = "Truc " + ten + " toi " + viTri.ToString("F2") + " mm";
            if (!(t == "Truc X toi 12.35 mm")) throw new Exception("Kết quả sai");
        }
    }
}
