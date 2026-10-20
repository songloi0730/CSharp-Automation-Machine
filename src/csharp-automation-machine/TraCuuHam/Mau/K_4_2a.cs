// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Linq;
using System.Collections.Generic;

namespace TraCuuHam.Mau
{
    internal static class K_4_2a
    {
        internal static void Chay()
        {
            List<double> ds = new List<double> { 1.5, 2.5, -1.0 };
            ds.Add(3.5);
            if (!(ds.Count == 4)) throw new Exception("Kết quả sai");
        }
    }
}
