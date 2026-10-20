// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace TraCuuHam.Mau
{
    internal static class K_8_1b
    {
        internal static async Task ChayAsync()
        {
            await Task.Delay(100);
            await Task.Delay(TimeSpan.FromSeconds(1));
            await Task.Delay(100, CancellationToken.None);   // có token: dừng được giữa chừng
        }
    }
}
