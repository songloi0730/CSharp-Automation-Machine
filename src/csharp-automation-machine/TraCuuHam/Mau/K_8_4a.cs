// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TraCuuHam.Mau
{
    internal static class K_8_4a
    {
        internal static void Chay()
        {
            var cts = new CancellationTokenSource();
            CancellationToken ct = cts.Token;
            cts.CancelAfter(1000);                 // tự huỷ sau 1 giây (FW 4.5+)
            bool daHuy = ct.IsCancellationRequested;
            cts.Cancel();                          // huỷ ngay — ví dụ khi bấm nút Dừng
            bool loi = Co.Nem<OperationCanceledException>(() => ct.ThrowIfCancellationRequested());
            cts.Dispose();
            if (!(loi)) throw new Exception("Kết quả sai");
        }
    }
}
