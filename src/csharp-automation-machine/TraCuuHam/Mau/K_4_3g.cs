// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Linq;
using System.Collections.Generic;

namespace TraCuuHam.Mau
{
    internal static class K_4_3g
    {
        internal static void Chay()
        {
            Dictionary<string, int> d = new Dictionary<string, int>();
            d["A"] = 1;
            bool xoa = d.Remove("A");
            if (!(xoa && d.Count == 0)) throw new Exception("Kết quả sai");
        }
    }
}
