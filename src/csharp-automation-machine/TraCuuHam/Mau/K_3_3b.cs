// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Globalization;

namespace TraCuuHam.Mau
{
    internal static class K_3_3b
    {
        internal static void Chay()
        {
            DateTime t;
            bool ok = DateTime.TryParseExact("30/09/2026", "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out t);
            if (!(ok && t.Day == 30)) throw new Exception("Kết quả sai");
        }
    }
}
