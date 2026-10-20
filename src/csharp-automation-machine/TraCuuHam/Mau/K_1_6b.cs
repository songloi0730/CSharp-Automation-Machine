// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_1_6b
    {
        internal static void Chay()
        {
            string s = "Truc X";
            s.Replace("X", "Y");
            if (!(s == "Truc X")) throw new Exception("Kết quả sai");
        }
    }
}
