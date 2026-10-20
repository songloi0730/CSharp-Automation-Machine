// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.IO;
using System.Text;

namespace TraCuuHam.Mau
{
    internal static class K_6_6b
    {
        internal static void Chay()
        {
            string chung = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            if (!(chung.Length > 0)) throw new Exception("Kết quả sai");
        }
    }
}
