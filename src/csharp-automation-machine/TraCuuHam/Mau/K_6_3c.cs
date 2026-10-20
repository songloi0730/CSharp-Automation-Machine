// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.IO;
using System.Text;

namespace TraCuuHam.Mau
{
    internal static class K_6_3c
    {
        internal static void Chay()
        {
            string thuMuc = Path.Combine(Path.GetTempPath(), "TraCuuHam"); Directory.CreateDirectory(thuMuc); string p = Path.Combine(thuMuc, "log.txt"); File.WriteAllText(p, "x");
            string q = p + ".moi"; if (File.Exists(q)) File.Delete(q);
            File.Move(p, q);
            if (!(File.Exists(q) && !File.Exists(p))) throw new Exception("Kết quả sai");
        }
    }
}
