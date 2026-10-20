// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Diagnostics;
using System.Threading;

namespace TraCuuHam.Mau
{
    internal static class K_3_6a
    {
        internal static void Chay()
        {
            Stopwatch sw = Stopwatch.StartNew();
            Thread.Sleep(20);
            long ms = sw.ElapsedMilliseconds;   // khoảng 20
            sw.Restart();                       // về 0 và chạy tiếp
            TimeSpan d = sw.Elapsed;            // dạng TimeSpan
            sw.Stop();
            if (!(ms >= 15)) throw new Exception("Kết quả sai");
        }
    }
}
