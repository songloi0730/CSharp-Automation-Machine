// -------------------------------------------------------
// File:    VanHanhThuc.cs
// Project: MeoBench
// Purpose: Những năng lực mà PHẦN MỀM MÁY THẬT nào cũng có, đo trên 13 dự án,
//          mà bản mẫu 40 bài + phần ghép G.11 còn thiếu.
//
// Xếp theo số dự án có (đo được, không phải phỏng đoán):
//   13/13  phân quyền theo mức người dùng
//   13/13  đếm giờ chạy / đếm tuổi thọ linh kiện
//   12/13  thử lại khi thiết bị lỗi thoáng qua
//   11/13  chạy tay / jog trục
//    9/13  truy xuất nguồn gốc (số sê-ri từng phôi)
//    7/13  đèn tháp và còi
//    7/13  sao lưu / khôi phục cấu hình
//    5/13  watchdog phát hiện chu kỳ treo
//    3/13  lịch sử cảnh báo lưu ra file
//    0/13  vết kiểm toán — KHÔNG dự án nào có, và đó là một thiếu sót
//
// CHƯA cài trong bản mẫu, và nói rõ vì sao (xem Phụ lục G mục G.13.11):
//    7/13  đa ngôn ngữ          — thuộc tầng giao diện, không dạy thêm gì ở đây
//    7/13  giao tiếp MES/host   — Chương 14 đã bàn kỹ, cài lại là lặp
//    4/13  chạy từng bước       — bài tập mở rộng, khuôn đã có sẵn ở G.5.5
// -------------------------------------------------------
using System.Globalization;
using System.Text;

namespace MeoBench;

// ══════════════ 13/13 — Mức người dùng và phân quyền ══════════════

public enum MucNguoiDung { ChuaDangNhap = 0, VanHanh = 1, KyThuat = 2, QuanTri = 3 }

public sealed record KetQuaKiemQuyen(bool ChoPhep, string? LyDoTuChoi);

/// <summary>
/// Không giữ mật khẩu ở đây — phần xác thực là việc của hệ thống khác. Lớp này
/// chỉ trả lời câu hỏi "mức hiện tại có được làm việc này không", và đó là thứ
/// tầng nghiệp vụ cần (mục 15.2.3).
/// </summary>
public sealed class PhienDangNhap(IDongHo dongHo)
{
    public MucNguoiDung Muc     { get; private set; } = MucNguoiDung.ChuaDangNhap;
    public string       Ten     { get; private set; } = "";
    public DateTime?    LucVao  { get; private set; }

    /// <summary>Tự hạ quyền sau khoảng không thao tác — người vận hành hay quên đăng xuất.</summary>
    public TimeSpan HetHanSauKhiKhongThaoTac { get; init; } = TimeSpan.FromMinutes(15);
    private DateTime _thaoTacCuoi;

    public void DangNhap(string ten, MucNguoiDung muc)
    {
        Ten = ten; Muc = muc; LucVao = dongHo.BayGio; _thaoTacCuoi = dongHo.BayGio;
    }

    public void DangXuat() { Ten = ""; Muc = MucNguoiDung.ChuaDangNhap; LucVao = null; }

    public void GhiNhanThaoTac() => _thaoTacCuoi = dongHo.BayGio;

    public MucNguoiDung MucHienTai()
    {
        if (Muc != MucNguoiDung.ChuaDangNhap
            && dongHo.BayGio - _thaoTacCuoi > HetHanSauKhiKhongThaoTac)
            DangXuat();
        return Muc;
    }

    public KetQuaKiemQuyen Kiem(MucNguoiDung mucCan, string viec)
    {
        var m = MucHienTai();
        if (m >= mucCan) { GhiNhanThaoTac(); return new KetQuaKiemQuyen(true, null); }
        return new KetQuaKiemQuyen(false,
            m == MucNguoiDung.ChuaDangNhap
                ? $"Cần đăng nhập mức {mucCan} để {viec}"
                : $"Mức {m} không đủ quyền {viec} (cần {mucCan})");
    }
}

// ══════════════ 0/13 — Vết kiểm toán: KHÔNG dự án nào có ══════════════

public sealed record BanGhiKiemToan(
    DateTime ThoiDiem, string NguoiDung, string Viec, string Truoc, string Sau);

/// <summary>
/// Ai đổi thông số gì, lúc nào, từ giá trị nào sang giá trị nào.
/// Đây là năng lực mà **không dự án nào trong bộ mẫu có** — và là thứ đầu tiên
/// bị hỏi khi một lô hàng bị trả về: "hôm đó ai sửa công thức?".
/// </summary>
public sealed class VetKiemToan(string duongDan, IDongHo dongHo)
{
    private readonly List<BanGhiKiemToan> _ban = [];
    public IReadOnlyList<BanGhiKiemToan> TatCa => _ban;

    public void Ghi(string nguoiDung, string viec, string truoc, string sau)
    {
        var b = new BanGhiKiemToan(dongHo.BayGio, nguoiDung, viec, truoc, sau);
        _ban.Add(b);

        Directory.CreateDirectory(Path.GetDirectoryName(duongDan)!);
        bool moi = !File.Exists(duongDan);
        var sb = new StringBuilder();
        if (moi) sb.AppendLine("ThoiDiem,NguoiDung,Viec,Truoc,Sau");
        sb.Append(b.ThoiDiem.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)).Append(',')
          .Append(Thoat(b.NguoiDung)).Append(',').Append(Thoat(b.Viec)).Append(',')
          .Append(Thoat(b.Truoc)).Append(',').Append(Thoat(b.Sau));
        File.AppendAllText(duongDan, sb.ToString() + Environment.NewLine, Encoding.UTF8);
    }

    private static string Thoat(string s)
        => s.Contains(',', StringComparison.Ordinal) ? "\"" + s.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"" : s;
}

// ══════════════ 13/13 — Giờ chạy và tuổi thọ linh kiện ══════════════

public sealed record TuoiThoLinhKien(string Ten, long SoLanDaDung, long NguongThayThe)
{
    public double PhanTramDaDung => NguongThayThe <= 0 ? 0 : Math.Round(SoLanDaDung * 100.0 / NguongThayThe, 1);
    public bool   SapDenHan      => PhanTramDaDung >= 80.0;
    public bool   DaQuaHan       => SoLanDaDung >= NguongThayThe;
}

public sealed class SoBaoTri
{
    private readonly Dictionary<string, TuoiThoLinhKien> _bang = new(StringComparer.Ordinal);

    public void KhaiBao(string ten, long nguongThayThe)
        => _bang[ten] = new TuoiThoLinhKien(ten, 0, nguongThayThe);

    public void Dem(string ten, long soLan = 1)
    {
        if (!_bang.TryGetValue(ten, out var t)) return;
        _bang[ten] = t with { SoLanDaDung = t.SoLanDaDung + soLan };
    }

    public void DaThayThe(string ten)
    {
        if (_bang.TryGetValue(ten, out var t)) _bang[ten] = t with { SoLanDaDung = 0 };
    }

    public IReadOnlyCollection<TuoiThoLinhKien> TatCa => _bang.Values;
    public IReadOnlyList<TuoiThoLinhKien> CanChuY
        => [.. _bang.Values.Where(t => t.SapDenHan)];
}

// ══════════════ 12/13 — Thử lại khi thiết bị lỗi thoáng qua ══════════════

public sealed record KetQuaThuLai<T>(bool ThanhCong, T? GiaTri, int SoLanThu, AlarmException? LoiCuoi);

public static class ThuLai
{
    /// <summary>
    /// Chỉ thử lại với lỗi ĐƯỢC PHÉP thử lại. Thử lại một lỗi vĩnh viễn (sai
    /// cấu hình, hết hành trình) chỉ làm chậm việc phát hiện ra nó.
    /// </summary>
    public static async Task<KetQuaThuLai<T>> ChayAsync<T>(
        Func<CancellationToken, Task<T>> viec,
        Func<AlarmException, bool> coThuLaiDuoc,
        int soLanToiDa = 3, int nghiBanDauMs = 10,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(viec);
        ArgumentNullException.ThrowIfNull(coThuLaiDuoc);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(soLanToiDa);

        AlarmException? cuoi = null;
        for (int lan = 1; lan <= soLanToiDa; lan++)
        {
            try
            {
                return new KetQuaThuLai<T>(true, await viec(ct).ConfigureAwait(false), lan, null);
            }
            catch (AlarmException ex) when (coThuLaiDuoc(ex) && lan < soLanToiDa)
            {
                cuoi = ex;
                // Lùi dần: 10 ms, 20 ms, 40 ms… để không dồn dập lên thiết bị đang lỗi.
                await Task.Delay(nghiBanDauMs * (1 << (lan - 1)), ct).ConfigureAwait(false);
            }
            catch (AlarmException ex)
            {
                return new KetQuaThuLai<T>(false, default, lan, ex);   // lỗi vĩnh viễn → bỏ ngay
            }
        }
        return new KetQuaThuLai<T>(false, default, soLanToiDa, cuoi);
    }

    /// <summary>Mã nào đáng thử lại: lỗi truyền thông thoáng qua. Không gồm lỗi cơ khí.</summary>
    public static bool MacDinhCoThuLai(AlarmException ex)
    {
        ArgumentNullException.ThrowIfNull(ex);
        return ex.Ma == MaCanhBao.CamBienKhongPhanHoi;
    }
}

// ══════════════ 11/13 — Chạy tay và jog trục ══════════════

public sealed record KetQuaJog(bool DaChay, double ViTriSauMm, string? LyDoTuChoi);

public sealed class ChayTay(ChuyenDong truc, PhienDangNhap phien, IAnToanChiDoc anToan)
{
    public double BuocJogMm { get; init; } = 1.0;

    /// <summary>
    /// Ba cửa phải qua, theo đúng thứ tự: quyền → an toàn → giới hạn hành trình.
    /// Thứ tự này có chủ ý: từ chối vì thiếu quyền là thông báo rõ nhất cho
    /// người vận hành, và nó không cần đọc thiết bị.
    /// </summary>
    public async Task<KetQuaJog> JogAsync(int soBuoc, CancellationToken ct = default)
    {
        var quyen = phien.Kiem(MucNguoiDung.KyThuat, "jog trục");
        if (!quyen.ChoPhep) return new KetQuaJog(false, truc.Truc.ViTriMm, quyen.LyDoTuChoi);

        if (anToan.DungKhanDangNhan)
            return new KetQuaJog(false, truc.Truc.ViTriMm, "Dừng khẩn đang nhấn");
        if (anToan.ManChanBiChe)
            return new KetQuaJog(false, truc.Truc.ViTriMm, "Màn chắn sáng đang bị che");

        double dich = truc.Truc.ViTriMm + soBuoc * BuocJogMm;
        try
        {
            await truc.DiToiAsync(dich, ct).ConfigureAwait(false);
            return new KetQuaJog(true, truc.Truc.ViTriMm, null);
        }
        catch (ArgumentOutOfRangeException)
        {
            return new KetQuaJog(false, truc.Truc.ViTriMm,
                string.Format(CultureInfo.InvariantCulture, "{0:F1} mm nằm ngoài hành trình", dich));
        }
    }
}

// ══════════════ 7/13 — Đèn tháp và còi ══════════════

public enum MauDen { Tat, Xanh, Vang, Do }

public sealed record TrangThaiDenThap(MauDen Den, bool NhapNhay, bool Coi);

public static class DenThap
{
    /// <summary>
    /// Quy ước phổ biến trong nhà máy điện tử. Điều quan trọng không phải màu
    /// nào mà là: **một trạng thái máy cho ra đúng một tổ hợp đèn**, và bảng
    /// này là nơi DUY NHẤT quyết định điều đó.
    /// </summary>
    public static TrangThaiDenThap Theo(TrangThaiMay tt, bool coCanhBaoChuaXacNhan) => tt switch
    {
        TrangThaiMay.DangChay    => new TrangThaiDenThap(MauDen.Xanh, false, false),
        TrangThaiMay.SanSang     => new TrangThaiDenThap(MauDen.Vang, false, false),
        TrangThaiMay.TamDung     => new TrangThaiDenThap(MauDen.Vang, true,  false),
        TrangThaiMay.DangVeGoc   => new TrangThaiDenThap(MauDen.Vang, true,  false),
        TrangThaiMay.BaoDong     => new TrangThaiDenThap(MauDen.Do,   true,  coCanhBaoChuaXacNhan),
        TrangThaiMay.ChuaKhoiTao => new TrangThaiDenThap(MauDen.Tat,  false, false),
        _                        => new TrangThaiDenThap(MauDen.Tat,  false, false),
    };
}

// ══════════════ 9/13 — Truy xuất nguồn gốc ══════════════

public sealed record SoSeriPhoi
{
    public const char DauPhanCach = '_';

    /// <summary>
    /// Bản mẫu đầu tiên dùng dấu '-' làm dấu phân cách, và nó SAI: mã máy
    /// "MEOBENCH-01" và mã ca "CA-A-20260920" đều chứa dấu '-', nên ghép xong
    /// KHÔNG tách ngược lại được. Phép kiểm bắt được, và bài học là:
    /// **một mã truy xuất phải tách ngược được, nên dấu phân cách phải là ký tự
    /// BỊ CẤM bên trong từng trường — và hàm dựng phải từ chối giá trị vi phạm.**
    /// </summary>
    public SoSeriPhoi(string maMay, string maCa, int soThuTu)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(maMay);
        ArgumentException.ThrowIfNullOrWhiteSpace(maCa);
        ArgumentOutOfRangeException.ThrowIfNegative(soThuTu);
        if (maMay.Contains(DauPhanCach, StringComparison.Ordinal))
            throw new ArgumentException($"Mã máy không được chứa '{DauPhanCach}'", nameof(maMay));
        if (maCa.Contains(DauPhanCach, StringComparison.Ordinal))
            throw new ArgumentException($"Mã ca không được chứa '{DauPhanCach}'", nameof(maCa));

        MaMay = maMay; MaCa = maCa; SoThuTu = soThuTu;
    }

    public string MaMay   { get; }
    public string MaCa    { get; }
    public int    SoThuTu { get; }

    /// <summary>Ghép được, tách được, và sắp xếp được — ba thứ một mã truy xuất phải có.</summary>
    public string Ma => string.Create(CultureInfo.InvariantCulture,
        $"{MaMay}{DauPhanCach}{MaCa}{DauPhanCach}{SoThuTu:D6}");

    public static bool ThuTach(string ma, out SoSeriPhoi? ra)
    {
        ra = null;
        if (ma is null) return false;
        string[] p = ma.Split(DauPhanCach);
        if (p.Length != 3) return false;
        if (!int.TryParse(p[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int stt)) return false;
        if (string.IsNullOrWhiteSpace(p[0]) || string.IsNullOrWhiteSpace(p[1])) return false;
        ra = new SoSeriPhoi(p[0], p[1], stt);
        return true;
    }
}

// ══════════════ 5/13 — Watchdog phát hiện chu kỳ treo ══════════════

public sealed class WatchdogChuKy(IDongHo dongHo, TimeSpan nguongTreo)
{
    private DateTime _nhipCuoi;
    public int SoLanBaoTreo { get; private set; }

    public void Nhip() => _nhipCuoi = dongHo.BayGio;

    public bool DangTreo
    {
        get
        {
            if (_nhipCuoi == default) return false;
            bool treo = dongHo.BayGio - _nhipCuoi > nguongTreo;
            return treo;
        }
    }

    public AlarmException? Kiem()
    {
        if (!DangTreo) return null;
        SoLanBaoTreo++;
        var ex = new AlarmException(MaCanhBao.TrucQuaThoiGian, "WATCHDOG",
            string.Format(CultureInfo.InvariantCulture,
                "không có nhịp chu kỳ trong {0:F0} giây", (dongHo.BayGio - _nhipCuoi).TotalSeconds));
        _nhipCuoi = dongHo.BayGio;    // báo một lần rồi tính lại, tránh lũ cảnh báo
        return ex;
    }
}

// ══════════════ 3/13 — Lịch sử cảnh báo lưu ra file ══════════════

public sealed record BanGhiCanhBao(DateTime ThoiDiem, int Ma, string ViTri, string ThongDiep, DateTime? LucXacNhan);

public sealed class LichSuCanhBao(string duongDan, IDongHo dongHo)
{
    private readonly List<BanGhiCanhBao> _ban = [];
    public IReadOnlyList<BanGhiCanhBao> TatCa => _ban;

    public void Phat(AlarmException ex)
    {
        ArgumentNullException.ThrowIfNull(ex);
        _ban.Add(new BanGhiCanhBao(dongHo.BayGio, ex.Ma, ex.ViTri, ex.Message, null));
        Luu();
    }

    public bool XacNhan(int ma)
    {
        for (int i = _ban.Count - 1; i >= 0; i--)
        {
            if (_ban[i].Ma == ma && _ban[i].LucXacNhan is null)
            {
                _ban[i] = _ban[i] with { LucXacNhan = dongHo.BayGio };
                Luu();
                return true;
            }
        }
        return false;
    }

    /// <summary>Mã nào hay nổ nhất — câu hỏi đầu tiên khi cải thiện độ ổn định.</summary>
    public IReadOnlyList<(int Ma, int SoLan)> XepTheoTanSuat()
        => [.. _ban.GroupBy(b => b.Ma).Select(g => (g.Key, g.Count())).OrderByDescending(x => x.Item2)];

    private void Luu()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(duongDan)!);
        var sb = new StringBuilder("ThoiDiem,Ma,ViTri,ThongDiep,LucXacNhan").AppendLine();
        foreach (var b in _ban)
            sb.Append(b.ThoiDiem.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)).Append(',')
              .Append(b.Ma.ToString(CultureInfo.InvariantCulture)).Append(',')
              .Append(b.ViTri).Append(',').Append(b.ThongDiep.Replace(',', ';')).Append(',')
              .AppendLine(b.LucXacNhan?.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture) ?? "");
        File.WriteAllText(duongDan, sb.ToString(), Encoding.UTF8);
    }
}

// ══════════════ 7/13 — Sao lưu và khôi phục cấu hình ══════════════

public static class SaoLuuCauHinh
{
    /// <summary>Sao lưu thư mục config sang một thư mục có dấu thời gian.</summary>
    public static string SaoLuu(KhoCauHinh kho, IDongHo dongHo)
    {
        ArgumentNullException.ThrowIfNull(kho);
        ArgumentNullException.ThrowIfNull(dongHo);

        string dich = Path.Combine(kho.Goc, "backup",
            "config_" + dongHo.BayGio.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture));
        Directory.CreateDirectory(dich);
        foreach (string f in Directory.GetFiles(kho.ThuMucConfig, "*.json"))
            File.Copy(f, Path.Combine(dich, Path.GetFileName(f)), overwrite: true);
        return dich;
    }

    /// <summary>
    /// Khôi phục. Trước khi đè, TỰ SAO LƯU bản hiện tại — vì thao tác khôi phục
    /// nhầm bản là chuyện xảy ra, và khi đó bản "hiện tại" là thứ vừa mất.
    /// </summary>
    public static string KhoiPhuc(KhoCauHinh kho, string thuMucSaoLuu, IDongHo dongHo)
    {
        ArgumentNullException.ThrowIfNull(kho);
        if (!Directory.Exists(thuMucSaoLuu))
            throw new DirectoryNotFoundException($"Không thấy bản sao lưu: {thuMucSaoLuu}");

        string truocKhiDe = SaoLuu(kho, dongHo);
        foreach (string f in Directory.GetFiles(thuMucSaoLuu, "*.json"))
            File.Copy(f, Path.Combine(kho.ThuMucConfig, Path.GetFileName(f)), overwrite: true);
        return truocKhiDe;
    }

    public static IReadOnlyList<string> LietKe(KhoCauHinh kho)
    {
        ArgumentNullException.ThrowIfNull(kho);
        string goc = Path.Combine(kho.Goc, "backup");
        return Directory.Exists(goc) ? [.. Directory.GetDirectories(goc).Order(StringComparer.Ordinal)] : [];
    }
}
