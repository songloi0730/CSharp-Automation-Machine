// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Linq;

namespace TraCuuHam.Mau
{
    internal static class K_7_4b
    {
        internal static void Chay()
        {
            string thuMuc = Path.Combine(Path.GetTempPath(), "TraCuuHam"); Directory.CreateDirectory(thuMuc); string p = Path.Combine(thuMuc, "log.txt");
            var x = XElement.Parse("<A><B>1</B><B>2</B></A>");
            int n = x.Elements("B").Count();
            if (!(n == 2)) throw new Exception("Kết quả sai");
        }
    }
}
