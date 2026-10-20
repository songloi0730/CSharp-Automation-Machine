// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.IO;
using System.Text;

namespace TraCuuHam.Mau
{
    internal static class K_6_5b
    {
        internal static void Chay()
        {
            string p = @"D:\Data\2026\ketqua.csv";
            string ten = Path.GetFileName(p);
            if (!(ten == "ketqua.csv")) throw new Exception("Kết quả sai");
        }
    }
}
