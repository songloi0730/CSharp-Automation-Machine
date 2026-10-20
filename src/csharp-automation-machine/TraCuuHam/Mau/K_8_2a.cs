// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TraCuuHam.Mau
{
    internal static class K_8_2a
    {
        internal static void Chay()
        {
            Thread t = new Thread(() =>
            {
                // vòng lặp trình tự
            });
            t.IsBackground = true;   // tắt chương trình thì luồng cũng dừng
            t.Start();
            bool xong = t.Join(1000);  // chờ tối đa 1 giây cho luồng kết thúc
        }
    }
}
