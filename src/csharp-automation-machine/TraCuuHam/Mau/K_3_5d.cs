// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_3_5d
    {
        internal static void Chay()
        {
            string s = TimeSpan.FromMinutes(150).ToString(@"hh\:mm\:ss");
            if (!(s == "02:30:00")) throw new Exception("Kết quả sai");
        }
    }
}
