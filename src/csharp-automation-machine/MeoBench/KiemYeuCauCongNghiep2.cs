// -------------------------------------------------------
// File:    KiemYeuCauCongNghiep2.cs
// Project: MeoBench
// Purpose: Phép kiểm cho Phụ lục L, lô 2: đèn tháp và còi, gói chẩn đoán, giấy phép.
// -------------------------------------------------------
using System.IO.Compression;
using System.Text;

namespace MeoBench;

/// <summary>Phép kiểm cho lô 2 của Phụ lục L.</summary>
public static class KiemYeuCauCongNghiep2
{
    /// <summary>Chạy toàn bộ.</summary>
    public static void Chay()
    {
        KiemDenThapVaCoi();
        KiemGoiChanDoan();
        KiemGiayPhep();
    }

    // ── 15.1.9 ─────────────────────────────────────────────────────────
    private static void KiemDenThapVaCoi()
    {
        Kiem.MoBai("15.1.9", "Đèn tháp và còi — tắt còi không phải xác nhận");
        var dt = new DenThapVaCoi();
        var khong = Array.Empty<CanhBaoChuaXacNhan>();

        Kiem.Bang(dt.Tinh(TrangThaiMay.DangChay, khong), new TinHieuThap(MauDen.Xanh, false, KieuCoi.Tat),
                  "đang chạy, không có cảnh báo: xanh sáng, còi im");
        Kiem.Bang(dt.Tinh(TrangThaiMay.TamDung, khong).NhapNhay, true, "tạm dừng: vàng nhấp nháy");

        var apThap = new CanhBaoChuaXacNhan("AP_KHI_THAP", MucUuTienCanhBao.Cao);
        var cuaMo = new CanhBaoChuaXacNhan("CUA_MO", MucUuTienCanhBao.KhanCap);
        Kiem.Bang(dt.Tinh(TrangThaiMay.DangChay, [apThap]), new TinHieuThap(MauDen.Vang, true, KieuCoi.NgatQuang),
                  "đang chạy mà có cảnh báo mức Cao: vàng nhấp nháy, còi ngắt quãng");
        Kiem.Bang(dt.Tinh(TrangThaiMay.BaoDong, [apThap, cuaMo]).Coi, KieuCoi.LienTuc,
                  "nhiều cảnh báo: còi theo mức CAO NHẤT (khẩn cấp → liên tục)");

        dt.TatCoi([apThap, cuaMo]);
        var sauTat = dt.Tinh(TrangThaiMay.BaoDong, [apThap, cuaMo]);
        Kiem.Dung(sauTat.Coi == KieuCoi.Tat && sauTat.Den == MauDen.Do && sauTat.NhapNhay,
                  "tắt còi: còi im, nhưng đèn đỏ VẪN nhấp nháy — hai cảnh báo vẫn chưa được xác nhận");

        var moi = new CanhBaoChuaXacNhan("SERVO_QUA_NHIET", MucUuTienCanhBao.Cao);
        Kiem.Bang(dt.Tinh(TrangThaiMay.BaoDong, [apThap, cuaMo, moi]).Coi, KieuCoi.NgatQuang,
                  "cảnh báo MỚI sau khi tắt còi: còi kêu lại theo mức của cảnh báo mới");

        dt.Quen("CUA_MO");
        Kiem.Bang(dt.Tinh(TrangThaiMay.BaoDong, [cuaMo]).Coi, KieuCoi.LienTuc,
                  "cửa mở đã xác nhận xong rồi xuất hiện lại: còi kêu lại");
        Kiem.Bang(dt.Tinh(TrangThaiMay.BaoDong, khong), new TinHieuThap(MauDen.Do, false, KieuCoi.Tat),
                  "báo động đã xác nhận hết nhưng chưa reset: đỏ sáng, không nhấp nháy, còi im");
    }

    // ── 19.7 ───────────────────────────────────────────────────────────
    private static void KiemGoiChanDoan()
    {
        Kiem.MoBai("19.7", "Gói chẩn đoán một nút");
        var thuMuc = Path.Combine(Path.GetTempPath(), "MeoBench_GoiCD_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(thuMuc);
        try
        {
            var json = Path.Combine(thuMuc, "may.json");
            File.WriteAllText(json, "{ \"TocDoTruc\": 50, \"MatKhauKySu\": \"abc123\", \"MesConnectionString\": \"Server=10.1.1.5;Password=xyz\" }");
            var ini = Path.Combine(thuMuc, "camera.ini");
            File.WriteAllText(ini, "[Camera]\nIp=192.168.0.10\nPassword=cam456\n");
            var logCu = Path.Combine(thuMuc, "may_cu.log");
            File.WriteAllText(logCu, new string('x', 4000));
            File.SetLastWriteTimeUtc(logCu, DateTime.UtcNow.AddDays(-3));
            var logMoi = Path.Combine(thuMuc, "may.log");

            var dh = new DongHoGia(new DateTime(2026, 10, 21, 2, 30, 0));
            string zip = Path.Combine(thuMuc, "chan_doan.zip");

            // Giữ tệp nhật ký MỞ để ghi, như bộ ghi nhật ký của một máy đang chạy
            using (var dangGhi = new FileStream(logMoi, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                var b = Encoding.UTF8.GetBytes("02:14 Lỗi trục Z quá giờ\n");
                dangGhi.Write(b, 0, b.Length);
                dangGhi.Flush();
                var goi = new GoiChanDoan(dh) { GioiHanByte = 2000 };
                var kq = goi.Tao(zip, "May: MAY-01 · Phiên bản 2.3.1", [logMoi, logCu], [json, ini]);
                Kiem.Dung(kq.BoQua.Count == 1 && kq.BoQua[0].Contains("may_cu.log", StringComparison.Ordinal),
                          "vượt giới hạn dung lượng: bỏ nhật ký CŨ, giữ nhật ký mới");
            }

            using var doc = ZipFile.OpenRead(zip);
            string Doc(string ten)
            {
                using var r = new StreamReader(doc.GetEntry(ten)!.Open(), Encoding.UTF8);
                return r.ReadToEnd();
            }
            Kiem.Dung(Doc("nhat_ky/may.log").Contains("trục Z", StringComparison.Ordinal),
                      "đọc được tệp nhật ký ĐANG bị bộ ghi mở");
            var cauHinh = Doc("cau_hinh/may.json");
            Kiem.Dung(!cauHinh.Contains("abc123", StringComparison.Ordinal) && !cauHinh.Contains("xyz", StringComparison.Ordinal)
                      && cauHinh.Contains("\"TocDoTruc\": 50", StringComparison.Ordinal),
                      "JSON: che mật khẩu và chuỗi kết nối, giữ nguyên tham số thường");
            var cam = Doc("cau_hinh/camera.ini");
            Kiem.Dung(!cam.Contains("cam456", StringComparison.Ordinal) && cam.Contains("Ip=192.168.0.10", StringComparison.Ordinal),
                      "INI: che dòng Password, giữ địa chỉ IP");
            var gt = Doc("GIOI_THIEU.txt");
            Kiem.Dung(gt.Contains("MAY-01", StringComparison.Ordinal) && gt.Contains("may_cu.log", StringComparison.Ordinal),
                      "tệp giới thiệu ghi máy, phiên bản, và thứ đã bị bỏ ra ngoài");
            Kiem.Bang(GoiChanDoan.CheBiMat("Pwd = 1234"), "Pwd =***", "che cả khoá viết tắt 'Pwd'");
        }
        finally
        {
            try { Directory.Delete(thuMuc, recursive: true); }
#pragma warning disable CA1031 // dọn thư mục tạm hỏng thì bỏ qua
            catch (Exception) { }
#pragma warning restore CA1031
        }
    }

    // ── 17.3.9 ─────────────────────────────────────────────────────────
    private static void KiemGiayPhep()
    {
        Kiem.MoBai("17.3.9", "Giấy phép của thư viện hãng — báo trước, không dừng giữa chu kỳ");
        var hom = new DateTime(2026, 10, 21, 23, 50, 0);
        var mat = XetGiayPhep.Xet(new TrangThaiGiayPhep("thư viện thị giác", false, null), hom);
        Kiem.Dung(mat.Muc == MucGiayPhep.KhongDung && mat.ThongBao.Contains("khoá cứng", StringComparison.Ordinal),
                  "không thấy khoá cứng: không dùng được, nói rõ cần kiểm gì");
        Kiem.Bang(XetGiayPhep.Xet(new TrangThaiGiayPhep("card chuyển động", true, null), hom).Muc, MucGiayPhep.Tot,
                  "giấy phép vĩnh viễn: tốt");
        var sap = XetGiayPhep.Xet(new TrangThaiGiayPhep("thư viện thị giác", true, new DateTime(2026, 11, 2)), hom);
        Kiem.Dung(sap.Muc == MucGiayPhep.SapHet && sap.SoNgayCon == 12 && sap.ThongBao.Contains("còn 12 ngày", StringComparison.Ordinal),
                  "còn 12 ngày: báo sắp hết, thông báo ghi đúng SỐ NGÀY (không phải một ngày tháng)");
        Kiem.Bang(XetGiayPhep.Xet(new TrangThaiGiayPhep("x", true, new DateTime(2026, 10, 22)), hom).SoNgayCon, 1,
                  "23:50 hôm nay tới hết hạn ngày mai: còn 1 ngày theo lịch, không phải 0");
        Kiem.Bang(XetGiayPhep.Xet(new TrangThaiGiayPhep("x", true, new DateTime(2026, 10, 21)), hom).Muc, MucGiayPhep.KhongDung,
                  "hết hạn hôm nay: không dùng được");
        Kiem.Bang(XetGiayPhep.Xet(new TrangThaiGiayPhep("x", true, new DateTime(2027, 6, 1)), hom).Muc, MucGiayPhep.Tot,
                  "còn hơn 30 ngày: tốt");
    }
}
