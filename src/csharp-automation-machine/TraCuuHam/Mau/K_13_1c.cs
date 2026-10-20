// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.IO;

namespace TraCuuHam.Mau
{
    internal static class K_13_1c
    {
        internal static void Chay()
        {
            try { int.Parse("x"); }
            catch (FormatException ex) when (ex.Message.Length > 0)
            {
                // chỉ bắt khi điều kiện đúng — C# 6 trở lên (mục 5.2)
            }
        }
    }
}
