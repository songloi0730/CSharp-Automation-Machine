// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.IO;

namespace TraCuuHam.Mau
{
    internal static class K_13_1b
    {
        internal static void Chay()
        {
            try { throw new TimeoutException("Trục X quá 2000 ms"); }
            catch (Exception ex)
            {
                string ngan = ex.Message;      // chỉ câu thông báo
                string day = ex.ToString();    // loại lỗi + thông báo + dòng mã gây lỗi — ghi nhật ký cái này
                // throw;                      // ném tiếp, GIỮ nguyên vết lỗi gốc
                // throw ex;                   // KHÔNG nên: mất vết, nhìn như lỗi phát sinh ở đây
            }
        }
    }
}
