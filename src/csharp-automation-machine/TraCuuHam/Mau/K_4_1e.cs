// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_4_1e
    {
        internal static void Chay()
        {
            int[] b = { 5, 2, 9 };
            Array.Resize(ref b, 5);
            if (!(b.Length == 5 && b[4] == 0)) throw new Exception("Kết quả sai");
        }
    }
}
