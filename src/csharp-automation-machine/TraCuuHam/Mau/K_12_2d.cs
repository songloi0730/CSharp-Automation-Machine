// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_12_2d
    {
        internal static void Chay()
        {
            byte[] b = BitConverter.GetBytes(12.5f);
            float f = BitConverter.ToSingle(b, 0);
            if (!(b.Length == 4 && f == 12.5f)) throw new Exception("Kết quả sai");
        }
    }
}
