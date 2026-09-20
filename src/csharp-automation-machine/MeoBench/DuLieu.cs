// -------------------------------------------------------
// File:    DuLieu.cs
// Project: MeoBench
// Purpose: Lời giải mẫu nhóm G.6 — công thức, cấu hình, dữ liệu sản xuất.
//          G.6.1 mô hình công thức có kiểm tra dải
//          G.6.2 nạp/ghi công thức, giữ mặc định khi file hỏng
//          G.6.3 ghi kết quả sản xuất ra CSV
//          G.6.4 đếm sản lượng theo ca, sống sót qua khởi động lại
//          G.6.5 xoay vòng và dọn file cũ
// -------------------------------------------------------
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace MeoBench;

// ══════════════ G.6.1 — mô hình CÔNG THỨC (khác CẤU HÌNH MÁY) ══════════════
//
// Phân biệt quan trọng: CauHinhMay (RapNoi.cs) mô tả CỖ MÁY — hành trình trục,
// hạt giống giả lập, vị trí máng. CongThuc mô tả SẢN PHẨM đang chạy — chiều dày
// danh định, dung sai, tốc độ. Một máy chạy nhiều công thức; đổi công thức không
// đổi cấu hình máy.

public sealed record CongThuc
{
    public string Ten               { get; init; } = "MacDinh";
    public double ChieuDayDanhDinhMm{ get; init; } = 2.000;
    public double DungSaiDuoiMm     { get; init; } = 0.050;
    public double DungSaiTrenMm     { get; init; } = 0.050;
    public double TocDoTrucMmS      { get; init; } = 50.0;
    public int    SoLanDoMoiPhoi    { get; init; } = 3;

    public double GioiHanDuoiMm => ChieuDayDanhDinhMm - DungSaiDuoiMm;
    public double GioiHanTrenMm => ChieuDayDanhDinhMm + DungSaiTrenMm;

    /// <summary>
    /// Trả về DANH SÁCH lỗi thay vì ném ở lỗi đầu tiên — để giao diện hiện
    /// được hết lỗi cùng lúc, người dùng không phải sửa từng cái một.
    /// </summary>
    public IReadOnlyList<string> KiemTra()
    {
        var loi = new List<string>();
        if (string.IsNullOrWhiteSpace(Ten))       loi.Add("Tên công thức để trống");
        if (ChieuDayDanhDinhMm <= 0)              loi.Add("ChieuDayDanhDinhMm phải lớn hơn 0");
        if (DungSaiDuoiMm < 0)                    loi.Add("DungSaiDuoiMm không được âm");
        if (DungSaiTrenMm < 0)                    loi.Add("DungSaiTrenMm không được âm");
        if (TocDoTrucMmS  <= 0)                   loi.Add("TocDoTrucMmS phải lớn hơn 0");
        if (SoLanDoMoiPhoi <= 0)                  loi.Add("SoLanDoMoiPhoi phải lớn hơn 0");
        if (GioiHanDuoiMm > GioiHanTrenMm)        loi.Add("Dung sai làm dải bị đảo");
        return loi;
    }

    public bool HopLe => KiemTra().Count == 0;
}

// ══════════════ G.6.2 — nạp và ghi công thức ══════════════

public static class KhoCongThuc
{
    // CA1869: tạo một lần, dùng lại — không new trong mỗi lời gọi.
    private static readonly JsonSerializerOptions TuyChon = new()
    {
        WriteIndented = true,
    };

    public static void Ghi(string duongDan, CongThuc ct)
    {
        ArgumentNullException.ThrowIfNull(ct);
        File.WriteAllText(duongDan, JsonSerializer.Serialize(ct, TuyChon), Encoding.UTF8);
    }

    /// <summary>
    /// Nạp công thức. File hỏng / thiếu / sai dải đều KHÔNG làm sập máy:
    /// trả về công thức dự phòng và nói rõ lý do qua <paramref name="lyDo"/>.
    /// </summary>
    public static CongThuc Nap(string duongDan, CongThuc duPhong, out string? lyDo)
    {
        ArgumentNullException.ThrowIfNull(duPhong);

        if (!File.Exists(duongDan))
        {
            lyDo = $"Không thấy file {Path.GetFileName(duongDan)} — dùng công thức dự phòng";
            return duPhong;
        }

        CongThuc? doc;
        try
        {
            doc = JsonSerializer.Deserialize<CongThuc>(File.ReadAllText(duongDan, Encoding.UTF8));
        }
#pragma warning disable CA1031 // cố ý bắt rộng: file cấu hình hỏng kiểu gì cũng không được làm sập máy
        catch (Exception ex)
#pragma warning restore CA1031
        {
            lyDo = $"File công thức hỏng ({ex.GetType().Name}) — dùng công thức dự phòng";
            return duPhong;
        }

        if (doc is null)
        {
            lyDo = "File công thức rỗng — dùng công thức dự phòng";
            return duPhong;
        }

        var loi = doc.KiemTra();
        if (loi.Count > 0)
        {
            lyDo = "Công thức trong file sai: " + string.Join("; ", loi) + " — dùng công thức dự phòng";
            return duPhong;
        }

        lyDo = null;
        return doc;
    }
}

// ══════════════ G.6.3 — ghi kết quả sản xuất ra CSV ══════════════

public sealed class SoGhiKetQua(string thuMuc, IDongHo dongHo)
{
    private readonly string  _thuMuc = thuMuc;
    private readonly IDongHo _dongHo = dongHo;

    public string DuongDanHomNay
        => Path.Combine(_thuMuc, $"KetQua_{_dongHo.BayGio:yyyy-MM-dd}.csv");

    public void Ghi(KetQuaDo kq, string tenCongThuc)
    {
        Directory.CreateDirectory(_thuMuc);
        string file = DuongDanHomNay;
        bool moi = !File.Exists(file);

        var dong = new StringBuilder();
        if (moi) dong.AppendLine("ThoiGian,SoHieuPhoi,ChieuDayMm,KetLuan,CongThuc");

        // Thời gian ISO 8601 và số theo văn hoá BẤT BIẾN — nếu không, trên máy
        // đặt vùng miền Việt Nam dấu phẩy thập phân sẽ phá cấu trúc CSV.
        dong.Append(kq.ThoiDiem.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)).Append(',')
            .Append(kq.SoHieuPhoi.ToString(CultureInfo.InvariantCulture)).Append(',')
            .Append(kq.ChieuDayMm.ToString("F4", CultureInfo.InvariantCulture)).Append(',')
            .Append(kq.KetLuan).Append(',')
            .Append(tenCongThuc);

        File.AppendAllText(file, dong.ToString() + Environment.NewLine, Encoding.UTF8);
    }
}

// ══════════════ G.6.4 — đếm sản lượng theo ca, sống sót qua khởi động lại ══════════════

public sealed record BanGhiCa(string MaCa, int Tong, int Ok, int Ng, DateTime BatDau);

public sealed class BoDemCa
{
    private static readonly JsonSerializerOptions TuyChon = new() { WriteIndented = true };

    private readonly string  _duongDan;
    private readonly IDongHo _dongHo;
    private BanGhiCa _hienTai;

    public BoDemCa(string duongDan, IDongHo dongHo, string maCa)
    {
        ArgumentNullException.ThrowIfNull(dongHo);
        _duongDan = duongDan;
        _dongHo   = dongHo;
        _hienTai  = Doc(duongDan, maCa) ?? new BanGhiCa(maCa, 0, 0, 0, dongHo.BayGio);
    }

    public BanGhiCa HienTai => _hienTai;

    public double TyLeDat => _hienTai.Tong == 0 ? 0.0
        : Math.Round(_hienTai.Ok * 100.0 / _hienTai.Tong, 2);

    /// <summary>Nhịp máy giây/phôi. Trả về 0 khi chưa đủ dữ liệu để nói gì.</summary>
    public double NhipGiayMoiPhoi
    {
        get
        {
            if (_hienTai.Tong == 0) return 0.0;
            double giay = (_dongHo.BayGio - _hienTai.BatDau).TotalSeconds;
            return giay <= 0 ? 0.0 : Math.Round(giay / _hienTai.Tong, 2);
        }
    }

    public void Dem(KetQuaDo kq)
    {
        _hienTai = _hienTai with
        {
            Tong = _hienTai.Tong + 1,
            Ok   = _hienTai.Ok + (kq.Dat ? 1 : 0),
            Ng   = _hienTai.Ng + (kq.Dat ? 0 : 1),
        };
        Luu();   // ghi sau MỖI phôi: mất điện giữa ca là chuyện có thật
    }

    public void DoiCa(string maCaMoi)
    {
        _hienTai = new BanGhiCa(maCaMoi, 0, 0, 0, _dongHo.BayGio);
        Luu();
    }

    private void Luu()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_duongDan)!);
        File.WriteAllText(_duongDan, JsonSerializer.Serialize(_hienTai, TuyChon), Encoding.UTF8);
    }

    private static BanGhiCa? Doc(string duongDan, string maCa)
    {
        if (!File.Exists(duongDan)) return null;
        try
        {
            var bg = JsonSerializer.Deserialize<BanGhiCa>(File.ReadAllText(duongDan, Encoding.UTF8));
            return bg is not null && string.Equals(bg.MaCa, maCa, StringComparison.Ordinal) ? bg : null;
        }
#pragma warning disable CA1031 // số liệu sản lượng hỏng thì đếm lại từ 0, không được làm sập máy
        catch (Exception)
#pragma warning restore CA1031
        {
            return null;
        }
    }
}

// ══════════════ G.6.5 — xoay vòng và dọn file cũ ══════════════

public static class DonFileCu
{
    /// <summary>
    /// Xoá file khớp mẫu và cũ hơn <paramref name="soNgayGiu"/> ngày.
    /// Ngày lấy từ TÊN FILE chứ không từ thời gian sửa đổi: OneDrive, sao lưu
    /// hay chép thư mục đều làm sai thời gian sửa đổi.
    /// </summary>
    public static int Don(string thuMuc, string tienTo, int soNgayGiu, IDongHo dongHo)
    {
        ArgumentNullException.ThrowIfNull(dongHo);
        ArgumentOutOfRangeException.ThrowIfNegative(soNgayGiu);
        if (!Directory.Exists(thuMuc)) return 0;

        DateTime moc = dongHo.BayGio.Date.AddDays(-soNgayGiu);
        int daXoa = 0;

        foreach (string f in Directory.GetFiles(thuMuc, tienTo + "*.csv"))
        {
            string ten = Path.GetFileNameWithoutExtension(f);
            string phanNgay = ten[Math.Min(tienTo.Length, ten.Length)..];

            // Định dạng ngày CỐ ĐỊNH, không theo ngôn ngữ máy.
            if (!DateTime.TryParseExact(phanNgay, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out DateTime ngay))
                continue;                       // tên lạ thì KHÔNG đụng tới

            if (ngay.Date < moc) { File.Delete(f); daXoa++; }
        }
        return daXoa;
    }
}

// ══════════════ Bộ tự kiểm ══════════════

public static class KiemDuLieu
{
    public static Task Chay()
    {
        string goc = Path.Combine(Path.GetTempPath(), "MeoBench_G6_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(goc);
        try
        {
            ChayThat(goc);
        }
        finally
        {
            try { Directory.Delete(goc, recursive: true); }
#pragma warning disable CA1031 // dọn thư mục tạm hỏng thì bỏ qua, không ảnh hưởng kết quả kiểm
            catch (Exception) { /* dọn dẹp, bỏ qua */ }
#pragma warning restore CA1031
        }
        return Task.CompletedTask;
    }

    private static void ChayThat(string goc)
    {
        // ---------- G.6.1 ----------
        Kiem.MoBai("G.6.1", "Mô hình công thức có kiểm tra dải");
        var ct = new CongThuc();
        Kiem.Dung(ct.HopLe, "công thức mặc định hợp lệ");
        Kiem.Gan(ct.GioiHanDuoiMm, 1.950, 1e-9, "giới hạn dưới suy ra đúng");
        Kiem.Gan(ct.GioiHanTrenMm, 2.050, 1e-9, "giới hạn trên suy ra đúng");

        var xau = new CongThuc { Ten = "  ", SoLanDoMoiPhoi = 0, DungSaiDuoiMm = -1, TocDoTrucMmS = 0 };
        var loi = xau.KiemTra();
        // 5 chứ không phải 4: DungSaiDuoiMm = -1 vừa âm, vừa LÀM ĐẢO DẢI —
        // một trường sai sinh ra hai thông báo, và cả hai đều đúng việc của nó.
        Kiem.Bang(loi.Count, 5, "báo ĐỦ 5 lỗi cùng lúc, không dừng ở lỗi đầu tiên");
        Kiem.Dung(loi.Any(l => l.Contains("dải bị đảo", StringComparison.Ordinal)),
                  "dung sai âm kéo theo lỗi dải bị đảo — bắt được cả hậu quả");
        Kiem.Dung(loi.Any(l => l.Contains("SoLanDoMoiPhoi", StringComparison.Ordinal)),
                  "thông báo nói rõ TÊN TRƯỜNG sai");
        Kiem.Dung(!xau.HopLe, "công thức sai thì HopLe = false");

        // ---------- G.6.2 ----------
        Kiem.MoBai("G.6.2", "Nạp và ghi công thức");
        string fileCt = Path.Combine(goc, "congthuc.json");
        var duPhong = new CongThuc { Ten = "DuPhong" };

        var nap0 = KhoCongThuc.Nap(fileCt, duPhong, out string? lyDo0);
        Kiem.Bang(nap0.Ten, "DuPhong", "chưa có file → dùng công thức dự phòng");
        Kiem.Dung(lyDo0 is not null && lyDo0.Contains("Không thấy file", StringComparison.Ordinal),
                  "nói rõ lý do vì sao dùng dự phòng");

        var goc1 = new CongThuc { Ten = "SanPhamA", ChieuDayDanhDinhMm = 3.200, SoLanDoMoiPhoi = 5 };
        KhoCongThuc.Ghi(fileCt, goc1);
        var nap1 = KhoCongThuc.Nap(fileCt, duPhong, out string? lyDo1);
        Kiem.Bang(nap1, goc1, "ghi rồi nạp lại ra đúng công thức cũ");
        Kiem.Dung(lyDo1 is null, "nạp thành công thì không có lý do cảnh báo");

        File.WriteAllText(fileCt, "{ đây không phải JSON", Encoding.UTF8);
        var nap2 = KhoCongThuc.Nap(fileCt, duPhong, out string? lyDo2);
        Kiem.Bang(nap2.Ten, "DuPhong", "★ file RÁC → vẫn chạy bằng dự phòng, KHÔNG sập");
        Kiem.Dung(lyDo2 is not null && lyDo2.Contains("hỏng", StringComparison.Ordinal),
                  "báo rõ file hỏng");

        KhoCongThuc.Ghi(fileCt, new CongThuc { Ten = "Xau", DungSaiDuoiMm = -5 });
        var nap3 = KhoCongThuc.Nap(fileCt, duPhong, out string? lyDo3);
        Kiem.Bang(nap3.Ten, "DuPhong", "★ file ĐÚNG CÚ PHÁP nhưng SAI DẢI → cũng bị chặn");
        Kiem.Dung(lyDo3 is not null && lyDo3.Contains("DungSaiDuoiMm", StringComparison.Ordinal),
                  "nói rõ trường nào sai");

        // ---------- G.6.3 ----------
        Kiem.MoBai("G.6.3", "Ghi kết quả sản xuất ra CSV");
        var dongHo = new DongHoGia(new DateTime(2026, 9, 20, 8, 0, 0));
        var so = new SoGhiKetQua(Path.Combine(goc, "ketqua"), dongHo);

        so.Ghi(new KetQuaDo(1, 2.0035, dongHo.BayGio, KetLuanDo.Dat), "SanPhamA");
        so.Ghi(new KetQuaDo(2, 2.1200, dongHo.BayGio, KetLuanDo.TrenNguong), "SanPhamA");

        string[] dong = File.ReadAllLines(so.DuongDanHomNay);
        Kiem.Bang(dong.Length, 3, "1 dòng tiêu đề + 2 dòng dữ liệu");
        Kiem.Bang(dong[0], "ThoiGian,SoHieuPhoi,ChieuDayMm,KetLuan,CongThuc", "tiêu đề đúng");
        Kiem.Bang(dong[1], "2026-09-20T08:00:00,1,2.0035,Dat,SanPhamA",
                  "★ số dùng dấu CHẤM, thời gian ISO 8601 — mở bằng Excel không sai");
        Kiem.Dung(!dong[1].Contains(',' + "0035", StringComparison.Ordinal),
                  "không có dấu phẩy thập phân phá cấu trúc CSV");
        Kiem.Dung(Path.GetFileName(so.DuongDanHomNay) == "KetQua_2026-09-20.csv",
                  "tên file chia theo ngày, định dạng cố định");

        // ---------- G.6.4 ----------
        Kiem.MoBai("G.6.4", "Đếm sản lượng theo ca");
        string fileCa = Path.Combine(goc, "ca", "ca.json");
        var dem = new BoDemCa(fileCa, dongHo, "CA-A-20260920");

        for (int i = 0; i < 8; i++) dem.Dem(new KetQuaDo(i, 2.0, dongHo.BayGio, KetLuanDo.Dat));
        dem.Dem(new KetQuaDo(9,  2.2, dongHo.BayGio, KetLuanDo.TrenNguong));
        dem.Dem(new KetQuaDo(10, 1.8, dongHo.BayGio, KetLuanDo.DuoiNguong));

        Kiem.Bang(dem.HienTai.Tong, 10, "đếm đủ 10 phôi");
        Kiem.Bang(dem.HienTai.Ok, 8, "8 phôi đạt");
        Kiem.Bang(dem.HienTai.Ng, 2, "2 phôi không đạt");
        Kiem.Gan(dem.TyLeDat, 80.0, 1e-9, "tỷ lệ đạt 80 %");

        dongHo.Tien(TimeSpan.FromSeconds(100));
        Kiem.Gan(dem.NhipGiayMoiPhoi, 10.0, 1e-9, "100 giây / 10 phôi = 10 s mỗi phôi");

        var demSauKhoiDongLai = new BoDemCa(fileCa, dongHo, "CA-A-20260920");
        Kiem.Bang(demSauKhoiDongLai.HienTai.Tong, 10,
                  "★ khởi động lại phần mềm giữa ca → số liệu KHÔNG mất");
        var demCaKhac = new BoDemCa(fileCa, dongHo, "CA-B-20260920");
        Kiem.Bang(demCaKhac.HienTai.Tong, 0, "sang ca khác → đếm lại từ 0");

        // ---------- G.6.5 ----------
        Kiem.MoBai("G.6.5", "Xoay vòng và dọn file cũ");
        string thuMucLog = Path.Combine(goc, "log");
        Directory.CreateDirectory(thuMucLog);
        var homNay = new DateTime(2026, 9, 20);
        foreach (int lui in new[] { 0, 1, 5, 29, 30, 31, 400 })
            File.WriteAllText(Path.Combine(thuMucLog,
                $"KetQua_{homNay.AddDays(-lui):yyyy-MM-dd}.csv"), "x", Encoding.UTF8);
        File.WriteAllText(Path.Combine(thuMucLog, "KetQua_ghi-chu-tay.csv"), "x", Encoding.UTF8);

        var dongHo2 = new DongHoGia(homNay);
        int daXoa = DonFileCu.Don(thuMucLog, "KetQua_", soNgayGiu: 30, dongHo2);
        var conLai = Directory.GetFiles(thuMucLog).Select(Path.GetFileName).ToList();

        Kiem.Bang(daXoa, 2, "xoá đúng 2 file quá 30 ngày (31 ngày và 400 ngày)");
        Kiem.Dung(conLai.Contains("KetQua_2026-09-20.csv"), "file hôm nay còn nguyên");
        Kiem.Dung(conLai.Contains("KetQua_2026-08-21.csv"), "file đúng 30 ngày còn nguyên (biên)");
        Kiem.Dung(!conLai.Contains("KetQua_2026-08-20.csv"), "file 31 ngày đã bị xoá");
        Kiem.Dung(conLai.Contains("KetQua_ghi-chu-tay.csv"),
                  "★ file tên lạ KHÔNG bị đụng tới — không đoán bừa rồi xoá nhầm");
        Kiem.Bang(DonFileCu.Don(Path.Combine(goc, "khong-ton-tai"), "KetQua_", 30, dongHo2), 0,
                  "thư mục không tồn tại → trả 0, không ném");
    }
}
