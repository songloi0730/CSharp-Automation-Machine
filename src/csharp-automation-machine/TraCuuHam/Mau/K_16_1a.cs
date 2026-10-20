// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Diagnostics;

namespace TraCuuHam.Mau
{
    internal static class K_16_1a
    {
        internal static void Chay()
        {
            Process.Start("notepad.exe", @"D:\Log\may.log");          // mở file bằng Notepad
            Process.Start("explorer.exe", @"D:\Data");                // mở thư mục
            bool dangChay = Process.GetProcessesByName("MeoFrame").Length > 0;   // tên không có ".exe"
        }
    }
}
