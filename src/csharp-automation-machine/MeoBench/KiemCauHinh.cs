// -------------------------------------------------------
// File:    KiemCauHinh.cs
// Project: MeoBench
// Purpose: Kiểm phần tách cấu hình — trọng tâm là câu hỏi CHỈ LỘ RA KHI
//          CẬP NHẬT PHẦN MỀM: "bản mới có đè mất cấu hình của cỗ máy này không?"
// -------------------------------------------------------
using System.Text;

namespace MeoBench;

public static class KiemCauHinh
{
    public static Task Chay()
    {
        string goc = Path.Combine(Path.GetTempPath(), "MeoBench_G12_" + Guid.NewGuid().ToString("N")[..8]);
        try { ChayThat(goc); }
        finally
        {
            try { if (Directory.Exists(goc)) Directory.Delete(goc, recursive: true); }
#pragma warning disable CA1031
            catch (Exception) { }
#pragma warning restore CA1031
        }
        return Task.CompletedTask;
    }

    private static void ChayThat(string goc)
    {
        // ---------- G.12.1 ----------
        Kiem.MoBai("G.12.1", "Ba thư mục: config, product, data");
        var kho = new KhoCauHinh(goc);
        Kiem.Dung(kho.TaoNeuThieu(), "lần đầu → tạo đủ thư mục và file mặc định");
        Kiem.Dung(Directory.Exists(kho.ThuMucConfig),  "có thư mục config/");
        Kiem.Dung(Directory.Exists(kho.ThuMucProduct), "có thư mục product/");
        Kiem.Dung(Directory.Exists(kho.ThuMucData),    "có thư mục data/");
        Kiem.Dung(File.Exists(kho.FileCoMay), "có config/may.json");
        Kiem.Dung(File.Exists(kho.FileDiem),  "có config/diem.json");
        Kiem.Dung(File.Exists(kho.FileMang),  "có config/mang.json");
        Kiem.Bang(kho.LietKeSanPham().Count, 1, "có sẵn một sản phẩm mặc định trong product/");
        Kiem.Dung(!kho.TaoNeuThieu(), "gọi lần hai → không tạo gì thêm");

        // ---------- G.12.2 — phép kiểm QUAN TRỌNG NHẤT của cả mục ----------
        Kiem.MoBai("G.12.2", "Cập nhật phần mềm KHÔNG đè cấu hình cỗ máy");

        // Kỹ thuật viên chỉnh máy: đổi điểm dạy và IP cho đúng cỗ máy này
        File.WriteAllText(kho.FileDiem,
            """{"ViTriDoMm":41.5,"ViTriAnToanMm":2.0,"MangOkMm":150.0,"MangNgMm":250.0,"ViTriChoMm":5.0}""",
            Encoding.UTF8);
        File.WriteAllText(kho.FileMang,
            """{"DiaChiIp":"10.20.30.44","Cong":502,"CongNoiTiep":"COM7","TocDoBaud":19200}""",
            Encoding.UTF8);
        kho.LuuSanPham(new CongThuc { Ten = "SanPhamKhachA", ChieuDayDanhDinhMm = 3.500 });

        // ★ "Cập nhật phần mềm": chương trình mới, CÙNG gốc dữ liệu
        var khoSauCapNhat = new KhoCauHinh(goc);
        var nap = khoSauCapNhat.Nap();

        Kiem.Dung(!nap.VuaTaoMacDinh, "★ bản mới KHÔNG tạo lại file nào — cấu hình cũ còn nguyên");
        Kiem.Gan(nap.Diem.ViTriDoMm, 41.5, 1e-9, "★ ĐIỂM DẠY của cỗ máy này SỐNG SÓT qua cập nhật");
        Kiem.Gan(nap.Diem.MangNgMm, 250.0, 1e-9, "★ vị trí máng NG cũng còn nguyên");
        Kiem.Bang(nap.Mang.DiaChiIp, "10.20.30.44", "★ ĐỊA CHỈ IP của cỗ máy này còn nguyên");
        Kiem.Bang(nap.Mang.CongNoiTiep, "COM7", "★ cổng nối tiếp còn nguyên");
        Kiem.Dung(khoSauCapNhat.LietKeSanPham().Contains("SanPhamKhachA"),
                  "★ công thức khách hàng còn nguyên sau cập nhật");
        Kiem.Bang(nap.CanhBao.Count, 0, "cấu hình hợp lệ → không cảnh báo");

        // ---------- G.12.3 ----------
        Kiem.MoBai("G.12.3", "Điểm dạy phải nằm TRONG hành trình của chính cỗ máy này");
        string goc3 = Path.Combine(goc, "m3");
        var kho3 = new KhoCauHinh(goc3);
        kho3.TaoNeuThieu();
        File.WriteAllText(kho3.FileCoMay,
            """
            {"MaMay":"MEOBENCH-02","BienThe":"Phai","HanhTrinhZMm":40.0,"HanhTrinhXMm":300.0,
             "XungMoiMmZ":1000.0,"XungMoiMmX":1000.0,"HanGioTrucMs":3000,"ApSuatNguongBar":5.0}
            """,
            Encoding.UTF8);
        // điểm dạy chép NHẦM từ một cỗ máy hành trình dài hơn
        File.WriteAllText(kho3.FileDiem,
            """{"ViTriDoMm":30.0,"ViTriAnToanMm":0.0,"MangOkMm":100.0,"MangNgMm":380.0,"ViTriChoMm":0.0}""",
            Encoding.UTF8);

        var nap3 = kho3.Nap();
        Kiem.Bang(nap3.CoMay.MaMay, "MEOBENCH-02", "đọc đúng mã cỗ máy");
        Kiem.Bang(nap3.CoMay.BienThe, "Phai", "đọc đúng biến thể");
        Kiem.Dung(nap3.CanhBao.Any(c => c.Contains("MangNgMm", StringComparison.Ordinal)),
                  "★ chép nhầm điểm dạy từ máy khác → BỊ BẮT ngay lúc nạp, không đợi trục đâm");
        Kiem.Dung(nap3.CanhBao.Any(c => c.Contains("380", StringComparison.Ordinal)),
                  "cảnh báo nói rõ giá trị nào và hành trình bao nhiêu");

        // ---------- G.12.4 ----------
        Kiem.MoBai("G.12.4", "File cấu hình hỏng không làm sập máy");
        string goc4 = Path.Combine(goc, "m4");
        var kho4 = new KhoCauHinh(goc4);
        kho4.TaoNeuThieu();
        File.WriteAllText(kho4.FileMang, "{{{ rác", Encoding.UTF8);

        var nap4 = kho4.Nap();
        Kiem.Bang(nap4.Mang.CongNoiTiep, "COM3", "★ mang.json hỏng → dùng mặc định, KHÔNG ném");
        Kiem.Dung(nap4.CanhBao.Any(c => c.Contains("mang.json", StringComparison.Ordinal)
                                     && c.Contains("hỏng", StringComparison.Ordinal)),
                  "★ và có cảnh báo nói rõ FILE NÀO hỏng — không im lặng");
        Kiem.Gan(nap4.Diem.ViTriDoMm, 30.0, 1e-9, "các file lành khác vẫn đọc bình thường");

        // ---------- G.12.5 ----------
        Kiem.MoBai("G.12.5", "Gốc dữ liệu: tìm ở đâu và cảnh báo khi nằm sai chỗ");
        var ep = GocDuLieu.Tim(epDuongDan: @"D:\DuLieuMay");
        Kiem.Bang(ep.Kieu, KieuGocDuLieu.BienMoiTruong, "ép đường dẫn → ưu tiên cao nhất");
        Kiem.Dung(ep.CanhBao is null, "đặt tường minh thì không cảnh báo");

        var mac = GocDuLieu.Tim();
        Kiem.Bang(mac.Kieu, KieuGocDuLieu.DuLieuHeDieuHanh,
                  "★ mặc định dùng thư mục dữ liệu hệ điều hành — chỗ bộ cài KHÔNG xoá khi cập nhật");
        Kiem.Dung(mac.DuongDan.Contains("MeoBench", StringComparison.Ordinal), "có tên ứng dụng trong đường dẫn");
        Kiem.Dung(mac.CanhBao is null, "chỗ này là chỗ đúng → không cảnh báo");

        // ---------- G.12.6 ----------
        Kiem.MoBai("G.12.6", "Sản phẩm chép được giữa các máy, cấu hình thì không");
        string gocA = Path.Combine(goc, "mayA");
        string gocB = Path.Combine(goc, "mayB");
        var khoA = new KhoCauHinh(gocA); khoA.TaoNeuThieu();
        var khoB = new KhoCauHinh(gocB); khoB.TaoNeuThieu();

        khoA.LuuSanPham(new CongThuc { Ten = "SP-X", ChieuDayDanhDinhMm = 1.200 });
        // chép SẢN PHẨM sang máy B — việc này HỢP LỆ
        File.Copy(Path.Combine(khoA.ThuMucProduct, "SP-X.json"),
                  Path.Combine(khoB.ThuMucProduct, "SP-X.json"));
        var spB = khoB.NapSanPham("SP-X", new CongThuc(), out string? lyDoB);
        Kiem.Gan(spB.ChieuDayDanhDinhMm, 1.200, 1e-9, "★ chép công thức sang máy khác → dùng được ngay");
        Kiem.Dung(lyDoB is null, "chép sản phẩm không sinh cảnh báo");
        Kiem.Bang(khoB.LietKeSanPham().Count, 2, "máy B nay có 2 sản phẩm");

        // còn CẤU HÌNH thì mỗi máy một bản — chép sang là sai
        File.WriteAllText(khoA.FileMang,
            """{"DiaChiIp":"10.0.0.11","Cong":502,"CongNoiTiep":"COM4","TocDoBaud":9600}""", Encoding.UTF8);
        var napA = khoA.Nap();
        var napB = khoB.Nap();
        Kiem.Dung(napA.Mang.DiaChiIp != napB.Mang.DiaChiIp,
                  "★ hai cỗ máy cùng loại có IP KHÁC NHAU — đó là lý do config không nằm chung với chương trình");

        Kiem.Bang(khoA.NapSanPham("KhongCo", new CongThuc { Ten = "DuPhong" }, out string? lyDoKhong).Ten,
                  "DuPhong", "gọi sản phẩm không tồn tại → dùng dự phòng");
        Kiem.Dung(lyDoKhong is not null, "và nói rõ lý do");
    }
}
