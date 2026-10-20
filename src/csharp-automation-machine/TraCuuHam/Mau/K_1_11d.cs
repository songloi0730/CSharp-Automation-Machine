// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_1_11d
    {
        internal static void Chay()
        {
            string t = "Dong 1\r\nDong 2\tTab \"ngoac\"";
            if (!(t.Contains("\"ngoac\""))) throw new Exception("Kết quả sai");
        }
    }
}
