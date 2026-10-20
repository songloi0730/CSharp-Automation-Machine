// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_12_2e
    {
        internal static void Chay()
        {
            bool le = BitConverter.IsLittleEndian;
            if (!(le)) throw new Exception("Kết quả sai");
        }
    }
}
