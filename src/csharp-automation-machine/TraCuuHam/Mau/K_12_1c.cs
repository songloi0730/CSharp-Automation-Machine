// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Text;

namespace TraCuuHam.Mau
{
    internal static class K_12_1c
    {
        internal static void Chay()
        {
            byte[] b = Encoding.UTF8.GetBytes("Trục");
            if (!(b.Length == 6 && Encoding.ASCII.GetString(Encoding.ASCII.GetBytes("ụ")) == "?")) throw new Exception("Kết quả sai");
        }
    }
}
