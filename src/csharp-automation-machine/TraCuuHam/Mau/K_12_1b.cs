// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Text;

namespace TraCuuHam.Mau
{
    internal static class K_12_1b
    {
        internal static void Chay()
        {
            string s = Encoding.ASCII.GetString(new byte[] { 0x4F, 0x4B });
            if (!(s == "OK")) throw new Exception("Kết quả sai");
        }
    }
}
