// -------------------------------------------------------
// File:    TrinhTuVaVanHanh2.cs
// Project: MeoBench
// Purpose: Lời giải mẫu G.4.5, G.5.3, G.5.4, G.5.5, G.8.2, G.8.3, G.8.4.
// -------------------------------------------------------
using System.Globalization;
using System.Text;

namespace MeoBench;

// ══════════════ G.4.5 + G.5.5 — bộ điều khiển có TẠM DỪNG đúng nghĩa ══════════════

public sealed class TrangThaiDoiEventArgs(TrangThaiMay cu, TrangThaiMay moi) : EventArgs
{
    public TrangThaiMay Cu  { get; } = cu;
    public TrangThaiMay Moi { get; } = moi;
}

/// <summary>
/// G.4.5: chỉ lớp này được đổi trạng thái, và mọi lần đổi đều đi qua ĐÚNG MỘT
/// phương thức — nên chỗ phát sự kiện, chỗ ghi log, chỗ chặn chuyển sai đều
/// chỉ có một bản.
/// G.5.5: tạm dừng ở RANH GIỚI BƯỚC, giữ nguyên phôi; chạy tiếp thì đi tiếp
/// từ bước còn dở, KHÔNG đo lại.
/// </summary>
public sealed class BoDieuKhien : IDisposable
{
    private readonly IReadOnlyList<IBuoc> _buocs;
    private readonly BoiCanh _bc;
    private SemaphoreSlim _congTamDung = new(1, 1);
    private bool _daHuy;

    public BoDieuKhien(IReadOnlyList<IBuoc> buocs, BoiCanh bc)
    {
        ArgumentNullException.ThrowIfNull(buocs);
        ArgumentNullException.ThrowIfNull(bc);
        _buocs = buocs; _bc = bc;
    }

    public TrangThaiMay TrangThai { get; private set; } = TrangThaiMay.SanSang;
    public int SoPhoi { get; private set; }
    public int SoBuocDaChay { get; private set; }
    public int ChiSoBuocHienTai { get; private set; }
    public bool DangTamDung { get; private set; }

    public event EventHandler<TrangThaiDoiEventArgs>? TrangThaiDaDoi;

    /// <summary>ĐÚNG MỘT chỗ trong toàn chương trình gán TrangThai.</summary>
    private void DatTrangThai(TrangThaiMay moi)
    {
        if (TrangThai == moi) return;
        var cu = TrangThai;
        TrangThai = moi;
        TrangThaiDaDoi?.Invoke(this, new TrangThaiDoiEventArgs(cu, moi));
    }

    public void TamDung()
    {
        if (DangTamDung || TrangThai != TrangThaiMay.DangChay) return;
        DangTamDung = true;
        _congTamDung.Wait();                 // đóng cổng: bước kế tiếp sẽ phải chờ
        DatTrangThai(TrangThaiMay.TamDung);
    }

    public void ChayTiep()
    {
        if (!DangTamDung) return;
        DangTamDung = false;
        _congTamDung.Release();              // mở cổng
        DatTrangThai(TrangThaiMay.DangChay);
    }

    public async Task ChayAsync(int soChuKy, CancellationToken ct = default)
    {
        DatTrangThai(TrangThaiMay.DangChay);

        for (int i = 0; i < soChuKy; i++)
        {
            _bc.SoHieuPhoi = SoPhoi + 1;
            _bc.KetQua     = default;
            ChiSoBuocHienTai = 0;

            try
            {
                foreach (var buoc in _buocs)
                {
                    // Cổng tạm dừng nằm ở RANH GIỚI giữa hai bước: bước đang
                    // chạy được chạy hết, phôi không bị bỏ dở giữa chừng.
                    await _congTamDung.WaitAsync(ct).ConfigureAwait(false);
                    _congTamDung.Release();

                    ct.ThrowIfCancellationRequested();
                    ChiSoBuocHienTai++;
                    await buoc.ThucThiAsync(_bc, ct).ConfigureAwait(false);
                    SoBuocDaChay++;
                }
                SoPhoi++;
            }
            catch (AlarmException)
            {
                DatTrangThai(TrangThaiMay.BaoDong);
                throw;
            }
            catch (OperationCanceledException)
            {
                DatTrangThai(TrangThaiMay.SanSang);
                throw;
            }
            // Nhánh thứ ba: lỗi KHÔNG lường trước. Không nuốt nó — nhưng cũng
            // không được để máy ở lại trạng thái "đang chạy" trong khi thực tế
            // đã ngừng. Đặt báo động rồi ném tiếp cho tầng trên xử lý.
#pragma warning disable CA1031 // cố ý bắt rộng rồi NÉM LẠI, chỉ để đặt trạng thái an toàn
            catch (Exception)
#pragma warning restore CA1031
            {
                DatTrangThai(TrangThaiMay.BaoDong);
                throw;
            }
        }

        DatTrangThai(TrangThaiMay.SanSang);
    }

    public void Dispose()
    {
        if (_daHuy) return;
        _congTamDung.Dispose();
        _congTamDung = null!;
        _daHuy = true;
    }
}

/// <summary>Bước cố ý ném lỗi LẠ (không phải AlarmException) — để kiểm nhánh thứ ba của G.5.3.</summary>
public sealed class BuocNemLoiLa(int ma) : IBuoc
{
    public int Ma => ma;
    public string Ten => "Bước ném lỗi lạ";
    public Task ThucThiAsync(BoiCanh bc, CancellationToken ct)
        => throw new InvalidOperationException("lỗi lập trình không lường trước");
}

/// <summary>Bước chỉ đếm số lần chạy — để chứng minh tạm dừng không làm chạy lại bước cũ.</summary>
public sealed class BuocDem(int ma, string ten) : IBuoc
{
    public int Ma => ma;
    public string Ten => ten;
    public int SoLanChay { get; private set; }
    public Task ThucThiAsync(BoiCanh bc, CancellationToken ct)
    {
        SoLanChay++;
        return Task.Delay(10, ct);
    }
}

// ══════════════ G.8.2 — nhật ký có cấu trúc ══════════════

public sealed record BanGhiLog(
    DateTime ThoiDiem, MucLog Muc, string Nguon, string Khuon,
    IReadOnlyDictionary<string, object?> ThuocTinh)
{
    /// <summary>Dựng câu chữ từ khuôn — chỉ để người đọc, KHÔNG dùng để tra cứu.</summary>
    public string DungCau()
    {
        var sb = new StringBuilder(Khuon);
        foreach (var kv in ThuocTinh)
            sb.Replace("{" + kv.Key + "}", Convert.ToString(kv.Value, CultureInfo.InvariantCulture));
        return sb.ToString();
    }
}

public interface ICuaRaLog { void Nhan(BanGhiLog ban); }

/// <summary>
/// Ghi log bằng KHUÔN có tham số đặt tên, không nối chuỗi. Nhờ vậy về sau lọc
/// được "mọi lần trục Z quá thời gian" theo THUỘC TÍNH chứ không phải tìm chuỗi.
/// Lớp này không biết gì về giao diện: ai muốn hiện thì tự cắm một cửa ra.
/// </summary>
public sealed class NhatKy(IDongHo dongHo)
{
    private readonly List<ICuaRaLog> _cuaRa = [];

    public MucLog MucToiThieu { get; set; } = MucLog.GoLoi;

    public void ThemCuaRa(ICuaRaLog cuaRa)
    {
        ArgumentNullException.ThrowIfNull(cuaRa);
        _cuaRa.Add(cuaRa);
    }

    public bool BoCuaRa(ICuaRaLog cuaRa) => _cuaRa.Remove(cuaRa);

    public void Ghi(MucLog muc, string nguon, string khuon,
                    params (string Ten, object? GiaTri)[] thuocTinh)
    {
        if (muc < MucToiThieu) return;
        ArgumentNullException.ThrowIfNull(thuocTinh);

        var tt = thuocTinh.ToDictionary(p => p.Ten, p => p.GiaTri, StringComparer.Ordinal);
        var ban = new BanGhiLog(dongHo.BayGio, muc, nguon, khuon, tt);
        foreach (var c in _cuaRa) c.Nhan(ban);
    }
}

/// <summary>Cửa ra giữ trong bộ nhớ — đủ để kiểm thử và để bảng log của G.7.4 cắm vào.</summary>
public sealed class CuaRaBoNho : ICuaRaLog
{
    private readonly List<BanGhiLog> _ban = [];
    public IReadOnlyList<BanGhiLog> TatCa => _ban;
    public void Nhan(BanGhiLog ban) => _ban.Add(ban);

    public IEnumerable<BanGhiLog> Loc(string tenThuocTinh, object giaTri)
        => _ban.Where(b => b.ThuocTinh.TryGetValue(tenThuocTinh, out var v)
                        && Equals(v, giaTri));
}

// ══════════════ G.8.3 — driver cảm biến nối tiếp, kiểm được không cần cổng COM ══════════════

/// <summary>
/// Trừu tượng hoá nguồn byte. Bản thật bọc SerialPort; bản giả trả về từng
/// mảnh do phép kiểm dựng — kể cả mảnh CỐ Ý CẮT GIỮA KHUNG.
/// </summary>
public interface INguonByte
{
    Task<byte[]> DocManhAsync(CancellationToken ct = default);
}

public sealed class NguonByteGia(params byte[][] cacManh) : INguonByte
{
    private readonly Queue<byte[]> _manh = new(cacManh);
    public Task<byte[]> DocManhAsync(CancellationToken ct = default)
        => Task.FromResult(_manh.Count > 0 ? _manh.Dequeue() : []);
}

public sealed class DriverCamBienNoiTiep(
    INguonByte nguon, int soManhToiDa = 50, bool coTongKiem = true) : ICamBienChieuDay
{
    private readonly BoTachKhung _tach = new();

    public int SoKhungHongTongKiem { get; private set; }

    public async Task<double> DocAsync(CancellationToken ct = default)
    {
        for (int i = 0; i < soManhToiDa; i++)
        {
            ct.ThrowIfCancellationRequested();
            byte[] manh = await nguon.DocManhAsync(ct).ConfigureAwait(false);
            if (manh.Length == 0) break;

            foreach (byte[] khung in _tach.Nap(manh))
            {
                byte[] noiDung = khung;
                if (coTongKiem)
                {
                    if (!TongKiem.XacMinh(khung)) { SoKhungHongTongKiem++; continue; }
                    noiDung = khung[..^1];
                }

                string s = Encoding.ASCII.GetString(noiDung).Trim();
                if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
                    return v;
            }
        }
        throw new AlarmException(MaCanhBao.CamBienKhongPhanHoi, "CB_DAY",
                                 "không nhận được khung hợp lệ nào");
    }
}

// ══════════════ G.8.4 — bắt tay hai dây với máy kế tiếp ══════════════

public enum LyDoDung { KhongDung, DoiHang, BiChan }

/// <summary>
/// Hai tín hiệu: "tôi có hàng" (ra) và "máy sau sẵn sàng nhận" (vào).
/// Điều quan trọng không phải là truyền được hàng, mà là PHÂN BIỆT ĐƯỢC
/// vì sao đang dừng: ĐÓI (máy trước hết hàng) hay BỊ CHẶN (máy sau đầy).
/// Hai nguyên nhân này dẫn tới hai hành động sửa chữa hoàn toàn khác nhau.
/// </summary>
public sealed class BatTayHaiDay(
    Func<bool> coPhoiChoSan, Func<bool> maySauSanSang, Action<bool> baoCoHang)
{
    public LyDoDung LyDoDungHienTai { get; private set; } = LyDoDung.KhongDung;
    public int SoGiayDoiHang { get; private set; }
    public int SoGiayBiChan  { get; private set; }
    public int SoPhoiDaChuyen { get; private set; }

    /// <summary>Một nhịp quét. Trả về true nếu vừa chuyển được một phôi.</summary>
    public bool Nhip()
    {
        bool coPhoi = coPhoiChoSan();
        bool sanSang = maySauSanSang();

        if (!coPhoi)
        {
            LyDoDungHienTai = LyDoDung.DoiHang;   // ĐÓI
            SoGiayDoiHang++;
            baoCoHang(false);
            return false;
        }

        if (!sanSang)
        {
            LyDoDungHienTai = LyDoDung.BiChan;    // BỊ CHẶN
            SoGiayBiChan++;
            baoCoHang(true);                      // vẫn giơ cờ "tôi có hàng"
            return false;
        }

        LyDoDungHienTai = LyDoDung.KhongDung;
        baoCoHang(true);
        SoPhoiDaChuyen++;
        return true;
    }
}

// ══════════════ Bộ tự kiểm ══════════════

public static class KiemTrinhTuVaVanHanh2
{
    private static BoiCanh TaoBoiCanh(out TrucGiaLap tz, out TrucGiaLap tx, out KepGiaLap kep)
    {
        tz = new TrucGiaLap("Z", mmMoiBuoc: 20.0);
        tx = new TrucGiaLap("X", mmMoiBuoc: 60.0);
        kep = new KepGiaLap();
        var cdZ = new ChuyenDong(tz, -1.0, 60.0, 1000);
        var cdX = new ChuyenDong(tx, -1.0, 400.0, 1000);
        cdZ.VeGocAsync().GetAwaiter().GetResult();
        cdX.VeGocAsync().GetAwaiter().GetResult();
        return new BoiCanh
        {
            TrucZ = cdZ, TrucX = cdX, Kep = kep,
            CumDo = new CumDo(cdZ, new CamBienGiaLap(11, tyLePhoiLoi: 0.0),
                              new DongHoGia(new DateTime(2026, 9, 20, 8, 0, 0)),
                              30.0, 0.0, 1.950, 2.050, 3),
            MangOkMm = 100.0, MangNgMm = 200.0, ChoMm = 0.0,
        };
    }

    public static async Task Chay()
    {
        // ---------- G.4.5 ----------
        Kiem.MoBai("G.4.5", "Bộ điều khiển — một chỗ duy nhất đổi trạng thái");
        var bc = TaoBoiCanh(out _, out _, out _);
        using (var dk = new BoDieuKhien(ChuKy.BayBuoc(), bc))
        {
            var lichSu = new List<string>();
            dk.TrangThaiDaDoi += (_, e) => lichSu.Add($"{e.Cu}→{e.Moi}");

            await dk.ChayAsync(2);
            Kiem.Bang(dk.SoPhoi, 2, "chạy trọn 2 chu kỳ");
            Kiem.Bang(dk.SoBuocDaChay, 14, "7 bước × 2 chu kỳ = 14 lần thực thi bước");
            Kiem.Bang(string.Join(" ", lichSu), "SanSang→DangChay DangChay→SanSang",
                      "★ mọi lần đổi trạng thái đều phát sự kiện, đúng thứ tự");
            Kiem.Bang(dk.TrangThai, TrangThaiMay.SanSang, "chạy xong về Sẵn sàng");
        }

        // ---------- G.5.3 ----------
        Kiem.MoBai("G.5.3", "Bộ chạy trình tự — ba nhánh lỗi, và chỉ ba");
        // Nhánh 1: AlarmException
        var bc1 = TaoBoiCanh(out var tz1, out _, out _);
        var buocLoi = new List<IBuoc> { new B2Do() };
        tz1.MoPhongTreo = true;
        using (var dk1 = new BoDieuKhien(buocLoi, bc1))
        {
            var ex1 = await Kiem.BatAsync<AlarmException>(() => dk1.ChayAsync(1));
            Kiem.Dung(ex1 is not null, "nhánh 1: sự cố thiết bị → AlarmException lan ra");
            Kiem.Bang(dk1.TrangThai, TrangThaiMay.BaoDong, "★ nhánh 1 → dừng ở BÁO ĐỘNG");
        }

        // Nhánh 2: người bấm Dừng
        var bc2 = TaoBoiCanh(out _, out _, out _);
        using (var dk2 = new BoDieuKhien(ChuKy.BayBuoc(), bc2))
        using (var cts = new CancellationTokenSource())
        {
            var chay = dk2.ChayAsync(1000, cts.Token);
            await Task.Delay(60);
            await cts.CancelAsync();
            await Kiem.NemAsync<OperationCanceledException>(() => chay, "nhánh 2: huỷ → OperationCanceled");
            Kiem.Bang(dk2.TrangThai, TrangThaiMay.SanSang,
                      "★ nhánh 2 → về SẴN SÀNG, KHÔNG phải báo động (không có cảnh báo giả)");
        }

        // Nhánh 3: lỗi lập trình không lường trước
        var bc3 = TaoBoiCanh(out _, out _, out _);
        using (var dk3 = new BoDieuKhien([new BuocNemLoiLa(99)], bc3))
        {
            await Kiem.NemAsync<InvalidOperationException>(() => dk3.ChayAsync(1),
                "★ nhánh 3: lỗi LẠ không bị nuốt — nó phải nổi lên tới chỗ xử lý chung");
            Kiem.Dung(dk3.TrangThai != TrangThaiMay.DangChay,
                      "gặp lỗi lạ thì không được ở lại trạng thái Đang chạy");
        }

        // ---------- G.5.4 ----------
        Kiem.MoBai("G.5.4", "Bảng chuyển trạng thái");
        Kiem.Dung(BangChuyen.ChoPhep(TrangThaiMay.ChuaKhoiTao, LenhMay.KhoiTao), "Chưa khởi tạo + Khởi tạo");
        Kiem.Dung(BangChuyen.ChoPhep(TrangThaiMay.DangVeGoc, LenhMay.VeGocXong), "Đang về gốc + Về gốc xong");
        Kiem.Dung(BangChuyen.ChoPhep(TrangThaiMay.DangChay, LenhMay.Loi), "Đang chạy + Lỗi → hợp lệ");
        Kiem.Dung(!BangChuyen.ChoPhep(TrangThaiMay.ChuaKhoiTao, LenhMay.BatDau),
                  "★ Chưa khởi tạo + Bắt đầu → BỊ TỪ CHỐI");
        Kiem.Dung(!BangChuyen.ChoPhep(TrangThaiMay.BaoDong, LenhMay.TamDung),
                  "Báo động + Tạm dừng → bị từ chối");
        Kiem.Dung(BangChuyen.ThuChuyen(TrangThaiMay.TamDung, LenhMay.ChayTiep, out var den1)
                  && den1 == TrangThaiMay.DangChay, "Tạm dừng + Chạy tiếp → Đang chạy");

        // Duyệt TOÀN BỘ ma trận: mỗi trạng thái phải có ít nhất một lối ra,
        // nếu không thì máy vào đó rồi kẹt vĩnh viễn.
        foreach (var tt in Enum.GetValues<TrangThaiMay>())
        {
            bool coLoiRa = Enum.GetValues<LenhMay>().Any(l => BangChuyen.ChoPhep(tt, l));
            Kiem.Dung(coLoiRa, $"trạng thái {tt} có ít nhất một lối ra (không phải ngõ cụt)");
        }

        // ---------- G.5.5 ----------
        Kiem.MoBai("G.5.5", "Tạm dừng, dừng và chạy tiếp");
        var bc5 = TaoBoiCanh(out _, out _, out _);
        var b1 = new BuocDem(1, "Bước A");
        var b2 = new BuocDem(2, "Bước B");
        var b3 = new BuocDem(3, "Bước C");
        using (var dk5 = new BoDieuKhien([b1, b2, b3], bc5))
        {
            var chay = dk5.ChayAsync(5);
            await Task.Delay(15);                 // đang ở đâu đó giữa chu kỳ đầu
            dk5.TamDung();
            Kiem.Bang(dk5.TrangThai, TrangThaiMay.TamDung, "tạm dừng → trạng thái Tạm dừng");

            // Tạm dừng ở RANH GIỚI BƯỚC: bước đang chạy dở được chạy NỐT rồi mới
            // dừng — đó là chủ ý, để không bỏ phôi ở trạng thái nửa vời.
            await Task.Delay(40);                 // để bước đang bay hoàn tất
            int buocLucDung = dk5.SoBuocDaChay;
            await Task.Delay(80);                 // rồi chờ tiếp để chứng minh nó ĐỨNG YÊN
            Kiem.Bang(dk5.SoBuocDaChay, buocLucDung,
                      "★ sau khi bước đang dở chạy nốt, KHÔNG bước nào chạy thêm nữa");

            dk5.ChayTiep();
            Kiem.Bang(dk5.TrangThai, TrangThaiMay.DangChay, "chạy tiếp → trạng thái Đang chạy");
            await chay;

            Kiem.Bang(dk5.SoPhoi, 5, "chạy tiếp rồi hoàn thành đủ 5 chu kỳ");
            Kiem.Bang(b1.SoLanChay, 5, "★ Bước A chạy đúng 5 lần — tạm dừng KHÔNG làm chạy lại bước cũ");
            Kiem.Bang(b2.SoLanChay, 5, "Bước B chạy đúng 5 lần");
            Kiem.Bang(b3.SoLanChay, 5, "Bước C chạy đúng 5 lần");
        }

        // ---------- G.8.2 ----------
        Kiem.MoBai("G.8.2", "Nhật ký có cấu trúc");
        var dongHo = new DongHoGia(new DateTime(2026, 9, 20, 8, 0, 0));
        var nk = new NhatKy(dongHo);
        var cuaRa = new CuaRaBoNho();
        nk.ThemCuaRa(cuaRa);

        nk.Ghi(MucLog.ThongTin, "TRUC", "Trục {Ten} tới {ViTri} mm", ("Ten", "Z"), ("ViTri", 25.0));
        nk.Ghi(MucLog.Loi,      "TRUC", "Trục {Ten} quá thời gian", ("Ten", "Z"));
        nk.Ghi(MucLog.ThongTin, "TRUC", "Trục {Ten} tới {ViTri} mm", ("Ten", "X"), ("ViTri", 100.0));
        nk.Ghi(MucLog.Loi,      "TRUC", "Trục {Ten} quá thời gian", ("Ten", "Z"));

        Kiem.Bang(cuaRa.TatCa.Count, 4, "cửa ra nhận đủ 4 bản ghi");
        Kiem.Bang(cuaRa.TatCa[0].DungCau(), "Trục Z tới 25 mm", "dựng được câu chữ cho người đọc");
        Kiem.Bang(cuaRa.Loc("Ten", "Z").Count(), 3,
                  "★ lọc theo THUỘC TÍNH Ten='Z' → 3 bản ghi, không phải tìm chuỗi văn bản");
        Kiem.Bang(cuaRa.TatCa.Count(b => b.Muc == MucLog.Loi && b.Khuon.Contains("quá thời gian", StringComparison.Ordinal)), 2,
                  "★ đếm được 'mọi lần trục quá thời gian' bằng KHUÔN, không phụ thuộc giá trị");
        Kiem.Bang(cuaRa.TatCa[0].ThoiDiem, dongHo.BayGio, "thời điểm lấy từ đồng hồ tiêm vào");

        nk.MucToiThieu = MucLog.Loi;
        nk.Ghi(MucLog.GoLoi, "TRUC", "chi tiết gỡ lỗi");
        Kiem.Bang(cuaRa.TatCa.Count, 4, "mức dưới ngưỡng bị bỏ ngay, không tới cửa ra");

        Kiem.Dung(nk.BoCuaRa(cuaRa), "★ bỏ bảng hiển thị chỉ là gỡ một cửa ra — lớp log không đổi");
        nk.MucToiThieu = MucLog.GoLoi;
        nk.Ghi(MucLog.Loi, "TRUC", "sau khi gỡ cửa ra");
        Kiem.Bang(cuaRa.TatCa.Count, 4, "gỡ rồi thì không nhận nữa");

        // ---------- G.8.3 ----------
        Kiem.MoBai("G.8.3", "Driver cảm biến nối tiếp");
        byte STX = 0x02, ETX = 0x03;
        byte[] than = Encoding.ASCII.GetBytes("2.015");
        byte tk = TongKiem.Xor(than);

        // Khung bị cắt làm BA mảnh, mảnh đầu còn dính rác
        var nguon = new NguonByteGia(
            [(byte)'r', (byte)'á', (byte)'c', STX, than[0]],
            [than[1], than[2]],
            [than[3], than[4], tk, ETX]);
        var driver = new DriverCamBienNoiTiep(nguon);
        Kiem.Gan(await driver.DocAsync(), 2.015, 1e-9,
                 "★ khung bị cắt làm ba mảnh, có rác ở đầu → vẫn đọc ra đúng 2,015");

        // Khung sai tổng kiểm phải bị BỎ, rồi khung sau mới được nhận
        byte[] than2 = Encoding.ASCII.GetBytes("9.999");
        var nguon2 = new NguonByteGia(
            [STX, .. than2, (byte)(TongKiem.Xor(than2) ^ 0xFF), ETX],   // tổng kiểm SAI
            [STX, .. than, tk, ETX]);                                    // khung đúng
        var driver2 = new DriverCamBienNoiTiep(nguon2);
        Kiem.Gan(await driver2.DocAsync(), 2.015, 1e-9,
                 "★ khung sai tổng kiểm bị BỎ, không trả về 9,999");
        Kiem.Bang(driver2.SoKhungHongTongKiem, 1, "đếm được số khung hỏng để chẩn đoán đường truyền");

        var driver3 = new DriverCamBienNoiTiep(new NguonByteGia([]));
        await Kiem.NemAsync<AlarmException>(() => driver3.DocAsync(),
            "không có dữ liệu → AlarmException, KHÔNG treo vô hạn");

        // ---------- G.8.4 ----------
        Kiem.MoBai("G.8.4", "Bắt tay hai dây với máy kế tiếp");
        bool coPhoi = false, maySauSanSang = true, coHang = false;
        var bt = new BatTayHaiDay(() => coPhoi, () => maySauSanSang, v => coHang = v);

        for (int i = 0; i < 3; i++) bt.Nhip();
        Kiem.Bang(bt.LyDoDungHienTai, LyDoDung.DoiHang, "không có phôi → ĐÓI");
        Kiem.Bang(bt.SoGiayDoiHang, 3, "đếm được 3 nhịp đói");
        Kiem.Dung(!coHang, "đói thì hạ cờ 'tôi có hàng'");

        coPhoi = true; maySauSanSang = false;
        for (int i = 0; i < 4; i++) bt.Nhip();
        Kiem.Bang(bt.LyDoDungHienTai, LyDoDung.BiChan, "★ có phôi nhưng máy sau đầy → BỊ CHẶN");
        Kiem.Bang(bt.SoGiayBiChan, 4, "đếm được 4 nhịp bị chặn");
        Kiem.Dung(coHang, "bị chặn thì VẪN giơ cờ 'tôi có hàng'");
        Kiem.Bang(bt.SoPhoiDaChuyen, 0, "chưa chuyển được phôi nào");

        maySauSanSang = true;
        Kiem.Dung(bt.Nhip(), "máy sau sẵn sàng → chuyển được phôi");
        Kiem.Bang(bt.SoPhoiDaChuyen, 1, "đếm phôi đã chuyển");
        Kiem.Bang(bt.LyDoDungHienTai, LyDoDung.KhongDung, "đang chạy bình thường");

        Kiem.Dung(bt.SoGiayDoiHang != bt.SoGiayBiChan,
                  "★ hai loại dừng được đếm RIÊNG — đói và bị chặn cần hai cách sửa khác nhau");
    }
}
