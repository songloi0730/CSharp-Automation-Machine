// -------------------------------------------------------
// File:    KiemYeuCauCongNghiep.cs
// Project: MeoBench
// Purpose: Phép kiểm cho Phụ lục L (yêu cầu công nghiệp). Phép kiểm 19.5 chạy
//          THẬT: MeoBench tự khởi chạy chính nó làm tiến trình con, cho nó chết
//          vì ngoại lệ chưa bắt, rồi để bộ giám sát mở lại.
// -------------------------------------------------------
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MeoBench;

/// <summary>Ví dụ lớp kiểm có gắn mã yêu cầu — dùng cho phép kiểm ma trận truy vết (18.7).</summary>
public static class ViDuKiemTheoYeuCau
{
    /// <summary>Kiểm yêu cầu YC-01.</summary>
    [YeuCau("YC-01")]
    public static void DungKhanCapNgatMoiChuyenDong() { }

    /// <summary>Kiểm hai yêu cầu cùng lúc.</summary>
    [YeuCau("YC-02")]
    [YeuCau("YC-03")]
    public static void MoCuaKhiChayThiTamDung() { }

    /// <summary>Phép kiểm trỏ tới yêu cầu đã bị bỏ khỏi đặc tả.</summary>
    [YeuCau("YC-99")]
    public static void KiemYeuCauDaBo() { }
}

/// <summary>Phép kiểm cho các yêu cầu công nghiệp của Phụ lục L.</summary>
public static class KiemYeuCauCongNghiep
{
    private const long GB = 1024L * 1024 * 1024;

    /// <summary>Chạy toàn bộ.</summary>
    public static async Task Chay()
    {
        KiemPhienChay();
        KiemChinhSachMoLai();
        await KiemGiamSatThat();
        KiemDongHo();
        KiemODia();
        KiemTaiNguyen();
        KiemDieuKienChay();
        KiemMauChuan();
        KiemKiemLai();
        KiemBoNG();
        KiemTruyVet();
        KiemYeuCauCongNghiep2.Chay();
    }

    private static string ThuMucTam(string ten)
    {
        var d = Path.Combine(Path.GetTempPath(), $"MeoBench_{ten}_{Guid.NewGuid():N}");
        Directory.CreateDirectory(d);
        return d;
    }

    private static void Don(string thuMuc)
    {
        try { Directory.Delete(thuMuc, recursive: true); }
#pragma warning disable CA1031 // dọn thư mục tạm hỏng thì bỏ qua, không ảnh hưởng kết quả kiểm
        catch (Exception) { }
#pragma warning restore CA1031
    }

    // ── 19.5.3 ─────────────────────────────────────────────────────────
    private static void KiemPhienChay()
    {
        Kiem.MoBai("19.5.3", "Cờ phiên chạy — biết lần trước tắt không đúng cách");
        var thuMuc = ThuMucTam("Phien");
        var co = Path.Combine(thuMuc, "phien.co");
        var dh = new DongHoGia(new DateTime(2026, 10, 21, 6, 0, 0));

        var p1 = new PhienChay(co, dh).Mo(1001, "2.3.0");
        Kiem.Dung(!p1.LanTruocTatBatThuong, "lần đầu: không có cờ cũ → bình thường");
        new PhienChay(co, dh).DongBinhThuong();
        Kiem.Dung(!new PhienChay(co, dh).Mo(1002, "2.3.0").LanTruocTatBatThuong, "tắt đúng cách rồi mở lại: bình thường");

        // Lần 1002 KHÔNG gọi DongBinhThuong — giống chết giữa chừng
        var p3 = new PhienChay(co, dh).Mo(1003, "2.3.1");
        Kiem.Dung(p3.LanTruocTatBatThuong, "lần trước không đóng phiên → báo tắt bất thường");
        Kiem.Bang(p3.PhienTruoc?.MaTienTrinh, 1002, "biết tiến trình nào đã chết");
        Kiem.Bang(p3.PhienTruoc?.PhienBan, "2.3.0", "biết phiên bản nào đã chết");

        File.WriteAllText(co, "hỏng giữa lúc ghi");
        var p4 = new PhienChay(co, dh).Mo(1004, "2.3.1");
        Kiem.Dung(p4.LanTruocTatBatThuong && p4.PhienTruoc is null, "cờ hỏng (mất điện lúc ghi) vẫn tính là tắt bất thường");
        Don(thuMuc);
    }

    // ── 19.5.4 ─────────────────────────────────────────────────────────
    private static void KiemChinhSachMoLai()
    {
        Kiem.MoBai("19.5.4", "Chính sách tự mở lại có giới hạn");
        var dh = new DongHoGia(new DateTime(2026, 10, 21, 2, 0, 0));
        var cs = new ChinhSachMoLai(dh);

        Kiem.Dung(!cs.SauKhiThoat(0, conCoPhien: false).MoLai, "thoát bình thường (mã 0, không còn cờ): không mở lại");
        var q1 = cs.SauKhiThoat(0, conCoPhien: true);
        Kiem.Dung(q1.MoLai && q1.LyDo.Contains("cờ phiên", StringComparison.Ordinal),
                  "mã thoát 0 nhưng còn cờ phiên: vẫn là chết — mở lại");
        var q2 = cs.SauKhiThoat(ChinhSachMoLai.MaThoatNgoaiLeChuaBat, conCoPhien: true);
        Kiem.Bang(q2.ChoTruoc, TimeSpan.FromSeconds(10), "lần chết thứ hai trong cửa sổ: chờ gấp đôi (10 giây)");
        Kiem.Dung(q2.LyDo.Contains("ngoại lệ chưa bắt", StringComparison.Ordinal), "nhận ra mã 0xE0434352");
        Kiem.Dung(cs.SauKhiThoat(-1, true).MoLai, "lần thứ ba: vẫn mở lại");
        var q4 = cs.SauKhiThoat(-1, true);
        Kiem.Dung(!q4.MoLai && q4.LyDo.Contains("cần người", StringComparison.Ordinal),
                  "lần thứ tư trong 10 phút: thôi mở lại, gọi người");

        var cs2 = new ChinhSachMoLai(dh);
        for (int i = 0; i < 5; i++) { cs2.SauKhiThoat(-1, true); dh.Tien(TimeSpan.FromMinutes(11)); }
        Kiem.Dung(cs2.SauKhiThoat(-1, true).MoLai, "chết thưa (mỗi 11 phút): cửa sổ trượt, vẫn mở lại");
    }

    private static ProcessStartInfo LenhChayChinhMinh(params string[] thamSo)
    {
        var exe = Environment.ProcessPath!;
        var psi = new ProcessStartInfo
        {
            FileName = exe,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };
        if (Path.GetFileNameWithoutExtension(exe).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            psi.ArgumentList.Add(Assembly.GetEntryAssembly()!.Location);
        foreach (var t in thamSo) psi.ArgumentList.Add(t);
        return psi;
    }

    private static async Task KiemGiamSatThat()
    {
        Kiem.MoBai("19.5", "Chết thật, mở lại thật — tiến trình con là chính MeoBench");
        var thuMuc = ThuMucTam("GiamSat");
        var co = Path.Combine(thuMuc, "phien.co");
        var dem = Path.Combine(thuMuc, "dem.txt");
        var thuMucDump = Path.Combine(thuMuc, "dump");
        Directory.CreateDirectory(thuMucDump);

        var dh = new DongHoGia(new DateTime(2026, 10, 21, 2, 0, 0));
        var nhatKy = new NhatKy(dh);
        var log = new CuaRaBoNho();
        nhatKy.ThemCuaRa(log);
        var gs = new GiamSatTienTrinh(new ChinhSachMoLai(dh), co, nhatKy);
        var choDaXin = new List<TimeSpan>();

        int soLan = await gs.ChayAsync(() =>
        {
            var psi = LenhChayChinhMinh("--chet-lan-dau", co, dem);
            psi.Environment["DOTNET_DbgEnableMiniDump"] = "1";
            psi.Environment["DOTNET_DbgMiniDumpType"] = "1";
            psi.Environment["DOTNET_DbgMiniDumpName"] = Path.Combine(thuMucDump, "may_%p.dmp");
            return psi;
        }, (t, _) => { choDaXin.Add(t); return Task.CompletedTask; });

        Kiem.Bang(soLan, 2, "chết lần đầu → mở lại → lần hai tắt đúng cách → bộ giám sát dừng");
        Kiem.Dung(log.TatCa.Any(b => b.ThuocTinh.TryGetValue("LyDo", out var v)
                                     && v is string s && s.Contains("ngoại lệ chưa bắt", StringComparison.Ordinal)),
                  "bộ giám sát đọc được mã thoát 0xE0434352 của lần chết");
        Kiem.Bang(File.ReadAllText(dem + ".lan2"), "BatThuong", "lần chạy thứ hai biết lần trước tắt bất thường");
        Kiem.Dung(!File.Exists(co), "lần hai tắt đúng cách: cờ phiên đã xoá");
        Kiem.Bang(Directory.GetFiles(thuMucDump, "*.dmp").Length, 1,
                  "có đúng một tệp dump của lần chết — chỉ nhờ biến môi trường, không đụng cấu hình Windows");
        Kiem.Bang(choDaXin.Count == 1 ? choDaXin[0] : TimeSpan.Zero, TimeSpan.FromSeconds(5), "chờ 5 giây trước khi mở lại");
        Don(thuMuc);
    }

    // ── 19.6.1 ─────────────────────────────────────────────────────────
    private static void KiemDongHo()
    {
        Kiem.MoBai("19.6.1", "Hai loại đồng hồ — đo khoảng thời gian không bằng giờ tường");
        var gio = new DongHoGia(new DateTime(2026, 10, 21, 1, 59, 57));
        var don = new DongHoDonDieuGia();

        var dung = new HanGio(don, TimeSpan.FromSeconds(10));
        var sai = new HanGioTheoGioTuong(gio, TimeSpan.FromSeconds(10));
        gio.Tien(TimeSpan.FromSeconds(3)); don.Tien(TimeSpan.FromSeconds(3));
        gio.Tien(TimeSpan.FromHours(-1));                    // đồng bộ giờ kéo đồng hồ lùi một tiếng
        gio.Tien(TimeSpan.FromSeconds(8)); don.Tien(TimeSpan.FromSeconds(8));
        Kiem.Dung(dung.DaHet, "11 giây thật đã trôi: hạn giờ đơn điệu đã hết");
        Kiem.Dung(!sai.DaHet, "đo bằng giờ tường sau khi giờ lùi: CHƯA hết — chờ thêm gần một tiếng");

        var dung2 = new HanGio(don, TimeSpan.FromSeconds(10));
        var sai2 = new HanGioTheoGioTuong(gio, TimeSpan.FromSeconds(10));
        gio.Tien(TimeSpan.FromSeconds(1)); don.Tien(TimeSpan.FromSeconds(1));
        gio.Tien(TimeSpan.FromHours(1));                     // giờ nhảy tới (người chỉnh tay)
        Kiem.Dung(!dung2.DaHet, "1 giây thật: hạn giờ đơn điệu chưa hết");
        Kiem.Dung(sai2.DaHet, "đo bằng giờ tường sau khi giờ nhảy tới: báo QUÁ GIỜ sau 1 giây — cảnh báo giả");

        var that = new DongHoDonDieuHeThong();
        long a = that.MiliGiay;
        Thread.Sleep(30);
        Kiem.Dung(that.MiliGiay - a >= 15, "TickCount64 thật đi tới");
    }

    // ── 19.6.3 ─────────────────────────────────────────────────────────
    private sealed class ODiaGia(long conTrong) : IThongTinODia
    {
        public long ConTrong(string duongDan) => conTrong;
    }

    private static void KiemODia()
    {
        Kiem.MoBai("19.6.3", "Ổ đĩa đầy — ngưỡng theo số ngày, và nhật ký không làm sập máy");
        CanhODia Canh(long con) => new(new ODiaGia(con)) { ByteMoiNgay = 2 * GB, SoNgayCanhBao = 7, SoNgayNgung = 1 };
        Kiem.Bang(Canh(40 * GB).Xet("D:\\Data").Muc, MucODia.Du, "còn 20 ngày: đủ");
        Kiem.Bang(Canh(10 * GB).Xet("D:\\Data").Muc, MucODia.CanhBao, "còn 5 ngày: cảnh báo");
        var ngung = Canh(1 * GB).Xet("D:\\Data");
        Kiem.Dung(ngung.Muc == MucODia.NgungNhanPhoi && ngung.ThongBao.Contains("12 giờ", StringComparison.Ordinal),
                  "còn nửa ngày: ngừng nhận phôi, nói rõ còn bao nhiêu giờ");
        Kiem.Dung(new CanhODia(new ThongTinODiaHeThong()).Xet(Path.GetTempPath()).ConTrongByte > 0,
                  "đọc ổ đĩa thật bằng DriveInfo");

        Kiem.Bang(GhiKhongSap.HResultODiaDay, unchecked((int)0x80070000) | 112, "HRESULT ổ đầy = mã Win32 112 (ERROR_DISK_FULL)");
        Kiem.Dung(!string.IsNullOrWhiteSpace(Marshal.GetPInvokeErrorMessage(112)), "Windows có thông báo cho mã 112");

        int daGhi = 0, soLanBao = 0;
        var ghi = new GhiKhongSap(_ =>
        {
            if (daGhi >= 2) throw new IOException(Marshal.GetPInvokeErrorMessage(112), GhiKhongSap.HResultODiaDay);
            daGhi++;
        });
        ghi.BatDauNgung += (_, _) => soLanBao++;
        for (int i = 0; i < 5; i++) ghi.Ghi("dòng " + i);
        Kiem.Dung(ghi.DangNgung && ghi.SoDongBoMat == 3, "ổ đầy ở dòng thứ 3: ngừng ghi, đếm được 3 dòng mất, không ném");
        Kiem.Bang(soLanBao, 1, "báo đúng một lần, không phải mỗi dòng một lần");

        var khac = new GhiKhongSap(_ => throw new IOException("đang bị chương trình khác giữ", unchecked((int)0x80070020)));
        Kiem.Nem<IOException>(() => khac.Ghi("x"), "lỗi IO KHÁC ổ đầy vẫn được ném ra — chỉ nuốt đúng loại đã biết");
    }

    // ── 19.6.4 ─────────────────────────────────────────────────────────
    private static void KiemTaiNguyen()
    {
        Kiem.MoBai("19.6.4", "Tự ghi chỉ số tài nguyên, và nhận ra rò rỉ");
        var c = TaiNguyen.Doc();
        Kiem.Dung(c.BoNhoRiengByte > 0 && c.SoHandle > 0 && c.SoLuong > 0, "đọc được bộ nhớ riêng, số handle, số luồng của chính mình");
        Kiem.Dung(c.SoGdi < TaiNguyen.HanMucMacDinh && c.SoUser < TaiNguyen.HanMucMacDinh, "GDI/USER còn xa hạn mức 10.000");

        double[] roRi = [100, 103, 105, 109, 110, 114, 117, 121, 124, 128];
        double[] rangCua = [100, 130, 160, 105, 135, 165, 102, 133, 161, 104];
        double[] phang = [100, 101, 99, 100, 102, 100, 99, 101, 100, 100];
        Kiem.Dung(XuHuong.NghiRoRi(roRi), "leo đều 28 %: nghi rò rỉ");
        Kiem.Dung(!XuHuong.NghiRoRi(rangCua), "răng cưa của bộ gom rác: không nghi");
        Kiem.Dung(!XuHuong.NghiRoRi(phang), "đi ngang: không nghi");
    }

    // ── 15.2.3b ────────────────────────────────────────────────────────
    private static void KiemDieuKienChay()
    {
        Kiem.MoBai("15.2.3b", "Điều kiện chạy — đủ mọi lý do, trong một lần");
        bool cuaDong = false, apDu = true, daVeGoc = false, daDangNhap = false, cameraLoi = true;
        var bo = new BoDieuKienChay();
        bo.Them("NGUOI_DUNG", NhomDieuKien.NguoiDung, "Chưa đăng nhập", "Đăng nhập mức Vận hành", () => daDangNhap);
        bo.Them("CUA_BAO_VE", NhomDieuKien.AnToan, "Cửa bảo vệ đang mở", "Đóng cửa bảo vệ", () => cuaDong);
        bo.Them("AP_KHI", NhomDieuKien.ThietBi, "Áp suất khí thấp", "Mở van khí tổng", () => apDu);
        bo.Them("VE_GOC", NhomDieuKien.ThietBi, "Trục chưa về gốc", "Bấm Về gốc", () => daVeGoc);
        bo.Them("CAMERA", NhomDieuKien.ThietBi, "Camera kiểm chưa sẵn sàng", "Kiểm cáp mạng camera",
                () => cameraLoi ? throw new TimeoutException("không trả lời sau 2 giây") : true);

        var ds = bo.XetTatCa();
        Kiem.Bang(ds.Count, 4, "bốn điều kiện chưa đạt: trả về cả bốn, không dừng ở cái đầu tiên");
        Kiem.Bang(string.Join(",", ds.Select(d => d.Ma)), "CUA_BAO_VE,VE_GOC,CAMERA,NGUOI_DUNG",
                  "thứ tự: an toàn trước, trong một nhóm giữ thứ tự khai báo");
        Kiem.Dung(ds.FirstOrDefault(d => d.Ma == "CAMERA")?.MoTa.Contains("không đọc được", StringComparison.Ordinal) == true,
                  "điều kiện đọc lỗi = chưa đạt, kèm lý do — không làm sập màn hình");
        cuaDong = daVeGoc = daDangNhap = true;
        cameraLoi = false;
        Kiem.Bang(bo.XetTatCa().Count, 0, "đủ điều kiện: danh sách rỗng = được chạy");
    }

    // ── 13.4.9 ─────────────────────────────────────────────────────────
    private static void KiemMauChuan()
    {
        Kiem.MoBai("13.4.9", "Mẫu chuẩn — máy phải chứng minh còn phân biệt được đạt và lỗi");
        var dh = new DongHoGia(new DateTime(2026, 10, 21, 6, 0, 0));
        var cong = new CongMauChuan(dh);
        cong.KhaiBao(new MauChuan("MAU-OK-01", true, null));
        cong.KhaiBao(new MauChuan("MAU-NG-XUOC", false, "XUOC"));
        cong.KhaiBao(new MauChuan("MAU-NG-THIEU", false, "THIEU_CHI_TIET"));

        Kiem.Dung(!cong.ChoPhepSanXuat().ChoPhep, "chưa kiểm mẫu: không cho sản xuất");
        cong.GhiKetQua("MAU-OK-01", true, null);
        cong.GhiKetQua("MAU-NG-XUOC", false, "XUOC");
        var kq = cong.ChoPhepSanXuat();
        Kiem.Dung(!kq.ChoPhep && kq.LyDoTuChoi!.Contains("1/3", StringComparison.Ordinal), "mới đúng 2/3 mẫu: chưa cho, nói rõ còn 1/3");
        cong.GhiKetQua("MAU-NG-THIEU", false, "THIEU_CHI_TIET");
        Kiem.Dung(cong.ChoPhepSanXuat().ChoPhep, "đủ ba mẫu đúng: cho sản xuất");

        cong.YeuCauKiemLai(LyDoKiemMau.DoiMaHang);
        Kiem.Dung(!cong.ChoPhepSanXuat().ChoPhep, "đổi mã hàng: phải kiểm lại");

        cong.GhiKetQua("MAU-OK-01", true, null);
        var lot = cong.GhiKetQua("MAU-NG-XUOC", true, null);
        Kiem.Dung(!lot.KhopMongDoi && cong.ChoPhepSanXuat().LyDoTuChoi!.Contains("LỌT", StringComparison.Ordinal),
                  "máy cho QUA mẫu lỗi: chặn sản xuất, nói rõ 'để lọt'");

        cong.GhiKetQua("MAU-OK-01", true, null);
        cong.GhiKetQua("MAU-NG-XUOC", false, "XUOC");
        var saiLyDo = cong.GhiKetQua("MAU-NG-THIEU", false, "XUOC");
        Kiem.Dung(!saiLyDo.KhopMongDoi && saiLyDo.GhiChu.Contains("sai lý do", StringComparison.Ordinal),
                  "mẫu lỗi bị chặn nhưng sai mã lỗi: KHÔNG tính là đạt");

        foreach (var (ma, dat, loi) in new[] { ("MAU-OK-01", true, (string?)null), ("MAU-NG-XUOC", false, "XUOC"),
                                               ("MAU-NG-THIEU", false, "THIEU_CHI_TIET") })
            cong.GhiKetQua(ma, dat, loi);
        Kiem.Dung(cong.ChoPhepSanXuat().ChoPhep, "chạy lại đủ ba mẫu đúng: cho sản xuất");
        dh.Tien(TimeSpan.FromHours(13));
        var het = cong.ChoPhepSanXuat();
        Kiem.Dung(!het.ChoPhep && het.LyDoTuChoi!.Contains("hết hiệu lực", StringComparison.Ordinal), "sau 13 giờ: hết hiệu lực");

        Kiem.Dung(CongMauChuan.LaMaMauChuan("MAU-OK-01") && !CongMauChuan.LaMaMauChuan("SN-000123"),
                  "nhận ra mã mẫu chuẩn để không đếm vào sản lượng");
        Kiem.Nem<ArgumentException>(() => cong.KhaiBao(new MauChuan("OK-01", true, null)), "mã mẫu không có tiền tố: từ chối");
        Kiem.Nem<ArgumentException>(() => cong.KhaiBao(new MauChuan("MAU-NG-X", false, null)), "mẫu lỗi không ghi mã lỗi: từ chối");
    }

    private static void KiemKiemLai()
    {
        Kiem.MoBai("13.4.9", "Kiểm lại có giới hạn — đạt lần đầu và đạt cuối là hai con số");
        var so = new SoKiemLai { SoLanToiDa = 2 };
        Kiem.Bang(so.Ghi("A", true).HuongXuLy, "Đạt lần đầu", "A đạt ngay");
        Kiem.Dung(so.Ghi("B", false).ChoKiemTiep, "B không đạt lần 1: được kiểm lại");
        Kiem.Dung(so.Ghi("B", true).HuongXuLy.Contains("KIỂM LẠI ĐẠT", StringComparison.Ordinal), "B đạt ở lần 2: đánh dấu kiểm lại đạt");
        so.Ghi("C", false);
        var c2 = so.Ghi("C", false);
        Kiem.Dung(!c2.ChoKiemTiep && c2.HuongXuLy.Contains("máng NG", StringComparison.Ordinal), "C hỏng cả hai lần: đưa vào máng NG");
        Kiem.Dung(!so.Ghi("C", true).Dat, "C kiểm lần 3: không được tính, dù lần này 'đạt'");
        Kiem.Dung(so.Ghi("A", false).Dat, "A đã đạt rồi: kiểm lại không ghi đè kết quả");
        Kiem.Gan(so.TiLeDatLanDau, 1.0 / 3, 1e-9, "tỉ lệ đạt lần đầu = 1/3 (chỉ A)");
        Kiem.Gan(so.TiLeDatCuoi, 2.0 / 3, 1e-9, "tỉ lệ đạt cuối = 2/3 (A và B)");
        Kiem.Bang(string.Join(",", so.LichSu("B")), "False,True", "giữ mọi lượt kiểm của B");
    }

    private static void KiemBoNG()
    {
        Kiem.MoBai("13.4.9", "Phôi NG phải thực sự rời máy");
        var dd = new DongHoDonDieuGia();
        var x = new XacNhanBoNG(dd) { ThoiGianCho = TimeSpan.FromSeconds(5) };
        Kiem.Dung(x.ChoChuKyMoi, "chưa có phôi NG: chạy bình thường");

        x.BatDauCho("SN-7");
        x.CapNhat(false);
        Kiem.Dung(!x.ChoChuKyMoi, "đang chờ phôi rơi: không cho chu kỳ mới");
        dd.Tien(TimeSpan.FromMilliseconds(400));
        Kiem.Bang(x.CapNhat(true), TrangThaiBoNG.DaXacNhan, "cảm biến máng thấy sườn lên: đã xác nhận");
        Kiem.Dung(x.ChoChuKyMoi, "xác nhận xong: cho chu kỳ mới");

        x.BatDauCho("SN-8");
        for (int i = 0; i < 6; i++) { dd.Tien(TimeSpan.FromSeconds(1)); x.CapNhat(true); }
        Kiem.Bang(x.TrangThai, TrangThaiBoNG.QuaHan, "cảm biến kẹt SÁNG sẵn: không được tính là đã thấy phôi");
        x.XacNhanTay("Binh");
        Kiem.Dung(x.ChoChuKyMoi && x.NguoiXacNhanTay == "Binh", "quá hạn: người vận hành tự kiểm và xác nhận, có tên");

        x.BatDauCho("SN-9");
        for (int i = 0; i < 6; i++) { dd.Tien(TimeSpan.FromSeconds(1)); x.CapNhat(false); }
        Kiem.Dung(x.TrangThai == TrangThaiBoNG.QuaHan && !x.ChoChuKyMoi, "phôi không rơi trong 5 giây: quá hạn, chặn chu kỳ mới");
    }

    // ── 18.7 ───────────────────────────────────────────────────────────
    private static void KiemTruyVet()
    {
        Kiem.MoBai("18.7", "Ma trận truy vết yêu cầu");
        var yeuCau = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["YC-01"] = "Dừng khẩn cấp ngắt mọi chuyển động trong 100 ms",
            ["YC-02"] = "Mở cửa bảo vệ khi đang chạy thì tạm dừng",
            ["YC-03"] = "Đóng cửa lại không tự chạy tiếp",
            ["YC-04"] = "Đổi mã hàng phải kiểm mẫu chuẩn",
        };
        var m = TruyVet.Dung(yeuCau, typeof(ViDuKiemTheoYeuCau));
        Kiem.Bang(m.Dong.Count, 4, "mỗi yêu cầu một dòng");
        Kiem.Bang(m.Dong.Single(d => d.MaYeuCau == "YC-02").PhepKiem.Count, 1, "YC-02 có một phép kiểm phủ");
        Kiem.Bang(string.Join(",", m.YeuCauChuaCoKiem), "YC-04", "phát hiện yêu cầu CHƯA có phép kiểm nào");
        Kiem.Dung(m.PhepKiemTroSai.Count == 1 && m.PhepKiemTroSai[0].Contains("YC-99", StringComparison.Ordinal),
                  "phát hiện phép kiểm trỏ tới yêu cầu không còn trong đặc tả");
        Kiem.Dung(TruyVet.XuatCsv(m).Contains("YC-04,", StringComparison.Ordinal)
               && TruyVet.XuatCsv(m).Contains("(CHƯA CÓ)", StringComparison.Ordinal),
                  "xuất CSV cho hồ sơ nghiệm thu, dòng thiếu kiểm hiện rõ");
    }
}
