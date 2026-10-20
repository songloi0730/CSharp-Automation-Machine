// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_9_1d
    {
        internal static void Chay()
        {
            var t = new System.Timers.Timer(1000);    // mili-giây, kiểu double
            t.Elapsed += (s, e) => { /* luồng khác */ };
            t.AutoReset = true;                        // false = chỉ chạy một lần
            t.Start();
            t.Stop();
        }
    }
}
