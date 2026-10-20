// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_2_9c
    {
        internal static void Chay()
        {
            Random r = new Random(12345);
            bool giong = new Random(5).Next() == new Random(5).Next();
            if (!(giong)) throw new Exception("Kết quả sai");
        }
    }
}
