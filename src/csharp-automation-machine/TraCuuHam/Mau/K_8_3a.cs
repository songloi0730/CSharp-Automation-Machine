// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Threading;

namespace TraCuuHam.Mau
{
    internal static class K_8_3a
    {
        static readonly object _khoa = new object();
        static int _demPhoi;
        static volatile bool _dungLai;          // cờ dừng đọc từ luồng khác

        static void TangDem()
        {
            lock (_khoa)                       // chỉ một luồng vào một lúc
            {
                _demPhoi++;
            }
            Interlocked.Increment(ref _demPhoi);  // cộng 1 an toàn, không cần lock
        }

        static bool ThuKhoa()
        {
            if (!Monitor.TryEnter(_khoa, 500)) return false;   // chờ khoá tối đa 500 ms
            try { _demPhoi = 0; return true; }
            finally { Monitor.Exit(_khoa); }
        }
    }
}
