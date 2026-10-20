// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam.Mau
{
    internal static class K_2_10d
    {
        internal static void Chay()
        {
            TrangThai t = TrangThai.DangChay;
            TrangThai x;
            bool ok = Enum.TryParse("Loi", out x);
            if (!(ok && x == TrangThai.Loi)) throw new Exception("Kết quả sai");
        }
    }
}
