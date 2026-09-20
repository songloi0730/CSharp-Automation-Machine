// -------------------------------------------------------
// File:    KiemVanHanhThuc.cs
// Project: MeoBench
// Purpose: Kiểm các năng lực "máy thật nào cũng có" mà bản mẫu từng thiếu.
// -------------------------------------------------------
namespace MeoBench;

public static class KiemVanHanhThuc
{
    public static async Task Chay()
    {
        string goc = Path.Combine(Path.GetTempPath(), "MeoBench_G13_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(goc);
        try { await ChayThat(goc).ConfigureAwait(false); }
        finally
        {
            try { Directory.Delete(goc, recursive: true); }
#pragma warning disable CA1031
            catch (Exception) { }
#pragma warning restore CA1031
        }
    }

    private static async Task ChayThat(string goc)
    {
        var dongHo = new DongHoGia(new DateTime(2026, 9, 20, 8, 0, 0));

        // ---------- G.13.1 — phân quyền (13/13 dự án có) ----------
        Kiem.MoBai("G.13.1", "Mức người dùng và phân quyền");
        var phien = new PhienDangNhap(dongHo) { HetHanSauKhiKhongThaoTac = TimeSpan.FromMinutes(15) };
        Kiem.Bang(phien.Muc, MucNguoiDung.ChuaDangNhap, "chưa đăng nhập thì mức thấp nhất");
        Kiem.Dung(!phien.Kiem(MucNguoiDung.KyThuat, "jog trục").ChoPhep, "chưa đăng nhập → không jog được");
        Kiem.Dung(phien.Kiem(MucNguoiDung.KyThuat, "jog trục").LyDoTuChoi!
                       .Contains("Cần đăng nhập", StringComparison.Ordinal),
                  "★ lý do từ chối nói rõ PHẢI LÀM GÌ, không chỉ nói 'không được'");

        phien.DangNhap("vanhanh01", MucNguoiDung.VanHanh);
        Kiem.Dung(phien.Kiem(MucNguoiDung.VanHanh, "bấm Bắt đầu").ChoPhep, "mức Vận hành → bấm Bắt đầu được");
        var tuChoi = phien.Kiem(MucNguoiDung.KyThuat, "sửa công thức");
        Kiem.Dung(!tuChoi.ChoPhep, "mức Vận hành → KHÔNG sửa công thức được");
        Kiem.Dung(tuChoi.LyDoTuChoi!.Contains("cần KyThuat", StringComparison.Ordinal),
                  "nói rõ cần mức nào");

        phien.DangNhap("kysu01", MucNguoiDung.KyThuat);
        Kiem.Dung(phien.Kiem(MucNguoiDung.KyThuat, "sửa công thức").ChoPhep, "mức Kỹ thuật → sửa được");

        dongHo.Tien(TimeSpan.FromMinutes(20));
        Kiem.Bang(phien.MucHienTai(), MucNguoiDung.ChuaDangNhap,
                  "★ quá 15 phút không thao tác → TỰ HẠ QUYỀN (người vận hành hay quên đăng xuất)");

        // ---------- G.13.2 — vết kiểm toán (0/13 dự án có!) ----------
        Kiem.MoBai("G.13.2", "Vết kiểm toán — năng lực KHÔNG dự án nào trong bộ mẫu có");
        var vet = new VetKiemToan(Path.Combine(goc, "data", "kiemtoan.csv"), dongHo);
        vet.Ghi("kysu01", "Đổi chiều dày danh định", "2.000", "2.100");
        vet.Ghi("kysu01", "Đổi dung sai trên", "0.050", "0.080");

        Kiem.Bang(vet.TatCa.Count, 2, "ghi được hai thay đổi");
        Kiem.Bang(vet.TatCa[0].Truoc, "2.000", "lưu giá trị TRƯỚC khi đổi");
        Kiem.Bang(vet.TatCa[0].Sau, "2.100", "lưu giá trị SAU khi đổi");
        Kiem.Bang(vet.TatCa[0].NguoiDung, "kysu01", "lưu ai đổi");
        string csv = File.ReadAllText(Path.Combine(goc, "data", "kiemtoan.csv"));
        Kiem.Dung(csv.Contains("2026-09-20T08:20:00", StringComparison.Ordinal),
                  "★ ghi ra file ngay — trả lời được câu 'hôm đó ai sửa công thức'");

        // ---------- G.13.3 — thử lại (12/13) ----------
        Kiem.MoBai("G.13.3", "Thử lại khi thiết bị lỗi thoáng qua");
        var kb = new CamBienTheoKichBan(hatGiong: 9)
            .Xep(LoaiLoiGiaLap.MatKetNoi, LoaiLoiGiaLap.MatKetNoi, LoaiLoiGiaLap.KhongLoi);

        var kq = await ThuLai.ChayAsync(ct => kb.DocAsync(ct), ThuLai.MacDinhCoThuLai,
                                        soLanToiDa: 3, nghiBanDauMs: 2);
        Kiem.Dung(kq.ThanhCong, "★ hỏng 2 lần rồi tốt → thử lại giúp chu kỳ đi tiếp");
        Kiem.Bang(kq.SoLanThu, 3, "đúng 3 lần thử");
        Kiem.Gan(kq.GiaTri, 2.000, 0.01, "giá trị lần thử thành công");

        var kb2 = new CamBienTheoKichBan(hatGiong: 9).Xep(
            LoaiLoiGiaLap.MatKetNoi, LoaiLoiGiaLap.MatKetNoi, LoaiLoiGiaLap.MatKetNoi);
        var kq2 = await ThuLai.ChayAsync(ct => kb2.DocAsync(ct), ThuLai.MacDinhCoThuLai,
                                         soLanToiDa: 3, nghiBanDauMs: 2);
        Kiem.Dung(!kq2.ThanhCong, "hỏng cả 3 lần → báo thất bại");
        Kiem.Dung(kq2.LoiCuoi is not null, "giữ lại lỗi cuối để phát cảnh báo");

        // lỗi VĨNH VIỄN thì KHÔNG thử lại
        int soLanGoi = 0;
        var kq3 = await ThuLai.ChayAsync<double>(
            _ => { soLanGoi++; throw new AlarmException(MaCanhBao.TrucChuaVeGoc, "TRUC_Z", "chưa về gốc"); },
            ThuLai.MacDinhCoThuLai, soLanToiDa: 5, nghiBanDauMs: 1);
        Kiem.Dung(!kq3.ThanhCong, "lỗi vĩnh viễn → thất bại");
        Kiem.Bang(soLanGoi, 1,
                  "★ lỗi VĨNH VIỄN chỉ gọi MỘT lần — thử lại một lỗi không thể tự khỏi chỉ làm chậm việc phát hiện");

        // ---------- G.13.4 — chạy tay và jog (11/13) ----------
        Kiem.MoBai("G.13.4", "Chạy tay và jog trục");
        var tz = new TrucGiaLap("Z", mmMoiBuoc: 20.0);
        var cd = new ChuyenDong(tz, -1.0, 60.0, 1000);
        await cd.VeGocAsync();
        var anToan = new AnToanGiaLap();
        var phien2 = new PhienDangNhap(dongHo);
        var tay = new ChayTay(cd, phien2, anToan) { BuocJogMm = 5.0 };

        var j1 = await tay.JogAsync(2);
        Kiem.Dung(!j1.DaChay, "chưa đăng nhập → KHÔNG jog được");

        phien2.DangNhap("vanhanh01", MucNguoiDung.VanHanh);
        var j2 = await tay.JogAsync(2);
        Kiem.Dung(!j2.DaChay, "★ mức Vận hành KHÔNG được jog — jog là quyền Kỹ thuật");

        phien2.DangNhap("kysu01", MucNguoiDung.KyThuat);
        anToan.ManChanBiChe = true;
        var j3 = await tay.JogAsync(2);
        Kiem.Dung(!j3.DaChay, "đủ quyền nhưng màn chắn bị che → vẫn từ chối");
        Kiem.Bang(j3.LyDoTuChoi, "Màn chắn sáng đang bị che", "★ nói đúng lý do từ chối");

        anToan.ManChanBiChe = false;
        var j4 = await tay.JogAsync(2);
        Kiem.Dung(j4.DaChay, "đủ quyền + an toàn cho phép → jog được");
        Kiem.Gan(j4.ViTriSauMm, 10.0, 1e-6, "jog 2 bước × 5 mm = 10 mm");

        var j5 = await tay.JogAsync(100);
        Kiem.Dung(!j5.DaChay, "★ jog vượt hành trình → bị chặn, KHÔNG đâm cơ khí");
        Kiem.Gan(j5.ViTriSauMm, 10.0, 1e-6, "trục đứng yên sau lần jog bị từ chối");

        // ---------- G.13.5 — đèn tháp (7/13) ----------
        Kiem.MoBai("G.13.5", "Đèn tháp và còi");
        Kiem.Bang(DenThap.Theo(TrangThaiMay.DangChay, false),
                  new TrangThaiDenThap(MauDen.Xanh, false, false), "Đang chạy → xanh, không nháy, không còi");
        Kiem.Bang(DenThap.Theo(TrangThaiMay.SanSang, false),
                  new TrangThaiDenThap(MauDen.Vang, false, false), "Sẵn sàng → vàng");
        Kiem.Bang(DenThap.Theo(TrangThaiMay.TamDung, false),
                  new TrangThaiDenThap(MauDen.Vang, true, false), "Tạm dừng → vàng NHÁY");
        Kiem.Bang(DenThap.Theo(TrangThaiMay.BaoDong, true),
                  new TrangThaiDenThap(MauDen.Do, true, true), "★ Báo động chưa xác nhận → đỏ nháy + CÒI");
        Kiem.Bang(DenThap.Theo(TrangThaiMay.BaoDong, false),
                  new TrangThaiDenThap(MauDen.Do, true, false),
                  "★ xác nhận xong → CÒI TẮT nhưng đèn đỏ vẫn nháy (sự cố chưa được sửa)");
        Kiem.Dung(Enum.GetValues<TrangThaiMay>().All(t => DenThap.Theo(t, false) is not null),
                  "mọi trạng thái đều có tổ hợp đèn — không trạng thái nào 'không biết bật gì'");

        // ---------- G.13.6 — truy xuất nguồn gốc (9/13) ----------
        Kiem.MoBai("G.13.6", "Số sê-ri từng phôi");
        var seri = new SoSeriPhoi("MEOBENCH-01", "CA-A-20260920", 42);
        Kiem.Bang(seri.Ma, "MEOBENCH-01_CA-A-20260920_000042",
                  "★ dấu phân cách là ký tự BỊ CẤM trong từng trường, nên tách ngược được");
        Kiem.Dung(SoSeriPhoi.ThuTach(seri.Ma, out var tach), "tách ngược lại được");
        Kiem.Bang(tach!.SoThuTu, 42, "★ tách ra đúng số thứ tự");
        Kiem.Bang(tach.MaMay, "MEOBENCH-01", "tách ra đúng mã máy");
        Kiem.Bang(tach.MaCa, "CA-A-20260920", "★ tách đúng mã ca dù mã ca CÓ dấu gạch nối");
        Kiem.Dung(string.CompareOrdinal(new SoSeriPhoi("M", "C", 9).Ma, new SoSeriPhoi("M", "C", 10).Ma) < 0,
                  "★ đệm số 0 để sắp xếp bằng chuỗi vẫn đúng thứ tự (9 trước 10)");
        Kiem.Dung(!SoSeriPhoi.ThuTach("rác", out _), "chuỗi lạ → tách thất bại, không ném");
        Kiem.Nem<ArgumentException>(() => _ = new SoSeriPhoi("MAY_01", "CA-A", 1).Ma,
                  "★ mã máy chứa dấu phân cách → BỊ TỪ CHỐI ngay ở hàm dựng, không đợi lúc tách");

        // ---------- G.13.7 — watchdog (5/13) ----------
        Kiem.MoBai("G.13.7", "Watchdog phát hiện chu kỳ treo");
        var dongHo2 = new DongHoGia(new DateTime(2026, 9, 20, 8, 0, 0));
        var wd = new WatchdogChuKy(dongHo2, TimeSpan.FromSeconds(30));
        Kiem.Dung(wd.Kiem() is null, "chưa có nhịp nào → chưa báo gì");

        wd.Nhip();
        dongHo2.Tien(TimeSpan.FromSeconds(10));
        Kiem.Dung(wd.Kiem() is null, "10 giây < ngưỡng 30 → bình thường");

        dongHo2.Tien(TimeSpan.FromSeconds(40));
        var exWd = wd.Kiem();
        Kiem.Dung(exWd is not null, "★ 50 giây không có nhịp → phát cảnh báo treo");
        Kiem.Dung(exWd!.Message.Contains("50", StringComparison.Ordinal), "nói rõ treo bao lâu");
        Kiem.Dung(wd.Kiem() is null, "★ báo một lần rồi tính lại — không đẻ ra lũ cảnh báo mỗi nhịp quét");

        // ---------- G.13.8 — lịch sử cảnh báo (3/13) ----------
        Kiem.MoBai("G.13.8", "Lịch sử cảnh báo lưu ra file");
        var ls = new LichSuCanhBao(Path.Combine(goc, "data", "canhbao.csv"), dongHo);
        ls.Phat(new AlarmException(MaCanhBao.CamBienKhongPhanHoi, "CB_DAY", "mất tín hiệu"));
        ls.Phat(new AlarmException(MaCanhBao.TrucQuaThoiGian, "TRUC_Z", "quá giờ"));
        ls.Phat(new AlarmException(MaCanhBao.CamBienKhongPhanHoi, "CB_DAY", "mất tín hiệu lần nữa"));

        Kiem.Bang(ls.TatCa.Count, 3, "ghi đủ 3 lần cảnh báo");
        Kiem.Dung(ls.XacNhan(MaCanhBao.CamBienKhongPhanHoi), "xác nhận được");
        Kiem.Dung(ls.TatCa[^1].LucXacNhan is not null, "★ lần GẦN NHẤT được đánh dấu đã xác nhận");
        Kiem.Dung(ls.TatCa[0].LucXacNhan is null, "lần cũ hơn vẫn chưa xác nhận");

        var xep = ls.XepTheoTanSuat();
        Kiem.Bang(xep[0].Ma, MaCanhBao.CamBienKhongPhanHoi, "★ xếp được mã nào hay nổ nhất");
        Kiem.Bang(xep[0].SoLan, 2, "đếm đúng số lần");
        Kiem.Dung(File.Exists(Path.Combine(goc, "data", "canhbao.csv")), "lưu ra file, sống sót khởi động lại");

        // ---------- G.13.9 — giờ chạy và tuổi thọ linh kiện (13/13) ----------
        Kiem.MoBai("G.13.9", "Đếm tuổi thọ linh kiện");
        var bt = new SoBaoTri();
        bt.KhaiBao("VanKep", nguongThayThe: 1_000_000);
        bt.KhaiBao("DayDaiTrucX", nguongThayThe: 100);

        bt.Dem("VanKep", 500_000);
        bt.Dem("DayDaiTrucX", 85);
        var vanKep = bt.TatCa.First(t => t.Ten == "VanKep");
        var day    = bt.TatCa.First(t => t.Ten == "DayDaiTrucX");

        Kiem.Gan(vanKep.PhanTramDaDung, 50.0, 1e-9, "van kẹp dùng 50 % tuổi thọ");
        Kiem.Dung(!vanKep.SapDenHan, "50 % → chưa cần chú ý");
        Kiem.Gan(day.PhanTramDaDung, 85.0, 1e-9, "dây đai dùng 85 %");
        Kiem.Dung(day.SapDenHan, "★ 85 % → vào danh sách cần chú ý TRƯỚC khi hỏng");
        Kiem.Bang(bt.CanChuY.Count, 1, "đúng một linh kiện cần chú ý");

        bt.Dem("DayDaiTrucX", 20);
        Kiem.Dung(bt.TatCa.First(t => t.Ten == "DayDaiTrucX").DaQuaHan, "vượt ngưỡng → quá hạn");
        bt.DaThayThe("DayDaiTrucX");
        Kiem.Bang(bt.TatCa.First(t => t.Ten == "DayDaiTrucX").SoLanDaDung, 0L, "thay xong → đếm lại từ 0");

        // ---------- G.13.10 — sao lưu và khôi phục cấu hình (7/13) ----------
        Kiem.MoBai("G.13.10", "Sao lưu và khôi phục cấu hình");
        var kho = new KhoCauHinh(Path.Combine(goc, "may"));
        kho.TaoNeuThieu();
        File.WriteAllText(kho.FileMang,
            """{"DiaChiIp":"10.1.1.1","Cong":502,"CongNoiTiep":"COM9","TocDoBaud":38400}""",
            System.Text.Encoding.UTF8);

        string ban1 = SaoLuuCauHinh.SaoLuu(kho, dongHo);
        Kiem.Dung(Directory.Exists(ban1), "sao lưu tạo được thư mục có dấu thời gian");
        Kiem.Bang(SaoLuuCauHinh.LietKe(kho).Count, 1, "liệt kê được 1 bản sao lưu");

        // kỹ thuật viên chỉnh nhầm
        File.WriteAllText(kho.FileMang,
            """{"DiaChiIp":"0.0.0.0","Cong":0,"CongNoiTiep":"","TocDoBaud":0}""",
            System.Text.Encoding.UTF8);
        Kiem.Bang(kho.Nap().Mang.DiaChiIp, "0.0.0.0", "cấu hình đã bị chỉnh nhầm");

        dongHo.Tien(TimeSpan.FromMinutes(5));
        string truocKhiDe = SaoLuuCauHinh.KhoiPhuc(kho, ban1, dongHo);
        Kiem.Bang(kho.Nap().Mang.DiaChiIp, "10.1.1.1", "★ khôi phục được cấu hình đúng");
        Kiem.Dung(Directory.Exists(truocKhiDe),
                  "★ khôi phục TỰ SAO LƯU bản hiện tại trước khi đè — khôi phục nhầm bản vẫn cứu được");
        Kiem.Bang(SaoLuuCauHinh.LietKe(kho).Count, 2, "nay có 2 bản sao lưu");
        Kiem.Nem<DirectoryNotFoundException>(
            () => SaoLuuCauHinh.KhoiPhuc(kho, Path.Combine(goc, "khong-co"), dongHo),
            "khôi phục từ thư mục không tồn tại → báo lỗi rõ ràng");
    }
}
