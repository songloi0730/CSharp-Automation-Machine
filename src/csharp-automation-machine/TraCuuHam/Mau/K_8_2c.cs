// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace TraCuuHam.Mau
{
    internal static class K_8_2c
    {
        internal static async Task ChayAsync()
        {
            Task a = Task.Delay(50), b = Task.Delay(80);
            await Task.WhenAll(a, b);                         // chờ cả hai xong
            Task xongTruoc = await Task.WhenAny(a, Task.Delay(1000));
            bool kipGio = xongTruoc == a;                     // tự đo hạn giờ (mục 5.1.2)
        }
    }
}
