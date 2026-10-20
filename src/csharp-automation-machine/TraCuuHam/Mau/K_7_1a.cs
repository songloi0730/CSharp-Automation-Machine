// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace TraCuuHam.Mau
{
    internal static class K_7_1a
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string muc, string khoa, string macDinh,
            StringBuilder ketQua, int kichThuoc, string duongDan);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern bool WritePrivateProfileString(string muc, string khoa, string giaTri, string duongDan);

        static string DocIni(string tep, string muc, string khoa, string macDinh)
        {
            var sb = new StringBuilder(255);
            GetPrivateProfileString(muc, khoa, macDinh, sb, sb.Capacity, tep);
            return sb.ToString();
        }

        // Dùng:   WritePrivateProfileString("Truc", "TocDo", "50", @"D:\Cfg\may.ini");
        //         string v = DocIni(@"D:\Cfg\may.ini", "Truc", "TocDo", "10");
    }
}
