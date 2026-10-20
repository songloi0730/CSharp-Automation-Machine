// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Net.Sockets;
using System.Text;

namespace TraCuuHam.Mau
{
    internal static class K_11_1a
    {
        internal static void Chay()
        {
            using (var client = new TcpClient())
            {
                client.ReceiveTimeout = 1000;              // ms; mặc định chờ vô hạn
                client.SendTimeout = 1000;
                if (!client.ConnectAsync("192.168.0.10", 5000).Wait(2000))
                    throw new TimeoutException("Không kết nối được trong 2 giây");

                NetworkStream ns = client.GetStream();
                byte[] lenh = Encoding.ASCII.GetBytes("TRIGGER\r\n");
                ns.Write(lenh, 0, lenh.Length);

                byte[] buf = new byte[1024];
                int n = ns.Read(buf, 0, buf.Length);      // 0 nghĩa là bên kia đã đóng kết nối
                string traLoi = Encoding.ASCII.GetString(buf, 0, n);
            }
        }
    }
}
