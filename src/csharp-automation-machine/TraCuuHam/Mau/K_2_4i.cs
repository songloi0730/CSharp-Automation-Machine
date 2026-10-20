// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Globalization;

namespace TraCuuHam.Mau
{
    internal static class K_2_4i
    {
        internal static void Chay()
        {
            double v = 1234.5678; int n = 255;
            string c = Math.Round(2.5, 0, MidpointRounding.AwayFromZero).ToString("F0", CultureInfo.InvariantCulture);
            if (!(c == "3" && Math.Round(1.005, 2, MidpointRounding.AwayFromZero).ToString("F2", CultureInfo.InvariantCulture) == "1.00")) throw new Exception("Kết quả sai");
        }
    }
}
