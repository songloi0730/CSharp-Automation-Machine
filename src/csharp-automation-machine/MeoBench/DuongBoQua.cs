// -------------------------------------------------------
// File:    DuongBoQua.cs
// Project: MeoBench
// Purpose: Mục 17.6 / Phụ lục J — đường bỏ qua một phép kiểm CHẤT LƯỢNG, thiết kế
//          sẵn thay vì để người sửa máy comment dòng kiểm tra. Học từ một máy thật
//          làm tốt (vẫn chạy phép kiểm, vẫn ghi kết quả gốc, đánh dấu sản phẩm) và
//          thêm ba thứ máy đó còn thiếu: phân quyền, tự hết hạn, vết kiểm toán.
// -------------------------------------------------------
using System.Globalization;

namespace MeoBench;

/// <summary>Phép kiểm thuộc loại nào. Loại an toàn KHÔNG bao giờ bỏ qua được bằng phần mềm.</summary>
public enum LoaiPhepKiem { ChatLuong, AnToan }

/// <summary>Kết quả thật của một phép kiểm, trước khi xét đường bỏ qua.</summary>
public sealed record KetQuaPhepKiem(bool Dat, string ChiTiet);

/// <summary>
/// Kết quả sau khi xét đường bỏ qua. Tách hai thứ hay bị gộp làm một:
/// <see cref="ChoChayTiep"/> là QUYẾT ĐỊNH cho máy, <see cref="DatGoc"/> là SỰ THẬT đo được.
/// </summary>
public sealed record KetQuaCoBoQua(bool ChoChayTiep, bool DatGoc, bool DaBoQua, string GhiChuSanPham);

/// <summary>Một lần bật bỏ qua: ai, vì sao, từ lúc nào, tới lúc nào.</summary>
public sealed record LenhBoQua(string PhepKiem, string LyDo, string NguoiBat, DateTime BatLuc, DateTime HetHanLuc);

/// <summary>
/// Quản lý mọi đường bỏ qua của máy ở MỘT chỗ. Tầng trình tự không đọc cờ bool rải rác;
/// nó luôn chạy phép kiểm thật rồi hỏi <see cref="Xet"/> xem có được chạy tiếp không.
/// </summary>
public sealed class QuanLyBoQua
{
    /// <summary>Thời hạn dài nhất cho một lần bật — quá một ca thì phải bật lại có chủ ý.</summary>
    public static readonly TimeSpan ThoiHanToiDa = TimeSpan.FromHours(12);

    private readonly PhienDangNhap _phien;
    private readonly VetKiemToan _vet;
    private readonly NhatKy _nhatKy;
    private readonly IDongHo _dongHo;
    private readonly Dictionary<string, LoaiPhepKiem> _khaiBao = new(StringComparer.Ordinal);
    private readonly Dictionary<string, LenhBoQua> _dangBat = new(StringComparer.Ordinal);

    /// <summary>Tạo bộ quản lý đường bỏ qua.</summary>
    public QuanLyBoQua(PhienDangNhap phien, VetKiemToan vet, NhatKy nhatKy, IDongHo dongHo)
    {
        ArgumentNullException.ThrowIfNull(phien);
        ArgumentNullException.ThrowIfNull(vet);
        ArgumentNullException.ThrowIfNull(nhatKy);
        ArgumentNullException.ThrowIfNull(dongHo);
        _phien = phien; _vet = vet; _nhatKy = nhatKy; _dongHo = dongHo;
    }

    /// <summary>Khai báo trước những phép kiểm có thể được bỏ qua, kèm loại của chúng.</summary>
    public void KhaiBao(string phepKiem, LoaiPhepKiem loai) => _khaiBao[phepKiem] = loai;

    /// <summary>Bật bỏ qua. Trả về lý do từ chối nếu không được phép — không ném.</summary>
    public KetQuaKiemQuyen Bat(string phepKiem, string lyDo, TimeSpan thoiHan)
    {
        if (!_khaiBao.TryGetValue(phepKiem, out var loai))
            return new KetQuaKiemQuyen(false, $"Không có phép kiểm tên '{phepKiem}' trong danh sách được phép bỏ qua");
        if (loai == LoaiPhepKiem.AnToan)
            return new KetQuaKiemQuyen(false, "Phép kiểm an toàn không bỏ qua được bằng phần mềm (mục 15.2.2)");
        if (string.IsNullOrWhiteSpace(lyDo))
            return new KetQuaKiemQuyen(false, "Phải ghi lý do bỏ qua");
        if (thoiHan <= TimeSpan.Zero || thoiHan > ThoiHanToiDa)
            return new KetQuaKiemQuyen(false, $"Thời hạn phải lớn hơn 0 và không quá {ThoiHanToiDa.TotalHours:0} giờ");

        var quyen = _phien.Kiem(MucNguoiDung.KyThuat, "bật bỏ qua phép kiểm");
        if (!quyen.ChoPhep) return quyen;

        var bayGio = _dongHo.BayGio;
        var lenh = new LenhBoQua(phepKiem, lyDo, _phien.Ten, bayGio, bayGio + thoiHan);
        _dangBat[phepKiem] = lenh;
        _vet.Ghi(_phien.Ten, $"Bật bỏ qua {phepKiem}", "đang kiểm",
                 $"bỏ qua tới {lenh.HetHanLuc.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)}: {lyDo}");
        _nhatKy.Ghi(MucLog.CanhBao, phepKiem, "Bật bỏ qua bởi {Nguoi}, lý do {LyDo}",
                    ("Nguoi", _phien.Ten), ("LyDo", lyDo));
        return new KetQuaKiemQuyen(true, null);
    }

    /// <summary>Tắt bỏ qua trước hạn.</summary>
    public void Tat(string phepKiem)
    {
        if (_dangBat.Remove(phepKiem))
            _vet.Ghi(_phien.Ten, $"Tắt bỏ qua {phepKiem}", "bỏ qua", "đang kiểm");
    }

    /// <summary>Lệnh còn hiệu lực; hết hạn thì tự gỡ và ghi vết — không cần ai nhớ.</summary>
    private LenhBoQua? ConHieuLuc(string phepKiem)
    {
        if (!_dangBat.TryGetValue(phepKiem, out var lenh)) return null;
        if (_dongHo.BayGio < lenh.HetHanLuc) return lenh;
        _dangBat.Remove(phepKiem);
        _vet.Ghi("hệ thống", $"Tự hết hạn bỏ qua {phepKiem}", "bỏ qua", "đang kiểm");
        return null;
    }

    /// <summary>
    /// Luôn nhận kết quả của phép kiểm ĐÃ CHẠY. Đường bỏ qua chỉ đổi quyết định,
    /// không bao giờ đổi hay che kết quả gốc.
    /// </summary>
    public KetQuaCoBoQua Xet(string phepKiem, KetQuaPhepKiem goc)
    {
        ArgumentNullException.ThrowIfNull(goc);
        var lenh = ConHieuLuc(phepKiem);
        if (goc.Dat) return new KetQuaCoBoQua(true, true, false, "");
        if (lenh is null) return new KetQuaCoBoQua(false, false, false, goc.ChiTiet);

        _nhatKy.Ghi(MucLog.CanhBao, phepKiem, "Bỏ qua kết quả trượt {ChiTiet}, bật bởi {Nguoi}",
                    ("ChiTiet", goc.ChiTiet), ("Nguoi", lenh.NguoiBat));
        return new KetQuaCoBoQua(true, false, true, $"BOQUA:{phepKiem}|{goc.ChiTiet}");
    }

    /// <summary>Các dòng banner phải hiện trên màn hình chừng nào còn bỏ qua.</summary>
    public IReadOnlyList<string> DongBanner()
    {
        var ra = new List<string>();
        foreach (var ten in _dangBat.Keys.ToList())
        {
            var lenh = ConHieuLuc(ten);
            if (lenh is null) continue;
            var con = lenh.HetHanLuc - _dongHo.BayGio;
            ra.Add($"ĐANG BỎ QUA: {ten} — còn {(int)con.TotalHours} giờ {con.Minutes} phút — bật bởi {lenh.NguoiBat} ({lenh.LyDo})");
        }
        return ra;
    }
}
