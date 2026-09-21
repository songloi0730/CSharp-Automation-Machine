// -------------------------------------------------------
// File:    KiemBaiXuongSong.cs
// Project: MeoBench
// Purpose: Phép kiểm cho 20 lời giải xương sống của Phụ lục I.
//          Mỗi lời giải kiểm ít nhất một ca THƯỜNG và một ca BIÊN —
//          vì bài luyện mà chỉ chạy ca thường thì không luyện được gì.
// -------------------------------------------------------

namespace MeoBench;

/// <summary>Phép kiểm cho các bài xương sống của Phụ lục I.</summary>
public static class KiemBaiXuongSong
{
    /// <summary>Chạy toàn bộ.</summary>
    public static async Task Chay()
    {
        CuaSoTruot();
        NganXepDonDieu();
        Khoang();
        CauTruc();
        ChuoiVaBit();
        MaTranVaDoThi();
        TimVaLich();
        await DongThoi().ConfigureAwait(false);
    }

    private static void CuaSoTruot()
    {
        Kiem.MoBai("I.1", "Cửa sổ trượt — đỉnh, trung bình, đếm sự kiện, đoạn ổn định");

        // 239 — đỉnh cửa sổ trượt
        var d = DinhCuaSoTruot.Tinh([1, 3, -1, -3, 5, 3, 6, 7], 3);
        Kiem.Bang(string.Join(",", d), "3,3,5,5,6,7", "LC239: đỉnh mỗi cửa sổ 3 mẫu");
        Kiem.Bang(DinhCuaSoTruot.Tinh([2, 1], 5).Length, 0, "LC239 ca biên: cửa sổ rộng hơn dữ liệu → rỗng");

        // 346 — trung bình trượt
        var tb = new TrungBinhTruot(3);
        Kiem.Gan(tb.Nap(1), 1.0, 1e-9, "LC346: mẫu đầu");
        _ = tb.Nap(10);
        Kiem.Gan(tb.Nap(3), 14.0 / 3, 1e-9, "LC346: đủ 3 mẫu");
        Kiem.Gan(tb.Nap(5), 18.0 / 3, 1e-9, "LC346: mẫu thứ 4 đẩy mẫu đầu ra");
        Kiem.Bang(tb.SoMau, 3, "LC346: bộ nhớ KHÔNG lớn lên theo số mẫu đã nạp");

        // 933 — đếm sự kiện trong cửa sổ
        var dem = new DemSuKienGanDay(3000);
        Kiem.Bang(dem.Ghi(1), 1, "LC933: sự kiện đầu");
        Kiem.Bang(dem.Ghi(100), 2, "LC933: còn trong cửa sổ");
        Kiem.Bang(dem.Ghi(3001), 3, "LC933: đúng mép cửa sổ vẫn tính");
        Kiem.Bang(dem.Ghi(3002), 3, "LC933: mốc 1 đã trôi ra");

        // 1438 — đoạn ổn định dài nhất
        Kiem.Bang(DoanOnDinhDaiNhat.Tinh([8, 2, 4, 7], 4), 2, "LC1438: biên độ ≤ 4");
        Kiem.Bang(DoanOnDinhDaiNhat.Tinh([4, 2, 2, 2, 4, 4, 2, 2], 0), 3,
            "LC1438 ca biên: biên độ 0 → đoạn các mẫu bằng nhau");
    }

    private static void NganXepDonDieu()
    {
        Kiem.MoBai("I.2", "Ngăn xếp đơn điệu — đáy O(1) và độ dài xu hướng");

        var ns = new NganXepCoDay();
        ns.Day(5.0); ns.Day(3.0); ns.Day(7.0);
        Kiem.Gan(ns.CucTieu, 3.0, 1e-9, "LC155: đáy là 3");
        _ = ns.Lay();
        Kiem.Gan(ns.CucTieu, 3.0, 1e-9, "LC155: bỏ 7 thì đáy vẫn 3");
        _ = ns.Lay();
        Kiem.Gan(ns.CucTieu, 5.0, 1e-9, "LC155: bỏ 3 thì đáy quay về 5");

        var xh = new DoDaiXuHuong();
        int[] mong = [1, 1, 1, 2, 1, 4, 6];
        double[] gia = [100, 80, 60, 70, 60, 75, 85];
        for (int i = 0; i < gia.Length; i++)
            Kiem.Bang(xh.Nap(gia[i]), mong[i], $"LC901: mẫu {i} → xu hướng dài {mong[i]}");
    }

    private static void Khoang()
    {
        Kiem.MoBai("I.3", "Khoảng thời gian — gộp, đếm tài nguyên, tải theo khoảng");

        var gop = LichKhoang.Gop([new(1, 3), new(2, 6), new(8, 10), new(15, 18)]);
        Kiem.Bang(gop.Count, 3, "LC56: gộp còn 3 khoảng");
        Kiem.Bang(gop[0].KetThuc, 6L, "LC56: hai khoảng đầu gộp thành [1;6)");

        Kiem.Bang(LichKhoang.SoTaiNguyenCanThiet([new(0, 30), new(5, 10), new(15, 20)]), 2,
            "LC253: cần 2 tài nguyên song song");
        Kiem.Bang(LichKhoang.SoTaiNguyenCanThiet([new(0, 5), new(5, 9)]), 1,
            "★ LC253 ca biên: [0;5) và [5;9) KHÔNG chồng nhau — chỉ cần 1");

        Kiem.Dung(!TaiTheoKhoang.CoQuaTai([(new(1, 5), 2), (new(3, 7), 3)], 5, 10),
            "LC1094: tải đỉnh 5, sức chứa 5 → vừa đủ");
        Kiem.Dung(TaiTheoKhoang.CoQuaTai([(new(1, 5), 2), (new(3, 7), 3)], 4, 10),
            "LC1094: sức chứa 4 → quá tải");
    }

    private static void CauTruc()
    {
        Kiem.MoBai("I.4", "Cấu trúc — bộ đệm vòng, LRU, chặn lặp");

        var hd = new HangDoiVong<int>(3);
        Kiem.Dung(hd.ThemCuoi(1) && hd.ThemCuoi(2) && hd.ThemCuoi(3), "LC622: thêm đủ 3");
        Kiem.Dung(!hd.ThemCuoi(4), "★ LC622: đầy thì TRẢ VỀ FALSE, không ném — đầy là chuyện bình thường");
        Kiem.Dung(hd.LayDau(out int x) && x == 1, "LC622: lấy ra đúng thứ tự vào");
        Kiem.Dung(hd.ThemCuoi(4), "LC622: lấy ra rồi thì thêm được, chỉ số chạy vòng");

        var lru = new DemLruCache<string, int>(2);
        lru.Dat("A", 1);
        lru.Dat("B", 2);
        Kiem.Dung(lru.ThuLay("A", out int a) && a == 1, "LC146: lấy A");
        lru.Dat("C", 3);                       // A vừa dùng nên B bị bỏ
        Kiem.Dung(!lru.ThuLay("B", out _), "★ LC146: B bị loại vì lâu không dùng nhất");
        Kiem.Dung(lru.ThuLay("A", out _) && lru.ThuLay("C", out _), "LC146: A và C còn");

        var chan = new ChanLapTheoThoiGian(10_000);
        Kiem.Dung(chan.ChoPhep("AL-1001", 0), "LC359: lần đầu cho qua");
        Kiem.Dung(!chan.ChoPhep("AL-1001", 5_000), "LC359: trong cửa sổ thì chặn");
        Kiem.Dung(chan.ChoPhep("AL-2002", 5_000), "LC359: mã khác không bị ảnh hưởng");
        Kiem.Dung(chan.ChoPhep("AL-1001", 10_000), "LC359: hết cửa sổ thì cho qua");
        Kiem.Bang(chan.SoLanBiChan("AL-1001"), 1,
            "★ bản máy ĐẾM số lần bị nuốt — thứ bản LeetCode không cần mà máy thì cần");
    }

    private static void ChuoiVaBit()
    {
        Kiem.MoBai("I.5", "Chuỗi, máy trạng thái và thao tác bit");

        Kiem.Dung(KhopCapLongNhau.HopLe("{[()]}"), "LC20: lồng nhau hợp lệ");
        Kiem.Dung(!KhopCapLongNhau.HopLe("{[(])}"), "LC20: lồng chéo → sai");
        Kiem.Dung(!KhopCapLongNhau.HopLe("{\"a\":1"), "★ LC20: tệp cấu hình bị cắt cụt → bắt được");

        foreach (string s in new[] { "0", "2", "0089", "-0.1", "+3.14", "4.", "-.9", "2e10", "-90E3", "3e+7" })
            Kiem.Dung(SoHopLe.KiemTra(s), $"LC65: \"{s}\" là số hợp lệ");
        foreach (string s in new[] { "abc", "1a", "1e", "e3", "99e2.5", "--6", "-+3", "95a54e53", "", "." })
            Kiem.Dung(!SoHopLe.KiemTra(s), $"LC65: \"{s}\" KHÔNG hợp lệ");

        Kiem.Bang(ThaoTacBit.DemBitBat(0b1011), 3, "LC191: đếm bit bật");
        Kiem.Bang(ThaoTacBit.DemBitBat(0), 0, "LC191 ca biên: không bit nào");
        Kiem.Bang(ThaoTacBit.TongKiemXor([0x02, 0x41, 0x30]), (byte)0x73, "LC136/G.2.4: checksum XOR");
        Kiem.Bang(ThaoTacBit.TongKiemXor([]), (byte)0, "checksum của khung rỗng");

        for (uint v = 0; v < 64; v++)
            if (ThaoTacBit.TuGray(ThaoTacBit.SangGray(v)) != v)
            {
                Kiem.Dung(false, $"LC89: Gray không khứ hồi được ở {v}");
                return;
            }
        Kiem.Dung(true, "★ LC89: mã Gray khứ hồi đúng cho cả 64 giá trị");
        Kiem.Bang(ThaoTacBit.DemBitBat(ThaoTacBit.SangGray(7) ^ ThaoTacBit.SangGray(8)), 1,
            "★ hai vạch encoder liền nhau chỉ khác ĐÚNG MỘT bit — đó là lý do dùng mã Gray");
    }

    private static void MaTranVaDoThi()
    {
        Kiem.MoBai("I.6", "Ma trận khay và thứ tự trạm");

        var khay = new[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
        BanDoKhay.Xoay90(khay);
        Kiem.Bang(khay[0, 0], 7, "LC48: xoay 90° — góc trên trái thành 7");
        Kiem.Bang(khay[2, 2], 3, "LC48: góc dưới phải thành 3");

        var luoi = BanDoKhay.ThanhLuoi([1, 2, 3, 4], 2, 2);
        Kiem.Dung(luoi is not null && luoi[1, 0] == 3, "LC2022: đổi 1 chiều sang lưới 2×2");
        Kiem.Dung(BanDoKhay.ThanhLuoi([1, 2, 3], 2, 2) is null,
            "★ LC2022 ca biên: số phần tử không khớp → trả null, KHÔNG đoán bừa rồi cắt bớt");

        var anh = new bool[4, 5];
        anh[0, 0] = anh[0, 1] = anh[1, 0] = true;      // vùng 1
        anh[2, 3] = true;                              // vùng 2
        anh[3, 3] = anh[3, 4] = true;                  // nối với vùng 2 qua (2,3)-(3,3)
        Kiem.Bang(DemVungLoi.Dem(anh), 2, "LC200: hai vùng khuyết tật tách biệt");
        Kiem.Bang(DemVungLoi.Dem(new bool[3, 3]), 0, "LC200 ca biên: tấm sạch → 0 vùng");

        var tt = ThuTuTram.SapThuTu(4, [(0, 1), (0, 2), (1, 3), (2, 3)]);
        Kiem.Dung(tt is not null && tt[0] == 0 && tt[^1] == 3, "LC210: thứ tự hợp lệ, 0 trước 3 sau");
        Kiem.Dung(ThuTuTram.SapThuTu(2, [(0, 1), (1, 0)]) is null,
            "★ LC207: hai trạm chờ vòng tròn → phát hiện KHOÁ CHẾT (mục 16.3.4)");
    }

    private static void TimVaLich()
    {
        Kiem.MoBai("I.7", "Tìm nhị phân trên đáp án, lập lịch, thống kê ca");

        Kiem.Bang(TimNhiPhanTrenDapAn.NhipToiThieu([3, 6, 7, 11], 8), 4L, "LC875: nhịp tối thiểu");
        Kiem.Bang(TimNhiPhanTrenDapAn.NhipToiThieu([30, 11, 23, 4, 20], 5), 30L,
            "★ LC875 ca biên: mỗi lô một giờ → nhịp phải bằng lô lớn nhất");

        Kiem.Bang(LichCoThoiGianHoi.TongNhip("AAABBB", 2), 8, "LC621: có thời gian hồi 2");
        Kiem.Bang(LichCoThoiGianHoi.TongNhip("AAABBB", 0), 6, "LC621: không hồi → bằng số việc");
        Kiem.Bang(LichCoThoiGianHoi.TongNhip("", 5), 0, "LC621 ca biên: không việc nào");

        var ca = new ThongKeCa([10, 12, 9, 15, 11]);
        Kiem.Bang(ca.Tong(1, 4), 36L, "LC303: tổng giờ 1..3");
        Kiem.Bang(ca.Tong(2, 2), 0L, "LC303 ca biên: khoảng rỗng");
        Kiem.Bang(ThongKeCa.DemDoanCoTong([1, 1, 1], 2), 2, "LC560: hai đoạn có tổng 2");
    }

    private static async Task DongThoi()
    {
        Kiem.MoBai("I.8", "Đồng thời — ép thứ tự giữa các luồng");

        using var chot = new ChotThuTu(3);
        var vet = new System.Collections.Concurrent.ConcurrentQueue<int>();

        // Cố tình khởi chạy NGƯỢC thứ tự: 2, 1, 0.
        var t2 = chot.ChayDungLuotAsync(2, async () => { await Task.Delay(1).ConfigureAwait(false); vet.Enqueue(2); });
        var t1 = chot.ChayDungLuotAsync(1, async () => { await Task.Delay(1).ConfigureAwait(false); vet.Enqueue(1); });
        var t0 = chot.ChayDungLuotAsync(0, async () => { await Task.Delay(1).ConfigureAwait(false); vet.Enqueue(0); });
        await Task.WhenAll(t0, t1, t2).ConfigureAwait(false);

        Kiem.Bang(string.Join(",", vet), "0,1,2",
            "★ LC1114: khởi chạy ngược 2-1-0 nhưng CHẠY đúng 0-1-2");

        // Việc lỗi cũng không được làm kẹt dây chuyền.
        using var chot2 = new ChotThuTu(2);
        var loi = chot2.ChayDungLuotAsync(0, () => throw new InvalidOperationException("hỏng"));
        bool xong = false;
        var sau = chot2.ChayDungLuotAsync(1, () => { xong = true; return Task.CompletedTask; });
        await Kiem.BatAsync<InvalidOperationException>(() => loi).ConfigureAwait(false);
        await sau.ConfigureAwait(false);
        Kiem.Dung(xong, "★ bước trước LỖI nhưng cổng kế tiếp vẫn mở — không kẹt cả dây chuyền");
    }
}
