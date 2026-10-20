// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Linq;
using System.Collections.Generic;

namespace TraCuuHam.Mau
{
    internal static class K_4_3e
    {
        internal static void Chay()
        {
            Dictionary<string, int> d = new Dictionary<string, int>();
            bool loi = Co.Nem<KeyNotFoundException>(() => { int x = d["X"]; });
            if (!(loi)) throw new Exception("Kết quả sai");
        }
    }
}
