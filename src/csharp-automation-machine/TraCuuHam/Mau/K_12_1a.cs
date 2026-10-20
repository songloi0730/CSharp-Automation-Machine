// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Text;

namespace TraCuuHam.Mau
{
    internal static class K_12_1a
    {
        internal static void Chay()
        {
            byte[] b = Encoding.ASCII.GetBytes("AB");
            if (!(b[0] == 0x41 && b[1] == 0x42)) throw new Exception("Kết quả sai");
        }
    }
}
