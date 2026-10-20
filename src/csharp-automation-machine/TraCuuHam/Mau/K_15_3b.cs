// Đoạn mẫu của Phụ lục K, tự sinh từ dữ liệu của phụ lục — đừng sửa tay. Tên file = số dòng: K_1_2a là K.1, việc 2, dòng a.
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TraCuuHam.Mau.K_15_3b
{
    public class TrangThaiTruc : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        double _viTri;
        public double ViTri
        {
            get { return _viTri; }
            set { _viTri = value; BaoDoi(); }
        }

        void BaoDoi([CallerMemberName] string ten = null)
        {
            var h = PropertyChanged;
            if (h != null) h(this, new PropertyChangedEventArgs(ten));
        }
    }
}
