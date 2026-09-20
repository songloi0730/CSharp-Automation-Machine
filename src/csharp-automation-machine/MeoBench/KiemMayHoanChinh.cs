// -------------------------------------------------------
// File:    KiemMayHoanChinh.cs
// Project: MeoBench
// Purpose: Kiểm THÍCH HỢP — không kiểm từng mảnh nữa mà kiểm CỖ MÁY GHÉP LẠI.
//          Đây là loại phép kiểm bắt được thứ mà kiểm từng mảnh bỏ sót:
//          các mảnh đúng nhưng nối sai, hoặc quên nối.
// -------------------------------------------------------
namespace MeoBench;

public static class KiemMayHoanChinh
{
    public static async Task Chay()
    {
        string goc = Path.Combine(Path.GetTempPath(), "MeoBench_G9_" + Guid.NewGuid().ToString("N")[..8]);
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

    private static (MayHoanChinh may, AnToanGiaLap at, DongHoGia dh) Tao(
        string goc, double apSuat = 6.0, int hatGiong = 2026)
    {
        var dh = new DongHoGia(new DateTime(2026, 9, 20, 8, 0, 0));
        var at = new AnToanGiaLap();
        var ch = new CauHinhHoanChinh
        {
            May = new CauHinhMay { HatGiongGiaLap = hatGiong },
            ThuMucDuLieu = goc,
            MaCa = "CA-A-20260920",
        };
        var may = new MayHoanChinh(ch, dh, at, new CongThuc { Ten = "SanPhamA" }, () => apSuat);
        return (may, at, dh);
    }

    private static async Task ChayThat(string goc)
    {
        // ---------- Ghép: chạy trọn một ca ----------
        Kiem.MoBai("G.11.1", "Ghép toàn máy — chạy trọn một ca");
        var (may, at, _) = Tao(goc);
        using (may)
        {
            Kiem.Bang(may.TrangThai, TrangThaiMay.ChuaKhoiTao, "máy mới ráp: Chưa khởi tạo");
            Kiem.Dung(!may.TrangThaiNut(LenhMay.BatDau).BatDuoc, "chưa về gốc → nút Bắt đầu mờ");

            await may.KhoiTaoAsync();
            Kiem.Bang(may.TrangThai, TrangThaiMay.SanSang, "khởi tạo xong → Sẵn sàng");
            Kiem.Dung(may.TrangThaiNut(LenhMay.BatDau).BatDuoc, "Sẵn sàng → nút Bắt đầu bật");

            await may.ChayAsync(12);
            Kiem.Bang(may.TrangThai, TrangThaiMay.SanSang, "chạy xong về Sẵn sàng");
            Kiem.Bang(may.SoPhoi, 12, "đếm đủ 12 phôi");
            Kiem.Bang(may.BoDem.HienTai.Ok + may.BoDem.HienTai.Ng, 12, "mọi phôi đều được phân loại");
            Kiem.Dung(may.BoDem.HienTai.Ng > 0, "có phôi không đạt");

            // ── Mọi mảnh phải thật sự ĐƯỢC NỐI, không chỉ tồn tại ──
            Kiem.Dung(File.Exists(may.SoGhi.DuongDanHomNay), "★ ĐÃ NỐI ghi CSV: file kết quả tồn tại");
            Kiem.Bang(File.ReadAllLines(may.SoGhi.DuongDanHomNay).Length, 13, "1 tiêu đề + 12 dòng phôi");
            // Ngưỡng suy ra TỪ SỰ KIỆN THẬT, không chọn một con số cho đẹp:
            // 12 bản ghi phôi + 4 lần đổi trạng thái (KhoiTao, VeGocXong, BatDau, Dung).
            Kiem.Dung(may.CuaRaBoNho.TatCa.Count >= 16,
                      $"★ ĐÃ NỐI nhật ký có cấu trúc ({may.CuaRaBoNho.TatCa.Count} bản ghi ≥ 16)");
            Kiem.Bang(may.CuaRaBoNho.Loc("SoHieu", 7).Count(), 1,
                      "★ tra được bản ghi của ĐÚNG phôi số 7 bằng thuộc tính");
            Kiem.Bang(may.BangLog.SoDongDangGiu, may.CuaRaBoNho.TatCa.Count,
                      "★ ĐÃ NỐI cửa ra thứ hai: bảng giao diện nhận ĐÚNG bằng số bản ghi của nhật ký");
            Kiem.Bang(may.ManHinh.SoPhoi, 12, "★ ĐÃ NỐI ViewModel: sản lượng cập nhật");
            Kiem.Bang(may.ManHinh.MoTaTienDo, "Bước 7/7: Về vị trí chờ", "★ ĐÃ NỐI tiến độ bước");
            Kiem.Dung(may.CuaSo.Tinh().DuMau, "★ ĐÃ NỐI cửa sổ trượt");
            Kiem.Bang(may.BatTay.SoPhoiDaChuyen, 12, "★ ĐÃ NỐI bắt tay hai dây");
            Kiem.Dung(may.VaoRa.DocRa(CumKep.TinHieuRaKep) == false, "★ ĐÃ NỐI vào-ra theo tên: kẹp đã nhả");
            Kiem.Dung(may.CuaRaBoNho.Loc("Lenh", LenhMay.BatDau).Any(),
                      "★ ĐÃ NỐI bảng chuyển trạng thái: lệnh đi qua bảng và được ghi log");

            may.TatMay();
            Kiem.Dung(may.CuaRaBoNho.TatCa[^1].Khuon.Contains("Tắt máy", StringComparison.Ordinal),
                      "★ tắt máy sạch: chốt số liệu ca vào nhật ký");
        }

        // ---------- Tín hiệu an toàn CHỈ ĐỌC khoá lệnh ----------
        Kiem.MoBai("G.11.2", "Tín hiệu an toàn chỉ đọc và việc khoá lệnh");
        var (may2, at2, _) = Tao(Path.Combine(goc, "b"));
        using (may2)
        {
            await may2.KhoiTaoAsync();
            at2.ManChanBiChe = true;
            Kiem.Dung(!may2.TrangThaiNut(LenhMay.BatDau).BatDuoc, "màn chắn bị che → nút Bắt đầu mờ");
            Kiem.Bang(may2.TrangThaiNut(LenhMay.BatDau).LyDoMo, "Màn chắn sáng đang bị che",
                      "★ nút mờ kèm ĐÚNG lý do lấy từ tín hiệu an toàn");

            await may2.ChayAsync(5);
            Kiem.Bang(may2.SoPhoi, 0, "★ màn chắn bị che → KHÔNG chạy, không phôi nào được xử lý");
            Kiem.Bang(may2.TrangThai, TrangThaiMay.SanSang, "vẫn ở Sẵn sàng, không tự nhảy trạng thái");
            Kiem.Dung(may2.CuaRaBoNho.Loc("Man", true).Any(), "ghi log rõ lý do từ chối");

            at2.ManChanBiChe = false;
            await may2.ChayAsync(3);
            Kiem.Bang(may2.SoPhoi, 3, "bỏ che → chạy lại bình thường");

            // Bất biến: phần mềm KHÔNG có đường nào ĐẶT tín hiệu an toàn
            Kiem.Dung(typeof(IAnToanChiDoc).GetProperties().All(p => !p.CanWrite),
                      "★ IAnToanChiDoc không có thuộc tính ghi được — phần mềm chỉ ĐỌC (mục 15.2.2b)");
        }

        // ---------- Cảnh báo → xác nhận → Reset → chạy lại ----------
        Kiem.MoBai("G.11.3", "Đường ra khỏi báo động");
        var (may3, _, _) = Tao(Path.Combine(goc, "c"));
        using (may3)
        {
            await may3.KhoiTaoAsync();
            may3.CamBien.MoPhongKhongPhanHoi = true;
            await may3.ChayAsync(5);

            Kiem.Bang(may3.TrangThai, TrangThaiMay.BaoDong, "cảm biến lỗi → Báo động");
            Kiem.Dung(may3.BangCanhBao.CoCanhBao, "★ ĐÃ NỐI bảng cảnh báo giao diện");
            Kiem.Dung(may3.CuaRaBoNho.Loc("Ma", MaCanhBao.CamBienKhongPhanHoi).Any(),
                      "★ ĐÃ NỐI danh mục cảnh báo: log kèm nhóm và việc nên làm");
            Kiem.Gan(may3.TrucZ.ViTriMm, 0.0, 1e-6, "có sự cố, trục Z vẫn ở vị trí an toàn");

            Kiem.Dung(!may3.Reset(), "★ còn cảnh báo chưa xác nhận → KHÔNG cho Reset");
            Kiem.Bang(may3.TrangThai, TrangThaiMay.BaoDong, "vẫn ở Báo động");

            Kiem.Dung(may3.XacNhanCanhBao(MaCanhBao.CamBienKhongPhanHoi), "xác nhận được cảnh báo");
            may3.CamBien.MoPhongKhongPhanHoi = false;
            Kiem.Dung(may3.Reset(), "★ xác nhận xong → Reset thành công");
            Kiem.Bang(may3.TrangThai, TrangThaiMay.ChuaKhoiTao, "Reset đưa về Chưa khởi tạo");

            await may3.KhoiTaoAsync();
            await may3.ChayAsync(4);
            Kiem.Bang(may3.TrangThai, TrangThaiMay.SanSang, "★ về gốc lại rồi CHẠY TIẾP ĐƯỢC sau sự cố");
            Kiem.Bang(may3.SoPhoi, 4, "4 phôi sau khi phục hồi");
        }

        // ---------- Tạm dừng / chạy tiếp trên máy thật ----------
        Kiem.MoBai("G.11.4", "Tạm dừng và chạy tiếp trên cỗ máy ghép");
        var (may4, _, _) = Tao(Path.Combine(goc, "d"));
        using (may4)
        {
            await may4.KhoiTaoAsync();
            var chay = may4.ChayAsync(30);
            await Task.Delay(60);
            may4.TamDung();
            Kiem.Bang(may4.TrangThai, TrangThaiMay.TamDung, "tạm dừng → trạng thái Tạm dừng");

            await Task.Delay(40);
            int phoiLucDung = may4.SoPhoi;
            await Task.Delay(100);
            Kiem.Bang(may4.SoPhoi, phoiLucDung, "★ đang tạm dừng thì sản lượng ĐỨNG YÊN");

            may4.ChayTiep();
            Kiem.Bang(may4.TrangThai, TrangThaiMay.DangChay, "chạy tiếp → Đang chạy");
            await chay;
            Kiem.Bang(may4.SoPhoi, 30, "★ chạy tiếp rồi hoàn thành đủ 30 phôi, không mất phôi nào");
        }

        // ---------- Khí nén yếu → dừng cuối chu kỳ ----------
        Kiem.MoBai("G.11.5", "Khí nén yếu làm máy dừng cuối chu kỳ");
        double ap = 6.0;
        var dh5 = new DongHoGia(new DateTime(2026, 9, 20, 8, 0, 0));
        var ch5 = new CauHinhHoanChinh
        {
            May = new CauHinhMay { HatGiongGiaLap = 55 },
            ThuMucDuLieu = Path.Combine(goc, "e"),
            MaCa = "CA-A",
        };
        using (var may5 = new MayHoanChinh(ch5, dh5, new AnToanGiaLap(),
                                           new CongThuc { Ten = "SanPhamA" }, () => ap))
        {
            await may5.KhoiTaoAsync();
            var chay5 = may5.ChayAsync(40);
            await Task.Delay(50);
            ap = 4.2;                              // khí tụt giữa chừng
            await chay5;

            Kiem.Dung(may5.SoPhoi < 40, "★ khí yếu → máy DỪNG SỚM, không chạy hết 40 phôi");
            Kiem.Dung(may5.SoPhoi > 0, "các phôi trước đó vẫn được xử lý trọn vẹn");
            Kiem.Bang(may5.TrangThai, TrangThaiMay.SanSang,
                      "★ dừng cuối chu kỳ là dừng CÓ TRẬT TỰ — về Sẵn sàng, không phải Báo động");
            Kiem.Dung(may5.CuaRaBoNho.TatCa.Any(b => b.Nguon == "KHI"),
                      "★ ĐÃ NỐI giám sát khí nén: có bản ghi nguồn KHI");
            Kiem.Dung(may5.GiamSatKhi.SoLanBaoDong >= 1, "giám sát đếm được lần báo động");
        }

        // ---------- Đói và bị chặn ----------
        Kiem.MoBai("G.11.6", "Đói và bị chặn — máy không tính nhầm sản lượng");
        var (may6, _, _) = Tao(Path.Combine(goc, "f"));
        using (may6)
        {
            await may6.KhoiTaoAsync();
            may6.DatMaySau(false);                 // máy sau đầy
            using var cts = new CancellationTokenSource(250);
            try { await may6.ChayAsync(10, cts.Token); }
            catch (OperationCanceledException) { /* mong đợi */ }

            Kiem.Bang(may6.SoPhoi, 0, "★ máy sau đầy → KHÔNG chuyển được phôi nào");
            Kiem.Dung(may6.BatTay.SoGiayBiChan > 0, "đếm được số nhịp BỊ CHẶN");
            Kiem.Bang(may6.BatTay.SoGiayDoiHang, 0, "★ không nhầm 'bị chặn' thành 'đói'");
        }

        // ---------- Số liệu ca sống sót qua khởi động lại ----------
        Kiem.MoBai("G.11.7", "Số liệu ca sống sót qua khởi động lại phần mềm");
        string gocG = Path.Combine(goc, "g");
        int truoc;
        var (mayA, _, _) = Tao(gocG, hatGiong: 77);
        using (mayA)
        {
            await mayA.KhoiTaoAsync();
            await mayA.ChayAsync(6);
            truoc = mayA.SoPhoi;
            mayA.TatMay();
        }
        var (mayB, _, _) = Tao(gocG, hatGiong: 77);   // "khởi động lại phần mềm"
        using (mayB)
        {
            Kiem.Bang(mayB.SoPhoi, truoc, "★ mở lại phần mềm → sản lượng ca KHÔNG mất");
            await mayB.KhoiTaoAsync();
            await mayB.ChayAsync(4);
            Kiem.Bang(mayB.SoPhoi, truoc + 4, "chạy tiếp thì cộng dồn đúng vào ca đang chạy");
        }

        // ---------- Đổi nguồn cảm biến bằng MỘT dòng cấu hình ----------
        Kiem.MoBai("G.11.8", "Đổi nguồn cảm biến: giả lập ↔ driver nối tiếp");
        var dh9 = new DongHoGia(new DateTime(2026, 9, 20, 8, 0, 0));
        var ch9 = new CauHinhHoanChinh
        {
            May = new CauHinhMay { HatGiongGiaLap = 404 },
            ThuMucDuLieu = Path.Combine(goc, "h"),
            MaCa = "CA-A",
            NguonCamBien = NguonCamBien.NoiTiep,       // ← ĐỔI ĐÚNG MỘT DÒNG
        };
        using (var may9 = new MayHoanChinh(ch9, dh9, new AnToanGiaLap(),
                                           new CongThuc { Ten = "SanPhamA" }, () => 6.0))
        {
            await may9.KhoiTaoAsync();
            await may9.ChayAsync(30);
            Kiem.Bang(may9.SoPhoi, 30,
                      "★ đổi MỘT dòng cấu hình → cả máy chạy bằng DRIVER NỐI TIẾP, không lớp nào phải sửa");
            Kiem.Bang(may9.TrangThai, TrangThaiMay.SanSang, "chạy xong về Sẵn sàng");
            Kiem.Bang(may9.BoDem.HienTai.Ok + may9.BoDem.HienTai.Ng, 30,
                      "dữ liệu đo đi qua phân khung + tổng kiểm vẫn phân loại được đủ 30 phôi");
            Kiem.Dung(may9.BoDem.HienTai.Ng > 0,
                      $"★ vẫn phát hiện được phôi dày qua đường nối tiếp ({may9.BoDem.HienTai.Ng} phôi NG)");
        }

        // ---------- Nạp công thức từ FILE ----------
        Kiem.MoBai("G.11.9", "Nạp công thức từ file lúc khởi động");
        string fileCt = Path.Combine(goc, "congthuc.json");
        KhoCongThuc.Ghi(fileCt, new CongThuc { Ten = "SanPhamB", ChieuDayDanhDinhMm = 3.000 });

        var chCt = new CauHinhHoanChinh
        {
            May = new CauHinhMay { HatGiongGiaLap = 5 },
            ThuMucDuLieu = Path.Combine(goc, "i"),
            DuongDanCongThuc = fileCt,
        };
        using (var mayCt = new MayHoanChinh(chCt, new DongHoGia(new DateTime(2026, 9, 20, 8, 0, 0)),
                                            new AnToanGiaLap(), new CongThuc { Ten = "DuPhong" }, () => 6.0))
        {
            Kiem.Bang(mayCt.CongThuc.Ten, "SanPhamB", "★ ĐÃ NỐI nạp công thức: lấy từ FILE, không phải dự phòng");
            Kiem.Gan(mayCt.CongThuc.GioiHanDuoiMm, 2.950, 1e-9, "dải đo lấy theo công thức trong file");
            Kiem.Dung(mayCt.CuaRaBoNho.TatCa.Any(b => b.Nguon == "CONGTHUC"),
                      "ghi log công thức đang dùng ngay lúc khởi động");
        }

        File.WriteAllText(fileCt, "{{{ rác", System.Text.Encoding.UTF8);
        using (var mayCtHong = new MayHoanChinh(chCt, new DongHoGia(new DateTime(2026, 9, 20, 8, 0, 0)),
                                                new AnToanGiaLap(), new CongThuc { Ten = "DuPhong" }, () => 6.0))
        {
            Kiem.Bang(mayCtHong.CongThuc.Ten, "DuPhong",
                      "★ file công thức HỎNG → máy vẫn ráp được bằng dự phòng, KHÔNG sập");
            Kiem.Dung(mayCtHong.CuaRaBoNho.TatCa.Any(
                          b => b.Muc == MucLog.CanhBao && b.Nguon == "CONGTHUC"),
                      "★ và có cảnh báo trong nhật ký — không im lặng dùng dự phòng");
        }

        // ---------- Công thức sai bị chặn ngay lúc ráp ----------
        Kiem.MoBai("G.11.10", "Công thức và bản đồ tín hiệu được kiểm ngay lúc ráp");
        Kiem.Nem<ArgumentException>(() =>
        {
            using var _ = new MayHoanChinh(
                new CauHinhHoanChinh { ThuMucDuLieu = goc },
                new DongHoGia(DateTime.Now), new AnToanGiaLap(),
                new CongThuc { Ten = "Xau", DungSaiDuoiMm = -1 }, () => 6.0);
        }, "★ công thức sai → chặn NGAY LÚC RÁP, không đợi tới lúc chạy");
    }
}
