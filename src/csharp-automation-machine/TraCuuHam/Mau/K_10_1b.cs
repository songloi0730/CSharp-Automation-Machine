// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.IO.Ports;
using System.Text;

namespace TraCuuHam.Mau
{
    internal static class K_10_1b
    {
        internal static void Chay()
        {
            var port = new SerialPort("COM3", 9600);
            var boDem = new StringBuilder();
            port.DataReceived += (s, e) =>
            {
                boDem.Append(port.ReadExisting());   // chỉ gom vào bộ đệm, tách khung ở nơi khác
            };
            // Khi đóng: huỷ đăng ký trước rồi mới Close
            // port.DataReceived -= ...; port.Close();
        }
    }
}
