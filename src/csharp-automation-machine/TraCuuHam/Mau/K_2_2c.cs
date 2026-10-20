// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Globalization;

namespace TraCuuHam.Mau
{
    internal static class K_2_2c
    {
        internal static void Chay()
        {
            double d;
            bool ok = double.TryParse("12.5", NumberStyles.Float, CultureInfo.InvariantCulture, out d);
            if (!(ok && d == 12.5)) throw new Exception("Kết quả sai");
        }
    }
}
