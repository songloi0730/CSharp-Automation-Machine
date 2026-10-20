// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Threading;

namespace TraCuuHam.Mau
{
    internal static class K_9_1c
    {
        internal static void Chay()
        {
            var t = new System.Threading.Timer(trangThai =>
            {
                // chạy trên luồng khác — muốn sửa giao diện phải qua Invoke/Dispatcher
            }, null, 0, 1000);                      // bắt đầu ngay, lặp mỗi 1000 ms
            t.Change(Timeout.Infinite, Timeout.Infinite);   // tạm dừng
            t.Dispose();
        }
    }
}
