// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TraCuuHam.Mau
{
    internal static class K_7_3a
    {
        internal static void Chay()
        {
            var cauHinh = new Dictionary<string, double> { { "TocDo", 50 }, { "GiaToc", 200 } };
            string json = JsonConvert.SerializeObject(cauHinh, Formatting.Indented);
            var doc = JsonConvert.DeserializeObject<Dictionary<string, double>>(json);
            if (!(doc["GiaToc"] == 200)) throw new Exception("Kết quả sai");
        }
    }
}
