// -------------------------------------------------------
// File:    YeuCauCongNghiep.cs
// Project: MeoBench
// Purpose: Phụ lục L — những yêu cầu chương trình máy công nghiệp phải đáp ứng
//          ngoài "chạy đúng chức năng": biết lần trước chết, mở lại có giới hạn,
//          đo thời gian không sợ chỉnh giờ, canh ổ đĩa, đủ lý do chưa chạy được,
//          mẫu chuẩn, kiểm lại, xác nhận bỏ NG, truy vết yêu cầu.
// -------------------------------------------------------
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace MeoBench;

// ══════════════ 19.5.3 — Cờ phiên chạy ══════════════

/// <summary>Thông tin một phiên chạy, ghi vào cờ phiên lúc khởi động.</summary>
public sealed record ThongTinPhien(int MaTienTrinh, DateTime BatDauLuc, string PhienBan);

/// <summary>Kết quả mở phiên: lần trước có đi hết trình tự tắt không.</summary>
public sealed record KetQuaMoPhien(bool LanTruocTatBatThuong, ThongTinPhien? PhienTruoc);

/// <summary>
/// Cờ phiên chạy: một tệp nhỏ tạo lúc khởi động, xoá ở bước CUỐI của trình tự tắt.
/// Khởi động mà tệp còn đó nghĩa là lần trước không đi hết trình tự tắt — chết, bị
/// tắt cưỡng bức, hoặc mất điện. Không dựa vào mã thoát: tiến trình chết vì ngoại lệ
/// vẫn có thể trả về 0 nếu luồng chính kịp kết thúc trước (mục 19.5.1).
/// </summary>
public sealed class PhienChay(string duongDanCo, IDongHo dongHo)
{
    /// <summary>Mở phiên mới. Gọi một lần, sớm nhất có thể trong trình tự khởi động.</summary>
    public KetQuaMoPhien Mo(int maTienTrinh, string phienBan)
    {
        bool batThuong = File.Exists(duongDanCo);
        var truoc = batThuong ? Doc(duongDanCo) : null;

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(duongDanCo))!);
        var tam = duongDanCo + ".tmp";
        File.WriteAllText(tam, string.Join(';',
            maTienTrinh.ToString(CultureInfo.InvariantCulture),
            dongHo.BayGio.ToString("o", CultureInfo.InvariantCulture),
            phienBan), Encoding.UTF8);
        File.Move(tam, duongDanCo, overwrite: true);
        return new KetQuaMoPhien(batThuong, truoc);
    }

    /// <summary>Đóng phiên đúng cách — bước CUỐI của trình tự tắt, sau khi đầu ra đã an toàn và nhật ký đã ghi xong.</summary>
    public void DongBinhThuong()
    {
        if (File.Exists(duongDanCo)) File.Delete(duongDanCo);
    }

    private static ThongTinPhien? Doc(string duongDan)
    {
        try
        {
            var p = File.ReadAllText(duongDan, Encoding.UTF8).Split(';');
            return new ThongTinPhien(
                int.Parse(p[0], CultureInfo.InvariantCulture),
                DateTime.Parse(p[1], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                p[2]);
        }
        catch (Exception ex) when (ex is FormatException or IndexOutOfRangeException or IOException or OverflowException)
        {
            return null;   // cờ hỏng (mất điện giữa lúc ghi) vẫn là dấu hiệu tắt bất thường
        }
    }
}

// ══════════════ 19.5.4 — Tự mở lại, có giới hạn ══════════════

/// <summary>Quyết định của bộ giám sát sau khi chương trình máy thoát.</summary>
public sealed record QuyetDinhMoLai(bool MoLai, TimeSpan ChoTruoc, string LyDo);

/// <summary>
/// Chính sách tự mở lại. Có giới hạn: chết quá N lần trong một khoảng thì thôi mở lại
/// và chờ người — chương trình chết liên tục là một triệu chứng, mở lại mãi chỉ giấu nó đi.
/// </summary>
public sealed class ChinhSachMoLai(IDongHo dongHo)
{
    /// <summary>Mã thoát của tiến trình .NET chết vì ngoại lệ chưa bắt (0xE0434352).</summary>
    public const int MaThoatNgoaiLeChuaBat = unchecked((int)0xE0434352);

    private readonly List<DateTime> _lanChet = [];

    /// <summary>Số lần chết tối đa trong <see cref="TrongVong"/> mà vẫn còn mở lại.</summary>
    public int SoLanToiDa { get; init; } = 3;

    /// <summary>Cửa sổ thời gian để đếm số lần chết.</summary>
    public TimeSpan TrongVong { get; init; } = TimeSpan.FromMinutes(10);

    /// <summary>Thời gian chờ trước lần mở lại đầu tiên; nhân đôi ở mỗi lần tiếp theo trong cửa sổ.</summary>
    public TimeSpan ChoBanDau { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>Xét sau khi tiến trình con thoát.</summary>
    /// <param name="maThoat">Mã thoát của tiến trình.</param>
    /// <param name="conCoPhien">Cờ phiên còn trên đĩa — chương trình không đi hết trình tự tắt.</param>
    public QuyetDinhMoLai SauKhiThoat(int maThoat, bool conCoPhien)
    {
        if (maThoat == 0 && !conCoPhien)
            return new QuyetDinhMoLai(false, TimeSpan.Zero, "Thoát bình thường — không mở lại");

        var bayGio = dongHo.BayGio;
        _lanChet.RemoveAll(t => bayGio - t > TrongVong);
        _lanChet.Add(bayGio);

        string vi = maThoat == MaThoatNgoaiLeChuaBat ? "ngoại lệ chưa bắt"
                  : maThoat == 0 ? "mã thoát 0 nhưng cờ phiên còn"
                  : "mã thoát " + maThoat.ToString(CultureInfo.InvariantCulture);

        if (_lanChet.Count > SoLanToiDa)
            return new QuyetDinhMoLai(false, TimeSpan.Zero,
                $"Chết {_lanChet.Count} lần trong {TrongVong.TotalMinutes:0} phút ({vi}) — thôi mở lại, cần người kiểm tra");

        var cho = ChoBanDau * Math.Pow(2, _lanChet.Count - 1);
        return new QuyetDinhMoLai(true, cho,
            $"Chết lần {_lanChet.Count} ({vi}) — mở lại sau {cho.TotalSeconds:0} giây");
    }
}

/// <summary>
/// Bộ giám sát tối giản: chạy chương trình máy, chờ nó thoát, hỏi chính sách có mở lại không.
/// Chạy như một tiến trình RIÊNG — chương trình máy chết thì bộ giám sát vẫn sống.
/// </summary>
public sealed class GiamSatTienTrinh(ChinhSachMoLai chinhSach, string duongDanCoPhien, NhatKy nhatKy)
{
    /// <summary>Chạy tới khi chính sách bảo dừng. Trả về số lần đã khởi chạy chương trình máy.</summary>
    public async Task<int> ChayAsync(Func<ProcessStartInfo> taoLenh,
                                     Func<TimeSpan, CancellationToken, Task> cho,
                                     CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(taoLenh);
        ArgumentNullException.ThrowIfNull(cho);
        int soLan = 0;
        while (!ct.IsCancellationRequested)
        {
            soLan++;
            var lenh = taoLenh();
            using var p = Process.Start(lenh)
                ?? throw new InvalidOperationException("Không khởi chạy được chương trình máy");
            // Đã chuyển hướng thì phải đọc hết, nếu không tiến trình con có thể tắc khi bộ đệm đầy
            var docRa = lenh.RedirectStandardOutput ? p.StandardOutput.ReadToEndAsync(ct) : Task.FromResult("");
            var docLoi = lenh.RedirectStandardError ? p.StandardError.ReadToEndAsync(ct) : Task.FromResult("");
            await p.WaitForExitAsync(ct).ConfigureAwait(false);
            await Task.WhenAll(docRa, docLoi).ConfigureAwait(false);

            bool conCoPhien = File.Exists(duongDanCoPhien);
            var qd = chinhSach.SauKhiThoat(p.ExitCode, conCoPhien);
            // Mức nhật ký theo việc chương trình máy có CHẾT không — không theo việc có mở lại không:
            // "chết liên tục, thôi mở lại" là chuyện nặng nhất, không được ghi thành mức Thông tin.
            bool daChet = p.ExitCode != 0 || conCoPhien;
            nhatKy.Ghi(daChet ? MucLog.Loi : MucLog.ThongTin, "GiamSat", "{LyDo}", ("LyDo", qd.LyDo));
            if (!qd.MoLai) break;
            await cho(qd.ChoTruoc, ct).ConfigureAwait(false);
        }
        return soLan;
    }
}

/// <summary>
/// Tiến trình thử cho phép kiểm 19.5: lần chạy đầu mở phiên rồi CHẾT vì ngoại lệ trên
/// luồng nền; lần sau mở phiên, ghi lại "lần trước có tắt bất thường không", rồi tắt đúng cách.
/// </summary>
public static class TienTrinhThu
{
    /// <summary>Điểm vào khi MeoBench được gọi với <c>--chet-lan-dau &lt;cờ phiên&gt; &lt;tệp đếm&gt;</c>.</summary>
    public static int Chay(string duongDanCo, string tepDem)
    {
        int lan = File.Exists(tepDem) ? int.Parse(File.ReadAllText(tepDem), CultureInfo.InvariantCulture) : 0;
        File.WriteAllText(tepDem, (lan + 1).ToString(CultureInfo.InvariantCulture));

        var phien = new PhienChay(duongDanCo, new DongHoHeThong());
        var kq = phien.Mo(Environment.ProcessId, "thu");
        File.WriteAllText(tepDem + ".lan" + (lan + 1).ToString(CultureInfo.InvariantCulture),
            kq.LanTruocTatBatThuong ? "BatThuong" : "BinhThuong");

        if (lan == 0)
        {
            var t = new Thread(() => throw new InvalidOperationException("chết thử trên luồng nền"));
            t.Start();
            t.Join();          // tiến trình kết thúc trong lúc đang chờ ở đây
            return 0;
        }
        phien.DongBinhThuong();
        return 0;
    }
}

// ══════════════ 19.6.1 — Hai loại đồng hồ ══════════════

/// <summary>Đồng hồ ĐƠN ĐIỆU: chỉ đi tới, không bị chỉnh giờ, không nhảy khi đồng bộ giờ.</summary>
public interface IDongHoDonDieu
{
    /// <summary>Số mili-giây từ một mốc bất kỳ. Chỉ có nghĩa khi lấy hiệu hai lần đọc.</summary>
    long MiliGiay { get; }
}

/// <summary>Bản thật: <see cref="Environment.TickCount64"/> — 64 bit, không tràn sau 24,9 ngày như bản 32 bit.</summary>
public sealed class DongHoDonDieuHeThong : IDongHoDonDieu
{
    /// <inheritdoc/>
    public long MiliGiay => Environment.TickCount64;
}

/// <summary>Bản giả cho phép kiểm.</summary>
public sealed class DongHoDonDieuGia : IDongHoDonDieu
{
    /// <inheritdoc/>
    public long MiliGiay { get; private set; }

    /// <summary>Cho thời gian trôi.</summary>
    public void Tien(TimeSpan khoang) => MiliGiay += (long)khoang.TotalMilliseconds;
}

/// <summary>Hạn giờ đo bằng đồng hồ đơn điệu — dùng cho mọi "chờ tối đa X giây".</summary>
public sealed class HanGio
{
    private readonly IDongHoDonDieu _dongHo;
    private readonly long _batDau;
    private readonly long _han;

    /// <summary>Bắt đầu đếm ngay lúc tạo.</summary>
    public HanGio(IDongHoDonDieu dongHo, TimeSpan thoiHan)
    {
        ArgumentNullException.ThrowIfNull(dongHo);
        _dongHo = dongHo;
        _batDau = dongHo.MiliGiay;
        _han = (long)thoiHan.TotalMilliseconds;
    }

    /// <summary>Đã hết hạn chưa.</summary>
    public bool DaHet => _dongHo.MiliGiay - _batDau >= _han;

    /// <summary>Thời gian đã trôi qua kể từ lúc tạo.</summary>
    public TimeSpan DaTroiQua => TimeSpan.FromMilliseconds(_dongHo.MiliGiay - _batDau);
}

/// <summary>
/// Cách SAI — đo khoảng thời gian bằng giờ tường. Giữ ở đây chỉ để phép kiểm chỉ ra
/// nó hỏng thế nào khi đồng hồ hệ thống bị chỉnh (mục 19.6.1).
/// </summary>
public sealed class HanGioTheoGioTuong(IDongHo dongHo, TimeSpan thoiHan)
{
    private readonly DateTime _batDau = dongHo.BayGio;

    /// <summary>Đã hết hạn chưa — theo giờ tường.</summary>
    public bool DaHet => dongHo.BayGio - _batDau >= thoiHan;
}

// ══════════════ 19.6.3 — Ổ đĩa đầy ══════════════

/// <summary>Ba mức của ổ đĩa dữ liệu.</summary>
public enum MucODia { Du, CanhBao, NgungNhanPhoi }

/// <summary>Kết quả xét ổ đĩa.</summary>
public sealed record KetQuaODia(MucODia Muc, long ConTrongByte, double SoNgayConLai, string ThongBao);

/// <summary>Nguồn số liệu ổ đĩa — tách ra để kiểm được ngưỡng mà không phải làm đầy ổ thật.</summary>
public interface IThongTinODia
{
    /// <summary>Dung lượng còn trống (byte) của ổ chứa đường dẫn.</summary>
    long ConTrong(string duongDan);
}

/// <summary>Bản thật đọc bằng <see cref="DriveInfo"/>.</summary>
public sealed class ThongTinODiaHeThong : IThongTinODia
{
    /// <inheritdoc/>
    public long ConTrong(string duongDan)
        => new DriveInfo(Path.GetPathRoot(Path.GetFullPath(duongDan))!).AvailableFreeSpace;
}

/// <summary>
/// Canh ổ đĩa dữ liệu. Ngưỡng tính theo SỐ NGÀY còn chạy được, không theo phần trăm:
/// ổ 1 TB còn 5 % vẫn là 50 GB, ổ 128 GB còn 5 % chỉ còn vài giờ ghi ảnh.
/// </summary>
public sealed class CanhODia(IThongTinODia oDia)
{
    /// <summary>Lượng dữ liệu máy ghi mỗi ngày (đo thật, không đoán) — byte.</summary>
    public long ByteMoiNgay { get; init; } = 2L * 1024 * 1024 * 1024;

    /// <summary>Dưới số ngày này thì cảnh báo — đủ thời gian để người lo việc kịp xử lý.</summary>
    public double SoNgayCanhBao { get; init; } = 7;

    /// <summary>Dưới số ngày này thì ngừng nhận phôi mới — còn chỗ cho phôi đang làm dở và cho nhật ký.</summary>
    public double SoNgayNgung { get; init; } = 1;

    /// <summary>Xét ổ chứa đường dẫn dữ liệu.</summary>
    public KetQuaODia Xet(string duongDan)
    {
        long con = oDia.ConTrong(duongDan);
        double ngay = (double)con / ByteMoiNgay;
        if (ngay < SoNgayNgung)
            return new KetQuaODia(MucODia.NgungNhanPhoi, con, ngay,
                $"Ổ dữ liệu chỉ còn đủ khoảng {ngay * 24:0} giờ — ngừng nhận phôi mới, giải phóng ổ đĩa");
        if (ngay < SoNgayCanhBao)
            return new KetQuaODia(MucODia.CanhBao, con, ngay,
                $"Ổ dữ liệu còn đủ khoảng {ngay:0.0} ngày — cần dọn hoặc chuyển dữ liệu");
        return new KetQuaODia(MucODia.Du, con, ngay, "");
    }
}

/// <summary>
/// Ghi nhật ký mà không bao giờ làm sập máy khi ổ đầy: gặp lỗi "ổ đầy" thì ngừng ghi,
/// đếm số dòng bị mất, báo một lần — rồi để cảnh báo ổ đĩa lo phần còn lại.
/// </summary>
public sealed class GhiKhongSap(Action<string> ghiThat)
{
    /// <summary>HRESULT của ERROR_DISK_FULL.</summary>
    public const int HResultODiaDay = unchecked((int)0x80070070);

    /// <summary>HRESULT của ERROR_HANDLE_DISK_FULL.</summary>
    public const int HResultODiaDay2 = unchecked((int)0x80070027);

    /// <summary>Đã gặp ổ đầy và đang ngừng ghi.</summary>
    public bool DangNgung { get; private set; }

    /// <summary>Số dòng không ghi được từ lúc ngừng.</summary>
    public int SoDongBoMat { get; private set; }

    /// <summary>Phát đúng một lần mỗi khi bắt đầu ngừng ghi.</summary>
    public event EventHandler? BatDauNgung;

    /// <summary>Ghi một dòng.</summary>
    public void Ghi(string dong)
    {
        if (DangNgung) { SoDongBoMat++; return; }
        try { ghiThat(dong); }
        catch (IOException ex) when (ex.HResult is HResultODiaDay or HResultODiaDay2)
        {
            DangNgung = true;
            SoDongBoMat++;
            BatDauNgung?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Gọi khi ổ đĩa đã có lại chỗ (do <see cref="CanhODia"/> báo).</summary>
    public void GhiLai() => DangNgung = false;
}

// ══════════════ 19.6.4 — Tự ghi chỉ số tài nguyên ══════════════

/// <summary>Một lần đọc chỉ số tài nguyên của chính tiến trình.</summary>
public sealed record ChiSoTaiNguyen(long BoNhoRiengByte, int SoHandle, int SoLuong, uint SoGdi, uint SoUser);

internal static class NativeMethods
{
    [DllImport("user32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern uint GetGuiResources(IntPtr hProcess, uint uiFlags);
}

/// <summary>Đọc chỉ số tài nguyên của tiến trình hiện tại — để ghi định kỳ vào nhật ký.</summary>
public static class TaiNguyen
{
    /// <summary>Hạn mức mặc định của Windows cho đối tượng GDI và USER của MỘT tiến trình.</summary>
    public const uint HanMucMacDinh = 10_000;

    /// <summary>Đọc ngay bây giờ.</summary>
    public static ChiSoTaiNguyen Doc()
    {
        using var p = Process.GetCurrentProcess();
        uint gdi = 0, user = 0;
        if (OperatingSystem.IsWindows())
        {
            gdi = NativeMethods.GetGuiResources(p.Handle, 0);    // 0 = GR_GDIOBJECTS
            user = NativeMethods.GetGuiResources(p.Handle, 1);   // 1 = GR_USEROBJECTS
        }
        return new ChiSoTaiNguyen(p.PrivateMemorySize64, p.HandleCount, p.Threads.Count, gdi, user);
    }
}

/// <summary>Nhận diện xu hướng tăng đều — dấu hiệu rò rỉ, khác với răng cưa bình thường của bộ gom rác.</summary>
public static class XuHuong
{
    /// <summary>
    /// Nghi rò rỉ khi phần lớn các bước liên tiếp là tăng VÀ giá trị cuối cao hơn đầu một mức đáng kể.
    /// Bộ nhớ của chương trình khoẻ mạnh lên xuống theo răng cưa; bộ nhớ rò rỉ leo đều.
    /// </summary>
    public static bool NghiRoRi(IReadOnlyList<double> chuoi, double tiLeBuocTang = 0.8, double tangToiThieu = 0.2)
    {
        ArgumentNullException.ThrowIfNull(chuoi);
        if (chuoi.Count < 6 || chuoi[0] <= 0) return false;
        int tang = 0;
        for (int i = 1; i < chuoi.Count; i++) if (chuoi[i] > chuoi[i - 1]) tang++;
        return (double)tang / (chuoi.Count - 1) >= tiLeBuocTang
            && chuoi[^1] >= chuoi[0] * (1 + tangToiThieu);
    }
}

// ══════════════ 15.2.3b — Điều kiện chạy: đủ mọi lý do, một lần ══════════════

/// <summary>Nhóm của một điều kiện chạy — quyết định thứ tự hiện cho người vận hành.</summary>
public enum NhomDieuKien { AnToan = 0, ThietBi = 1, DuLieu = 2, NguoiDung = 3 }

/// <summary>Một lý do máy chưa Start được, kèm việc người vận hành cần làm.</summary>
public sealed record LyDoChuaChay(string Ma, NhomDieuKien Nhom, string MoTa, string CachKhacPhuc);

/// <summary>
/// Bộ điều kiện chạy: xét TẤT CẢ rồi trả về MỌI điều kiện chưa đạt, không dừng ở cái đầu tiên.
/// Người vận hành sửa xong một lý do mới thấy lý do tiếp theo là thói quen tốn thời gian nhất
/// lúc giao ca.
/// </summary>
public sealed class BoDieuKienChay
{
    private readonly List<(string Ma, NhomDieuKien Nhom, string MoTa, string CachKhacPhuc, Func<bool> DaDat)> _ds = [];

    /// <summary>Khai báo một điều kiện.</summary>
    public void Them(string ma, NhomDieuKien nhom, string moTa, string cachKhacPhuc, Func<bool> daDat)
    {
        ArgumentNullException.ThrowIfNull(daDat);
        _ds.Add((ma, nhom, moTa, cachKhacPhuc, daDat));
    }

    /// <summary>Xét hết. Danh sách rỗng nghĩa là được chạy.</summary>
    public IReadOnlyList<LyDoChuaChay> XetTatCa()
    {
        var chua = new List<LyDoChuaChay>();
        foreach (var d in _ds)
        {
            bool dat;
            try { dat = d.DaDat(); }
#pragma warning disable CA1031 // đọc điều kiện lỗi = điều kiện CHƯA đạt; không được làm sập màn hình
            catch (Exception ex)
            {
                chua.Add(new LyDoChuaChay(d.Ma, d.Nhom, $"{d.MoTa} (không đọc được: {ex.Message})", d.CachKhacPhuc));
                continue;
            }
#pragma warning restore CA1031
            if (!dat) chua.Add(new LyDoChuaChay(d.Ma, d.Nhom, d.MoTa, d.CachKhacPhuc));
        }
        return chua.OrderBy(c => c.Nhom).ToList();   // OrderBy ổn định: trong một nhóm giữ thứ tự khai báo
    }
}

// ══════════════ 13.4.9 — Mẫu chuẩn ══════════════

/// <summary>Một mẫu chuẩn: phôi đã biết trước kết luận đúng của nó.</summary>
/// <param name="Ma">Mã mẫu, luôn bắt đầu bằng <see cref="CongMauChuan.TienToMa"/>.</param>
/// <param name="LaMauDat">Mẫu đạt (máy phải cho qua) hay mẫu lỗi (máy phải chặn).</param>
/// <param name="MaLoiMongDoi">Với mẫu lỗi: mã lỗi máy phải báo — chặn đúng lý do, không phải chặn đại.</param>
public sealed record MauChuan(string Ma, bool LaMauDat, string? MaLoiMongDoi);

/// <summary>Vì sao phải kiểm mẫu lại.</summary>
public enum LyDoKiemMau { DauCa, DoiMaHang, SauBaoTri, HetHieuLuc }

/// <summary>Kết quả chạy một mẫu.</summary>
public sealed record KetQuaChayMau(string MaMau, bool KhopMongDoi, string GhiChu);

/// <summary>
/// Cổng mẫu chuẩn: trước khi sản xuất, máy phải chứng minh nó còn phân biệt được đạt và lỗi —
/// mẫu đạt phải qua, mỗi mẫu lỗi phải bị chặn ĐÚNG mã lỗi. Chưa qua cổng thì không được sản xuất.
/// </summary>
public sealed class CongMauChuan(IDongHo dongHo)
{
    /// <summary>Tiền tố mã của mọi mẫu chuẩn — để mẫu không bao giờ bị đếm hay xuất như hàng thật.</summary>
    public const string TienToMa = "MAU-";

    private readonly Dictionary<string, MauChuan> _mau = new(StringComparer.Ordinal);
    private readonly HashSet<string> _daQua = new(StringComparer.Ordinal);
    private DateTime? _hopLeTu;
    private string _lyDoCanKiem = "Chưa kiểm mẫu chuẩn từ lúc khởi động";

    /// <summary>Kết quả kiểm mẫu còn hiệu lực trong bao lâu.</summary>
    public TimeSpan HieuLuc { get; init; } = TimeSpan.FromHours(12);

    /// <summary>Khai báo một mẫu chuẩn.</summary>
    public void KhaiBao(MauChuan mau)
    {
        ArgumentNullException.ThrowIfNull(mau);
        if (!LaMaMauChuan(mau.Ma))
            throw new ArgumentException($"Mã mẫu chuẩn phải bắt đầu bằng '{TienToMa}'", nameof(mau));
        if (!mau.LaMauDat && string.IsNullOrEmpty(mau.MaLoiMongDoi))
            throw new ArgumentException("Mẫu lỗi phải ghi rõ mã lỗi máy phải báo", nameof(mau));
        _mau[mau.Ma] = mau;
    }

    /// <summary>Mã phôi này có phải mẫu chuẩn không — dùng để không đếm nó vào sản lượng.</summary>
    public static bool LaMaMauChuan(string maPhoi)
        => maPhoi is not null && maPhoi.StartsWith(TienToMa, StringComparison.Ordinal);

    /// <summary>Huỷ kết quả kiểm mẫu cũ — đầu ca, đổi mã hàng, sau bảo trì hay hiệu chuẩn.</summary>
    public void YeuCauKiemLai(LyDoKiemMau lyDo)
    {
        _daQua.Clear();
        _hopLeTu = null;
        _lyDoCanKiem = lyDo switch
        {
            LyDoKiemMau.DauCa      => "Đầu ca — cần kiểm mẫu chuẩn",
            LyDoKiemMau.DoiMaHang  => "Vừa đổi mã hàng — cần kiểm mẫu chuẩn",
            LyDoKiemMau.SauBaoTri  => "Vừa bảo trì/hiệu chuẩn — cần kiểm mẫu chuẩn",
            _                      => "Kết quả kiểm mẫu chuẩn đã hết hiệu lực",
        };
    }

    /// <summary>Ghi kết luận máy đưa ra cho một mẫu chuẩn.</summary>
    public KetQuaChayMau GhiKetQua(string maMau, bool ketLuanDat, string? maLoi)
    {
        if (!_mau.TryGetValue(maMau, out var m))
            return new KetQuaChayMau(maMau, false, "Không phải mẫu chuẩn đã khai báo");

        bool khop = m.LaMauDat
            ? ketLuanDat
            : !ketLuanDat && string.Equals(maLoi, m.MaLoiMongDoi, StringComparison.Ordinal);
        if (!khop)
        {
            _daQua.Clear();
            _hopLeTu = null;
            _lyDoCanKiem = m.LaMauDat ? $"Mẫu ĐẠT {maMau} bị máy đánh lỗi ({maLoi})"
                         : ketLuanDat ? $"Máy để LỌT mẫu lỗi {maMau}"
                         : $"Mẫu lỗi {maMau} bị chặn nhưng sai lý do: {maLoi} ≠ {m.MaLoiMongDoi}";
            return new KetQuaChayMau(maMau, false, _lyDoCanKiem);
        }
        _daQua.Add(maMau);
        if (_daQua.Count == _mau.Count) _hopLeTu = dongHo.BayGio;
        return new KetQuaChayMau(maMau, true, "Khớp");
    }

    /// <summary>Được sản xuất chưa. Từ chối thì kèm lý do để hiện cho người vận hành.</summary>
    public KetQuaKiemQuyen ChoPhepSanXuat()
    {
        if (_mau.Count == 0) return new KetQuaKiemQuyen(false, "Chưa khai báo mẫu chuẩn nào");
        if (_hopLeTu is DateTime tu && dongHo.BayGio - tu > HieuLuc) YeuCauKiemLai(LyDoKiemMau.HetHieuLuc);
        if (_hopLeTu is null)
            return new KetQuaKiemQuyen(false,
                $"{_lyDoCanKiem} — còn {_mau.Count - _daQua.Count}/{_mau.Count} mẫu chưa chạy đúng");
        return new KetQuaKiemQuyen(true, null);
    }
}

// ══════════════ 13.4.9 — Kiểm lại có giới hạn ══════════════

/// <summary>Kết quả một lượt kiểm, theo chính sách kiểm lại.</summary>
public sealed record KetQuaLuotKiem(int LanThu, bool Dat, bool ChoKiemTiep, string HuongXuLy);

/// <summary>
/// Sổ kiểm lại: mọi lượt kiểm của một phôi đều được giữ; số lần có giới hạn; tỉ lệ đạt LẦN ĐẦU
/// và tỉ lệ đạt CUỐI là hai con số khác nhau. Bản thật lưu cùng bản ghi sản phẩm — không giữ
/// trong bộ nhớ, vì khởi động lại là mất và phôi lỗi lại được "kiểm lại từ đầu".
/// </summary>
public sealed class SoKiemLai
{
    private readonly Dictionary<string, List<bool>> _luot = new(StringComparer.Ordinal);

    /// <summary>Tổng số lần kiểm tối đa cho một phôi, tính cả lần đầu.</summary>
    public int SoLanToiDa { get; init; } = 2;

    /// <summary>Ghi kết quả một lượt kiểm.</summary>
    public KetQuaLuotKiem Ghi(string maPhoi, bool dat)
    {
        if (!_luot.TryGetValue(maPhoi, out var ds)) { ds = []; _luot[maPhoi] = ds; }
        if (ds.Count > 0 && ds[^1])
            return new KetQuaLuotKiem(ds.Count, true, false, "Phôi đã đạt — không kiểm lại; xử lý như mã trùng (mục 13.4.5)");
        if (ds.Count >= SoLanToiDa)
            return new KetQuaLuotKiem(ds.Count, false, false, "Đã hết số lần kiểm — đưa vào máng NG");

        ds.Add(dat);
        if (dat)
            return new KetQuaLuotKiem(ds.Count, true, false,
                ds.Count == 1 ? "Đạt lần đầu" : $"Đạt ở lần {ds.Count} — bản ghi đánh dấu KIỂM LẠI ĐẠT");
        if (ds.Count >= SoLanToiDa)
            return new KetQuaLuotKiem(ds.Count, false, false, "Không đạt lần cuối — đưa vào máng NG");
        return new KetQuaLuotKiem(ds.Count, false, true, $"Không đạt lần {ds.Count} — được kiểm lại");
    }

    /// <summary>Mọi lượt kiểm của một phôi, theo thứ tự.</summary>
    public IReadOnlyList<bool> LichSu(string maPhoi)
        => _luot.TryGetValue(maPhoi, out var ds) ? ds : [];

    /// <summary>Tỉ lệ phôi đạt ngay lần kiểm đầu.</summary>
    public double TiLeDatLanDau => _luot.Count == 0 ? 0 : (double)_luot.Values.Count(d => d[0]) / _luot.Count;

    /// <summary>Tỉ lệ phôi cuối cùng được cho qua.</summary>
    public double TiLeDatCuoi => _luot.Count == 0 ? 0 : (double)_luot.Values.Count(d => d[^1]) / _luot.Count;
}

// ══════════════ 13.4.9 — Phôi NG phải thực sự rời máy ══════════════

/// <summary>Trạng thái chờ xác nhận phôi NG đã vào máng.</summary>
public enum TrangThaiBoNG { KhongCho, ChoXacNhan, DaXacNhan, QuaHan }

/// <summary>
/// Sau chữ "NG" trên màn hình, phôi phải THỰC SỰ rơi vào máng NG: cảm biến ở máng phải thấy
/// một sườn lên trong thời gian cho phép. Không thấy thì không chạy chu kỳ mới — phôi NG còn
/// trên đường đi là phôi NG sắp lẫn vào hàng đạt.
/// </summary>
public sealed class XacNhanBoNG(IDongHoDonDieu dongHo)
{
    private long _batDau;
    private bool _truoc;

    /// <summary>Thời gian tối đa từ lúc ra lệnh bỏ tới lúc cảm biến máng thấy phôi.</summary>
    public TimeSpan ThoiGianCho { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>Trạng thái hiện tại.</summary>
    public TrangThaiBoNG TrangThai { get; private set; } = TrangThaiBoNG.KhongCho;

    /// <summary>Mã phôi NG đang chờ xác nhận.</summary>
    public string? MaPhoi { get; private set; }

    /// <summary>Người xác nhận tay khi quá hạn (để ghi vết).</summary>
    public string? NguoiXacNhanTay { get; private set; }

    /// <summary>Gọi ngay khi ra lệnh đẩy phôi NG ra máng.</summary>
    public void BatDauCho(string maPhoi)
    {
        TrangThai = TrangThaiBoNG.ChoXacNhan;
        MaPhoi = maPhoi;
        NguoiXacNhanTay = null;
        _batDau = dongHo.MiliGiay;
        _truoc = true;   // coi như đang sáng: cảm biến kẹt sáng sẵn sẽ KHÔNG được tính là đã thấy phôi
    }

    /// <summary>Gọi mỗi vòng quét với tín hiệu cảm biến máng NG.</summary>
    public TrangThaiBoNG CapNhat(bool camBienMang)
    {
        if (TrangThai == TrangThaiBoNG.ChoXacNhan)
        {
            if (camBienMang && !_truoc) TrangThai = TrangThaiBoNG.DaXacNhan;
            else if (dongHo.MiliGiay - _batDau >= (long)ThoiGianCho.TotalMilliseconds) TrangThai = TrangThaiBoNG.QuaHan;
        }
        _truoc = camBienMang;
        return TrangThai;
    }

    /// <summary>Quá hạn thì người vận hành phải tự kiểm và xác nhận — có tên, để ghi vết.</summary>
    public void XacNhanTay(string nguoi)
    {
        if (TrangThai != TrangThaiBoNG.QuaHan) return;
        NguoiXacNhanTay = nguoi;
        TrangThai = TrangThaiBoNG.DaXacNhan;
    }

    /// <summary>Có được chạy chu kỳ mới không.</summary>
    public bool ChoChuKyMoi => TrangThai is TrangThaiBoNG.KhongCho or TrangThaiBoNG.DaXacNhan;
}

// ══════════════ 18.7 — Truy vết yêu cầu ══════════════

/// <summary>Gắn một phép kiểm với mã yêu cầu trong bản đặc tả.</summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class YeuCauAttribute(string ma) : Attribute
{
    /// <summary>Mã yêu cầu, ví dụ "YC-012".</summary>
    public string Ma { get; } = ma;
}

/// <summary>Một dòng của ma trận truy vết: yêu cầu nào, được những phép kiểm nào phủ.</summary>
public sealed record DongTruyVet(string MaYeuCau, string MoTa, IReadOnlyList<string> PhepKiem);

/// <summary>Ma trận truy vết và hai danh sách lỗi của nó.</summary>
public sealed record MaTranTruyVet(
    IReadOnlyList<DongTruyVet> Dong,
    IReadOnlyList<string> YeuCauChuaCoKiem,
    IReadOnlyList<string> PhepKiemTroSai);

/// <summary>Dựng ma trận truy vết từ danh sách yêu cầu và các phép kiểm có gắn <see cref="YeuCauAttribute"/>.</summary>
public static class TruyVet
{
    /// <summary>Dựng ma trận cho các phương thức public static của một lớp kiểm.</summary>
    public static MaTranTruyVet Dung(IReadOnlyDictionary<string, string> yeuCau, Type lopKiem)
    {
        ArgumentNullException.ThrowIfNull(yeuCau);
        ArgumentNullException.ThrowIfNull(lopKiem);
        var theoYeuCau = yeuCau.Keys.ToDictionary(k => k, _ => new List<string>(), StringComparer.Ordinal);
        var troSai = new List<string>();
        foreach (var pt in lopKiem.GetMethods(BindingFlags.Public | BindingFlags.Static))
            foreach (var a in pt.GetCustomAttributes<YeuCauAttribute>())
            {
                if (theoYeuCau.TryGetValue(a.Ma, out var ds)) ds.Add(pt.Name);
                else troSai.Add($"{pt.Name} → {a.Ma}");
            }
        var dong = yeuCau.OrderBy(kv => kv.Key, StringComparer.Ordinal)
                         .Select(kv => new DongTruyVet(kv.Key, kv.Value, theoYeuCau[kv.Key]))
                         .ToList();
        return new MaTranTruyVet(dong, dong.Where(d => d.PhepKiem.Count == 0).Select(d => d.MaYeuCau).ToList(), troSai);
    }

    /// <summary>Xuất CSV để đưa vào hồ sơ nghiệm thu.</summary>
    public static string XuatCsv(MaTranTruyVet maTran)
    {
        ArgumentNullException.ThrowIfNull(maTran);
        var sb = new StringBuilder("MaYeuCau,MoTa,PhepKiem").AppendLine();
        foreach (var d in maTran.Dong)
            sb.Append(d.MaYeuCau).Append(",\"").Append(d.MoTa.Replace("\"", "\"\"", StringComparison.Ordinal))
              .Append("\",").AppendLine(d.PhepKiem.Count == 0 ? "(CHƯA CÓ)" : string.Join(" ", d.PhepKiem));
        return sb.ToString();
    }
}
