// -------------------------------------------------------
// File:    YeuCauCongNghiep2.cs
// Project: MeoBench
// Purpose: Phụ lục L, lô 2 — đèn tháp và còi (tắt còi không phải xác nhận),
//          gói chẩn đoán một nút (che mật khẩu, đọc được tệp nhật ký đang mở),
//          và kiểm giấy phép của thư viện hãng (báo trước khi hết hạn).
// -------------------------------------------------------
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace MeoBench;

// ══════════════ 15.1.9 — Đèn tháp và còi ══════════════

/// <summary>Mức ưu tiên của cảnh báo — tương ứng AlarmSeverity ở Chương 15 (Warning, Minor, Major, Critical).</summary>
public enum MucUuTienCanhBao { Thap = 1, Vua = 2, Cao = 3, KhanCap = 4 }

/// <summary>Kiểu kêu của còi.</summary>
public enum KieuCoi { Tat, MotTieng, NgatQuang, LienTuc }

/// <summary>Tín hiệu ra đèn tháp và còi tại một thời điểm.</summary>
public sealed record TinHieuThap(MauDen Den, bool NhapNhay, KieuCoi Coi);

/// <summary>Một cảnh báo chưa được xác nhận, như bộ tính đèn còi nhìn thấy.</summary>
public sealed record CanhBaoChuaXacNhan(string Ma, MucUuTienCanhBao Muc);

/// <summary>
/// Quy ước đèn tháp và còi đặt ở MỘT chỗ. Còi kêu theo mức cao nhất trong các cảnh báo chưa xác
/// nhận và chưa bị tắt còi. TẮT CÒI chỉ làm im những cảnh báo đang có — không xác nhận chúng; một
/// cảnh báo MỚI xuất hiện sau đó vẫn kêu.
/// </summary>
public sealed class DenThapVaCoi
{
    private readonly HashSet<string> _daTatCoi = new(StringComparer.Ordinal);

    /// <summary>Người vận hành bấm Tắt còi.</summary>
    public void TatCoi(IEnumerable<CanhBaoChuaXacNhan> dangCo)
    {
        ArgumentNullException.ThrowIfNull(dangCo);
        foreach (var c in dangCo) _daTatCoi.Add(c.Ma);
    }

    /// <summary>Gọi khi một cảnh báo đã được xác nhận hoặc đã hết — lần sau nó xuất hiện lại thì còi kêu lại.</summary>
    public void Quen(string ma) => _daTatCoi.Remove(ma);

    /// <summary>Tính tín hiệu ra đèn và còi.</summary>
    public TinHieuThap Tinh(TrangThaiMay trangThai, IReadOnlyCollection<CanhBaoChuaXacNhan> chuaXacNhan)
    {
        ArgumentNullException.ThrowIfNull(chuaXacNhan);
        var mucKeu = chuaXacNhan.Where(c => !_daTatCoi.Contains(c.Ma))
                                .Select(c => (int)c.Muc).DefaultIfEmpty(0).Max();
        var coi = mucKeu switch
        {
            (int)MucUuTienCanhBao.KhanCap => KieuCoi.LienTuc,
            (int)MucUuTienCanhBao.Cao     => KieuCoi.NgatQuang,
            (int)MucUuTienCanhBao.Vua     => KieuCoi.MotTieng,
            _                             => KieuCoi.Tat,
        };

        bool coCanhBaoCao = chuaXacNhan.Any(c => c.Muc >= MucUuTienCanhBao.Cao);
        var (den, nhay) = trangThai switch
        {
            TrangThaiMay.BaoDong                          => (MauDen.Do, chuaXacNhan.Count > 0),
            TrangThaiMay.DangChay when coCanhBaoCao       => (MauDen.Vang, true),   // chạy nhưng có chuyện cần người
            TrangThaiMay.DangChay                         => (MauDen.Xanh, false),
            TrangThaiMay.TamDung or TrangThaiMay.DangVeGoc => (MauDen.Vang, true),
            TrangThaiMay.SanSang                          => (MauDen.Vang, false),
            _                                             => (MauDen.Tat, false),
        };
        return new TinHieuThap(den, nhay, coi);
    }
}

// ══════════════ 19.7 — Gói chẩn đoán một nút ══════════════

/// <summary>Kết quả tạo gói chẩn đoán.</summary>
public sealed record KetQuaGoiChanDoan(string DuongDanZip, int SoTep, IReadOnlyList<string> BoQua);

/// <summary>
/// Gói chẩn đoán: MỘT tệp zip chứa thứ người hỗ trợ ở xa cần — nhật ký, cấu hình đã che mật khẩu,
/// thông tin máy và phiên bản — để người vận hành gửi đi bằng một nút, không cần biết tệp nằm đâu.
/// </summary>
public sealed partial class GoiChanDoan(IDongHo dongHo)
{
    /// <summary>Giới hạn tổng dung lượng nội dung đưa vào gói.</summary>
    public long GioiHanByte { get; init; } = 200L * 1024 * 1024;

    [GeneratedRegex("(\"[^\"]*(?:password|matkhau|pwd|secret|token|connectionstring)[^\"]*\"\\s*:\\s*)\"[^\"]*\"",
                    RegexOptions.IgnoreCase)]
    private static partial Regex KhoaBiMatJson();

    [GeneratedRegex(@"^(\s*[^=\r\n]*(?:password|matkhau|pwd|secret|token|connectionstring)[^=\r\n]*=).*$",
                    RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex KhoaBiMatIni();

    /// <summary>Che giá trị của mọi khoá có tên giống mật khẩu, chuỗi kết nối, token.</summary>
    public static string CheBiMat(string noiDung)
    {
        ArgumentNullException.ThrowIfNull(noiDung);
        var s = KhoaBiMatJson().Replace(noiDung, "$1\"***\"");
        return KhoaBiMatIni().Replace(s, "$1***");
    }

    /// <summary>Tạo gói. Tệp cấu hình được che bí mật; tệp nhật ký đọc được kể cả khi bộ ghi đang mở nó.</summary>
    public KetQuaGoiChanDoan Tao(string duongDanZip, string thongTinMay,
                                 IEnumerable<string> tepNhatKy, IEnumerable<string> tepCauHinh)
    {
        ArgumentNullException.ThrowIfNull(tepNhatKy);
        ArgumentNullException.ThrowIfNull(tepCauHinh);
        var boQua = new List<string>();
        var daDua = new List<string>();
        long tong = 0;

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(duongDanZip))!);
        using (var zip = ZipFile.Open(duongDanZip, ZipArchiveMode.Create))
        {
            // Cấu hình trước — nhỏ, và quan trọng hơn nhật ký
            foreach (var tep in tepCauHinh)
                DuaVao(zip, tep, "cau_hinh/", cheBiMat: true, daDua, boQua, ref tong);
            // Nhật ký mới nhất trước — hết chỗ thì bỏ bản cũ, không bỏ bản mới
            foreach (var tep in tepNhatKy.OrderByDescending(File.GetLastWriteTimeUtc))
                DuaVao(zip, tep, "nhat_ky/", cheBiMat: false, daDua, boQua, ref tong);

            var gioiThieu = new StringBuilder()
                .AppendLine("GÓI CHẨN ĐOÁN")
                .Append("Tạo lúc: ").AppendLine(dongHo.BayGio.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))
                .AppendLine(thongTinMay)
                .AppendLine().AppendLine("Đã đưa vào:");
            foreach (var d in daDua) gioiThieu.Append("  ").AppendLine(d);
            gioiThieu.AppendLine().AppendLine("Không đưa vào:");
            foreach (var b in boQua) gioiThieu.Append("  ").AppendLine(b);
            using var w = new StreamWriter(zip.CreateEntry("GIOI_THIEU.txt").Open(), Encoding.UTF8);
            w.Write(gioiThieu.ToString());
        }
        return new KetQuaGoiChanDoan(duongDanZip, daDua.Count + 1, boQua);
    }

    private void DuaVao(ZipArchive zip, string tep, string thuMucTrongGoi, bool cheBiMat,
                        List<string> daDua, List<string> boQua, ref long tong)
    {
        if (!File.Exists(tep)) { boQua.Add($"{tep} (không còn trên đĩa)"); return; }
        long kichThuoc = new FileInfo(tep).Length;
        if (tong + kichThuoc > GioiHanByte) { boQua.Add($"{tep} (vượt giới hạn dung lượng gói)"); return; }

        // FileShare.ReadWrite: bộ ghi nhật ký đang mở tệp để ghi — vẫn đọc được, không chặn nó
        using var vao = new FileStream(tep, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        var muc = zip.CreateEntry(thuMucTrongGoi + Path.GetFileName(tep), CompressionLevel.Optimal);
        using var ra = muc.Open();
        if (cheBiMat)
        {
            using var doc = new StreamReader(vao, Encoding.UTF8);
            var b = Encoding.UTF8.GetBytes(CheBiMat(doc.ReadToEnd()));
            ra.Write(b, 0, b.Length);
        }
        else
        {
            vao.CopyTo(ra);
        }
        tong += kichThuoc;
        daDua.Add(thuMucTrongGoi + Path.GetFileName(tep));
    }
}

// ══════════════ 17.3.9 — Giấy phép của thư viện hãng ══════════════

/// <summary>Trạng thái giấy phép của một thư viện hãng, đọc từ API của hãng.</summary>
/// <param name="Ten">Tên hiển thị: "thư viện thị giác", "card chuyển động"…</param>
/// <param name="CoMat">Có tìm thấy giấy phép/khoá cứng không.</param>
/// <param name="HetHan">Ngày hết hạn; null nếu vĩnh viễn.</param>
public sealed record TrangThaiGiayPhep(string Ten, bool CoMat, DateTime? HetHan);

/// <summary>Ba mức của một giấy phép.</summary>
public enum MucGiayPhep { Tot, SapHet, KhongDung }

/// <summary>Kết quả xét một giấy phép.</summary>
public sealed record KetQuaGiayPhep(MucGiayPhep Muc, int? SoNgayCon, string ThongBao);

/// <summary>
/// Xét giấy phép: không có hay hết hạn thì KHÔNG cho Start (đưa vào bộ điều kiện chạy, mục 15.2.3b)
/// thay vì dừng máy giữa chu kỳ; sắp hết hạn thì báo trước đủ lâu để kịp mua.
/// </summary>
public static class XetGiayPhep
{
    /// <summary>Xét một giấy phép tại thời điểm cho trước.</summary>
    public static KetQuaGiayPhep Xet(TrangThaiGiayPhep gp, DateTime bayGio, int soNgayBaoTruoc = 30)
    {
        ArgumentNullException.ThrowIfNull(gp);
        if (!gp.CoMat)
            return new KetQuaGiayPhep(MucGiayPhep.KhongDung, null,
                $"Không thấy giấy phép của {gp.Ten} — kiểm khoá cứng và cổng USB (mục 17.3.8)");
        if (gp.HetHan is not DateTime han)
            return new KetQuaGiayPhep(MucGiayPhep.Tot, null, "");

        int con = (han.Date - bayGio.Date).Days;      // số NGÀY, tính theo lịch
        string ngay = han.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        if (con <= 0)
            return new KetQuaGiayPhep(MucGiayPhep.KhongDung, con, $"Giấy phép của {gp.Ten} đã hết hạn ngày {ngay}");
        if (con <= soNgayBaoTruoc)
            return new KetQuaGiayPhep(MucGiayPhep.SapHet, con, $"Giấy phép của {gp.Ten} còn {con} ngày (hết hạn {ngay})");
        return new KetQuaGiayPhep(MucGiayPhep.Tot, con, "");
    }
}
