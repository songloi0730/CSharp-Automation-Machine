// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.IO;
using System.Text;

namespace TraCuuHam.Mau
{
    internal static class K_6_4a
    {
        internal static void Chay()
        {
            string thuMuc = Path.Combine(Path.GetTempPath(), "TraCuuHam"); Directory.CreateDirectory(thuMuc); string p = Path.Combine(thuMuc, "log.txt"); File.WriteAllText(p, "x");
            Directory.CreateDirectory(Path.Combine(thuMuc, "2026", "09"));
            if (!(Directory.Exists(Path.Combine(thuMuc, "2026")))) throw new Exception("Kết quả sai");
        }
    }
}
