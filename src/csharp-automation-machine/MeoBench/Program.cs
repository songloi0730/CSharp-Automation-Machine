// -------------------------------------------------------
// File:    Program.cs
// Project: MeoBench — lời giải mẫu ĐỦ 40 bài của Phụ lục G
// Purpose: Bộ chạy tự kiểm. Chạy được TỪNG NHÓM riêng, không phải làm xong
//          hết mới biết sai ở đâu.
//
//   dotnet run                → chạy toàn bộ 40 bài
//   dotnet run -- G4          → chỉ chạy nhóm G.4 (bài G.4.1 và G.4.3)
//   dotnet run -- G8          → chỉ chạy nhóm G.8
//   dotnet run -- G9          → chỉ kiểm CỖ MÁY GHÉP HOÀN CHỈNH
//   dotnet run -- G12         → chỉ kiểm phần TÁCH CẤU HÌNH (config/product)
//   dotnet run -- G13         → năng lực vận hành máy thật (quyền, jog, đèn tháp…)
//   dotnet run -- --demo      → chạy máy 20 chu kỳ và in nhật ký
//   dotnet run -- H           → kiểm các khẳng định của Phụ lục H (boxing, closure, Span…)
//   dotnet run -- --danhsach  → liệt kê 40 bài
// -------------------------------------------------------
using System.Globalization;
using MeoBench;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var danhSachBai = new[]
{
    ("G.1.1", "Đơn vị không lẫn được"),     ("G.1.2", "Bản ghi một lần đo"),
    ("G.1.3", "Enum trạng thái"),           ("G.1.4", "Bảng mã cảnh báo có metadata"),
    ("G.1.5", "Ngoại lệ mang mã cảnh báo"), ("G.2.1", "Kiểm dải đo"),
    ("G.2.2", "Quy đổi mm ↔ xung"),         ("G.2.3", "Tách khung từ dòng byte"),
    ("G.2.4", "Tổng kiểm"),                 ("G.2.5", "Cửa sổ trượt và lọc nhiễu"),
    ("G.3.1", "Hợp đồng ITruc"),            ("G.3.2", "Hạn giờ cho cảm biến"),
    ("G.3.3", "Vào-ra số theo tên"),        ("G.3.4", "Bản giả lập tái hiện được"),
    ("G.3.5", "Giả lập biết hỏng"),         ("G.4.1", "Chuyển động có hạn giờ"),
    ("G.4.2", "Cụm kẹp chờ xác nhận"),      ("G.4.3", "Cụm đo"),
    ("G.4.4", "Giám sát khí nén"),          ("G.4.5", "Bộ điều khiển máy"),
    ("G.5.1", "Hợp đồng IBuoc"),            ("G.5.2", "Bảy bước của một chu kỳ"),
    ("G.5.3", "Bộ chạy trình tự"),          ("G.5.4", "Bảng chuyển trạng thái"),
    ("G.5.5", "Tạm dừng và chạy tiếp"),     ("G.6.1", "Mô hình công thức"),
    ("G.6.2", "Nạp và ghi công thức"),      ("G.6.3", "Ghi kết quả sản xuất"),
    ("G.6.4", "Đếm sản lượng theo ca"),     ("G.6.5", "Xoay vòng và dọn file cũ"),
    ("G.7.1", "ViewModel trạng thái máy"),  ("G.7.2", "Thanh tiến độ bước"),
    ("G.7.3", "Ô nhập số có dải"),          ("G.7.4", "Bảng nhật ký và cảnh báo"),
    ("G.7.5", "Nút khoá theo trạng thái"),  ("G.8.1", "Điểm ráp nối"),
    ("G.8.2", "Nhật ký có cấu trúc"),       ("G.8.3", "Driver cảm biến nối tiếp"),
    ("G.8.4", "Bắt tay hai dây"),           ("G.8.5", "Ghép tất cả và chạy"),
};

// Mỗi nhóm là một hàm kiểm chạy độc lập được.
var nhom = new Dictionary<string, Func<Task>>(StringComparer.OrdinalIgnoreCase)
{
    ["G1"] = async () => { await KiemMien.Chay(); await KiemMienVaLogic2.Chay(); },
    ["G2"] = async () => { await KiemLogicVaThietBi.Chay(); await KiemMienVaLogic2.Chay(); },
    ["G3"] = async () => { await KiemLogicVaThietBi.Chay(); await KiemThietBiVaNghiepVu2.Chay(); },
    ["G4"] = async () => { await KiemNghiepVuVaTrinhTu.Chay(); await KiemThietBiVaNghiepVu2.Chay(); },
    ["G5"] = async () => { await KiemNghiepVuVaTrinhTu.Chay(); await KiemTrinhTuVaVanHanh2.Chay(); },
    ["G6"] = KiemDuLieu.Chay,
    ["G7"] = KiemGiaoDien.Chay,
    ["G8"] = async () => { await KiemRapNoi.Chay(); await KiemTrinhTuVaVanHanh2.Chay(); },
    ["G9"] = KiemMayHoanChinh.Chay,          // ghép toàn máy
    ["G11"] = KiemMayHoanChinh.Chay,
    ["G12"] = KiemCauHinh.Chay,            // tách cấu hình máy khỏi chương trình
    ["G13"] = KiemVanHanhThuc.Chay,        // năng lực vận hành máy thật            // tách cấu hình máy khỏi chương trình
    ["H"]   = KiemPhuLucH.Chay,            // Phụ lục H: khẳng định về ngôn ngữ C#
    ["G14"] = KiemNguoc.Chay,              // kiểm ngược: đối chiếu bất biến với mã thật
};

string tuyChon = args.Length > 0 ? args[0].Trim() : "";

if (tuyChon == "--demo")
{
    await ChayDemo();
    return 0;
}

if (tuyChon == "--danhsach")
{
    foreach (var (ma, ten) in danhSachBai) Console.WriteLine($"{ma}  {ten}");
    return 0;
}

Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
Console.WriteLine("║  MeoBench — tự kiểm ĐỦ 40 bài của Phụ lục G              ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

if (string.IsNullOrEmpty(tuyChon))
{
    await KiemMien.Chay();
    await KiemMienVaLogic2.Chay();
    await KiemLogicVaThietBi.Chay();
    await KiemNghiepVuVaTrinhTu.Chay();
    await KiemThietBiVaNghiepVu2.Chay();
    await KiemDuLieu.Chay();
    await KiemGiaoDien.Chay();
    await KiemRapNoi.Chay();
    await KiemTrinhTuVaVanHanh2.Chay();
    await KiemMayHoanChinh.Chay();
    await KiemCauHinh.Chay();
    await KiemVanHanhThuc.Chay();
    await KiemPhuLucH.Chay();
    await KiemNguoc.Chay();
}
else
{
    // "G4.1" / "G.4.1" / "g4" đều quy về khoá nhóm "G4"; "H" là khoá một ký tự.
    // Thử khoá DÀI trước: nếu không, "G12" sẽ bị cắt thành "G1" và chạy nhầm nhóm.
    string chuan = tuyChon.Replace(".", "", StringComparison.Ordinal);
    Func<Task>? chay = null;
    for (int n = Math.Min(chuan.Length, 3); n >= 1 && chay is null; n--)
        nhom.TryGetValue(chuan[..n], out chay);

    if (chay is null)
    {
        Console.WriteLine($"Không có nhóm nào khớp '{tuyChon}'. Các bài có sẵn:");
        foreach (var (ma, ten) in danhSachBai) Console.WriteLine($"   {ma}  {ten}");
        return 2;
    }

    Console.WriteLine($"Chỉ chạy nhóm khớp '{tuyChon}':");
    await chay();
}

return Kiem.TongKet();

// ── Chạy CỖ MÁY HOÀN CHỈNH để nhìn bằng mắt ───────────────────────────
static async Task ChayDemo()
{
    string goc = Path.Combine(Path.GetTempPath(), "MeoBench_demo");
    Directory.CreateDirectory(goc);

    var dongHo = new DongHoGia(new DateTime(2026, 9, 20, 6, 0, 0));
    var anToan = new AnToanGiaLap();
    double apSuat = 6.00;

    var ch = new CauHinhHoanChinh
    {
        May          = new CauHinhMay { HatGiongGiaLap = 2026 },
        ThuMucDuLieu = goc,
        MaCa         = "CA-A-" + dongHo.BayGio.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
    };

    using var may = new MayHoanChinh(ch, dongHo, anToan, new CongThuc { Ten = "SanPhamA" }, () => apSuat);

    // In nhật ký có cấu trúc ra màn hình — đây là CỬA RA THỨ BA, cắm thêm
    // vào đường ống log mà không lớp nào phải sửa (mục 19.4.1b).
    may.NhatKy.ThemCuaRa(new CuaRaManHinh());

    Console.WriteLine("═══ MeoBench-01 · nhật ký cỗ máy hoàn chỉnh ═══");
    Console.WriteLine();

    await may.KhoiTaoAsync();
    await may.ChayAsync(6);

    Console.WriteLine();
    Console.WriteLine("--- khí nén tụt xuống 4,2 bar ---");
    apSuat = 4.20;
    await may.ChayAsync(6);

    Console.WriteLine();
    Console.WriteLine("--- khí phục hồi, màn chắn sáng bị che ---");
    apSuat = 6.00;
    anToan.ManChanBiChe = true;
    await may.ChayAsync(3);

    Console.WriteLine();
    Console.WriteLine("--- bỏ che, cảm biến chiều dày hỏng ---");
    anToan.ManChanBiChe = false;
    may.CamBien.MoPhongKhongPhanHoi = true;
    try { await may.ChayAsync(5); } catch (AlarmException) { /* đã ghi log */ }

    Console.WriteLine();
    Console.WriteLine("--- xác nhận cảnh báo, reset, chạy lại ---");
    may.CamBien.MoPhongKhongPhanHoi = false;
    foreach (var cb in may.BangCanhBao.DangHoatDong.ToList()) may.XacNhanCanhBao(cb.Ma);
    may.Reset();
    await may.KhoiTaoAsync();
    await may.ChayAsync(4);

    may.TatMay();

    Console.WriteLine();
    Console.WriteLine("┌─ TỔNG KẾT ──────────────────────────────────────────────");
    Console.WriteLine($"│ Trạng thái cuối : {may.TrangThai}");
    Console.WriteLine($"│ Công thức       : {may.CongThuc.Ten} "
                    + $"({may.CongThuc.GioiHanDuoiMm:F3}–{may.CongThuc.GioiHanTrenMm:F3} mm)");
    Console.WriteLine($"│ Sản lượng ca    : {may.BoDem.HienTai.Tong} phôi · "
                    + $"{may.BoDem.HienTai.Ok} đạt · {may.BoDem.HienTai.Ng} không đạt · "
                    + $"tỷ lệ {may.BoDem.TyLeDat:F1} %");
    Console.WriteLine($"│ Bắt tay máy sau : chuyển {may.BatTay.SoPhoiDaChuyen} phôi · "
                    + $"đói {may.BatTay.SoGiayDoiHang} nhịp · bị chặn {may.BatTay.SoGiayBiChan} nhịp");
    Console.WriteLine($"│ Nhật ký         : {may.CuaRaBoNho.TatCa.Count} bản ghi "
                    + $"(bảng giao diện giữ {may.BangLog.SoDongDangGiu})");
    Console.WriteLine($"│ File kết quả    : {Path.GetFileName(may.SoGhi.DuongDanHomNay)}");
    Console.WriteLine($"│ Hạt giống       : {ch.May.HatGiongGiaLap} — chạy lại ra đúng kết quả trên");
    Console.WriteLine("└─────────────────────────────────────────────────────────");

    try { Directory.Delete(goc, recursive: true); }
#pragma warning disable CA1031
    catch (Exception) { }
#pragma warning restore CA1031
}

/// <summary>Cửa ra thứ ba: in nhật ký ra màn hình console.</summary>
internal sealed class CuaRaManHinh : ICuaRaLog
{
    public void Nhan(BanGhiLog ban)
    {
        ArgumentNullException.ThrowIfNull(ban);
        string muc = ban.Muc switch
        {
            MucLog.Loi      => "LỖI  ",
            MucLog.CanhBao  => "CẢNH ",
            MucLog.ThongTin => "     ",
            _               => "gỡ   ",
        };
        Console.WriteLine($"  {ban.ThoiDiem:HH:mm:ss} {muc} {ban.Nguon,-9} {ban.DungCau()}");
    }
}
