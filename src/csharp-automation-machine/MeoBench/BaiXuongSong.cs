// -------------------------------------------------------
// File:    BaiXuongSong.cs
// Project: MeoBench
// Purpose: Lời giải mẫu C# cho 20 bài xương sống của Phụ lục I.
//          Mỗi lớp đặt tên theo VIỆC NÓ LÀM TRONG MÁY, không theo tên bài —
//          vì đó mới là thứ bạn sẽ gõ lại khi viết phần mềm máy thật.
//          Số hiệu bài LeetCode ghi trong tài liệu XML của từng lớp.
// -------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace MeoBench;

// ══════════════════ NHÓM 1 — cửa sổ trượt ══════════════════

/// <summary>
/// <b>LeetCode 239 — Sliding Window Maximum.</b> Giá trị lớn nhất trong mỗi cửa sổ k mẫu,
/// bằng <b>hàng đợi đơn điệu</b>: O(n) cho cả dãy thay vì O(n·k).
/// Trong máy: "áp suất đỉnh trong 30 giây gần nhất" hỏi mỗi vòng quét.
/// </summary>
public static class DinhCuaSoTruot
{
    /// <summary>Trả về mảng max của từng cửa sổ độ rộng <paramref name="k"/>.</summary>
    public static double[] Tinh(double[] mau, int k)
    {
        ArgumentNullException.ThrowIfNull(mau);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(k);
        if (mau.Length < k) return [];

        var ketQua = new double[mau.Length - k + 1];
        var chiSo = new LinkedList<int>();          // giữ CHỈ SỐ, giảm dần theo giá trị

        for (int i = 0; i < mau.Length; i++)
        {
            // Bỏ phần tử đã trôi ra khỏi cửa sổ
            if (chiSo.Count > 0 && chiSo.First!.Value <= i - k) chiSo.RemoveFirst();

            // Mọi giá trị nhỏ hơn mẫu mới thì không bao giờ còn là max nữa
            while (chiSo.Count > 0 && mau[chiSo.Last!.Value] <= mau[i]) chiSo.RemoveLast();

            chiSo.AddLast(i);
            if (i >= k - 1) ketQua[i - k + 1] = mau[chiSo.First!.Value];
        }
        return ketQua;
    }
}

/// <summary>
/// <b>LeetCode 346 — Moving Average from Data Stream.</b> Trung bình trượt trên bộ đệm vòng.
/// Trong máy: làm mượt một kênh đo. Khác bản LeetCode ở chỗ <b>không giữ lịch sử</b> —
/// cộng dồn và trừ đi mẫu rơi ra, nên bộ nhớ cố định dù chạy nhiều tháng.
/// </summary>
public sealed class TrungBinhTruot
{
    private readonly double[] _dem;
    private int _viTri;
    private int _soMau;
    private double _tong;

    public TrungBinhTruot(int cuaSo)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cuaSo);
        _dem = new double[cuaSo];
    }

    /// <summary>Nạp một mẫu mới, trả về trung bình hiện tại.</summary>
    public double Nap(double giaTri)
    {
        if (_soMau == _dem.Length) _tong -= _dem[_viTri];
        else _soMau++;

        _dem[_viTri] = giaTri;
        _tong += giaTri;
        _viTri = (_viTri + 1) % _dem.Length;
        return _tong / _soMau;
    }

    /// <summary>Số mẫu đang có trong cửa sổ.</summary>
    public int SoMau => _soMau;
}

/// <summary>
/// <b>LeetCode 933 — Number of Recent Calls.</b> Đếm sự kiện trong cửa sổ thời gian trượt.
/// Trong máy: đây CHÍNH LÀ lõi của chống lũ cảnh báo ở mục 15.1.7 —
/// "mã lỗi này đã kêu mấy lần trong 60 giây qua".
/// </summary>
public sealed class DemSuKienGanDay
{
    private readonly Queue<long> _moc = new();
    private readonly long _cuaSoMs;

    public DemSuKienGanDay(long cuaSoMs)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cuaSoMs);
        _cuaSoMs = cuaSoMs;
    }

    /// <summary>Ghi nhận một sự kiện tại mốc <paramref name="tMs"/>, trả về số sự kiện trong cửa sổ.</summary>
    public int Ghi(long tMs)
    {
        _moc.Enqueue(tMs);
        while (_moc.Count > 0 && _moc.Peek() < tMs - _cuaSoMs) _moc.Dequeue();
        return _moc.Count;
    }
}

/// <summary>
/// <b>LeetCode 1438 — Longest Continuous Subarray With Absolute Diff ≤ Limit.</b>
/// Hai hàng đợi đơn điệu cùng lúc: một giữ max, một giữ min.
/// Trong máy: "đoạn dài nhất mà biên độ dao động không vượt ngưỡng" — phát hiện rung,
/// và cũng là cách đo xem một đại lượng đã <i>ổn định</i> đủ lâu chưa trước khi lấy mẫu.
/// </summary>
public static class DoanOnDinhDaiNhat
{
    /// <summary>Độ dài đoạn liên tiếp dài nhất có (max − min) ≤ <paramref name="bienDo"/>.</summary>
    public static int Tinh(double[] mau, double bienDo)
    {
        ArgumentNullException.ThrowIfNull(mau);
        var qMax = new LinkedList<int>();
        var qMin = new LinkedList<int>();
        int trai = 0, dai = 0;

        for (int phai = 0; phai < mau.Length; phai++)
        {
            while (qMax.Count > 0 && mau[qMax.Last!.Value] <= mau[phai]) qMax.RemoveLast();
            while (qMin.Count > 0 && mau[qMin.Last!.Value] >= mau[phai]) qMin.RemoveLast();
            qMax.AddLast(phai);
            qMin.AddLast(phai);

            while (mau[qMax.First!.Value] - mau[qMin.First!.Value] > bienDo)
            {
                if (qMax.First!.Value == trai) qMax.RemoveFirst();
                if (qMin.First!.Value == trai) qMin.RemoveFirst();
                trai++;
            }
            dai = Math.Max(dai, phai - trai + 1);
        }
        return dai;
    }
}

// ══════════════════ NHÓM 2 — ngăn xếp đơn điệu ══════════════════

/// <summary>
/// <b>LeetCode 155 — Min Stack.</b> Lấy min trong O(1) bằng cách mỗi phần tử tự nhớ min tại thời điểm nó vào.
/// Trong máy: đáy áp suất của phiên hiện tại, bỏ được theo từng lớp khi lùi trạng thái.
/// </summary>
public sealed class NganXepCoDay
{
    private readonly Stack<(double GiaTri, double Day)> _s = new();

    public void Day(double giaTri)
        => _s.Push((giaTri, _s.Count == 0 ? giaTri : Math.Min(giaTri, _s.Peek().Day)));

    public double Lay() => _s.Pop().GiaTri;
    public double Dinh => _s.Peek().GiaTri;

    /// <summary>Giá trị nhỏ nhất đang có — O(1).</summary>
    public double CucTieu => _s.Peek().Day;
    public int SoPhanTu => _s.Count;
}

/// <summary>
/// <b>LeetCode 901 — Online Stock Span.</b> Ngăn xếp đơn điệu chạy trên một DÒNG, không phải mảng.
/// Trong máy: "giá trị hiện tại đã là mức cao nhất trong bao nhiêu mẫu liên tiếp" —
/// độ dài một xu hướng, dùng để phát hiện trôi dần (drift) trước khi chạm ngưỡng.
/// </summary>
public sealed class DoDaiXuHuong
{
    private readonly Stack<(double GiaTri, int Dai)> _s = new();

    /// <summary>Nạp mẫu mới, trả về số mẫu liên tiếp (kể cả mẫu này) không lớn hơn nó.</summary>
    public int Nap(double giaTri)
    {
        int dai = 1;
        while (_s.Count > 0 && _s.Peek().GiaTri <= giaTri) dai += _s.Pop().Dai;
        _s.Push((giaTri, dai));
        return dai;
    }
}

// ══════════════════ NHÓM 3 — khoảng thời gian ══════════════════

/// <summary>Một khoảng thời gian nửa mở [Bắt đầu, Kết thúc).</summary>
public readonly record struct Khoang(long BatDau, long KetThuc)
{
    public bool ChongLan(Khoang k) => BatDau < k.KetThuc && k.BatDau < KetThuc;
}

/// <summary>
/// <b>LeetCode 56 — Merge Intervals</b> và <b>253 — Meeting Rooms II</b>.
/// Trong máy: gộp cửa sổ bảo trì (56) và "cần bao nhiêu trạm song song mới kịp" (253).
/// </summary>
public static class LichKhoang
{
    /// <summary>Gộp các khoảng chồng lấn, trả về danh sách đã sắp và rời nhau.</summary>
    public static List<Khoang> Gop(IEnumerable<Khoang> nguon)
    {
        ArgumentNullException.ThrowIfNull(nguon);
        var ds = nguon.ToList();
        ds.Sort((a, b) => a.BatDau.CompareTo(b.BatDau));

        var kq = new List<Khoang>();
        foreach (var k in ds)
        {
            if (kq.Count > 0 && k.BatDau <= kq[^1].KetThuc)
                kq[^1] = new Khoang(kq[^1].BatDau, Math.Max(kq[^1].KetThuc, k.KetThuc));
            else
                kq.Add(k);
        }
        return kq;
    }

    /// <summary>
    /// Số tài nguyên song song tối thiểu để chứa hết các khoảng — quét đường (sweep line).
    /// Trong máy: số đồ gá, số trạm, hay số vị trí đệm cần có.
    /// </summary>
    public static int SoTaiNguyenCanThiet(IEnumerable<Khoang> nguon)
    {
        ArgumentNullException.ThrowIfNull(nguon);
        var moc = new List<(long T, int Delta)>();
        foreach (var k in nguon)
        {
            moc.Add((k.BatDau, +1));
            moc.Add((k.KetThuc, -1));
        }
        // Kết thúc xử lý TRƯỚC bắt đầu tại cùng một mốc: [0;5) và [5;9) KHÔNG chồng nhau.
        moc.Sort((a, b) => a.T != b.T ? a.T.CompareTo(b.T) : a.Delta.CompareTo(b.Delta));

        int dang = 0, dinh = 0;
        foreach (var (_, d) in moc)
        {
            dang += d;
            if (dang > dinh) dinh = dang;
        }
        return dinh;
    }
}

/// <summary>
/// <b>LeetCode 1094 — Car Pooling.</b> Mảng hiệu (difference array): cộng dồn trên khoảng
/// trong O(1) mỗi lần, chỉ trả giá O(n) một lần khi cần đọc.
/// Trong máy: tải của băng tải/khay theo thời gian, hay kế hoạch sản lượng theo lô.
/// </summary>
public static class TaiTheoKhoang
{
    /// <summary>Có lúc nào tổng tải vượt <paramref name="sucChua"/> không.</summary>
    public static bool CoQuaTai(IEnumerable<(Khoang K, int Tai)> nguon, int sucChua, int mocToiDa)
    {
        ArgumentNullException.ThrowIfNull(nguon);
        var hieu = new int[mocToiDa + 2];
        foreach (var (k, tai) in nguon)
        {
            hieu[k.BatDau] += tai;
            hieu[k.KetThuc] -= tai;
        }
        int dang = 0;
        foreach (int d in hieu)
        {
            dang += d;
            if (dang > sucChua) return true;
        }
        return false;
    }
}

// ══════════════════ NHÓM 4 — thiết kế cấu trúc ══════════════════

/// <summary>
/// <b>LeetCode 622 — Design Circular Queue.</b> Bộ đệm vòng dung lượng cố định.
/// Trong máy: đây là cấu trúc nền của mọi vùng đệm dữ liệu thời gian thực — nó
/// <b>không bao giờ cấp phát thêm</b>, nên không gây khựng vì thu gom rác (mục 3.1.2).
/// </summary>
public sealed class HangDoiVong<T>
{
    private readonly T[] _o;
    private int _dau, _soPhanTu;

    public HangDoiVong(int sucChua)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sucChua);
        _o = new T[sucChua];
    }

    public int SoPhanTu => _soPhanTu;
    public bool Day => _soPhanTu == _o.Length;
    public bool Rong => _soPhanTu == 0;

    /// <summary>Thêm vào cuối. Trả về false nếu đầy — KHÔNG ném, vì đầy là chuyện bình thường.</summary>
    public bool ThemCuoi(T x)
    {
        if (Day) return false;
        _o[(_dau + _soPhanTu) % _o.Length] = x;
        _soPhanTu++;
        return true;
    }

    /// <summary>Lấy khỏi đầu.</summary>
    public bool LayDau([MaybeNullWhen(false)] out T x)
    {
        if (Rong) { x = default; return false; }
        x = _o[_dau];
        _o[_dau] = default!;                 // nhả tham chiếu, tránh giữ đối tượng sống
        _dau = (_dau + 1) % _o.Length;
        _soPhanTu--;
        return true;
    }
}

/// <summary>
/// <b>LeetCode 146 — LRU Cache.</b> Dictionary + danh sách liên kết đôi, mọi thao tác O(1).
/// Trong máy: bộ nhớ đệm ảnh kiểm hoặc công thức — giữ thứ vừa dùng, bỏ thứ lâu không đụng.
/// </summary>
public sealed class DemLruCache<TKhoa, TGiaTri> where TKhoa : notnull
{
    private readonly int _sucChua;
    private readonly Dictionary<TKhoa, LinkedListNode<(TKhoa K, TGiaTri V)>> _bang;
    private readonly LinkedList<(TKhoa K, TGiaTri V)> _thuTu = new();

    public DemLruCache(int sucChua)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sucChua);
        _sucChua = sucChua;
        _bang = new Dictionary<TKhoa, LinkedListNode<(TKhoa, TGiaTri)>>(sucChua);
    }

    public bool ThuLay(TKhoa khoa, [MaybeNullWhen(false)] out TGiaTri giaTri)
    {
        if (!_bang.TryGetValue(khoa, out var nut)) { giaTri = default; return false; }
        _thuTu.Remove(nut);
        _thuTu.AddFirst(nut);
        giaTri = nut.Value.V;
        return true;
    }

    public void Dat(TKhoa khoa, TGiaTri giaTri)
    {
        if (_bang.TryGetValue(khoa, out var cu))
        {
            _thuTu.Remove(cu);
            _bang.Remove(khoa);
        }
        else if (_bang.Count == _sucChua)
        {
            var bo = _thuTu.Last!;
            _thuTu.RemoveLast();
            _bang.Remove(bo.Value.K);
        }
        var nut = new LinkedListNode<(TKhoa, TGiaTri)>((khoa, giaTri));
        _thuTu.AddFirst(nut);
        _bang[khoa] = nut;
    }

    public int SoPhanTu => _bang.Count;
}

/// <summary>
/// <b>LeetCode 359 — Logger Rate Limiter.</b> Chặn lặp cùng một thông điệp trong cửa sổ thời gian.
/// Trong máy: đúng cơ chế chống lũ cảnh báo của mục 15.1.7 — và khác bản LeetCode một điểm
/// quan trọng: bản máy phải <b>đếm số lần bị chặn</b> để còn báo "đã kêu 412 lần trong 5 phút".
/// </summary>
public sealed class ChanLapTheoThoiGian
{
    private readonly Dictionary<string, long> _lanCuoi = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _soLanChan = new(StringComparer.Ordinal);
    private readonly long _cuaSoMs;

    public ChanLapTheoThoiGian(long cuaSoMs)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cuaSoMs);
        _cuaSoMs = cuaSoMs;
    }

    /// <summary>Có cho phép phát thông điệp này tại mốc <paramref name="tMs"/> không.</summary>
    public bool ChoPhep(string thongDiep, long tMs)
    {
        ArgumentNullException.ThrowIfNull(thongDiep);
        if (_lanCuoi.TryGetValue(thongDiep, out long cuoi) && tMs < cuoi + _cuaSoMs)
        {
            _soLanChan[thongDiep] = _soLanChan.GetValueOrDefault(thongDiep) + 1;
            return false;
        }
        _lanCuoi[thongDiep] = tMs;
        return true;
    }

    /// <summary>Số lần thông điệp này đã bị nuốt — thứ bản LeetCode không cần mà máy thì cần.</summary>
    public int SoLanBiChan(string thongDiep) => _soLanChan.GetValueOrDefault(thongDiep);
}
