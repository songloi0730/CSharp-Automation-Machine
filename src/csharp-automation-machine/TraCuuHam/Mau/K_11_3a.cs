// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Net.NetworkInformation;

namespace TraCuuHam.Mau
{
    internal static class K_11_3a
    {
        internal static void Chay()
        {
            using (var ping = new Ping())
            {
                PingReply r = ping.Send("192.168.0.10", 1000);   // hạn giờ 1000 ms
                bool songSot = r.Status == IPStatus.Success;
                long tre = r.RoundtripTime;                      // ms
            }
        }
    }
}
