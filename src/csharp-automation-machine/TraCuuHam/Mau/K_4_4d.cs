// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace TraCuuHam.Mau
{
    internal static class K_4_4d
    {
        internal static void Chay()
        {
            var cq = new ConcurrentQueue<string>();
            cq.Enqueue("A");
            string x;
            bool lay = cq.TryDequeue(out x);
            if (!(lay && x == "A")) throw new Exception("Kết quả sai");
        }
    }
}
