// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.IO.Ports;
using System.Text;

namespace TraCuuHam.Mau
{
    internal static class K_10_1a
    {
        internal static void Chay()
        {
            string[] cong = SerialPort.GetPortNames();      // { "COM1", "COM3", ... }

            var port = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
            port.ReadTimeout = 500;          // mặc định là chờ VÔ HẠN — luôn đặt
            port.WriteTimeout = 500;
            port.NewLine = "\r";             // ký tự kết thúc dòng của thiết bị
            port.Open();

            port.WriteLine("READ");           // gửi kèm NewLine
            string traLoi = port.ReadLine();  // đọc tới NewLine; quá ReadTimeout thì ném TimeoutException

            byte[] lenh = { 0x02, 0x31, 0x03 };
            port.Write(lenh, 0, lenh.Length); // gửi byte
            int co = port.BytesToRead;        // số byte đang chờ đọc
            byte[] buf = new byte[co];
            int n = port.Read(buf, 0, co);
            string tatCa = port.ReadExisting(); // lấy mọi thứ đang có, có thể là nửa khung

            port.Close();
        }
    }
}
