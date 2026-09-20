// -------------------------------------------------------
// File:    GiaoDien.cs
// Project: MeoBench
// Purpose: Lời giải mẫu nhóm G.7 — giao diện.
//          G.7.1 ViewModel trạng thái máy
//          G.7.2 thanh tiến độ bước
//          G.7.3 ô nhập số có dải
//          G.7.4 bảng nhật ký và bảng cảnh báo
//          G.7.5 màn hình chính — nút khoá theo trạng thái
//
// LƯU Ý QUAN TRỌNG: cả năm bài chạy được và kiểm được TRONG MỘT ỨNG DỤNG
// CONSOLE, không cần WPF, không cần mở cửa sổ nào. Đó không phải mẹo lách —
// đó chính là điều Chương 9 nói: nếu ViewModel cần cửa sổ thật mới chạy được
// thì bạn đã để logic giao diện dính vào khung nhìn.
// -------------------------------------------------------
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace MeoBench;

// ══════════════ G.7.1 — ViewModel trạng thái máy ══════════════

/// <summary>
/// Trừu tượng hoá việc "đưa về luồng giao diện". WPF dùng Dispatcher,
/// WinForms dùng Control.Invoke — ViewModel không cần biết cái nào.
/// </summary>
public interface IDieuPhoi
{
    void Chay(Action hanhDong);
}

/// <summary>Bản dùng khi kiểm thử: chạy thẳng và ĐẾM số lần được gọi.</summary>
public sealed class DieuPhoiTrucTiep : IDieuPhoi
{
    public int SoLanGoi { get; private set; }
    public void Chay(Action hanhDong)
    {
        ArgumentNullException.ThrowIfNull(hanhDong);
        SoLanGoi++;
        hanhDong();
    }
}

public sealed class ManHinhVM : INotifyPropertyChanged
{
    private readonly IDieuPhoi _dieuPhoi;

    public ManHinhVM(IDieuPhoi dieuPhoi)
    {
        ArgumentNullException.ThrowIfNull(dieuPhoi);
        _dieuPhoi = dieuPhoi;
    }

    private TrangThaiMay _trangThai = TrangThaiMay.ChuaKhoiTao;
    private int    _buocHienTai;
    private int    _tongSoBuoc;
    private string _tenBuoc = "";
    private int    _soPhoi, _soOk, _soNg;
    private string? _canhBao;

    public TrangThaiMay TrangThai   { get => _trangThai;   private set => Dat(ref _trangThai, value); }
    public int          BuocHienTai { get => _buocHienTai; private set => Dat(ref _buocHienTai, value); }
    public int          TongSoBuoc  { get => _tongSoBuoc;  private set => Dat(ref _tongSoBuoc, value); }
    public string       TenBuoc     { get => _tenBuoc;     private set => Dat(ref _tenBuoc, value); }
    public int          SoPhoi      { get => _soPhoi;      private set => Dat(ref _soPhoi, value); }
    public int          SoOk        { get => _soOk;        private set => Dat(ref _soOk, value); }
    public int          SoNg        { get => _soNg;        private set => Dat(ref _soNg, value); }
    public string?      CanhBao     { get => _canhBao;     private set => Dat(ref _canhBao, value); }

    // ── G.7.2 — thanh tiến độ bước ──
    public string MoTaTienDo => TongSoBuoc == 0
        ? "Chưa chạy"
        : string.Format(CultureInfo.InvariantCulture, "Bước {0}/{1}: {2}", BuocHienTai, TongSoBuoc, TenBuoc);

    public double PhanTramTienDo => TongSoBuoc == 0
        ? 0.0
        : Math.Round(BuocHienTai * 100.0 / TongSoBuoc, 1);

    public double TyLeDat => SoPhoi == 0 ? 0.0 : Math.Round(SoOk * 100.0 / SoPhoi, 2);

    /// <summary>Tầng dưới gọi hàm này TỪ LUỒNG NỀN — ViewModel tự đưa về đúng luồng.</summary>
    public void CapNhatTrangThai(TrangThaiMay tt, string? canhBao = null)
        => _dieuPhoi.Chay(() => { TrangThai = tt; CanhBao = canhBao; });

    public void CapNhatBuoc(int chiSo, int tong, string ten)
        => _dieuPhoi.Chay(() =>
        {
            BuocHienTai = chiSo; TongSoBuoc = tong; TenBuoc = ten;
            Bao(nameof(MoTaTienDo)); Bao(nameof(PhanTramTienDo));
        });

    public void CapNhatSanLuong(int tong, int ok, int ng)
        => _dieuPhoi.Chay(() =>
        {
            SoPhoi = tong; SoOk = ok; SoNg = ng;
            Bao(nameof(TyLeDat));
        });

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Bao([CallerMemberName] string? ten = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(ten));

    private void Dat<T>(ref T truong, T giaTri, [CallerMemberName] string? ten = null)
    {
        if (EqualityComparer<T>.Default.Equals(truong, giaTri)) return;   // không báo khi không đổi
        truong = giaTri;
        Bao(ten);
    }
}

// ══════════════ G.7.3 — ô nhập số có dải ══════════════

public sealed record KetQuaNhap(bool HopLe, double GiaTri, string? ThongBao);

public sealed class BoNhapSo(double nhoNhat, double lonNhat, string donVi, int soChuSoThapPhan = 3)
{
    public double NhoNhat => nhoNhat;
    public double LonNhat => lonNhat;
    public string DonVi   => donVi;

    /// <summary>Ký tự này có được phép gõ vào ô không (chặn ngay lúc gõ).</summary>
    public bool ChoPhepKyTu(char c, string dangCo)
    {
        if (char.IsDigit(c)) return true;
        if (c == '-') return dangCo.Length == 0 && nhoNhat < 0;      // dấu âm chỉ ở đầu, và chỉ khi dải cho phép
        if (c is '.' or ',') return !dangCo.Contains('.', StringComparison.Ordinal)
                                 && !dangCo.Contains(',', StringComparison.Ordinal);
        return false;
    }

    /// <summary>
    /// Kiểm chuỗi người dùng đã gõ. CHẤP NHẬN CẢ dấu chấm lẫn dấu phẩy làm dấu
    /// thập phân: người vận hành gõ theo thói quen và theo bàn phím họ có,
    /// không theo vùng miền Windows đang đặt.
    /// </summary>
    public KetQuaNhap Kiem(string? vanBan)
    {
        if (string.IsNullOrWhiteSpace(vanBan))
            return new KetQuaNhap(false, 0, "Chưa nhập giá trị");

        string chuan = vanBan.Trim().Replace(',', '.');

        if (!double.TryParse(chuan, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
            return new KetQuaNhap(false, 0, "Không phải số hợp lệ");

        if (v < nhoNhat || v > lonNhat)
            return new KetQuaNhap(false, v, string.Format(CultureInfo.InvariantCulture,
                "Phải trong khoảng {0} – {1} {2}", nhoNhat, lonNhat, donVi));

        return new KetQuaNhap(true, Math.Round(v, soChuSoThapPhan), null);
    }
}

// ══════════════ G.7.4 — bảng nhật ký và bảng cảnh báo ══════════════

public enum MucLog { GoLoi = 0, ThongTin = 1, CanhBao = 2, Loi = 3 }

public sealed record DongLog(DateTime ThoiDiem, MucLog Muc, string Nguon, string NoiDung);

/// <summary>
/// Bảng log là một CỬA RA cắm vào đường ống log — nó không được lớp ghi log
/// giữ tham chiếu tới. Đảo chiều này là toàn bộ khác biệt ở mục 19.4.1b.
/// </summary>
public sealed class BangLogVM(int soDongGiuToiDa = 2000)
{
    private readonly Queue<DongLog> _dong = new();

    public MucLog MucLoc { get; set; } = MucLog.GoLoi;
    public int    TongDaNhan { get; private set; }

    public void Nhan(DongLog d)
    {
        TongDaNhan++;
        _dong.Enqueue(d);
        // Giới hạn bộ nhớ: bảng log chạy 12 tiếng không giới hạn sẽ ăn hết RAM.
        while (_dong.Count > soDongGiuToiDa) _dong.Dequeue();
    }

    public IReadOnlyList<DongLog> DangHien
        => _dong.Where(d => d.Muc >= MucLoc).ToList();

    public int SoDongDangGiu => _dong.Count;
}

public sealed class BangCanhBaoVM
{
    private readonly List<AlarmException> _dangHoatDong = [];

    public IReadOnlyList<AlarmException> DangHoatDong => _dangHoatDong;
    public bool CoCanhBao => _dangHoatDong.Count > 0;

    public void Phat(AlarmException ex)
    {
        ArgumentNullException.ThrowIfNull(ex);
        if (!_dangHoatDong.Exists(a => a.Ma == ex.Ma)) _dangHoatDong.Add(ex);  // không nhân bản cùng một mã
    }

    public bool XacNhan(int ma) => _dangHoatDong.RemoveAll(a => a.Ma == ma) > 0;
}

// ══════════════ G.5.4 (phụ thuộc của G.7.5) — bảng chuyển trạng thái ══════════════

public enum LenhMay { KhoiTao, VeGocXong, BatDau, TamDung, ChayTiep, Dung, Loi, Reset }

public static class BangChuyen
{
    private static readonly Dictionary<(TrangThaiMay, LenhMay), TrangThaiMay> Bang = new()
    {
        [(TrangThaiMay.ChuaKhoiTao, LenhMay.KhoiTao)]  = TrangThaiMay.DangVeGoc,
        [(TrangThaiMay.DangVeGoc,   LenhMay.VeGocXong)]= TrangThaiMay.SanSang,
        [(TrangThaiMay.DangVeGoc,   LenhMay.Loi)]      = TrangThaiMay.BaoDong,
        [(TrangThaiMay.SanSang,     LenhMay.BatDau)]   = TrangThaiMay.DangChay,
        [(TrangThaiMay.SanSang,     LenhMay.Dung)]     = TrangThaiMay.ChuaKhoiTao,
        [(TrangThaiMay.DangChay,    LenhMay.TamDung)]  = TrangThaiMay.TamDung,
        [(TrangThaiMay.DangChay,    LenhMay.Dung)]     = TrangThaiMay.SanSang,
        [(TrangThaiMay.DangChay,    LenhMay.Loi)]      = TrangThaiMay.BaoDong,
        [(TrangThaiMay.TamDung,     LenhMay.ChayTiep)] = TrangThaiMay.DangChay,
        [(TrangThaiMay.TamDung,     LenhMay.Dung)]     = TrangThaiMay.SanSang,
        [(TrangThaiMay.BaoDong,     LenhMay.Reset)]    = TrangThaiMay.ChuaKhoiTao,
    };

    public static bool ChoPhep(TrangThaiMay tu, LenhMay lenh) => Bang.ContainsKey((tu, lenh));

    public static bool ThuChuyen(TrangThaiMay tu, LenhMay lenh, out TrangThaiMay den)
        => Bang.TryGetValue((tu, lenh), out den);
}

// ══════════════ G.7.5 — màn hình chính: nút khoá theo trạng thái ══════════════

public sealed record TrangThaiNut(bool BatDuoc, string? LyDoMo);

public static class NutManHinhChinh
{
    /// <summary>
    /// Điều kiện bật/tắt nút lấy TỪ CHÍNH bảng chuyển trạng thái — một nguồn sự
    /// thật. Không viết lại điều kiện ở mỗi nút, vì viết lại là quên.
    /// Nút mờ LUÔN kèm lý do: "nút mờ mà không nói vì sao" là lỗi giao diện
    /// gây ức chế nhất cho người vận hành.
    /// </summary>
    public static TrangThaiNut Tinh(TrangThaiMay tt, LenhMay lenh)
    {
        if (BangChuyen.ChoPhep(tt, lenh)) return new TrangThaiNut(true, null);

        string lyDo = tt switch
        {
            TrangThaiMay.BaoDong     => "Máy đang báo động — bấm Reset trước",
            TrangThaiMay.ChuaKhoiTao => "Máy chưa về gốc — bấm Khởi tạo trước",
            TrangThaiMay.DangVeGoc   => "Máy đang về gốc — chờ xong",
            TrangThaiMay.DangChay    => "Máy đang chạy",
            TrangThaiMay.TamDung     => "Máy đang tạm dừng",
            TrangThaiMay.SanSang     => "Không dùng được ở trạng thái Sẵn sàng",
            _                        => "Không dùng được lúc này",
        };
        return new TrangThaiNut(false, lyDo);
    }
}

// ══════════════ Bộ tự kiểm ══════════════

public static class KiemGiaoDien
{
    public static Task Chay()
    {
        // ---------- G.7.1 ----------
        Kiem.MoBai("G.7.1", "ViewModel trạng thái máy");
        var dp = new DieuPhoiTrucTiep();
        var vm = new ManHinhVM(dp);
        var daBao = new List<string>();
        vm.PropertyChanged += (_, e) => daBao.Add(e.PropertyName ?? "");

        vm.CapNhatTrangThai(TrangThaiMay.DangChay);
        Kiem.Bang(vm.TrangThai, TrangThaiMay.DangChay, "trạng thái được cập nhật");
        Kiem.Dung(daBao.Contains("TrangThai"), "có phát PropertyChanged cho TrangThai");
        Kiem.Bang(dp.SoLanGoi, 1, "★ cập nhật ĐI QUA bộ điều phối — chỗ đưa về luồng giao diện");

        daBao.Clear();
        vm.CapNhatTrangThai(TrangThaiMay.DangChay);
        Kiem.Dung(!daBao.Contains("TrangThai"),
                  "gán lại CÙNG giá trị thì KHÔNG phát sự kiện (tránh vẽ lại vô ích)");

        vm.CapNhatSanLuong(tong: 10, ok: 8, ng: 2);
        Kiem.Gan(vm.TyLeDat, 80.0, 1e-9, "tỷ lệ đạt tính đúng");
        Kiem.Dung(daBao.Contains("TyLeDat"), "thuộc tính suy ra cũng được báo đổi");

        // ---------- G.7.2 ----------
        Kiem.MoBai("G.7.2", "Thanh tiến độ bước");
        var vm2 = new ManHinhVM(new DieuPhoiTrucTiep());
        Kiem.Bang(vm2.MoTaTienDo, "Chưa chạy", "chưa chạy thì không hiện 'Bước 0/0'");

        var buocs = ChuKy.BayBuoc();
        vm2.CapNhatBuoc(3, buocs.Count, buocs[2].Ten);
        Kiem.Bang(vm2.MoTaTienDo, "Bước 3/7: Kết luận đạt / không đạt", "mô tả tiến độ đúng khuôn");
        Kiem.Gan(vm2.PhanTramTienDo, 42.9, 0.05, "phần trăm tiến độ 3/7 ≈ 42,9 %");

        var themBuoc = new List<IBuoc>(buocs) { new B7VeCho() };     // giả lập thêm bước thứ 8
        vm2.CapNhatBuoc(8, themBuoc.Count, "Bước mới thêm");
        Kiem.Bang(vm2.MoTaTienDo, "Bước 8/8: Bước mới thêm",
                  "★ thêm bước thứ 8 → giao diện tự hiện 8, KHÔNG sửa mã giao diện");

        // ---------- G.7.3 ----------
        Kiem.MoBai("G.7.3", "Ô nhập số có dải");
        var o = new BoNhapSo(nhoNhat: 1.500, lonNhat: 2.500, donVi: "mm");

        Kiem.Dung(o.Kiem("2.0").HopLe,  "gõ '2.0' (dấu chấm) → hợp lệ");
        Kiem.Dung(o.Kiem("2,0").HopLe,  "★ gõ '2,0' (dấu PHẨY) → cũng hợp lệ");
        Kiem.Gan(o.Kiem("2,0").GiaTri, 2.0, 1e-9, "dấu phẩy cho ra đúng 2,0 — KHÔNG thành 20");
        Kiem.Dung(!o.Kiem("abc").HopLe, "gõ chữ → không hợp lệ");
        Kiem.Dung(!o.Kiem("").HopLe,    "để trống → không hợp lệ");
        Kiem.Dung(!o.Kiem("3.0").HopLe, "vượt dải trên → không hợp lệ");
        Kiem.Dung(o.Kiem("3.0").ThongBao!.Contains("1.5", StringComparison.Ordinal),
                  "thông báo nói rõ dải cho phép");
        Kiem.Dung(o.Kiem("3.0").ThongBao!.Contains("mm", StringComparison.Ordinal),
                  "thông báo kèm ĐƠN VỊ");
        Kiem.Dung(o.ChoPhepKyTu('5', "2."),  "cho gõ chữ số");
        Kiem.Dung(!o.ChoPhepKyTu('a', "2."), "chặn chữ cái ngay lúc gõ");
        Kiem.Dung(!o.ChoPhepKyTu('.', "2.1"),"chặn dấu thập phân THỨ HAI");
        Kiem.Dung(!o.ChoPhepKyTu('-', ""),   "dải không âm → chặn luôn dấu trừ");

        // ---------- G.7.4 ----------
        Kiem.MoBai("G.7.4", "Bảng nhật ký và bảng cảnh báo");
        var bang = new BangLogVM(soDongGiuToiDa: 100);
        var t0 = new DateTime(2026, 9, 20, 8, 0, 0);
        for (int i = 0; i < 250; i++)
            bang.Nhan(new DongLog(t0, i % 5 == 0 ? MucLog.Loi : MucLog.ThongTin, "TRUC_Z", $"dòng {i}"));

        Kiem.Bang(bang.TongDaNhan, 250, "nhận đủ 250 dòng");
        Kiem.Bang(bang.SoDongDangGiu, 100, "★ chỉ GIỮ 100 dòng gần nhất — không ăn hết bộ nhớ");
        bang.MucLoc = MucLog.Loi;
        Kiem.Bang(bang.DangHien.Count, 20, "lọc theo mức Lỗi → 20 dòng trong 100 dòng đang giữ");
        bang.MucLoc = MucLog.GoLoi;
        Kiem.Bang(bang.DangHien.Count, 100, "bỏ lọc → hiện lại đủ 100");

        var bcb = new BangCanhBaoVM();
        Kiem.Dung(!bcb.CoCanhBao, "ban đầu không có cảnh báo");
        bcb.Phat(new AlarmException(MaCanhBao.TrucQuaThoiGian, "TRUC_Z", "quá giờ"));
        bcb.Phat(new AlarmException(MaCanhBao.TrucQuaThoiGian, "TRUC_Z", "quá giờ lần nữa"));
        Kiem.Bang(bcb.DangHoatDong.Count, 1, "cùng một mã phát hai lần → chỉ một dòng (chống lũ cảnh báo)");
        bcb.Phat(new AlarmException(MaCanhBao.CamBienKhongPhanHoi, "CB_DAY", "mất tín hiệu"));
        Kiem.Bang(bcb.DangHoatDong.Count, 2, "mã khác → thêm dòng mới");
        Kiem.Dung(bcb.XacNhan(MaCanhBao.TrucQuaThoiGian), "xác nhận được cảnh báo theo mã");
        Kiem.Bang(bcb.DangHoatDong.Count, 1, "xác nhận xong thì còn 1");
        Kiem.Dung(!bcb.XacNhan(99999), "xác nhận mã không tồn tại → trả false, không ném");

        // ---------- G.7.5 ----------
        Kiem.MoBai("G.7.5", "Màn hình chính — nút khoá theo trạng thái");
        var nutBatDau = NutManHinhChinh.Tinh(TrangThaiMay.SanSang, LenhMay.BatDau);
        Kiem.Dung(nutBatDau.BatDuoc, "Sẵn sàng → nút Bắt đầu BẬT");
        Kiem.Dung(nutBatDau.LyDoMo is null, "nút bật thì không cần lý do");

        var batDauKhiBaoDong = NutManHinhChinh.Tinh(TrangThaiMay.BaoDong, LenhMay.BatDau);
        Kiem.Dung(!batDauKhiBaoDong.BatDuoc, "Báo động → nút Bắt đầu MỜ");
        Kiem.Bang(batDauKhiBaoDong.LyDoMo, "Máy đang báo động — bấm Reset trước",
                  "★ nút mờ LUÔN kèm lý do đọc được");

        Kiem.Dung(NutManHinhChinh.Tinh(TrangThaiMay.BaoDong, LenhMay.Reset).BatDuoc,
                  "Báo động → nút Reset bật");
        Kiem.Dung(!NutManHinhChinh.Tinh(TrangThaiMay.SanSang, LenhMay.TamDung).BatDuoc,
                  "Sẵn sàng → nút Tạm dừng mờ (chưa chạy thì tạm dừng cái gì)");
        Kiem.Dung(NutManHinhChinh.Tinh(TrangThaiMay.DangChay, LenhMay.TamDung).BatDuoc,
                  "Đang chạy → nút Tạm dừng bật");
        Kiem.Dung(NutManHinhChinh.Tinh(TrangThaiMay.TamDung, LenhMay.ChayTiep).BatDuoc,
                  "Tạm dừng → nút Chạy tiếp bật");

        // bảng chuyển trạng thái (G.5.4) — nguồn sự thật cho cả phần nút
        Kiem.Dung(BangChuyen.ThuChuyen(TrangThaiMay.SanSang, LenhMay.BatDau, out var den)
                  && den == TrangThaiMay.DangChay, "Sẵn sàng + Bắt đầu → Đang chạy");
        Kiem.Dung(!BangChuyen.ThuChuyen(TrangThaiMay.BaoDong, LenhMay.BatDau, out _),
                  "★ chuyển sai bị TỪ CHỐI ở bảng, không cần if ở chỗ gọi");

        return Task.CompletedTask;
    }
}
