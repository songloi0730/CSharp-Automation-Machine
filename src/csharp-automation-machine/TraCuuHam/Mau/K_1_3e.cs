// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_1_3e
    {
        internal static void Chay()
        {
            string ma = "SN-2026-00042";
            bool b = ma.EndsWith("42");
            if (!(b)) throw new Exception("Kết quả sai");
        }
    }
}
