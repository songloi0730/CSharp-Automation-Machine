// -------------------------------------------------------
// File:    KiemNguoc.cs
// Project: MeoBench
// Purpose: Phép kiểm sinh ra từ đợt KIỂM NGƯỢC (mục G.14) — đem bất biến của
//          bản mẫu đối chiếu với hình dạng mã máy thật, rồi kiểm lại bản mẫu
//          bằng một bản giả lập KHÔNG hợp tác huỷ (mô hình SDK chặn).
// -------------------------------------------------------

namespace MeoBench;

/// <summary>
/// Bản giả lập trục mô phỏng đúng thứ mà <see cref="TrucGiaLap"/> KHÔNG mô phỏng:
/// một SDK hãng gọi qua P/Invoke, <b>không nhìn CancellationToken</b>.
/// Huỷ token chỉ làm người gọi thôi chờ; trục vẫn chạy tới đích.
/// </summary>
public sealed class TrucKhongHopTac : ITruc
{
    private readonly int _msMoiBuoc;
    private double _dich;

    public TrucKhongHopTac(string ten, int msMoiBuoc = 10)
    {
        Ten = ten;
        _msMoiBuoc = msMoiBuoc;
    }

    public string Ten { get; }
    public double ViTriMm { get; private set; }
    public bool DaVeGoc { get; private set; }

    /// <summary>Trục còn đang chạy hay không — thứ phép kiểm cần nhìn vào.</summary>
    public bool DangChay { get; private set; }

    /// <summary>Số lần nhận lệnh dừng.</summary>
    public int SoLanBiDung { get; private set; }

    public async Task VeGocAsync(CancellationToken ct = default)
    {
        await ChayAsync(0.0).ConfigureAwait(false);
        DaVeGoc = true;
    }

    public Task DiToiAsync(double viTriMm, CancellationToken ct = default)
        => ChayAsync(viTriMm);

    public Task DungAsync(CancellationToken ct = default)
    {
        SoLanBiDung++;
        DangChay = false;               // ĐÂY mới là thứ làm trục dừng
        return Task.CompletedTask;
    }

    // Cố tình KHÔNG nhận ct: mô hình đúng một lời gọi SDK chặn.
    private async Task ChayAsync(double dich)
    {
        _dich = dich;
        DangChay = true;
        while (DangChay && Math.Abs(ViTriMm - _dich) > 1e-6)
        {
            await Task.Delay(_msMoiBuoc).ConfigureAwait(false);   // không có ct
            double delta = Math.Clamp(_dich - ViTriMm, -1.0, 1.0);
            ViTriMm = Math.Round(ViTriMm + delta, 6);
        }
        DangChay = false;
    }
}

/// <summary>Phép kiểm của mục G.14.</summary>
public static class KiemNguoc
{
    /// <summary>Chạy toàn bộ phép kiểm sinh ra từ đợt kiểm ngược.</summary>
    public static async Task Chay()
    {
        await ChayG141().ConfigureAwait(false);
        ChayG142();
    }

    // ── G.14.1 — hết hạn giờ thì PHẢI ra lệnh dừng ───────────────────
    private static async Task ChayG141()
    {
        Kiem.MoBai("G.14.1", "Hết hạn giờ phải RA LỆNH DỪNG, không chỉ thôi chờ");

        // Trục cần 400 bước × 10 ms = 4 giây; hạn giờ 120 ms → chắc chắn quá giờ.
        var truc = new TrucKhongHopTac("TRUC_X", msMoiBuoc: 10);
        await truc.VeGocAsync().ConfigureAwait(false);

        var cd = new ChuyenDong(truc, -1.0, 400.0, hanGioMs: 120);

        var ex = await Kiem.BatAsync<AlarmException>(
            () => cd.DiToiAsync(400.0)).ConfigureAwait(false);

        Kiem.Dung(ex is not null, "quá giờ → ném AlarmException");
        Kiem.Bang(ex!.Ma, MaCanhBao.TrucQuaThoiGian, "đúng mã cảnh báo quá thời gian");

        // ★ Đây là phép kiểm mà bản mẫu TRƯỢT trước đợt kiểm ngược.
        Kiem.Bang(truc.SoLanBiDung, 1, "★ trục ĐÃ nhận đúng một lệnh dừng");
        Kiem.Dung(!truc.DangChay, "★ và trục đã thật sự dừng — không chạy tiếp tới đích");
        Kiem.Dung(truc.ViTriMm < 400.0,
            $"trục dừng giữa đường tại {truc.ViTriMm:F1} mm, không tới 400 mm");

        // Đối chứng: với bản giả lập HỢP TÁC, phép kiểm cũ vẫn xanh dù thiếu lệnh dừng.
        var ngoan = new TrucGiaLap("TRUC_NGOAN", mmMoiBuoc: 1.0, msMoiBuoc: 10);
        await ngoan.VeGocAsync().ConfigureAwait(false);
        var cd2 = new ChuyenDong(ngoan, -1.0, 400.0, hanGioMs: 120);
        _ = await Kiem.BatAsync<AlarmException>(() => cd2.DiToiAsync(400.0)).ConfigureAwait(false);
        Kiem.Dung(!ngoan.DangChay,
            "bản giả lập hợp tác tự dừng khi token bị huỷ — nên nó KHÔNG phát hiện được lỗi");
    }

    // ── G.14.2 — hợp đồng ITruc phải có đường dừng ───────────────────
    private static void ChayG142()
    {
        Kiem.MoBai("G.14.2", "Hợp đồng thiết bị phải có đường DỪNG");

        var ten = typeof(ITruc).GetMethods().Select(m => m.Name).ToList();
        Kiem.Dung(ten.Contains("DungAsync"),
            "★ ITruc có DungAsync — mọi hợp đồng thiết bị chuyển động đều cần một đường dừng");
        Kiem.Dung(ten.Contains("VeGocAsync") && ten.Contains("DiToiAsync"),
            "và vẫn giữ nguyên hai lệnh chuyển động");

        // Bất biến mang đi: đã có lệnh RA thì phải có lệnh THÔI.
        var raLenh = ten.Count(n => n is "VeGocAsync" or "DiToiAsync");
        var thoiLenh = ten.Count(n => n is "DungAsync");
        Kiem.Dung(thoiLenh >= 1 && raLenh >= 1,
            $"{raLenh} lệnh ra, {thoiLenh} lệnh thôi — hợp đồng không có đường lùi là hợp đồng thiếu");
    }
}
