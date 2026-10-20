// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace TraCuuHam.Mau
{
    internal static class K_4_4b
    {
        internal static void Chay()
        {
            var st = new Stack<int>();
            st.Push(1); st.Push(2);
            int tren = st.Pop();
            if (!(tren == 2)) throw new Exception("Kết quả sai");
        }
    }
}
