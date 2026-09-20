// -------------------------------------------------------
// File:    NghiepVuVaTrinhTu.cs
// Project: MeoBench
// Purpose: Lời giải mẫu G.4.1 (chuyển động có hạn giờ), G.4.3 (cụm đo),
//          G.5.1 (IBuoc), G.5.2 (bảy bước của một chu kỳ).
// -------------------------------------------------------
using System.Globalization;

namespace MeoBench;

// ══════════════ G.4.1 — chuyển động có hạn giờ ══════════════

public sealed class ChuyenDong
{
    private readonly ITruc _truc;
    private readonly int    _hanGioMs;
    private readonly double _gioiHanDuoiMm, _gioiHanTrenMm;

    public ChuyenDong(ITruc truc, double gioiHanDuoiMm, double gioiHanTrenMm, int hanGioMs = 3000)
    {
        ArgumentNullException.ThrowIfNull(truc);
        _truc          = truc;
        _gioiHanDuoiMm = gioiHanDuoiMm;
        _gioiHanTrenMm = gioiHanTrenMm;
        _hanGioMs      = hanGioMs;
    }

    public ITruc Truc => _truc;

    public async Task DiToiAsync(double viTriMm, CancellationToken ct = default)
    {
        if (!_truc.DaVeGoc)
            throw new AlarmException(MaCanhBao.TrucChuaVeGoc, _truc.Ten, "chưa về gốc mà đã ra lệnh đi");

        if (viTriMm < _gioiHanDuoiMm || viTriMm > _gioiHanTrenMm)
            throw new ArgumentOutOfRangeException(nameof(viTriMm),
                string.Format(CultureInfo.InvariantCulture,
                    "{0} nằm ngoài hành trình [{1}; {2}]", viTriMm, _gioiHanDuoiMm, _gioiHanTrenMm));

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(_hanGioMs);

        try
        {
            await _truc.DiToiAsync(viTriMm, cts.Token).ConfigureAwait(false);
        }
        // Bộ lọc này là thứ phân biệt NGƯỜI BẤM DỪNG với THIẾT BỊ HẾT GIỜ.
        // Thiếu nó: mỗi lần bấm Dừng máy lại đẻ ra một cảnh báo giả.
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new AlarmException(MaCanhBao.TrucQuaThoiGian, _truc.Ten,
                string.Format(CultureInfo.InvariantCulture,
                    "quá {0} ms khi đi tới {1:F1} mm", _hanGioMs, viTriMm));
        }
    }

    public async Task VeGocAsync(CancellationToken ct = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(_hanGioMs);
        try
        {
            await _truc.VeGocAsync(cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new AlarmException(MaCanhBao.TrucQuaThoiGian, _truc.Ten, $"quá {_hanGioMs} ms khi về gốc");
        }
    }
}

// ══════════════ G.4.3 — cụm đo ══════════════

public sealed class CumDo
{
    private readonly ChuyenDong       _z;
    private readonly ICamBienChieuDay _camBien;
    private readonly IDongHo          _dongHo;
    private readonly double _viTriDoMm, _viTriAnToanMm, _duoiMm, _trenMm;
    private readonly int    _soLanDo;

    public CumDo(ChuyenDong z, ICamBienChieuDay camBien, IDongHo dongHo,
                 double viTriDoMm, double viTriAnToanMm,
                 double duoiMm, double trenMm, int soLanDo = 3)
    {
        ArgumentNullException.ThrowIfNull(z);
        ArgumentNullException.ThrowIfNull(camBien);
        ArgumentNullException.ThrowIfNull(dongHo);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(soLanDo);
        _z = z; _camBien = camBien; _dongHo = dongHo;
        _viTriDoMm = viTriDoMm; _viTriAnToanMm = viTriAnToanMm;
        _duoiMm = duoiMm; _trenMm = trenMm; _soLanDo = soLanDo;
    }

    public async Task<KetQuaDo> DoAsync(int soHieuPhoi, CancellationToken ct = default)
    {
        await _z.DiToiAsync(_viTriDoMm, ct).ConfigureAwait(false);
        try
        {
            double tong = 0;
            for (int i = 0; i < _soLanDo; i++)
                tong += await _camBien.DocAsync(ct).ConfigureAwait(false);

            double trungBinh = Math.Round(tong / _soLanDo, 4);
            var    ketLuan   = KiemDai.DanhGia(trungBinh, _duoiMm, _trenMm);
            return new KetQuaDo(soHieuPhoi, trungBinh, _dongHo.BayGio, ketLuan);
        }
        finally
        {
            // Luôn nâng đầu đo, KỂ CẢ khi có lỗi — không nâng thì phôi kế tiếp trôi vào sẽ va.
            // Dùng CancellationToken.None: việc dọn dẹp không được bỏ dở vì người bấm Dừng.
            await _z.DiToiAsync(_viTriAnToanMm, CancellationToken.None).ConfigureAwait(false);
        }
    }
}

// ══════════════ G.5.1 — hợp đồng một bước ══════════════

/// <summary>Dữ liệu đi chung giữa các bước trong một chu kỳ.</summary>
public sealed class BoiCanh
{
    public required ChuyenDong TrucZ     { get; init; }
    public required ChuyenDong TrucX     { get; init; }
    public required IKep       Kep       { get; init; }
    public required CumDo      CumDo     { get; init; }
    public required double     MangOkMm  { get; init; }
    public required double     MangNgMm  { get; init; }
    public required double     ChoMm     { get; init; }

    public int       SoHieuPhoi { get; set; }
    public KetQuaDo  KetQua     { get; set; }
}

public interface IBuoc
{
    int    Ma  { get; }     // ổn định giữa các phiên bản — dùng cho log và MES
    string Ten { get; }     // cho người vận hành đọc
    Task ThucThiAsync(BoiCanh bc, CancellationToken ct);
}

// ══════════════ G.5.2 — bảy bước của một chu kỳ ══════════════

public sealed class B1ChoPhoi : IBuoc
{
    public int Ma => 10; public string Ten => "Chờ phôi vào";
    public Task ThucThiAsync(BoiCanh bc, CancellationToken ct) => Task.Delay(2, ct);
}

public sealed class B2Do : IBuoc
{
    public int Ma => 20; public string Ten => "Hạ đầu đo và đo";
    public async Task ThucThiAsync(BoiCanh bc, CancellationToken ct)
        => bc.KetQua = await bc.CumDo.DoAsync(bc.SoHieuPhoi, ct).ConfigureAwait(false);
}

public sealed class B3KetLuan : IBuoc
{
    public int Ma => 30; public string Ten => "Kết luận đạt / không đạt";
    public Task ThucThiAsync(BoiCanh bc, CancellationToken ct)
    {
        if (bc.KetQua.KetLuan == KetLuanDo.ChuaDo)
            throw new InvalidOperationException("Bước kết luận chạy trước bước đo");
        return Task.CompletedTask;
    }
}

public sealed class B4Kep : IBuoc
{
    public int Ma => 40; public string Ten => "Kẹp phôi";
    public Task ThucThiAsync(BoiCanh bc, CancellationToken ct) => bc.Kep.KepAsync(ct);
}

public sealed class B5DiToiMang : IBuoc
{
    public int Ma => 50; public string Ten => "Đưa tới máng";
    public Task ThucThiAsync(BoiCanh bc, CancellationToken ct)
        => bc.TrucX.DiToiAsync(bc.KetQua.Dat ? bc.MangOkMm : bc.MangNgMm, ct);
}

public sealed class B6Nha : IBuoc
{
    public int Ma => 60; public string Ten => "Nhả phôi";
    public Task ThucThiAsync(BoiCanh bc, CancellationToken ct) => bc.Kep.NhaAsync(ct);
}

public sealed class B7VeCho : IBuoc
{
    public int Ma => 70; public string Ten => "Về vị trí chờ";
    public Task ThucThiAsync(BoiCanh bc, CancellationToken ct) => bc.TrucX.DiToiAsync(bc.ChoMm, ct);
}

public static class ChuKy
{
    /// <summary>Chu trình là DỮ LIỆU — một danh sách. Nhờ vậy hiện được "bước 3/7".</summary>
    public static IReadOnlyList<IBuoc> BayBuoc() =>
    [
        new B1ChoPhoi(), new B2Do(), new B3KetLuan(), new B4Kep(),
        new B5DiToiMang(), new B6Nha(), new B7VeCho(),
    ];
}

// ══════════════ Bộ tự kiểm ══════════════

public static class KiemNghiepVuVaTrinhTu
{
    private static (ChuyenDong z, ChuyenDong x, TrucGiaLap tz, TrucGiaLap tx) TaoTruc()
    {
        var tz = new TrucGiaLap("Z", mmMoiBuoc: 10.0);
        var tx = new TrucGiaLap("X", mmMoiBuoc: 40.0);
        return (new ChuyenDong(tz, -1.0, 60.0, hanGioMs: 500),
                new ChuyenDong(tx, -1.0, 400.0, hanGioMs: 500), tz, tx);
    }

    public static async Task Chay()
    {
        // ---------- G.4.1 ----------
        Kiem.MoBai("G.4.1", "Chuyển động có hạn giờ");
        var (z, _, tz, _) = TaoTruc();

        await Kiem.NemAsync<AlarmException>(() => z.DiToiAsync(20.0),
            "chưa về gốc mà ra lệnh đi → AlarmException");

        await z.VeGocAsync();
        await z.DiToiAsync(25.0);
        Kiem.Gan(tz.ViTriMm, 25.0, 1e-6, "đi tới 25 mm thành công");

        Kiem.Nem<ArgumentOutOfRangeException>(() => z.DiToiAsync(999.0).GetAwaiter().GetResult(),
            "vị trí ngoài hành trình → ArgumentOutOfRangeException");

        tz.MoPhongTreo = true;
        var exTreo = await Kiem.BatAsync<AlarmException>(() => z.DiToiAsync(0.0));
        Kiem.Dung(exTreo is not null && exTreo.Ma == MaCanhBao.TrucQuaThoiGian,
            "trục treo → AlarmException mã 10001 (KHÔNG phải OperationCanceledException)");
        tz.MoPhongTreo = false;

        using (var cts = new CancellationTokenSource())
        {
            var chay = z.DiToiAsync(50.0, cts.Token);
            cts.Cancel();
            await Kiem.NemAsync<OperationCanceledException>(() => chay,
                "NGƯỜI bấm Dừng → OperationCanceledException, KHÔNG sinh cảnh báo giả");
        }

        // ---------- G.4.3 ----------
        Kiem.MoBai("G.4.3", "Cụm đo");
        var (z2, _, tz2, _) = TaoTruc();
        await z2.VeGocAsync();
        var dongHo = new DongHoGia(new DateTime(2026, 9, 20, 8, 0, 0));
        var cb     = new CamBienGiaLap(hatGiong: 7, tyLePhoiLoi: 0.0);
        var cumDo  = new CumDo(z2, cb, dongHo, viTriDoMm: 30.0, viTriAnToanMm: 0.0,
                               duoiMm: 1.950, trenMm: 2.050, soLanDo: 3);

        var kq = await cumDo.DoAsync(soHieuPhoi: 1);
        Kiem.Bang(kq.SoHieuPhoi, 1, "trả về đúng số hiệu phôi");
        Kiem.Bang(kq.KetLuan, KetLuanDo.Dat, "phôi trong dải → Đạt");
        Kiem.Gan(kq.ChieuDayMm, 2.000, 0.01, "trung bình 3 lần đo quanh 2,000 mm");
        Kiem.Bang(kq.ThoiDiem, dongHo.BayGio, "thời điểm lấy từ đồng hồ TIÊM VÀO, không phải DateTime.Now");
        Kiem.Gan(tz2.ViTriMm, 0.0, 1e-6, "đo xong thì trục Z đã nâng về vị trí an toàn");

        cb.MoPhongKhongPhanHoi = true;
        var exDo = await Kiem.BatAsync<AlarmException>(() => cumDo.DoAsync(2));
        Kiem.Dung(exDo is not null && exDo.Ma == MaCanhBao.CamBienKhongPhanHoi,
            "cảm biến lỗi → AlarmException lan ra ngoài");
        Kiem.Gan(tz2.ViTriMm, 0.0, 1e-6,
            "★ CÓ LỖI thì trục Z VẪN được nâng (nhờ finally) — đây là điểm chấm chính của bài");

        // ---------- G.5.1 ----------
        Kiem.MoBai("G.5.1", "Hợp đồng IBuoc");
        var buocs = ChuKy.BayBuoc();
        Kiem.Bang(buocs.Count, 7, "chu trình là một DANH SÁCH 7 phần tử");
        Kiem.Dung(buocs.Select(b => b.Ma).Distinct().Count() == 7, "mã bước không trùng nhau");
        Kiem.Dung(buocs.All(b => !string.IsNullOrWhiteSpace(b.Ten)), "bước nào cũng có tên đọc được");
        Kiem.Bang($"Bước 3/{buocs.Count}: {buocs[2].Ten}", "Bước 3/7: Kết luận đạt / không đạt",
                  "hiện được tiến độ mà không sửa gì trong các bước");

        // ---------- G.5.2 ----------
        Kiem.MoBai("G.5.2", "Bảy bước của một chu kỳ");
        var (z3, x3, tz3, tx3) = TaoTruc();
        await z3.VeGocAsync();
        await x3.VeGocAsync();
        var kep = new KepGiaLap();
        var bc = new BoiCanh
        {
            TrucZ = z3, TrucX = x3, Kep = kep,
            CumDo = new CumDo(z3, new CamBienGiaLap(hatGiong: 7, tyLePhoiLoi: 0.0),
                              new DongHoGia(new DateTime(2026, 9, 20, 8, 0, 0)),
                              30.0, 0.0, 1.950, 2.050, 3),
            MangOkMm = 100.0, MangNgMm = 200.0, ChoMm = 0.0,
            SoHieuPhoi = 1,
        };

        foreach (var b in ChuKy.BayBuoc())
            await b.ThucThiAsync(bc, CancellationToken.None);

        Kiem.Bang(bc.KetQua.KetLuan, KetLuanDo.Dat, "chạy hết 7 bước, phôi được kết luận Đạt");
        Kiem.Dung(!kep.DangKep, "cuối chu kỳ kẹp đã nhả");
        Kiem.Gan(tx3.ViTriMm, 0.0, 1e-6, "cuối chu kỳ trục X về vị trí chờ");
        Kiem.Gan(tz3.ViTriMm, 0.0, 1e-6, "cuối chu kỳ trục Z ở vị trí an toàn");

        // phôi NG phải đi máng khác
        var bcNg = new BoiCanh
        {
            TrucZ = z3, TrucX = x3, Kep = kep,
            CumDo = new CumDo(z3, new CamBienGiaLap(hatGiong: 7, tamMm: 2.500, nhieuMm: 0.0, tyLePhoiLoi: 0.0),
                              new DongHoGia(new DateTime(2026, 9, 20, 8, 0, 0)),
                              30.0, 0.0, 1.950, 2.050, 3),
            MangOkMm = 100.0, MangNgMm = 200.0, ChoMm = 0.0,
            SoHieuPhoi = 2,
        };
        foreach (var b in ChuKy.BayBuoc().Take(5))       // dừng ngay sau bước đưa tới máng
            await b.ThucThiAsync(bcNg, CancellationToken.None);
        Kiem.Bang(bcNg.KetQua.KetLuan, KetLuanDo.TrenNguong, "phôi dày 2,5 mm → TrenNguong");
        Kiem.Gan(tx3.ViTriMm, 200.0, 1e-6, "phôi NG được đưa tới MÁNG NG (200 mm), không phải máng OK");

        foreach (var b in ChuKy.BayBuoc().Skip(5))       // dọn nốt cho gọn
            await b.ThucThiAsync(bcNg, CancellationToken.None);
    }
}
