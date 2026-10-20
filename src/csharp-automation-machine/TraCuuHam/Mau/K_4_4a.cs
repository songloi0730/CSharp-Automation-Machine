// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace TraCuuHam.Mau
{
    internal static class K_4_4a
    {
        internal static void Chay()
        {
            var q = new Queue<string>();
            q.Enqueue("P1"); q.Enqueue("P2");
            string dau = q.Dequeue();
            if (!(dau == "P1" && q.Count == 1)) throw new Exception("Kết quả sai");
        }
    }
}
