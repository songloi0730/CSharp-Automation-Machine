// -------------------------------------------------------
// File:    BaiXuongSong2.cs
// Project: MeoBench
// Purpose: Lời giải mẫu C# cho 20 bài xương sống của Phụ lục I (phần 2/2):
//          phân tích chuỗi, thao tác bit, ma trận, đồ thị, tìm nhị phân, lập lịch.
// -------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

namespace MeoBench;

// ══════════════════ NHÓM 5 — phân tích chuỗi, máy trạng thái ══════════════════

/// <summary>
/// <b>LeetCode 20 — Valid Parentheses.</b> Khớp cặp bằng ngăn xếp.
/// Trong máy: nền của mọi bộ phân tích khung có lồng nhau — và là cách kiểm nhanh
/// một tệp cấu hình JSON/XML có bị cắt cụt giữa chừng không.
/// </summary>
public static class KhopCapLongNhau
{
    private static readonly Dictionary<char, char> Cap =
        new() { [')'] = '(', [']'] = '[', ['}'] = '{' };

    public static bool HopLe(string s)
    {
        ArgumentNullException.ThrowIfNull(s);
        var xep = new Stack<char>();
        foreach (char c in s)
        {
            if (c is '(' or '[' or '{') xep.Push(c);
            else if (Cap.TryGetValue(c, out char mo))
            {
                if (xep.Count == 0 || xep.Pop() != mo) return false;
            }
        }
        return xep.Count == 0;
    }
}

/// <summary>
/// <b>LeetCode 65 — Valid Number.</b> Bài dạy máy trạng thái rõ nhất của LeetCode.
/// Viết bằng <b>bảng chuyển trạng thái</b> chứ không bằng rừng <c>if</c> — đúng lý do
/// mục 12.3.2 nêu: bảng thì đếm được, test được, và nhìn ra được chỗ thiếu.
/// Trong máy: kiểm một trường số đọc về từ thiết bị trước khi đem đi tính.
/// </summary>
public static class SoHopLe
{
    private enum T { Dau, Dau_, So, Cham, ChamSo, E, EDau, ESo, Loi }

    private static T Chuyen(T t, char c)
    {
        bool so = c is >= '0' and <= '9';
        return t switch
        {
            T.Dau      => c is '+' or '-' ? T.Dau_ : so ? T.So : c == '.' ? T.Cham : T.Loi,
            T.Dau_     => so ? T.So : c == '.' ? T.Cham : T.Loi,
            T.So       => so ? T.So : c == '.' ? T.ChamSo : c is 'e' or 'E' ? T.E : T.Loi,
            T.Cham     => so ? T.ChamSo : T.Loi,
            T.ChamSo   => so ? T.ChamSo : c is 'e' or 'E' ? T.E : T.Loi,
            T.E        => c is '+' or '-' ? T.EDau : so ? T.ESo : T.Loi,
            T.EDau     => so ? T.ESo : T.Loi,
            T.ESo      => so ? T.ESo : T.Loi,
            _          => T.Loi,
        };
    }

    /// <summary>Chuỗi có phải một số hợp lệ không.</summary>
    public static bool KiemTra(string s)
    {
        ArgumentNullException.ThrowIfNull(s);
        if (s.Length == 0) return false;

        var t = T.Dau;
        foreach (char c in s)
        {
            t = Chuyen(t, c);
            if (t == T.Loi) return false;
        }
        // Chỉ ba trạng thái này là kết thúc hợp lệ — thiếu dòng này là lỗi hay gặp nhất.
        return t is T.So or T.ChamSo or T.ESo;
    }
}

// ══════════════════ NHÓM 6 — thao tác bit ══════════════════

/// <summary>
/// <b>LeetCode 191 — Number of 1 Bits</b>, <b>136 — Single Number</b>, <b>89 — Gray Code</b>.
/// Ba bài bit mà phần mềm máy dùng thật: đếm tín hiệu đang tích cực, checksum XOR (mục G.2.4),
/// và mã Gray — thứ encoder tuyệt đối dùng để không bao giờ đọc nhầm quá một bit khi chuyển vạch.
/// </summary>
public static class ThaoTacBit
{
    /// <summary>Số bit đang bật — số tín hiệu đang tích cực trong một thanh ghi.</summary>
    public static int DemBitBat(uint thanhGhi)
    {
        int n = 0;
        while (thanhGhi != 0)
        {
            thanhGhi &= thanhGhi - 1;      // xoá bit 1 thấp nhất
            n++;
        }
        return n;
    }

    /// <summary>Checksum XOR của một khung — đúng phép dùng ở mục G.2.4.</summary>
    public static byte TongKiemXor(ReadOnlySpan<byte> khung)
    {
        byte t = 0;
        foreach (byte b in khung) t ^= b;
        return t;
    }

    /// <summary>Đổi số nhị phân thường sang mã Gray (encoder tuyệt đối dùng mã này).</summary>
    public static uint SangGray(uint x) => x ^ (x >> 1);

    /// <summary>Đổi ngược mã Gray đọc từ encoder về số vạch thật.</summary>
    public static uint TuGray(uint g)
    {
        uint x = g;
        for (uint d = g >> 1; d != 0; d >>= 1) x ^= d;
        return x;
    }
}

// ══════════════════ NHÓM 7 — ma trận ══════════════════

/// <summary>
/// <b>LeetCode 48 — Rotate Image</b> và <b>2022 — Convert 1D Array Into 2D Array</b>.
/// Trong máy: khay đặt xoay 90° thì bản đồ vị trí phải xoay theo, và danh sách vị trí
/// đọc từ tệp cấu hình (một chiều) phải đổi thành lưới (hai chiều) — mục 13.4.6.
/// </summary>
public static class BanDoKhay
{
    /// <summary>Xoay ma trận vuông 90° theo chiều kim đồng hồ, <b>tại chỗ</b>.</summary>
    public static void Xoay90(int[,] o)
    {
        ArgumentNullException.ThrowIfNull(o);
        int n = o.GetLength(0);
        // Chuyển vị rồi lật ngang — hai bước đơn giản, không cần mảng phụ.
        for (int i = 0; i < n; i++)
            for (int j = i + 1; j < n; j++)
                (o[i, j], o[j, i]) = (o[j, i], o[i, j]);

        for (int i = 0; i < n; i++)
            for (int j = 0; j < n / 2; j++)
                (o[i, j], o[i, n - 1 - j]) = (o[i, n - 1 - j], o[i, j]);
    }

    /// <summary>Đổi danh sách vị trí một chiều thành lưới <paramref name="hang"/>×<paramref name="cot"/>.</summary>
    public static int[,]? ThanhLuoi(int[] phang, int hang, int cot)
    {
        ArgumentNullException.ThrowIfNull(phang);
        if (hang <= 0 || cot <= 0 || phang.Length != hang * cot) return null;   // không đoán bừa

        var luoi = new int[hang, cot];
        for (int i = 0; i < phang.Length; i++) luoi[i / cot, i % cot] = phang[i];
        return luoi;
    }
}

/// <summary>
/// <b>LeetCode 200 — Number of Islands.</b> Đếm vùng liên thông bằng loang (BFS).
/// Trong máy: đếm số khuyết tật TÁCH BIỆT trên một tấm ảnh kiểm — khác hẳn đếm số điểm ảnh lỗi.
/// Dùng BFS lặp chứ không đệ quy: ảnh 4000×3000 sẽ làm tràn ngăn xếp nếu đệ quy.
/// </summary>
public static class DemVungLoi
{
    private static readonly (int R, int C)[] Huong = [(-1, 0), (1, 0), (0, -1), (0, 1)];

    /// <summary>Số vùng liên thông gồm các ô <c>true</c>.</summary>
    public static int Dem(bool[,] anh)
    {
        ArgumentNullException.ThrowIfNull(anh);
        int h = anh.GetLength(0), c = anh.GetLength(1);
        var daXet = new bool[h, c];
        int vung = 0;
        var hang = new Queue<(int R, int C)>();

        for (int r = 0; r < h; r++)
            for (int k = 0; k < c; k++)
            {
                if (!anh[r, k] || daXet[r, k]) continue;
                vung++;
                hang.Enqueue((r, k));
                daXet[r, k] = true;
                while (hang.Count > 0)
                {
                    var (x, y) = hang.Dequeue();
                    foreach (var (dr, dc) in Huong)
                    {
                        int nx = x + dr, ny = y + dc;
                        if (nx < 0 || ny < 0 || nx >= h || ny >= c) continue;
                        if (!anh[nx, ny] || daXet[nx, ny]) continue;
                        daXet[nx, ny] = true;
                        hang.Enqueue((nx, ny));
                    }
                }
            }
        return vung;
    }
}

// ══════════════════ NHÓM 9 — đồ thị, thứ tự ══════════════════

/// <summary>
/// <b>LeetCode 207/210 — Course Schedule.</b> Sắp xếp tô-pô bằng thuật toán Kahn.
/// Trong máy: đây là câu trả lời cho câu hỏi mà mục 16.3.4 nêu ra —
/// <b>các trạm chờ nhau có bao giờ chờ vòng tròn không</b>. Có chu trình = khoá chết.
/// </summary>
public static class ThuTuTram
{
    /// <summary>
    /// Trả về thứ tự chạy hợp lệ, hoặc <c>null</c> nếu có chu trình (khoá chết).
    /// <paramref name="phuThuoc"/>: cặp (A, B) nghĩa là A phải xong trước B.
    /// </summary>
    public static int[]? SapThuTu(int soTram, IEnumerable<(int Truoc, int Sau)> phuThuoc)
    {
        ArgumentNullException.ThrowIfNull(phuThuoc);
        ArgumentOutOfRangeException.ThrowIfNegative(soTram);

        var ke = new List<int>[soTram];
        for (int i = 0; i < soTram; i++) ke[i] = [];
        var bacVao = new int[soTram];

        foreach (var (a, b) in phuThuoc)
        {
            ke[a].Add(b);
            bacVao[b]++;
        }

        var hang = new Queue<int>();
        for (int i = 0; i < soTram; i++)
            if (bacVao[i] == 0) hang.Enqueue(i);

        var thuTu = new List<int>(soTram);
        while (hang.Count > 0)
        {
            int t = hang.Dequeue();
            thuTu.Add(t);
            foreach (int s in ke[t])
                if (--bacVao[s] == 0) hang.Enqueue(s);
        }
        // Còn trạm chưa xếp được = chúng nằm trong một vòng chờ nhau.
        return thuTu.Count == soTram ? [.. thuTu] : null;
    }
}

// ══════════════════ NHÓM 11 — tìm nhị phân trên đáp án ══════════════════

/// <summary>
/// <b>LeetCode 875 — Koko Eating Bananas</b> và <b>1011 — Capacity To Ship Packages</b>.
/// Kỹ thuật <b>tìm nhị phân trên ĐÁP ÁN</b>: không tìm trong dữ liệu, mà tìm trong tập giá trị
/// có thể của câu trả lời, với một hàm kiểm "mức này có đủ không" đơn điệu.
/// Trong máy: "tốc độ tối thiểu để kịp nhịp", "cỡ khay tối thiểu để xong lô trong ca".
/// </summary>
public static class TimNhiPhanTrenDapAn
{
    /// <summary>
    /// Giá trị nhỏ nhất trong [thap; cao] mà <paramref name="du"/> trả về true.
    /// Yêu cầu: <paramref name="du"/> phải ĐƠN ĐIỆU — false… rồi true… và không quay lại.
    /// </summary>
    public static long NhoNhatThoaMan(long thap, long cao, Func<long, bool> du)
    {
        ArgumentNullException.ThrowIfNull(du);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(thap, cao);
        while (thap < cao)
        {
            long giua = thap + (cao - thap) / 2;      // tránh tràn, không dùng (thap+cao)/2
            if (du(giua)) cao = giua;
            else thap = giua + 1;
        }
        return thap;
    }

    /// <summary>Nhịp tối thiểu (phôi/giờ) để xử lý hết <paramref name="loHang"/> trong <paramref name="soGio"/>.</summary>
    public static long NhipToiThieu(int[] loHang, int soGio)
    {
        ArgumentNullException.ThrowIfNull(loHang);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(soGio);
        long max = 1;
        foreach (int x in loHang) max = Math.Max(max, x);

        return NhoNhatThoaMan(1, max, nhip =>
        {
            long gio = 0;
            foreach (int x in loHang) gio += (x + nhip - 1) / nhip;   // trần của phép chia
            return gio <= soGio;
        });
    }
}

// ══════════════════ NHÓM 12 — lập lịch ══════════════════

/// <summary>
/// <b>LeetCode 621 — Task Scheduler.</b> Xếp lịch có thời gian nghỉ bắt buộc giữa hai lần
/// cùng loại. Trong máy: nguội khuôn, chờ keo khô, chờ buồng hút phục hồi chân không —
/// cùng một tài nguyên không được dùng lại trước khi hết thời gian hồi.
/// </summary>
public static class LichCoThoiGianHoi
{
    /// <summary>
    /// Tổng số nhịp tối thiểu để chạy hết các việc, biết hai việc CÙNG LOẠI phải cách nhau
    /// ít nhất <paramref name="nhipHoi"/> nhịp.
    /// </summary>
    public static int TongNhip(IEnumerable<char> viec, int nhipHoi)
    {
        ArgumentNullException.ThrowIfNull(viec);
        ArgumentOutOfRangeException.ThrowIfNegative(nhipHoi);

        var dem = new Dictionary<char, int>();
        int tong = 0;
        foreach (char v in viec)
        {
            dem[v] = dem.GetValueOrDefault(v) + 1;
            tong++;
        }
        if (tong == 0) return 0;

        int nhieuNhat = 0, soLoaiNhieuNhat = 0;
        foreach (int n in dem.Values)
        {
            if (n > nhieuNhat) { nhieuNhat = n; soLoaiNhieuNhat = 1; }
            else if (n == nhieuNhat) soLoaiNhieuNhat++;
        }

        // Khung do loại việc nhiều nhất dựng nên; nếu việc khác lấp đầy hết khe thì tổng = số việc.
        int khung = (nhieuNhat - 1) * (nhipHoi + 1) + soLoaiNhieuNhat;
        return Math.Max(khung, tong);
    }
}

// ══════════════════ NHÓM 13 — tổng tiền tố ══════════════════

/// <summary>
/// <b>LeetCode 303 — Range Sum Query</b> và <b>560 — Subarray Sum Equals K</b>.
/// Trong máy: "từ 8 giờ tới 14 giờ làm được bao nhiêu" trả lời trong O(1) nếu giữ tổng tiền tố.
/// </summary>
public sealed class ThongKeCa
{
    private readonly long[] _tienTo;

    public ThongKeCa(int[] sanLuongMoiGio)
    {
        ArgumentNullException.ThrowIfNull(sanLuongMoiGio);
        _tienTo = new long[sanLuongMoiGio.Length + 1];
        for (int i = 0; i < sanLuongMoiGio.Length; i++)
            _tienTo[i + 1] = _tienTo[i] + sanLuongMoiGio[i];
    }

    /// <summary>Tổng sản lượng trong khoảng giờ [tu; den) — O(1).</summary>
    public long Tong(int tu, int den)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(tu);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(den, _tienTo.Length - 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(tu, den);
        return _tienTo[den] - _tienTo[tu];
    }

    /// <summary>Số đoạn giờ liên tiếp có tổng đúng bằng <paramref name="muc"/> (LeetCode 560).</summary>
    public static int DemDoanCoTong(int[] sanLuong, long muc)
    {
        ArgumentNullException.ThrowIfNull(sanLuong);
        var daGap = new Dictionary<long, int> { [0] = 1 };
        long cong = 0;
        int dem = 0;
        foreach (int x in sanLuong)
        {
            cong += x;
            if (daGap.TryGetValue(cong - muc, out int n)) dem += n;
            daGap[cong] = daGap.GetValueOrDefault(cong) + 1;
        }
        return dem;
    }
}

// ══════════════════ NHÓM 14 — đồng thời ══════════════════

/// <summary>
/// <b>LeetCode 1114 — Print in Order.</b> Ép thứ tự giữa ba luồng chạy song song.
/// Trong máy: đúng bài "trạm B không được bắt đầu trước khi trạm A báo xong" —
/// và bản máy dùng <see cref="SemaphoreSlim"/> chứ không <c>lock</c>, vì phải
/// <b>chờ được bất đồng bộ</b> và <b>huỷ được</b> (mục 5.2, 5.3.2).
/// </summary>
public sealed class ChotThuTu : IDisposable
{
    private readonly SemaphoreSlim[] _cong;
    private bool _daHuy;

    /// <summary>Tạo <paramref name="soBuoc"/> cổng; bước 0 mở sẵn, các bước sau đóng.</summary>
    public ChotThuTu(int soBuoc)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(soBuoc, 1);
        _cong = new SemaphoreSlim[soBuoc];
        _cong[0] = new SemaphoreSlim(1, 1);
        for (int i = 1; i < soBuoc; i++) _cong[i] = new SemaphoreSlim(0, 1);
    }

    /// <summary>Chờ tới lượt bước <paramref name="buoc"/>, chạy <paramref name="viec"/>, rồi mở cổng kế tiếp.</summary>
    public async Task ChayDungLuotAsync(int buoc, Func<Task> viec, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(viec);
        await _cong[buoc].WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await viec().ConfigureAwait(false);
        }
        finally
        {
            // Mở cổng kế tiếp trong finally: việc lỗi cũng không được làm KẸT cả dây chuyền.
            if (buoc + 1 < _cong.Length) _cong[buoc + 1].Release();
        }
    }

    public void Dispose()
    {
        if (_daHuy) return;
        foreach (var c in _cong) c.Dispose();
        _daHuy = true;
    }
}
