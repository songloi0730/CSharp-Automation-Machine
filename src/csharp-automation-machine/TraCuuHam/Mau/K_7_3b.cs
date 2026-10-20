// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace TraCuuHam.Mau
{
    internal static class K_7_3b
    {
        internal static void Chay()
        {
            var cauHinh = new Dictionary<string, double> { { "TocDo", 50 } };
            string json = JsonSerializer.Serialize(cauHinh, new JsonSerializerOptions { WriteIndented = true });
            var doc = JsonSerializer.Deserialize<Dictionary<string, double>>(json);
            if (!(doc["TocDo"] == 50)) throw new Exception("Kết quả sai");
        }
    }
}
