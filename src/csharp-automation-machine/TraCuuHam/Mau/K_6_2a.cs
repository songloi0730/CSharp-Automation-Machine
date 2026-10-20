// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.IO;
using System.Text;

namespace TraCuuHam.Mau
{
    internal static class K_6_2a
    {
        internal static void Chay()
        {
            string thuMuc = Path.Combine(Path.GetTempPath(), "TraCuuHam"); Directory.CreateDirectory(thuMuc); string p = Path.Combine(thuMuc, "log.txt");
            using (StreamWriter w = new StreamWriter(p, true, Encoding.UTF8))   // true = ghi thêm
            {
                w.WriteLine("Dong 1");
                w.WriteLine("Dong 2");
            }                                   // ra khỏi khối using là file được đóng

            using (StreamReader r = new StreamReader(p, Encoding.UTF8))
            {
                string dong;
                while ((dong = r.ReadLine()) != null)
                    Console.WriteLine(dong);
            }
            if (!(File.ReadAllText(p).Contains("Dong 2"))) throw new Exception("Kết quả sai");
        }
    }
}
