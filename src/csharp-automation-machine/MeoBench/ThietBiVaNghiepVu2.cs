// -------------------------------------------------------
// File:    ThietBiVaNghiepVu2.cs
// Project: MeoBench
// Purpose: Lời giải mẫu G.3.2, G.3.3, G.3.5, G.4.2, G.4.4.
// -------------------------------------------------------
namespace MeoBench;

// ══════════════ G.3.2 — hạn giờ cho cảm biến, bằng lớp BỌC ══════════════
//
// Không sửa từng bản cài đặt cảm biến để thêm hạn giờ. Bọc BẤT KỲ
// ICamBienChieuDay nào lại — bản giả lập, driver nối tiếp, hay bản của hãng
// khác sau này đều dùng chung một cơ chế hạn giờ.

public sealed class CamBienCoHanGio(ICamBienChieuDay trong, string ten, int hanGioMs = 1000)
    : ICamBienChieuDay
{
    public async Task<double> DocAsync(CancellationToken ct = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(hanGioMs);
        try
        {
            return await trong.DocAsync(cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new AlarmException(MaCanhBao.CamBienKhongPhanHoi, ten,
                                     $"không trả lời trong {hanGioMs} ms");
        }
    }
}

/// <summary>Cảm biến giả lập chạy chậm — để kiểm nhánh hết giờ.</summary>
public sealed class CamBienCham(int treMs) : ICamBienChieuDay
{
    public async Task<double> DocAsync(CancellationToken ct = default)
    {
        await Task.Delay(treMs, ct).ConfigureAwait(false);
        return 2.000;
    }
}

// ══════════════ G.3.3 — vào-ra số theo TÊN, không theo số kênh ══════════════

public interface IVaoRaSo
{
    bool Doc(string tenTinHieu);
    void Ghi(string tenTinHieu, bool giaTri);
}

/// <summary>
/// Bản đồ tên → kênh. Đổi một tín hiệu từ kênh 3 sang kênh 11 chỉ sửa bản đồ,
/// không sửa dòng mã nào. Cái giá: gõ sai tên chỉ lộ LÚC CHẠY — nên bản đồ
/// phải tự kiểm khi nạp, và tên lạ phải ném ngay chứ không trả về false.
/// </summary>
public sealed class BanDoTinHieu
{
    private readonly Dictionary<string, int> _vao;
    private readonly Dictionary<string, int> _ra;

    public BanDoTinHieu(IDictionary<string, int> vao, IDictionary<string, int> ra)
    {
        ArgumentNullException.ThrowIfNull(vao);
        ArgumentNullException.ThrowIfNull(ra);
        _vao = new Dictionary<string, int>(vao, StringComparer.OrdinalIgnoreCase);
        _ra  = new Dictionary<string, int>(ra,  StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<string> KiemTra()
    {
        var loi = new List<string>();
        foreach (var nhom in new[] { ("vào", _vao), ("ra", _ra) })
        {
            var trung = nhom.Item2.GroupBy(kv => kv.Value).Where(g => g.Count() > 1);
            foreach (var g in trung)
                loi.Add($"Kênh {nhom.Item1} số {g.Key} bị gán cho {g.Count()} tín hiệu: "
                        + string.Join(", ", g.Select(kv => kv.Key)));
            foreach (var kv in nhom.Item2.Where(kv => kv.Value < 0))
                loi.Add($"Tín hiệu {nhom.Item1} '{kv.Key}' có số kênh âm ({kv.Value})");
        }
        return loi;
    }

    public int KenhVao(string ten) => _vao.TryGetValue(ten, out int k) ? k
        : throw new KeyNotFoundException($"Không có tín hiệu VÀO tên '{ten}' trong bản đồ");

    public int KenhRa(string ten) => _ra.TryGetValue(ten, out int k) ? k
        : throw new KeyNotFoundException($"Không có tín hiệu RA tên '{ten}' trong bản đồ");

    public IReadOnlyCollection<string> TenVao => _vao.Keys;
    public IReadOnlyCollection<string> TenRa  => _ra.Keys;
}

public sealed class VaoRaGiaLap(BanDoTinHieu banDo, int soKenh = 32) : IVaoRaSo
{
    private readonly bool[] _vao = new bool[soKenh];
    private readonly bool[] _ra  = new bool[soKenh];

    public bool Doc(string tenTinHieu) => _vao[banDo.KenhVao(tenTinHieu)];
    public void Ghi(string tenTinHieu, bool giaTri) => _ra[banDo.KenhRa(tenTinHieu)] = giaTri;

    /// <summary>Chỉ dùng khi giả lập: ép một tín hiệu vào lên/xuống.</summary>
    public void EpTinHieuVao(string ten, bool giaTri) => _vao[banDo.KenhVao(ten)] = giaTri;

    public bool DocRa(string ten) => _ra[banDo.KenhRa(ten)];
}

// ══════════════ G.3.5 — giả lập BIẾT HỎNG theo kịch bản ══════════════

public enum LoaiLoiGiaLap { KhongLoi, HetGio, NgoaiDai, MatKetNoi }

/// <summary>
/// Hàng đợi kịch bản lỗi: mô phỏng được một CHUỖI sự cố, không chỉ một lỗi
/// đơn lẻ — ví dụ "đọc tốt 2 lần, rồi mất kết nối, rồi tốt trở lại".
/// </summary>
public sealed class CamBienTheoKichBan(int hatGiong, double tamMm = 2.000) : ICamBienChieuDay
{
    private readonly Queue<LoaiLoiGiaLap> _kichBan = new();
    private readonly Random _ngauNhien = new(hatGiong);

    public int SoLanDaDoc { get; private set; }

    public CamBienTheoKichBan Xep(params LoaiLoiGiaLap[] cacLoi)
    {
        ArgumentNullException.ThrowIfNull(cacLoi);
        foreach (var l in cacLoi) _kichBan.Enqueue(l);
        return this;
    }

    public async Task<double> DocAsync(CancellationToken ct = default)
    {
        SoLanDaDoc++;
        var loi = _kichBan.Count > 0 ? _kichBan.Dequeue() : LoaiLoiGiaLap.KhongLoi;

        switch (loi)
        {
            case LoaiLoiGiaLap.HetGio:
                await Task.Delay(Timeout.Infinite, ct).ConfigureAwait(false);
                return 0;   // không bao giờ tới đây
            case LoaiLoiGiaLap.MatKetNoi:
                throw new AlarmException(MaCanhBao.CamBienKhongPhanHoi, "CB_DAY", "mất kết nối");
            case LoaiLoiGiaLap.NgoaiDai:
                return tamMm + 5.0;
            case LoaiLoiGiaLap.KhongLoi:
            default:
                return Math.Round(tamMm + (_ngauNhien.NextDouble() - 0.5) * 0.004, 4);
        }
    }
}

// ══════════════ G.4.2 — cụm kẹp CHỜ XÁC NHẬN ══════════════

public sealed class CumKep : IKep
{
    public const string TinHieuRaKep    = "KEP_DONG";
    public const string TinHieuVaoDaKep = "CB_DA_KEP";

    private readonly IVaoRaSo _io;
    private readonly int _hanGioMs, _nhipHoiMs;

    public CumKep(IVaoRaSo io, int hanGioMs = 1000, int nhipHoiMs = 5)
    {
        ArgumentNullException.ThrowIfNull(io);
        _io = io; _hanGioMs = hanGioMs; _nhipHoiMs = nhipHoiMs;
    }

    public bool DangKep => _io.Doc(TinHieuVaoDaKep);

    public async Task KepAsync(CancellationToken ct = default)
    {
        if (DangKep) return;                 // đã kẹp rồi thì không làm gì, KHÔNG lỗi
        _io.Ghi(TinHieuRaKep, true);
        await ChoTinHieuAsync(TinHieuVaoDaKep, true, "kẹp", ct).ConfigureAwait(false);
    }

    public async Task NhaAsync(CancellationToken ct = default)
    {
        if (!DangKep) return;
        _io.Ghi(TinHieuRaKep, false);
        await ChoTinHieuAsync(TinHieuVaoDaKep, false, "nhả", ct).ConfigureAwait(false);
    }

    private async Task ChoTinHieuAsync(string ten, bool mongDoi, string viec, CancellationToken ct)
    {
        var dongHo = System.Diagnostics.Stopwatch.StartNew();
        while (_io.Doc(ten) != mongDoi)
        {
            ct.ThrowIfCancellationRequested();
            if (dongHo.ElapsedMilliseconds > _hanGioMs)
                throw new AlarmException(MaCanhBao.KepKhongXacNhan, "KEP",
                    $"cảm biến không xác nhận {viec} trong {_hanGioMs} ms");
            await Task.Delay(_nhipHoiMs, ct).ConfigureAwait(false);
        }
    }
}

// ══════════════ G.4.4 — giám sát khí nén PHÁT SỰ KIỆN ══════════════

public sealed class ApSuatThapEventArgs(double barDoDuoc, double barNguong) : EventArgs
{
    public double BarDoDuoc { get; } = barDoDuoc;
    public double BarNguong { get; } = barNguong;
}

/// <summary>
/// Lớp này KHÔNG tham chiếu tới bộ điều khiển máy. Nó chỉ đo và kêu.
/// Ai muốn phản ứng thì tự đăng ký — mũi tên phụ thuộc đi đúng chiều (mục 7.4).
/// </summary>
public sealed class GiamSatKhiNen(Func<double> docApSuatBar, double nguongBar = 5.00)
{
    public double NguongBar  { get; } = nguongBar;
    public double BarGanNhat { get; private set; }
    public bool   DangThap   { get; private set; }
    public int    SoLanBaoDong { get; private set; }

    public event EventHandler<ApSuatThapEventArgs>? ApSuatXuongThap;
    public event EventHandler<ApSuatThapEventArgs>? ApSuatHoiPhuc;

    public void Kiem()
    {
        BarGanNhat = docApSuatBar();
        bool thap  = BarGanNhat < NguongBar;

        // Chỉ phát khi ĐỔI TRẠNG THÁI — không bắn sự kiện mỗi nhịp quét.
        if (thap && !DangThap)
        {
            DangThap = true; SoLanBaoDong++;
            ApSuatXuongThap?.Invoke(this, new ApSuatThapEventArgs(BarGanNhat, NguongBar));
        }
        else if (!thap && DangThap)
        {
            DangThap = false;
            ApSuatHoiPhuc?.Invoke(this, new ApSuatThapEventArgs(BarGanNhat, NguongBar));
        }
    }
}

// ══════════════ Bộ tự kiểm ══════════════

public static class KiemThietBiVaNghiepVu2
{
    public static async Task Chay()
    {
        // ---------- G.3.2 ----------
        Kiem.MoBai("G.3.2", "Hợp đồng ICamBienChieuDay và hạn giờ");
        var nhanh = new CamBienCoHanGio(new CamBienCham(5), "CB_DAY", hanGioMs: 300);
        Kiem.Gan(await nhanh.DocAsync(), 2.000, 1e-9, "cảm biến trả lời kịp → có số");

        var cham = new CamBienCoHanGio(new CamBienCham(2000), "CB_DAY", hanGioMs: 100);
        var ex = await Kiem.BatAsync<AlarmException>(() => cham.DocAsync());
        Kiem.Dung(ex is not null && ex.Ma == MaCanhBao.CamBienKhongPhanHoi,
                  "★ cảm biến chậm → AlarmException mã 20001, KHÔNG treo mãi");
        Kiem.Dung(ex!.Message.Contains("100 ms", StringComparison.Ordinal),
                  "thông báo nói rõ hạn giờ là bao nhiêu");

        using (var cts = new CancellationTokenSource())
        {
            var cham2 = new CamBienCoHanGio(new CamBienCham(2000), "CB_DAY", hanGioMs: 5000);
            var chay = cham2.DocAsync(cts.Token);
            await cts.CancelAsync();
            await Kiem.NemAsync<OperationCanceledException>(() => chay,
                "★ người bấm Dừng → OperationCanceledException, KHÔNG thành cảnh báo giả");
        }

        var boc = new CamBienCoHanGio(new CamBienGiaLap(7, tyLePhoiLoi: 0.0), "CB_DAY");
        Kiem.Gan(await boc.DocAsync(), 2.000, 0.01,
                 "★ lớp bọc dùng được cho BẤT KỲ cảm biến nào — không sửa bản cài đặt nào");

        // ---------- G.3.3 ----------
        Kiem.MoBai("G.3.3", "Vào-ra số theo tên");
        var banDo = new BanDoTinHieu(
            vao: new Dictionary<string, int> { ["CB_PHOI"] = 0, ["CB_DA_KEP"] = 3, ["CB_CUA"] = 7 },
            ra:  new Dictionary<string, int> { ["KEP_DONG"] = 1, ["DEN_XANH"] = 2 });
        Kiem.Bang(banDo.KiemTra().Count, 0, "bản đồ hợp lệ");
        Kiem.Bang(banDo.KenhVao("CB_DA_KEP"), 3, "tra được số kênh từ tên");
        Kiem.Bang(banDo.KenhVao("cb_da_kep"), 3, "tên không phân biệt hoa thường");
        Kiem.Nem<KeyNotFoundException>(() => banDo.KenhVao("CB_KHONG_CO"),
            "★ tên lạ NÉM ngay — không âm thầm trả về false rồi để máy hiểu nhầm");

        var banDoXau = new BanDoTinHieu(
            vao: new Dictionary<string, int> { ["A"] = 5, ["B"] = 5 },
            ra:  new Dictionary<string, int> { ["C"] = -1 });
        var loiBanDo = banDoXau.KiemTra();
        Kiem.Bang(loiBanDo.Count, 2, "bắt được 2 lỗi bản đồ");
        Kiem.Dung(loiBanDo.Any(l => l.Contains("bị gán cho 2 tín hiệu", StringComparison.Ordinal)),
                  "★ hai tín hiệu trùng kênh bị phát hiện — lỗi đấu nối kinh điển");

        var io = new VaoRaGiaLap(banDo);
        io.Ghi("DEN_XANH", true);
        Kiem.Dung(io.DocRa("DEN_XANH"), "ghi rồi đọc lại đúng");
        Kiem.Dung(!io.Doc("CB_PHOI"), "tín hiệu vào mặc định là false");
        io.EpTinHieuVao("CB_PHOI", true);
        Kiem.Dung(io.Doc("CB_PHOI"), "ép được tín hiệu vào để kiểm thử");

        // ---------- G.3.5 ----------
        Kiem.MoBai("G.3.5", "Giả lập biết hỏng theo kịch bản");
        var kb = new CamBienTheoKichBan(hatGiong: 3)
            .Xep(LoaiLoiGiaLap.KhongLoi, LoaiLoiGiaLap.KhongLoi,
                 LoaiLoiGiaLap.MatKetNoi, LoaiLoiGiaLap.NgoaiDai, LoaiLoiGiaLap.KhongLoi);

        Kiem.Gan(await kb.DocAsync(), 2.000, 0.01, "lần 1 tốt");
        Kiem.Gan(await kb.DocAsync(), 2.000, 0.01, "lần 2 tốt");
        var exKb = await Kiem.BatAsync<AlarmException>(() => kb.DocAsync());
        Kiem.Dung(exKb is not null, "★ lần 3 MẤT KẾT NỐI đúng như kịch bản đã xếp");
        Kiem.Gan(await kb.DocAsync(), 7.000, 0.01, "lần 4 trả giá trị ngoài dải");
        Kiem.Gan(await kb.DocAsync(), 2.000, 0.01, "lần 5 tốt trở lại");
        Kiem.Bang(kb.SoLanDaDoc, 5, "đếm đủ 5 lần đọc");
        Kiem.Gan(await kb.DocAsync(), 2.000, 0.01, "hết kịch bản → mặc định chạy tốt");

        var kbTreo = new CamBienTheoKichBan(1).Xep(LoaiLoiGiaLap.HetGio);
        var bocTreo = new CamBienCoHanGio(kbTreo, "CB_DAY", hanGioMs: 80);
        await Kiem.NemAsync<AlarmException>(() => bocTreo.DocAsync(),
            "★ kịch bản HetGio + lớp bọc hạn giờ → dựng được tình huống treo mà không cần phần cứng");

        // ---------- G.4.2 ----------
        Kiem.MoBai("G.4.2", "Cụm kẹp chờ xác nhận");
        var banDoKep = new BanDoTinHieu(
            vao: new Dictionary<string, int> { [CumKep.TinHieuVaoDaKep] = 3 },
            ra:  new Dictionary<string, int> { [CumKep.TinHieuRaKep] = 1 });
        var ioKep = new VaoRaGiaLap(banDoKep);
        var kep = new CumKep(ioKep, hanGioMs: 200, nhipHoiMs: 2);

        // cảm biến phản hồi sau 30 ms — mô phỏng van đóng
        var kepXong = Task.Run(async () => { await Task.Delay(30); ioKep.EpTinHieuVao(CumKep.TinHieuVaoDaKep, true); });
        await kep.KepAsync();
        await kepXong;
        Kiem.Dung(kep.DangKep, "kẹp xong thì DangKep = true");
        Kiem.Dung(ioKep.DocRa(CumKep.TinHieuRaKep), "tín hiệu ra đã được ghi");

        await kep.KepAsync();
        Kiem.Dung(kep.DangKep, "★ gọi Kep() lần nữa khi đã kẹp → không làm gì và KHÔNG lỗi");

        var nhaXong = Task.Run(async () => { await Task.Delay(30); ioKep.EpTinHieuVao(CumKep.TinHieuVaoDaKep, false); });
        await kep.NhaAsync();
        await nhaXong;
        Kiem.Dung(!kep.DangKep, "nhả xong thì DangKep = false");

        // cảm biến KHÔNG lên — khí yếu
        var kep2 = new CumKep(ioKep, hanGioMs: 100, nhipHoiMs: 2);
        var exKep = await Kiem.BatAsync<AlarmException>(() => kep2.KepAsync());
        Kiem.Dung(exKep is not null && exKep.Ma == MaCanhBao.KepKhongXacNhan,
                  "★ ghi tín hiệu mà cảm biến KHÔNG lên → cảnh báo, không coi là xong");
        Kiem.Dung(exKep!.Message.Contains("kẹp", StringComparison.Ordinal),
                  "thông báo nói rõ đang làm việc gì thì hỏng");

        // ---------- G.4.4 ----------
        Kiem.MoBai("G.4.4", "Giám sát khí nén phát sự kiện");
        double ap = 6.00;
        var gs = new GiamSatKhiNen(() => ap, nguongBar: 5.00);
        int soLanThap = 0, soLanHoi = 0;
        gs.ApSuatXuongThap += (_, e) => { soLanThap++; Kiem.Dung(e.BarDoDuoc < e.BarNguong, "sự kiện mang đúng số đo"); };
        gs.ApSuatHoiPhuc   += (_, __) => soLanHoi++;

        gs.Kiem(); gs.Kiem(); gs.Kiem();
        Kiem.Bang(soLanThap, 0, "áp suất bình thường → không có sự kiện");

        ap = 4.80;
        gs.Kiem(); gs.Kiem(); gs.Kiem();
        Kiem.Bang(soLanThap, 1, "★ áp suất thấp 3 nhịp liên tiếp → CHỈ MỘT sự kiện (không bắn mỗi nhịp)");
        Kiem.Dung(gs.DangThap, "cờ DangThap lên");

        ap = 6.10;
        gs.Kiem();
        Kiem.Bang(soLanHoi, 1, "áp suất hồi phục → phát sự kiện hồi phục");
        Kiem.Dung(!gs.DangThap, "cờ DangThap xuống");

        ap = 4.50; gs.Kiem();
        Kiem.Bang(gs.SoLanBaoDong, 2, "xuống thấp lần hai → đếm thành 2 lần báo động");

        // Bất biến quan trọng nhất của bài: lớp này KHÔNG biết gì về máy.
        Kiem.Dung(typeof(GiamSatKhiNen).GetConstructors()
                    .All(c => c.GetParameters().All(p => p.ParameterType != typeof(May))),
                  "★ GiamSatKhiNen KHÔNG nhận May — nó chỉ đo và kêu, không tự dừng máy");
    }
}
