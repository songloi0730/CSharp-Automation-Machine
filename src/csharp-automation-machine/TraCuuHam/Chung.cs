// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;

namespace TraCuuHam
{
    /// <summary>Kiểu dùng chung cho các đoạn mẫu.</summary>
    public enum TrangThai { ChuaChay, DangChay, Loi }

    /// <summary>Một phôi đã đo — dùng cho các ví dụ LINQ.</summary>
    public class Phoi
    {
        public int So;
        public double Day;
        public bool Dat;
        public Phoi(int so, double day, bool dat) { So = so; Day = day; Dat = dat; }
    }

    /// <summary>Tiện ích cho phép kiểm.</summary>
    public static class Co
    {
        /// <summary>Đúng nếu hành động ném đúng loại lỗi.</summary>
        public static bool Nem<T>(Action a) where T : Exception
        {
            try { a(); return false; }
            catch (T) { return true; }
        }
    }
}
