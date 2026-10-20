// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Linq;
using System.Collections.Generic;

namespace TraCuuHam.Mau
{
    internal static class K_4_3f
    {
        internal static void Chay()
        {
            Dictionary<string, int> d = new Dictionary<string, int>();
            d["A"] = 1; d["B"] = 2;
            foreach (KeyValuePair<string, int> kv in d)
                Console.Write(kv.Key + "=" + kv.Value + " ");
            if (!(d.Count == 2)) throw new Exception("Kết quả sai");
        }
    }
}
