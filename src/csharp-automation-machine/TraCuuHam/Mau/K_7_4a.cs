// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Linq;

namespace TraCuuHam.Mau
{
    internal static class K_7_4a
    {
        internal static void Chay()
        {
            string thuMuc = Path.Combine(Path.GetTempPath(), "TraCuuHam"); Directory.CreateDirectory(thuMuc); string p = Path.Combine(thuMuc, "log.txt");
            var may = new XElement("May",
                new XElement("Ten", "MAY-01"),
                new XElement("Truc", new XAttribute("ten", "X"), new XElement("TocDo", 50)));
            may.Save(p);

            XDocument doc = XDocument.Load(p);
            string ten = doc.Root.Element("Ten").Value;                  // "MAY-01"
            XElement truc = doc.Root.Element("Truc");
            string tenTruc = truc.Attribute("ten").Value;                 // "X"
            double tocDo = (double)truc.Element("TocDo");                 // ép kiểu thẳng ra số
            if (!(ten == "MAY-01" && tenTruc == "X" && tocDo == 50)) throw new Exception("Kết quả sai");
        }
    }
}
