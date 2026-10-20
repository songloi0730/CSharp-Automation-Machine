// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_1_4b
    {
        internal static void Chay()
        {
            string dong = "X,12.5,,OK";
            string[] p = dong.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (!(p.Length == 3)) throw new Exception("Kết quả sai");
        }
    }
}
