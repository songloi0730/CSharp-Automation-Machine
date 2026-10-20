// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Linq;
using System.Collections.Generic;

namespace TraCuuHam.Mau
{
    internal static class K_4_2n
    {
        internal static void Chay()
        {
            List<double> ds = new List<double> { 1.5, 2.5, -1.0 };
            bool loi = Co.Nem<InvalidOperationException>(() => { foreach (double v in ds) ds.Remove(v); });
            if (!(loi)) throw new Exception("Kết quả sai");
        }
    }
}
