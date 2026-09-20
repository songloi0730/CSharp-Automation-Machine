// -------------------------------------------------------
// File:    RapNoi.cs
// Project: MeoBench
// Purpose: Lời giải mẫu G.8.1 (điểm ráp nối) và G.8.5 (ghép tất cả và chạy).
// -------------------------------------------------------
using System.Globalization;

namespace MeoBench;

// ══════════════ Cấu hình — nằm NGOÀI mã, ở đây là một record cho gọn ══════════════

public sealed record CauHinhMay
{
    public bool   GiaLap        { get; init; } = true;
    public int    HatGiongGiaLap{ get; init; } = 12345;
    public double ChieuDayDuoiMm{ get; init; } = 1.950;
    public double ChieuDayTrenMm{ get; init; } = 2.050;
    public int    SoLanDoMoiPhoi{ get; init; } = 3;
    public double ViTriDoMm     { get; init; } = 30.0;
    public double ViTriAnToanMm { get; init; }          // mặc định 0 = vị trí nâng hết
    public double MangOkMm      { get; init; } = 100.0;
    public double MangNgMm      { get; init; } = 200.0;
    public double ViTriChoMm    { get; init; }          // mặc định 0 = vị trí chờ của trục X
    public int    HanGioTrucMs  { get; init; } = 3000;

    public IReadOnlyList<string> KiemTra()
    {
        var loi = new List<string>();
        if (ChieuDayDuoiMm > ChieuDayTrenMm) loi.Add("ChieuDayDuoiMm lớn hơn ChieuDayTrenMm (dải bị đảo)");
        if (SoLanDoMoiPhoi <= 0)             loi.Add("SoLanDoMoiPhoi phải lớn hơn 0");
        if (HanGioTrucMs   <= 0)             loi.Add("HanGioTrucMs phải lớn hơn 0");
        if (MangOkMm == MangNgMm)            loi.Add("Máng OK và máng NG trùng vị trí");
        return loi;
    }
}

// ══════════════ G.8.1 — ĐIỂM RÁP NỐI: nơi DUY NHẤT được `new` thiết bị ══════════════

public sealed class May
{
    public required BoiCanh          BoiCanh    { get; init; }
    public required CauHinhMay       CauHinh    { get; init; }
    public required IReadOnlyList<IBuoc> Buocs  { get; init; }
    public required TrucGiaLap?      TrucZGiaLap{ get; init; }   // chỉ có khi chạy giả lập
    public required CamBienGiaLap?   CamBienGiaLap { get; init; }

    public TrangThaiMay TrangThai { get; private set; } = TrangThaiMay.ChuaKhoiTao;
    public int SoPhoi { get; private set; }
    public int SoOk   { get; private set; }
    public int SoNg   { get; private set; }
    public string? CanhBaoCuoi { get; private set; }

    public event EventHandler<string>? DaBaoCao;
    private void BaoCao(string dong) => DaBaoCao?.Invoke(this, dong);

    public async Task VeGocAsync(CancellationToken ct = default)
    {
        TrangThai = TrangThaiMay.DangVeGoc;
        await BoiCanh.TrucZ.VeGocAsync(ct).ConfigureAwait(false);
        await BoiCanh.TrucX.VeGocAsync(ct).ConfigureAwait(false);
        TrangThai = TrangThaiMay.SanSang;
        BaoCao("Đã về gốc, máy sẵn sàng");
    }

    public async Task ChayAsync(int soChuKy, CancellationToken ct = default)
    {
        if (TrangThai != TrangThaiMay.SanSang)
            throw new InvalidOperationException($"Không thể chạy từ trạng thái {TrangThai}");

        TrangThai = TrangThaiMay.DangChay;

        for (int i = 0; i < soChuKy && !ct.IsCancellationRequested; i++)
        {
            BoiCanh.SoHieuPhoi = SoPhoi + 1;
            BoiCanh.KetQua     = default;

            try
            {
                foreach (var buoc in Buocs)
                {
                    ct.ThrowIfCancellationRequested();
                    await buoc.ThucThiAsync(BoiCanh, ct).ConfigureAwait(false);
                }

                SoPhoi++;
                if (BoiCanh.KetQua.Dat) SoOk++; else SoNg++;
                BaoCao(string.Format(CultureInfo.InvariantCulture,
                    "Chu kỳ {0,3} · {1:F3} mm · {2}", SoPhoi, BoiCanh.KetQua.ChieuDayMm, BoiCanh.KetQua.KetLuan));
            }
            catch (AlarmException ex)                      // (1) sự cố có mã — dừng ở báo động
            {
                TrangThai   = TrangThaiMay.BaoDong;
                CanhBaoCuoi = ex.DongHienThi;
                BaoCao(ex.DongHienThi);
                return;
            }
            catch (OperationCanceledException)             // (2) người bấm Dừng — thoát êm
            {
                TrangThai = TrangThaiMay.SanSang;
                BaoCao("Dừng theo yêu cầu người vận hành");
                return;
            }
#pragma warning disable CA1031 // cố ý bắt rộng: lỗi bất ngờ vẫn phải đưa máy về trạng thái an toàn
            catch (Exception ex)                           // (3) lỗi bất ngờ — báo động nghiêm trọng
#pragma warning restore CA1031
            {
                TrangThai   = TrangThaiMay.BaoDong;
                CanhBaoCuoi = $"LỖI KHÔNG LƯỜNG TRƯỚC: {ex.GetType().Name} — {ex.Message}";
                BaoCao(CanhBaoCuoi);
                return;
            }
        }

        TrangThai = TrangThaiMay.SanSang;
        BaoCao($"Dừng bình thường sau {SoPhoi} phôi");
    }
}

public static class RapNoi
{
    /// <summary>
    /// Nơi DUY NHẤT trong toàn chương trình được `new` một lớp thiết bị.
    /// Đổi cờ GiaLap là đổi toàn máy — không có cờ thật/giả nào rải ở lớp khác.
    /// </summary>
    public static May Tao(CauHinhMay ch, IDongHo? dongHo = null)
    {
        ArgumentNullException.ThrowIfNull(ch);

        var loi = ch.KiemTra();
        if (loi.Count > 0)
            throw new ArgumentException("Cấu hình sai: " + string.Join("; ", loi), nameof(ch));

        dongHo ??= new DongHoHeThong();

        ITruc            trucZ;
        ITruc            trucX;
        ICamBienChieuDay camBien;
        IKep             kep;
        TrucGiaLap?      zGiaLap    = null;
        CamBienGiaLap?   cbGiaLap   = null;

        if (ch.GiaLap)
        {
            zGiaLap  = new TrucGiaLap("Z", mmMoiBuoc: 10.0);
            cbGiaLap = new CamBienGiaLap(ch.HatGiongGiaLap, ch.SoLanDoMoiPhoi);
            trucZ    = zGiaLap;
            trucX    = new TrucGiaLap("X", mmMoiBuoc: 40.0);
            camBien  = cbGiaLap;
            kep      = new KepGiaLap();
        }
        else
        {
            // Chỗ duy nhất cần sửa khi có phần cứng thật.
            // Ném rõ ràng thay vì trả về null: thà chết lúc khởi động còn hơn chết giữa chu kỳ.
            throw new NotSupportedException(
                "Chưa nối phần cứng thật. Thay các lớp giả lập bằng driver thật tại RapNoi.Tao().");
        }

        var cdZ = new ChuyenDong(trucZ, -1.0,  60.0, ch.HanGioTrucMs);
        var cdX = new ChuyenDong(trucX, -1.0, 400.0, ch.HanGioTrucMs);

        var boiCanh = new BoiCanh
        {
            TrucZ = cdZ,
            TrucX = cdX,
            Kep   = kep,
            CumDo = new CumDo(cdZ, camBien, dongHo,
                              ch.ViTriDoMm, ch.ViTriAnToanMm,
                              ch.ChieuDayDuoiMm, ch.ChieuDayTrenMm, ch.SoLanDoMoiPhoi),
            MangOkMm = ch.MangOkMm,
            MangNgMm = ch.MangNgMm,
            ChoMm    = ch.ViTriChoMm,
        };

        return new May
        {
            BoiCanh       = boiCanh,
            CauHinh       = ch,
            Buocs         = ChuKy.BayBuoc(),
            TrucZGiaLap   = zGiaLap,
            CamBienGiaLap = cbGiaLap,
        };
    }
}

// ══════════════ Bộ tự kiểm ══════════════

public static class KiemRapNoi
{
    public static async Task Chay()
    {
        // ---------- G.8.1 ----------
        Kiem.MoBai("G.8.1", "Điểm ráp nối");
        var may = RapNoi.Tao(new CauHinhMay { HatGiongGiaLap = 2026 });
        Kiem.Bang(may.TrangThai, TrangThaiMay.ChuaKhoiTao, "máy mới ráp thì chưa khởi tạo");
        Kiem.Bang(may.Buocs.Count, 7, "ráp đủ 7 bước");
        Kiem.Bang(may.CamBienGiaLap!.HatGiong, 2026, "hạt giống từ cấu hình đi tới tận cảm biến");

        Kiem.Nem<ArgumentException>(
            () => RapNoi.Tao(new CauHinhMay { ChieuDayDuoiMm = 3.0, ChieuDayTrenMm = 1.0 }),
            "cấu hình dải đảo bị chặn NGAY LÚC RÁP, không đợi tới lúc chạy");
        Kiem.Nem<ArgumentException>(
            () => RapNoi.Tao(new CauHinhMay { SoLanDoMoiPhoi = 0 }),
            "số lần đo bằng 0 bị chặn ngay lúc ráp");
        Kiem.Nem<NotSupportedException>(
            () => RapNoi.Tao(new CauHinhMay { GiaLap = false }),
            "bật cờ thiết bị thật → báo rõ ràng ở MỘT chỗ, không lỗi mơ hồ giữa chu kỳ");

        // ---------- G.8.5 ----------
        Kiem.MoBai("G.8.5", "Ghép tất cả và chạy");

        // (1) chạy 50 chu kỳ trọn vẹn
        var m1 = RapNoi.Tao(new CauHinhMay { HatGiongGiaLap = 777 },
                            new DongHoGia(new DateTime(2026, 9, 20, 8, 0, 0)));
        await m1.VeGocAsync();
        Kiem.Bang(m1.TrangThai, TrangThaiMay.SanSang, "về gốc xong thì Sẵn sàng");
        await m1.ChayAsync(50);
        Kiem.Bang(m1.SoPhoi, 50, "chạy trọn 50 chu kỳ");
        Kiem.Bang(m1.SoOk + m1.SoNg, 50, "mọi phôi đều được phân loại");
        Kiem.Dung(m1.SoNg > 0, "có phôi NG (giả lập sinh phôi lỗi ~10 %)");
        Kiem.Bang(m1.TrangThai, TrangThaiMay.SanSang, "chạy xong quay về Sẵn sàng");

        // (2) TÁI HIỆN ĐƯỢC: cùng hạt giống → cùng kết quả
        var m2 = RapNoi.Tao(new CauHinhMay { HatGiongGiaLap = 777 },
                            new DongHoGia(new DateTime(2026, 9, 20, 8, 0, 0)));
        await m2.VeGocAsync();
        await m2.ChayAsync(50);
        Kiem.Bang(m2.SoOk, m1.SoOk, "★ cùng hạt giống → CÙNG số phôi OK (chạy lại được)");
        Kiem.Bang(m2.SoNg, m1.SoNg, "★ cùng hạt giống → cùng số phôi NG");

        // (3) hạt giống khác → kết quả khác
        var m3 = RapNoi.Tao(new CauHinhMay { HatGiongGiaLap = 31337 });
        await m3.VeGocAsync();
        await m3.ChayAsync(50);
        Kiem.Dung(m3.SoNg != m1.SoNg || m3.SoOk != m1.SoOk, "khác hạt giống → khác kết quả");

        // (4) sự cố giữa chừng: máy dừng ở báo động, cơ cấu về trạng thái an toàn
        var m4 = RapNoi.Tao(new CauHinhMay { HatGiongGiaLap = 5 });
        await m4.VeGocAsync();
        m4.CamBienGiaLap!.MoPhongKhongPhanHoi = true;
        await m4.ChayAsync(10);
        Kiem.Bang(m4.TrangThai, TrangThaiMay.BaoDong, "cảm biến lỗi → máy dừng ở Báo động");
        Kiem.Dung(m4.CanhBaoCuoi?.Contains("[20001]", StringComparison.Ordinal) == true,
                  "cảnh báo ghi đúng mã 20001");
        Kiem.Gan(m4.TrucZGiaLap!.ViTriMm, 0.0, 1e-6, "★ có sự cố, trục Z VẪN ở vị trí an toàn");
        Kiem.Dung(!m4.BoiCanh.Kep.DangKep, "★ có sự cố, kẹp KHÔNG bị bỏ ở trạng thái đang kẹp");

        // (5) người bấm Dừng giữa chu kỳ: thoát êm, KHÔNG cảnh báo giả
        var m5 = RapNoi.Tao(new CauHinhMay { HatGiongGiaLap = 9 });
        await m5.VeGocAsync();
        using (var cts = new CancellationTokenSource())
        {
            var chay = m5.ChayAsync(1000, cts.Token);
            await Task.Delay(120);
            await cts.CancelAsync();
            await chay;
        }
        Kiem.Bang(m5.TrangThai, TrangThaiMay.SanSang, "bấm Dừng → về Sẵn sàng, KHÔNG phải Báo động");
        Kiem.Dung(m5.CanhBaoCuoi is null, "★ bấm Dừng KHÔNG sinh cảnh báo giả");
        Kiem.Dung(m5.SoPhoi > 0, "các phôi đã xong trước khi dừng vẫn được tính");

        // (6) không cho chạy từ trạng thái sai
        Kiem.Nem<InvalidOperationException>(
            () => RapNoi.Tao(new CauHinhMay()).ChayAsync(1).GetAwaiter().GetResult(),
            "chạy khi chưa về gốc → bị từ chối");
    }
}
