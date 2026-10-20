// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Net;
using System.Net.Sockets;

namespace TraCuuHam.Mau
{
    internal static class K_11_2a
    {
        internal static void Chay()
        {
            var server = new TcpListener(IPAddress.Any, 5000);
            server.Start();
            TcpClient khach = server.AcceptTcpClient();   // chặn tới khi có máy kết nối
            NetworkStream ns = khach.GetStream();
            // ... đọc/ghi như máy khách ...
            khach.Close();
            server.Stop();
        }
    }
}
