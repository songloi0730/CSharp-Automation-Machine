// -------------------------------------------------------
// File:    KiemMayCongNghiep.cs
// Project: MeoBench
// Purpose: Mục G.16 — kiểm cỗ máy G.11 SAU KHI ráp yêu cầu công nghiệp. Từng
//          lớp của Phụ lục L đã có phép kiểm riêng ở nhóm L; ở đây kiểm chỗ
//          chúng GẶP cỗ máy: đúng lúc, đúng thứ tự, không làm hỏng thứ đã đúng.
// -------------------------------------------------------
using System.IO.Compression;

namespace MeoBench;

public static class KiemMayCongNghiep
{
    public static async Task Chay()
    {
        string goc = Path.Combine(Path.GetTempPath(), "MeoBench_G16_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(goc);
        try { await ChayThat(goc).ConfigureAwait(false); }
        finally
        {
            try { Directory.Delete(goc, recursive: true); }
#pragma warning disable CA1031 // dọn thư mục tạm hỏng thì bỏ qua
            catch (Exception) { }
#pragma warning restore CA1031
        }
    }

    /// <summary>Ổ đĩa giả mà phép kiểm đổi được dung lượng giữa chừng.</summary>
    private sealed class ODiaDoiDuoc : IThongTinODia
    {
        public Func<long> Ham { get; set; } = () => 1L << 40;     // 1 TB
        public long ConTrong(string duongDan) => Ham();
    }

    private static (MayCongNghiep may, AnToanGiaLap at, DongHoGia dh, ODiaDoiDuoc od) Tao(
        string thuMuc, Func<double>? docApSuat = null, Action<string>? ghiTep = null)
    {
        var dh = new DongHoGia(new DateTime(2026, 10, 22, 6, 0, 0));
        var at = new AnToanGiaLap();
        var od = new ODiaDoiDuoc();
        var ch = new CauHinhHoanChinh
        {
            May = new CauHinhMay { HatGiongGiaLap = 2026 },
            ThuMucDuLieu = thuMuc,
            MaCa = "CA-A-20261022",
        };
        var cn = new CauHinhCongNghiep { ThoiGianChoPhoiNg = TimeSpan.FromMilliseconds(300) };
        var may = new MayCongNghiep(ch, cn, dh, new DongHoDonDieuHeThong(), at,
                                    new CongThuc { Ten = "SanPhamA" }, docApSuat ?? (() => 6.0), od, ghiTep);
        may.MauChuan.KhaiBao(new MauChuan("MAU-DAT",  true,  null));
        may.MauChuan.KhaiBao(new MauChuan("MAU-DAY",  false, nameof(KetLuanDo.TrenNguong)));
        may.MauChuan.KhaiBao(new MauChuan("MAU-MONG", false, nameof(KetLuanDo.DuoiNguong)));
        return (may, at, dh, od);
    }

    /// <summary>Người vận hành đặt lần lượt ba mẫu lên máy và chạy.</summary>
    private static async Task<List<KetQuaChayMau>> ChayBaMau(MayCongNghiep may)
    {
        var ds = new List<KetQuaChayMau>();
        foreach (var (ma, day) in new[] { ("MAU-DAT", 2.000), ("MAU-DAY", 2.120), ("MAU-MONG", 1.880) })
        {
            may.May.CamBien.DatPhoiTiepTheo(day);
            ds.Add(await may.ChayMauChuanAsync(ma));
        }
        return ds;
    }

    /// <summary>Đăng nhập, về gốc, qua đủ mẫu chuẩn.</summary>
    private static async Task SanSangSanXuat(MayCongNghiep may)
    {
        may.DangNhap.DangNhap("An", MucNguoiDung.VanHanh);
        await may.KhoiTaoAsync();
        await ChayBaMau(may);
    }

    private static string MaCua(IReadOnlyList<LyDoChuaChay> ds) => string.Join(",", ds.Select(l => l.Ma));

    /// <summary>Số dòng của tệp; tệp chưa có thì 0 — phép kiểm hỏng không được làm dừng cả lượt kiểm.</summary>
    private static int SoDong(string tep) => File.Exists(tep) ? File.ReadAllLines(tep).Length : 0;

    private static bool NhatKyCo(MayCongNghiep may, string nguon, string doan)
        => may.May.CuaRaBoNho.TatCa.Any(b => b.Nguon == nguon && b.DungCau().Contains(doan, StringComparison.Ordinal));

    private static async Task ChayThat(string goc)
    {
        // ---------- G.16.1 ----------
        Kiem.MoBai("G.16.1", "Cờ phiên — mở lúc khởi động, đóng ở bước cuối");
        string d1 = Path.Combine(goc, "phien");
        string co = Path.Combine(d1, "phien_chay.co");
        var (may, _, _, _) = Tao(d1);
        using (may)
        {
            Kiem.Dung(!may.KhoiDong().LanTruocTatBatThuong, "lần đầu: không báo tắt bất thường");
            Kiem.Dung(File.Exists(co), "đang chạy thì cờ phiên có mặt");
            await SanSangSanXuat(may);
            Kiem.Bang((await may.BatDauAsync(5)).Count, 0, "đủ điều kiện → chạy");
            Kiem.Bang(may.May.SoPhoi, 5, "5 phôi");
            may.TatMay();
            Kiem.Dung(!File.Exists(co), "★ tắt đúng trình tự → cờ phiên đã xoá");
            var tep = Directory.GetFiles(may.ThuMucNhatKy, "*.log");
            Kiem.Bang(tep.Length, 1, "nhật ký ra tệp: một tệp cho một ngày");
            Kiem.Dung(tep.Length == 1 && File.ReadAllLines(tep[0])[^1].Contains("Đóng phiên", StringComparison.Ordinal),
                      "★ dòng CUỐI của tệp nhật ký là đóng phiên — không gì ghi sau nó");
        }
        var (may1b, _, _, _) = Tao(d1);
        using (may1b)
        {
            Kiem.Dung(!may1b.KhoiDong().LanTruocTatBatThuong, "mở lại sau một lần tắt đúng: không báo bất thường");
            may1b.TatMay();
        }

        // Tắt DỞ DANG — một bước giữa trình tự tắt ném lỗi — thì cờ phải còn nguyên
        bool hongKhiTat = false;
        var (may1c, _, _, _) = Tao(d1, ghiTep: _ =>
        {
            if (hongKhiTat) throw new IOException("ổ nhật ký hỏng giữa lúc tắt");
        });
        using (may1c)
        {
            may1c.KhoiDong();
            hongKhiTat = true;
            Kiem.Nem<IOException>(may1c.TatMay, "một bước của trình tự tắt ném lỗi");
            Kiem.Dung(File.Exists(co), "★ tắt dở dang → cờ phiên CÒN: lần sau vẫn biết là bất thường");
        }
        var (may1d, _, _, _) = Tao(d1);
        using (may1d)
        {
            Kiem.Dung(may1d.KhoiDong().LanTruocTatBatThuong, "lần khởi động sau báo tắt bất thường");
            may1d.DangNhap.DangNhap("An", MucNguoiDung.VanHanh);
            may1d.XacNhanDaKiemSauSuCo();
            may1d.TatMay();
        }

        // ---------- G.16.2 ----------
        Kiem.MoBai("G.16.2", "Bộ điều kiện chạy — đủ mọi lý do trong một lần");
        double apSuat = 4.2;
        var (may2, at2, _, _) = Tao(Path.Combine(goc, "dieukien"), () => apSuat);
        using (may2)
        {
            may2.KhoiDong();
            at2.DungKhanDangNhan = true;
            var chua = await may2.BatDauAsync(5);
            Kiem.Bang(MaCua(chua), "AT-01,TB-01,TB-02,DL-01,ND-01",
                      "★ NĂM lý do trong MỘT lần bấm, nhóm an toàn đứng đầu");
            Kiem.Dung(chua.All(l => l.CachKhacPhuc.Length > 0), "mỗi lý do kèm cách khắc phục");
            Kiem.Bang(may2.May.SoPhoi, 0, "không phôi nào chạy");

            at2.DungKhanDangNhan = false;
            apSuat = 6.0;
            may2.DangNhap.DangNhap("An", MucNguoiDung.VanHanh);
            await may2.KhoiTaoAsync();
            Kiem.Bang(MaCua(await may2.BatDauAsync(5)), "DL-01", "sửa bốn cái → còn đúng một: chưa qua mẫu chuẩn");
            await ChayBaMau(may2);
            Kiem.Bang((await may2.BatDauAsync(5)).Count, 0, "qua mẫu → chạy");
            Kiem.Bang(may2.May.SoPhoi, 5, "5 phôi");
            may2.TatMay();
        }

        // ---------- G.16.3 ----------
        Kiem.MoBai("G.16.3", "Mẫu chuẩn — đi đủ chu kỳ nhưng không bị đếm");
        var (may3, _, dh3, _) = Tao(Path.Combine(goc, "mauchuan"));
        using (may3)
        {
            may3.KhoiDong();
            may3.DangNhap.DangNhap("An", MucNguoiDung.VanHanh);
            await may3.KhoiTaoAsync();
            var kq = await ChayBaMau(may3);
            Kiem.Dung(kq.All(k => k.KhopMongDoi), "ba mẫu đều đúng — hai mẫu lỗi bị chặn ĐÚNG mã lỗi");
            Kiem.Bang(may3.May.SoPhoi, 0, "★ mẫu chuẩn KHÔNG được đếm vào sản lượng");
            Kiem.Dung(!File.Exists(may3.May.SoGhi.DuongDanHomNay), "★ mẫu chuẩn không vào sổ kết quả");
            Kiem.Bang(may3.MangNg.SoPhoiDaRoi, 2, "hai mẫu lỗi rơi vào máng NG — người vận hành lấy lại ở đó");
            Kiem.Dung(may3.MauChuan.ChoPhepSanXuat().ChoPhep, "qua đủ ba mẫu → được sản xuất");

            may3.MauChuan.YeuCauKiemLai(LyDoKiemMau.DoiMaHang);
            may3.May.CamBien.DatPhoiTiepTheo(2.000);          // đặt nhầm tấm dày chuẩn vào chỗ mẫu lỗi
            var lot = await may3.ChayMauChuanAsync("MAU-DAY");
            Kiem.Dung(!lot.KhopMongDoi && lot.GhiChu.Contains("LỌT", StringComparison.Ordinal),
                      "★ máy để lọt mẫu lỗi → bị bắt và nói rõ");
            Kiem.Dung((await may3.BatDauAsync(3)).Any(l => l.Ma == "DL-01"), "→ không được sản xuất");

            await ChayBaMau(may3);
            Kiem.Bang((await may3.BatDauAsync(3)).Count, 0, "chạy lại đủ ba mẫu → được sản xuất");
            dh3.Tien(TimeSpan.FromHours(13));
            Kiem.Bang(MaCua(await may3.BatDauAsync(3)), "DL-01,ND-01",
                      "★ 13 giờ sau: kết quả mẫu hết hiệu lực, người vận hành cũng đã bị đăng xuất");
            may3.TatMay();
        }

        // ---------- G.16.4 ----------
        Kiem.MoBai("G.16.4", "Phôi NG phải thật sự rời máy — bước chèn vào chu trình");
        var (may4, _, _, _) = Tao(Path.Combine(goc, "phoing"));
        using (may4)
        {
            may4.KhoiDong();
            await SanSangSanXuat(may4);
            int roiTruoc = may4.MangNg.SoPhoiDaRoi;
            Kiem.Bang((await may4.BatDauAsync(20)).Count, 0, "chạy 20 phôi");
            Kiem.Bang(may4.May.SoPhoi, 20, "đủ 20 phôi");
            Kiem.Dung(may4.May.BoDem.HienTai.Ng > 0, "có phôi NG");
            Kiem.Bang(may4.MangNg.SoPhoiDaRoi - roiTruoc, may4.May.BoDem.HienTai.Ng,
                      "★ MỌI phôi NG đều được cảm biến máng thấy rơi");
            Kiem.Bang(may4.May.ManHinh.MoTaTienDo, "Bước 8/8: Về vị trí chờ",
                      "★ bước chèn hiện trên màn hình: 8 bước, vòng chạy không sửa một dòng");

            may4.MangNg.MoPhongPhoiKet = true;
            may4.May.CamBien.DatPhoiTiepTheo(2.120);         // phôi kế tiếp chắc chắn NG
            int soTruoc = may4.May.SoPhoi;
            await may4.BatDauAsync(20);
            Kiem.Bang(may4.May.TrangThai, TrangThaiMay.BaoDong, "★ phôi NG không rơi → Báo động, KHÔNG chạy tiếp");
            Kiem.Dung(may4.May.BangCanhBao.DangHoatDong.Any(a => a.Ma == MaCanhBao.PhoiNgKhongRoiMay),
                      "cảnh báo mã 30002");
            Kiem.Bang(may4.May.SoPhoi, soTruoc, "★ phôi kẹt không được đếm — chu kỳ của nó chưa xong");
            Kiem.Bang(may4.BoNG.TrangThai, TrangThaiBoNG.QuaHan, "trạng thái chờ: quá hạn");

            // ---------- G.16.5 ----------
            Kiem.MoBai("G.16.5", "Đèn tháp và còi — tắt còi không phải xác nhận");
            var tn = may4.DenVaCoi();
            Kiem.Dung(tn is { Den: MauDen.Do, NhapNhay: true, Coi: KieuCoi.NgatQuang },
                      "báo động chưa xác nhận → đỏ nháy, còi ngắt quãng");
            may4.TatCoi();
            tn = may4.DenVaCoi();
            Kiem.Dung(tn is { Den: MauDen.Do, NhapNhay: true, Coi: KieuCoi.Tat },
                      "★ tắt còi: còi im nhưng đèn VẪN đỏ nháy — cảnh báo chưa được xác nhận");
            may4.May.BangCanhBao.Phat(new AlarmException(MaCanhBao.CamBienKhongPhanHoi, "CB_DAY", "giả lập"));
            Kiem.Bang(may4.DenVaCoi().Coi, KieuCoi.NgatQuang, "★ cảnh báo MỚI sau khi tắt còi → còi kêu lại");
            may4.XacNhanCanhBao(MaCanhBao.CamBienKhongPhanHoi);
            may4.XacNhanCanhBao(MaCanhBao.PhoiNgKhongRoiMay);
            tn = may4.DenVaCoi();
            Kiem.Dung(tn is { Den: MauDen.Do, NhapNhay: false, Coi: KieuCoi.Tat },
                      "xác nhận hết → đỏ đứng yên: vẫn báo động, chờ Reset");

            // ---------- G.16.6 ----------
            Kiem.MoBai("G.16.6", "Xác nhận có tên — xác nhận cảnh báo chưa đủ");
            may4.May.Reset();
            may4.MangNg.MoPhongPhoiKet = false;
            await may4.KhoiTaoAsync();
            Kiem.Bang(may4.DenVaCoi(), new TinHieuThap(MauDen.Vang, false, KieuCoi.Tat), "về gốc xong → vàng, im lặng");
            Kiem.Bang(MaCua(await may4.BatDauAsync(5)), "TB-03",
                      "★ đã xác nhận cảnh báo, đã về gốc, vẫn chưa chạy: chưa ai nói đã lấy phôi ra");
            may4.DangNhap.DangXuat();
            Kiem.Dung(!may4.XacNhanPhoiNgBangTay().ChoPhep, "chưa đăng nhập → không xác nhận được");
            may4.DangNhap.DangNhap("Binh", MucNguoiDung.VanHanh);
            Kiem.Dung(may4.XacNhanPhoiNgBangTay().ChoPhep, "đăng nhập rồi → xác nhận được");
            Kiem.Bang(may4.BoNG.NguoiXacNhanTay, "Binh", "★ tên người xác nhận được giữ");
            Kiem.Dung(NhatKyCo(may4, "MANG_NG", "Binh xác nhận tay"), "★ và được ghi nhật ký");
            Kiem.Bang((await may4.BatDauAsync(5)).Count, 0, "chạy lại được");
            may4.TatMay();
        }

        // ---------- G.16.7 ----------
        Kiem.MoBai("G.16.7", "Ổ cạn giữa ca — dừng ở ranh giới chu kỳ");
        var (may7, _, _, od7) = Tao(Path.Combine(goc, "odia"));
        using (may7)
        {
            may7.KhoiDong();
            await SanSangSanXuat(may7);
            od7.Ham = () => may7.May.SoPhoi >= 4 ? 100L * 1024 * 1024 : 1L << 40;
            Kiem.Bang((await may7.BatDauAsync(10)).Count, 0, "lúc bấm Bắt đầu ổ còn đủ");
            Kiem.Bang(may7.May.SoPhoi, 4, "★ ổ cạn sau phôi thứ 4 → dừng sau ĐÚNG phôi thứ 4");
            Kiem.Bang(may7.May.TrangThai, TrangThaiMay.SanSang, "★ dừng có trật tự, không phải Báo động");
            Kiem.Bang(SoDong(may7.May.SoGhi.DuongDanHomNay), 5,
                      "sổ kết quả: tiêu đề + 4 dòng, không có phôi dở");
            Kiem.Dung((await may7.BatDauAsync(10)).Any(l => l.Ma == "DL-02"), "nút Bắt đầu nói rõ lý do: ổ dữ liệu");
            may7.TatMay();
        }

        // ---------- G.16.8 ----------
        Kiem.MoBai("G.16.8", "Ghi nhật ký gặp ổ đầy — máy không sập");
        MayCongNghiep? m8 = null;
        bool daDon = false;
        var (may8, _, _, _) = Tao(Path.Combine(goc, "nhatky"), ghiTep: _ =>
        {
            if (m8?.May.SoPhoi >= 3 && !daDon)
                throw new IOException("There is not enough space on the disk.", GhiKhongSap.HResultODiaDay);
        });
        m8 = may8;
        using (may8)
        {
            may8.KhoiDong();
            await SanSangSanXuat(may8);
            Kiem.Bang((await may8.BatDauAsync(10)).Count, 0, "chạy");
            Kiem.Bang(may8.May.SoPhoi, 3, "★ ghi tệp hỏng sau phôi 3 → máy dừng ở ranh giới chu kỳ, KHÔNG ném");
            Kiem.Bang(may8.May.TrangThai, TrangThaiMay.SanSang, "về Sẵn sàng");
            Kiem.Dung(may8.SoDongNhatKyBoMat > 0, $"đếm được số dòng mất ({may8.SoDongNhatKyBoMat})");
            Kiem.Dung(NhatKyCo(may8, "MAY", "nhật ký đã ngừng ghi"),
                      "★ nhật ký trong bộ nhớ và trên màn hình vẫn ghi đủ — chỉ cửa ra tệp ngừng");
            Kiem.Dung((await may8.BatDauAsync(3)).Any(l => l.Ma == "DL-02"), "nút Bắt đầu nói rõ lý do");
            daDon = true;                                   // người phụ trách đã giải phóng ổ
            may8.GhiNhatKyLai();
            Kiem.Dung(NhatKyCo(may8, "ODIA", "đã mất"), "ghi lại, kèm số dòng đã mất");
            Kiem.Bang((await may8.BatDauAsync(3)).Count, 0, "chạy lại được");
            may8.TatMay();
        }

        // ---------- G.16.9 ----------
        Kiem.MoBai("G.16.9", "Tắt bất thường — lần khởi động sau phải biết");
        string d9 = Path.Combine(goc, "tatbatthuong");
        var (may9a, _, _, _) = Tao(d9);
        may9a.KhoiDong();
        await SanSangSanXuat(may9a);
        await may9a.BatDauAsync(3);
        may9a.Dispose();                                     // KHÔNG gọi TatMay — giả lập chết, mất điện
        var (may9, _, _, _) = Tao(d9);
        using (may9)
        {
            Kiem.Dung(may9.KhoiDong().LanTruocTatBatThuong, "★ cờ phiên còn nguyên → biết lần trước tắt bất thường");
            Kiem.Bang(may9.May.SoPhoi, 3, "sản lượng ca không mất (G.11.7)");
            Kiem.Bang(MaCua(await may9.BatDauAsync(2)), "TB-01,DL-01,ND-01,ND-02",
                      "phải về gốc, chạy lại mẫu, đăng nhập — VÀ có người xác nhận đã kiểm máy");
            Kiem.Dung(!may9.XacNhanDaKiemSauSuCo().ChoPhep, "chưa đăng nhập → không xác nhận được");
            may9.DangNhap.DangNhap("An", MucNguoiDung.VanHanh);
            Kiem.Dung(!(await may9.KhoiTaoAsync()).ChoPhep,
                      "★ chưa ai xác nhận đã kiểm máy thì trục CHƯA được chạy — về gốc bị từ chối");
            Kiem.Bang(may9.May.TrangThai, TrangThaiMay.ChuaKhoiTao, "trục đứng yên");
            Kiem.Dung(may9.XacNhanDaKiemSauSuCo().ChoPhep, "xác nhận đã kiểm máy");
            Kiem.Dung(NhatKyCo(may9, "PHIEN", "An xác nhận"), "★ ghi tên người xác nhận");
            await may9.KhoiTaoAsync();
            await ChayBaMau(may9);
            Kiem.Bang((await may9.BatDauAsync(2)).Count, 0, "chạy được");
            Kiem.Bang(may9.May.SoPhoi, 5, "cộng dồn đúng: 3 + 2");
            may9.TatMay();
        }

        // ---------- G.16.10 ----------
        Kiem.MoBai("G.16.10", "Giấy phép thư viện hãng và gói chẩn đoán");
        string d10 = Path.Combine(goc, "giayphep");
        var (may10, _, dh10, _) = Tao(d10);
        using (may10)
        {
            may10.GiayPhep = new TrangThaiGiayPhep("thư viện đo", CoMat: true, HetHan: dh10.BayGio.AddDays(10));
            may10.KhoiDong();
            Kiem.Dung(NhatKyCo(may10, "GIAYPHEP", "còn 10 ngày"), "★ sắp hết hạn → báo trước ngay lúc khởi động");
            await SanSangSanXuat(may10);
            Kiem.Bang((await may10.BatDauAsync(3)).Count, 0, "sắp hết hạn vẫn chạy — báo trước, không chặn");
            may10.GiayPhep = may10.GiayPhep with { HetHan = dh10.BayGio.AddDays(-1) };
            Kiem.Dung((await may10.BatDauAsync(3)).Any(l => l.Ma == "TB-04"),
                      "★ hết hạn → chặn ở nút Bắt đầu, không dừng giữa chu kỳ");

            string zip = Path.Combine(d10, "chandoan.zip");
            var goi = may10.TaoGoiChanDoan(zip);
            using (var z = ZipFile.OpenRead(zip))
            {
                Kiem.Dung(z.Entries.Any(e => e.FullName.StartsWith("nhat_ky/NhatKy_", StringComparison.Ordinal)),
                          "gói có tệp nhật ký");
                var gt = z.GetEntry("GIOI_THIEU.txt");
                using var r = gt is null ? StreamReader.Null : new StreamReader(gt.Open());
                Kiem.Dung(r.ReadToEnd().Contains("Phiên bản: 1.0.0", StringComparison.Ordinal),
                          "trang giới thiệu ghi phiên bản");
            }
            Kiem.Bang(goi.BoQua.Count, 0, "không bỏ sót tệp nào");
            may10.TatMay();
        }
    }
}
