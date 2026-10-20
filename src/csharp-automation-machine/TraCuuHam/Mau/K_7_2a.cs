// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.IO;
using System.Text;
using System.Globalization;

namespace TraCuuHam.Mau
{
    internal static class K_7_2a
    {
        internal static void Chay()
        {
            string thuMuc = Path.Combine(Path.GetTempPath(), "TraCuuHam"); Directory.CreateDirectory(thuMuc); string p = Path.Combine(thuMuc, "log.txt");
            double day = 2.0042;
            string dong = string.Join(",", "2026-09-30T08:00:00", "42",
                day.ToString("F4", CultureInfo.InvariantCulture), "Dat");
            File.AppendAllText(p, dong + Environment.NewLine, Encoding.UTF8);

            // Đọc lại
            string[] o = File.ReadAllLines(p)[0].Split(',');
            double doc = double.Parse(o[2], CultureInfo.InvariantCulture);
            if (!(doc == 2.0042)) throw new Exception("Kết quả sai");
        }
    }
}
