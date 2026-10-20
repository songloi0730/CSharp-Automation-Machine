// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Linq;
using System.Collections.Generic;

namespace TraCuuHam.Mau
{
    internal static class K_5_2d
    {
        internal static void Chay()
        {
            var ds = new List<Phoi> { new Phoi(1, 2.01, true), new Phoi(2, 2.12, false), new Phoi(3, 1.99, true) };
            Phoi day = ds.MaxBy(p => p.Day);
            if (!(day.So == 2)) throw new Exception("Kết quả sai");
        }
    }
}
