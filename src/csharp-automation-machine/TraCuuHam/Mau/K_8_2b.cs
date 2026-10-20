// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TraCuuHam.Mau
{
    internal static class K_8_2b
    {
        internal static void Chay()
        {
            Task viec = Task.Run(() => Console.WriteLine("chạy nền"));
            viec.Wait();
            Task<int> tinh = Task.Run(() => 6 * 7);
            int kq = tinh.Result;     // chặn tới khi có kết quả — không gọi trên luồng giao diện
            if (!(kq == 42)) throw new Exception("Kết quả sai");
        }
    }
}
