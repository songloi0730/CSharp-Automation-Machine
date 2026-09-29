// -------------------------------------------------------
// File:    KiemDuongBoQua.cs
// Project: MeoBench
// Purpose: Phép kiểm cho đường bỏ qua có chủ ý (mục 17.6, Phụ lục J).
//          Mỗi phép kiểm ứng với một tính chất mà bản sửa vội bằng cách
//          comment dòng kiểm tra KHÔNG có.
// -------------------------------------------------------

namespace MeoBench;

/// <summary>Phép kiểm cho <see cref="QuanLyBoQua"/>.</summary>
public static class KiemDuongBoQua
{
    private static readonly KetQuaPhepKiem OcrTruot = new(false, "đọc 'A1B2C3' ≠ MES 'A1B2C8'");

    /// <summary>Chạy toàn bộ.</summary>
    public static Task Chay()
    {
        Kiem.MoBai("17.6", "Đường bỏ qua có chủ ý cho phép kiểm chất lượng");

        var dongHo = new DongHoGia(new DateTime(2026, 9, 29, 2, 0, 0));
        var phien = new PhienDangNhap(dongHo) { HetHanSauKhiKhongThaoTac = TimeSpan.FromDays(1) };
        var thuMuc = Path.Combine(Path.GetTempPath(), "MeoBench_BoQua_" + Guid.NewGuid().ToString("N"));
        var vet = new VetKiemToan(Path.Combine(thuMuc, "vet.csv"), dongHo);
        var nhatKy = new NhatKy(dongHo);
        var log = new CuaRaBoNho();
        nhatKy.ThemCuaRa(log);
        var bq = new QuanLyBoQua(phien, vet, nhatKy, dongHo);
        bq.KhaiBao("OCR_TRAM1", LoaiPhepKiem.ChatLuong);
        bq.KhaiBao("CUA_BAO_VE", LoaiPhepKiem.AnToan);

        // 1. Chưa bỏ qua: kết quả thật quyết định
        var kq = bq.Xet("OCR_TRAM1", OcrTruot);
        Kiem.Dung(!kq.ChoChayTiep && !kq.DaBoQua, "chưa bật bỏ qua: OCR trượt thì máy dừng");

        // 2. Chưa đăng nhập đủ quyền thì không bật được
        var tuChoi = bq.Bat("OCR_TRAM1", "đầu đọc hỏng, chờ hàng thay", TimeSpan.FromHours(8));
        Kiem.Dung(!tuChoi.ChoPhep && tuChoi.LyDoTuChoi!.Contains("đăng nhập", StringComparison.Ordinal),
                  "chưa đăng nhập: từ chối kèm lý do");

        phien.DangNhap("An", MucNguoiDung.VanHanh);
        Kiem.Dung(!bq.Bat("OCR_TRAM1", "đầu đọc hỏng", TimeSpan.FromHours(8)).ChoPhep,
                  "mức Vận hành: không đủ quyền bật");

        phien.DangNhap("Binh", MucNguoiDung.KyThuat);

        // 3. Phép kiểm an toàn: không bao giờ, kể cả mức cao nhất
        phien.DangNhap("Quan", MucNguoiDung.QuanTri);
        Kiem.Dung(!bq.Bat("CUA_BAO_VE", "căn chỉnh trục", TimeSpan.FromMinutes(10)).ChoPhep,
                  "phép kiểm AN TOÀN: từ chối kể cả mức Quản trị");
        phien.DangNhap("Binh", MucNguoiDung.KyThuat);

        // 4. Phải có lý do, phải có hạn
        Kiem.Dung(!bq.Bat("OCR_TRAM1", "  ", TimeSpan.FromHours(8)).ChoPhep, "không ghi lý do: từ chối");
        Kiem.Dung(!bq.Bat("OCR_TRAM1", "đầu đọc hỏng", TimeSpan.FromDays(3)).ChoPhep, "thời hạn quá 12 giờ: từ chối");
        Kiem.Dung(!bq.Bat("KHONG_CO", "thử", TimeSpan.FromHours(1)).ChoPhep, "phép kiểm chưa khai báo: từ chối");

        // 5. Bật hợp lệ: có vết kiểm toán, có banner
        Kiem.Dung(bq.Bat("OCR_TRAM1", "đầu đọc hỏng, chờ hàng thay", TimeSpan.FromHours(8)).ChoPhep,
                  "mức Kỹ thuật, có lý do, có hạn: bật được");
        Kiem.Dung(vet.TatCa.Count == 1 && vet.TatCa[0].NguoiDung == "Binh", "vết kiểm toán ghi ai bật");
        var banner = bq.DongBanner();
        Kiem.Dung(banner.Count == 1 && banner[0].Contains("OCR_TRAM1", StringComparison.Ordinal)
                  && banner[0].Contains("còn 8 giờ", StringComparison.Ordinal), "banner hiện tên phép kiểm và thời gian còn lại");

        // 6. Đang bỏ qua: máy chạy tiếp NHƯNG kết quả gốc vẫn còn và sản phẩm bị đánh dấu
        kq = bq.Xet("OCR_TRAM1", OcrTruot);
        Kiem.Dung(kq.ChoChayTiep && kq.DaBoQua, "đang bỏ qua: OCR trượt vẫn cho chạy tiếp");
        Kiem.Dung(!kq.DatGoc, "kết quả GỐC vẫn là trượt, không bị che");
        Kiem.Dung(kq.GhiChuSanPham.StartsWith("BOQUA:OCR_TRAM1|", StringComparison.Ordinal),
                  "bản ghi sản phẩm mang dấu BOQUA — truy vết được về sau");
        Kiem.Dung(log.Loc("ChiTiet", OcrTruot.ChiTiet).Any(b => b.Muc == MucLog.CanhBao
                                                              && Equals(b.ThuocTinh["Nguoi"], "Binh")),
                  "mỗi lần bỏ qua ghi một dòng cảnh báo: kết quả gốc + người bật");

        // 7. Phép kiểm đạt thì không đánh dấu gì
        kq = bq.Xet("OCR_TRAM1", new KetQuaPhepKiem(true, ""));
        Kiem.Dung(kq.ChoChayTiep && !kq.DaBoQua && kq.GhiChuSanPham.Length == 0, "OCR đạt: không gắn dấu bỏ qua");

        // 8. Hết hạn thì tự trở lại kiểm — không cần ai nhớ tắt
        dongHo.Tien(TimeSpan.FromHours(8) + TimeSpan.FromMinutes(1));
        kq = bq.Xet("OCR_TRAM1", OcrTruot);
        Kiem.Dung(!kq.ChoChayTiep && !kq.DaBoQua, "quá hạn: tự trở lại kiểm thật");
        Kiem.Bang(bq.DongBanner().Count, 0, "quá hạn: banner biến mất");
        Kiem.Dung(vet.TatCa.Any(b => b.Viec.StartsWith("Tự hết hạn", StringComparison.Ordinal)), "vết kiểm toán ghi lúc tự hết hạn");

        try { Directory.Delete(thuMuc, recursive: true); } catch (IOException) { /* thư mục tạm, bỏ qua */ }
        return Task.CompletedTask;
    }
}
