// -------------------------------------------------------
// File:    LogicVaThietBi.cs
// Project: MeoBench
// Purpose: Lời giải mẫu G.2.1 (logic thuần), G.3.1 (hợp đồng thiết bị),
//          G.3.4 (bản giả lập tái hiện được).
// -------------------------------------------------------
using System.Globalization;

namespace MeoBench;

// ══════════════ G.2.1 — kiểm dải đo (logic thuần, không chạm thiết bị) ══════════════

public static class KiemDai
{
    /// <summary>
    /// So chiều dày đo được với dải cho phép.
    /// Dải bị đảo là LỖI LẬP TRÌNH, không phải dữ liệu xấu → ném ngay,
    /// thay vì im lặng trả về "đạt" như lớp bảo vệ hỏng ở mục 3.3.3.
    /// </summary>
    public static KetLuanDo DanhGia(double doDuocMm, double duoiMm, double trenMm)
    {
        if (double.IsNaN(doDuocMm))
            return KetLuanDo.LoiDo;

        if (duoiMm > trenMm)
            throw new ArgumentException(
                string.Format(CultureInfo.InvariantCulture,
                    "Dải bị đảo: dưới {0} > trên {1}", duoiMm, trenMm), nameof(duoiMm));

        if (doDuocMm < duoiMm) return KetLuanDo.DuoiNguong;
        if (doDuocMm > trenMm) return KetLuanDo.TrenNguong;
        return KetLuanDo.Dat;
    }
}

// ══════════════ G.3.1 — hợp đồng thiết bị ══════════════

public interface ITruc
{
    string Ten      { get; }
    double ViTriMm  { get; }
    bool   DaVeGoc  { get; }
    Task VeGocAsync(CancellationToken ct = default);
    Task DiToiAsync(double viTriMm, CancellationToken ct = default);
}

public interface ICamBienChieuDay
{
    Task<double> DocAsync(CancellationToken ct = default);
}

public interface IKep
{
    bool DangKep { get; }
    Task KepAsync(CancellationToken ct = default);
    Task NhaAsync(CancellationToken ct = default);
}

// ══════════════ G.3.4 — bản giả lập TÁI HIỆN ĐƯỢC ══════════════

public sealed class TrucGiaLap : ITruc
{
    private readonly double _mmMoiBuoc;
    private readonly int    _msMoiBuoc;

    public TrucGiaLap(string ten, double mmMoiBuoc = 20.0, int msMoiBuoc = 2)
    {
        Ten        = ten;
        _mmMoiBuoc = mmMoiBuoc;
        _msMoiBuoc = msMoiBuoc;
    }

    public string Ten     { get; }
    public double ViTriMm { get; private set; }
    public bool   DaVeGoc { get; private set; }

    /// <summary>G.3.5 — ép trục "treo" để kiểm nhánh hết giờ mà không cần phần cứng.</summary>
    public bool MoPhongTreo { get; set; }

    public async Task VeGocAsync(CancellationToken ct = default)
    {
        await DiChuyenAsync(0.0, ct).ConfigureAwait(false);
        DaVeGoc = true;
    }

    public Task DiToiAsync(double viTriMm, CancellationToken ct = default)
        => DiChuyenAsync(viTriMm, ct);

    private async Task DiChuyenAsync(double dich, CancellationToken ct)
    {
        while (Math.Abs(ViTriMm - dich) > 1e-6)
        {
            ct.ThrowIfCancellationRequested();
            await Task.Delay(_msMoiBuoc, ct).ConfigureAwait(false);

            if (MoPhongTreo) continue;            // treo: thời gian trôi, vị trí không đổi

            double delta = Math.Clamp(dich - ViTriMm, -_mmMoiBuoc, _mmMoiBuoc);
            ViTriMm = Math.Round(ViTriMm + delta, 6);
        }
    }
}

public sealed class CamBienGiaLap : ICamBienChieuDay
{
    private readonly Random _ngauNhien;
    private readonly double _tamMm, _nhieuMm, _tyLePhoiLoi, _lechPhoiLoi;
    private readonly int    _soLanDoMoiPhoi;
    private int    _soLanDaDoc;
    private int    _phoiHienTai = -1;
    private double _chieuDayThatCuaPhoi;

    public CamBienGiaLap(int hatGiong, int soLanDoMoiPhoi = 3, double tamMm = 2.000,
                         double nhieuMm = 0.004, double tyLePhoiLoi = 0.10, double lechPhoiLoi = 0.120)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(soLanDoMoiPhoi);
        _ngauNhien      = new Random(hatGiong);   // hạt giống CỐ ĐỊNH → tái hiện được
        _soLanDoMoiPhoi = soLanDoMoiPhoi;
        _tamMm          = tamMm;
        _nhieuMm        = nhieuMm;
        _tyLePhoiLoi    = tyLePhoiLoi;
        _lechPhoiLoi    = lechPhoiLoi;
        HatGiong        = hatGiong;
    }

    public int  HatGiong { get; }
    public bool MoPhongKhongPhanHoi { get; set; }

    public Task<double> DocAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (MoPhongKhongPhanHoi)
            throw new AlarmException(MaCanhBao.CamBienKhongPhanHoi, "CB_DAY",
                                     "cảm biến chiều dày không phản hồi");

        // MỘT PHÔI CÓ MỘT CHIỀU DÀY THẬT; cảm biến chỉ thêm nhiễu lên nó.
        // Mô hình đúng chiều này mới làm cho việc lấy trung bình nhiều lần đo (G.2.5)
        // có ý nghĩa — nếu mỗi lần đọc lại tự sinh một phôi mới thì trung bình vô nghĩa.
        int phoi = _soLanDaDoc / _soLanDoMoiPhoi;
        if (phoi != _phoiHienTai)
        {
            _phoiHienTai         = phoi;
            _chieuDayThatCuaPhoi = _tamMm
                + (_ngauNhien.NextDouble() < _tyLePhoiLoi ? _lechPhoiLoi : 0.0);
        }
        _soLanDaDoc++;

        double v = _chieuDayThatCuaPhoi + (_ngauNhien.NextDouble() - 0.5) * 2 * _nhieuMm;
        return Task.FromResult(Math.Round(v, 4));
    }
}

public sealed class KepGiaLap : IKep
{
    public bool DangKep { get; private set; }
    public bool MoPhongKhongXacNhan { get; set; }

    public async Task KepAsync(CancellationToken ct = default)
    {
        await Task.Delay(5, ct).ConfigureAwait(false);
        if (MoPhongKhongXacNhan)
            throw new AlarmException(MaCanhBao.KepKhongXacNhan, "KEP", "cảm biến kẹp không lên trong 1 s");
        DangKep = true;
    }

    public async Task NhaAsync(CancellationToken ct = default)
    {
        await Task.Delay(5, ct).ConfigureAwait(false);
        DangKep = false;
    }
}

// ══════════════ Bộ tự kiểm ══════════════

public static class KiemLogicVaThietBi
{
    public static async Task Chay()
    {
        // ---------- G.2.1 ----------
        Kiem.MoBai("G.2.1", "Kiểm dải đo");
        Kiem.Bang(KiemDai.DanhGia(2.000, 1.950, 2.050), KetLuanDo.Dat,        "giữa dải → Đạt");
        Kiem.Bang(KiemDai.DanhGia(1.950, 1.950, 2.050), KetLuanDo.Dat,        "đúng biên dưới → Đạt (biên tính là đạt)");
        Kiem.Bang(KiemDai.DanhGia(2.050, 1.950, 2.050), KetLuanDo.Dat,        "đúng biên trên → Đạt");
        Kiem.Bang(KiemDai.DanhGia(1.949, 1.950, 2.050), KetLuanDo.DuoiNguong, "dưới biên → DuoiNguong");
        Kiem.Bang(KiemDai.DanhGia(2.051, 1.950, 2.050), KetLuanDo.TrenNguong, "trên biên → TrenNguong");
        Kiem.Bang(KiemDai.DanhGia(double.NaN, 1.950, 2.050), KetLuanDo.LoiDo, "NaN → LoiDo, không ném");
        Kiem.Nem<ArgumentException>(() => KiemDai.DanhGia(2.0, 2.050, 1.950),
                                    "dải bị đảo → ném ArgumentException (KHÔNG im lặng trả Đạt)");

        // ---------- G.3.1 ----------
        Kiem.MoBai("G.3.1", "Hợp đồng ITruc");
        var truc = new TrucGiaLap("Z");
        Kiem.Dung(!truc.DaVeGoc, "trục mới tạo thì chưa về gốc");
        await truc.VeGocAsync();
        Kiem.Dung(truc.DaVeGoc, "về gốc xong thì DaVeGoc = true");
        Kiem.Gan(truc.ViTriMm, 0.0, 1e-6, "về gốc thì vị trí = 0");
        await truc.DiToiAsync(25.0);
        Kiem.Gan(truc.ViTriMm, 25.0, 1e-6, "đi tới 25 mm thì dừng đúng 25 mm");
#pragma warning disable CA1859 // cố ý dùng interface: đây chính là điều bài G.3.1 phải chứng minh
        ITruc quaHopDong = truc;
#pragma warning restore CA1859
        Kiem.Bang(quaHopDong.Ten, "Z", "gọi được qua interface, tầng trên không cần biết lớp cụ thể");

        using (var cts = new CancellationTokenSource())
        {
            var chay = truc.DiToiAsync(0.0, cts.Token);
            cts.Cancel();
            await Kiem.NemAsync<OperationCanceledException>(() => chay,
                "huỷ giữa chuyển động → OperationCanceledException");
        }

        // ---------- G.3.4 ----------
        Kiem.MoBai("G.3.4", "Bản giả lập tái hiện được");
        var cb1 = new CamBienGiaLap(hatGiong: 12345);
        var cb2 = new CamBienGiaLap(hatGiong: 12345);
        var cb3 = new CamBienGiaLap(hatGiong: 999);

        var day1 = new List<double>();
        var day2 = new List<double>();
        var day3 = new List<double>();
        for (int i = 0; i < 20; i++)
        {
            day1.Add(await cb1.DocAsync());
            day2.Add(await cb2.DocAsync());
            day3.Add(await cb3.DocAsync());
        }
        Kiem.Dung(day1.SequenceEqual(day2), "CÙNG hạt giống → cùng dãy 20 giá trị (chạy lại được)");
        Kiem.Dung(!day1.SequenceEqual(day3), "KHÁC hạt giống → khác dãy");
        Kiem.Dung(day1.Distinct().Count() > 10, "có nhiễu thật, không phải hằng số");
        Kiem.Dung(day1.All(v => v > 1.9 && v < 2.2), "mọi giá trị nằm trong khoảng hợp lý");
        Kiem.Dung(day1.Take(3).Max() - day1.Take(3).Min() < 0.01,
                  "3 lần đo CÙNG một phôi chỉ lệch nhau bằng nhiễu (mô hình phôi đúng)");
        Kiem.Bang(cb1.HatGiong, 12345, "hạt giống lộ ra ngoài để ghi vào log");

        var cbLoi = new CamBienGiaLap(1) { MoPhongKhongPhanHoi = true };
        var bat = await Kiem.BatAsync<AlarmException>(() => cbLoi.DocAsync());
        Kiem.Dung(bat is not null && bat.Ma == MaCanhBao.CamBienKhongPhanHoi,
                  "ép lỗi → ném AlarmException đúng mã 20001");
    }
}
