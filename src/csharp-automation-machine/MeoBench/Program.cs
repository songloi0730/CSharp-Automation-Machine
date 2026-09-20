// -------------------------------------------------------
// File:    Program.cs
// Project: MeoBench — lời giải mẫu ĐỦ 40 bài của Phụ lục G
// Purpose: Bộ chạy tự kiểm. Chạy được TỪNG NHÓM riêng, không phải làm xong
//          hết mới biết sai ở đâu.
//
//   dotnet run                → chạy toàn bộ 40 bài
//   dotnet run -- G4          → chỉ chạy nhóm G.4 (bài G.4.1 và G.4.3)
//   dotnet run -- G8          → chỉ chạy nhóm G.8
//   dotnet run -- --demo      → chạy máy 20 chu kỳ và in nhật ký
//   dotnet run -- --danhsach  → liệt kê 40 bài
// -------------------------------------------------------
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
}
else
{
    // "G4.1" / "G.4.1" / "g4" đều quy về khoá nhóm "G4"
    string chuan = tuyChon.Replace(".", "", StringComparison.Ordinal);
    string khoa  = chuan.Length >= 2 ? chuan[..2] : chuan;

    if (!nhom.TryGetValue(khoa, out var chay))
    {
        Console.WriteLine($"Không có nhóm nào khớp '{tuyChon}'. Các bài có sẵn:");
        foreach (var (ma, ten) in danhSachBai) Console.WriteLine($"   {ma}  {ten}");
        return 2;
    }

    Console.WriteLine($"Chỉ chạy nhóm khớp '{tuyChon}':");
    await chay();
}

return Kiem.TongKet();

// ── Chạy máy thật sự để nhìn bằng mắt ──────────────────────────────────
static async Task ChayDemo()
{
    Console.WriteLine("=== MeoBench-01 · chạy 20 chu kỳ trên bản giả lập ===");
    var may = RapNoi.Tao(new CauHinhMay { HatGiongGiaLap = 2026 });
    may.DaBaoCao += (_, dong) => Console.WriteLine("  " + dong);

    await may.VeGocAsync();
    await may.ChayAsync(20);

    Console.WriteLine();
    Console.WriteLine($"Trạng thái cuối : {may.TrangThai}");
    Console.WriteLine($"Tổng phôi       : {may.SoPhoi}");
    Console.WriteLine($"Đạt / Không đạt : {may.SoOk} / {may.SoNg}");
    Console.WriteLine($"Hạt giống       : {may.CauHinh.HatGiongGiaLap}  (chạy lại số này ra đúng kết quả trên)");
}
