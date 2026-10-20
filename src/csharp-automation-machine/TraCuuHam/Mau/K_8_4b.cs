// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TraCuuHam.Mau
{
    internal static class K_8_4b
    {
        internal static void Chay()
        {
            var coPhoi = new ManualResetEvent(false);
            // luồng cảm biến:   coPhoi.Set();
            bool kipGio = coPhoi.WaitOne(500);     // chờ tối đa 500 ms; False nếu hết giờ
            coPhoi.Reset();                        // về trạng thái chưa có

            var moiLan = new AutoResetEvent(false); // tự Reset sau mỗi lần WaitOne thành công
            if (!(!kipGio)) throw new Exception("Kết quả sai");
        }
    }
}
