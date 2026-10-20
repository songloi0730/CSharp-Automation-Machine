// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_2_10b
    {
        internal static void Chay()
        {
            TrangThai t = TrangThai.DangChay;
            string ten = t.ToString();
            if (!(ten == "DangChay")) throw new Exception("Kết quả sai");
        }
    }
}
