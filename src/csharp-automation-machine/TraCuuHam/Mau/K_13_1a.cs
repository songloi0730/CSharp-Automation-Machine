// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.IO;

namespace TraCuuHam.Mau
{
    internal static class K_13_1a
    {
        internal static void Chay()
        {
            try
            {
                string s = File.ReadAllText(@"D:\khong_co.txt");
            }
            catch (FileNotFoundException ex)        // loại cụ thể trước
            {
                Console.WriteLine("Không thấy file: " + ex.FileName);
            }
            catch (IOException ex)                  // loại rộng hơn sau
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // luôn chạy, có lỗi hay không — đóng kết nối, nâng đầu đo...
            }
        }
    }
}
