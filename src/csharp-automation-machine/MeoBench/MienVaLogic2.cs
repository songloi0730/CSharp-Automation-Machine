// -------------------------------------------------------
// File:    MienVaLogic2.cs
// Project: MeoBench
// Purpose: Lời giải mẫu G.1.1, G.1.4, G.2.2, G.2.3, G.2.4, G.2.5.
// -------------------------------------------------------
using System.Globalization;
using System.Text;

namespace MeoBench;

// ══════════════ G.1.1 — đơn vị không lẫn được ══════════════
//
// Ba kiểu, ba vai trò khác nhau:
//   ViTriMm       — một ĐIỂM trên trục (toạ độ tuyệt đối)
//   KhoangCachMm  — một ĐOẠN (hiệu của hai điểm, hoặc lượng dịch chuyển)
//   ChieuDayMm    — một phép ĐO trên phôi
// Cộng hai vị trí là vô nghĩa; vị trí + khoảng cách thì có nghĩa.
// Trình biên dịch phải bắt được điều đó, không để mắt người canh.

public readonly record struct ViTriMm(double GiaTri)
{
    public static ViTriMm Tu(double mm) => new(mm);

    public static ViTriMm operator +(ViTriMm a, KhoangCachMm d) => new(a.GiaTri + d.GiaTri);
    public static ViTriMm operator -(ViTriMm a, KhoangCachMm d) => new(a.GiaTri - d.GiaTri);
    public static KhoangCachMm operator -(ViTriMm a, ViTriMm b) => new(a.GiaTri - b.GiaTri);

    // CA2225: mọi toán tử phải có phương thức tên chữ cho ngôn ngữ .NET khác gọi.
    public static ViTriMm Cong(ViTriMm a, KhoangCachMm d) => a + d;
    public static ViTriMm Tru(ViTriMm a, KhoangCachMm d) => a - d;
    public static KhoangCachMm Hieu(ViTriMm a, ViTriMm b) => a - b;

    public override string ToString() => GiaTri.ToString("F3", CultureInfo.InvariantCulture) + " mm";
}

public readonly record struct KhoangCachMm(double GiaTri)
{
    public static KhoangCachMm Tu(double mm) => new(mm);
    public KhoangCachMm TriTuyetDoi => new(Math.Abs(GiaTri));

    public static KhoangCachMm operator +(KhoangCachMm a, KhoangCachMm b) => new(a.GiaTri + b.GiaTri);
    public static KhoangCachMm Cong(KhoangCachMm a, KhoangCachMm b) => a + b;

    public override string ToString() => GiaTri.ToString("F3", CultureInfo.InvariantCulture) + " mm";
}

public readonly record struct ChieuDayMm(double GiaTri)
{
    public static ChieuDayMm Tu(double mm) => new(mm);

    /// <summary>Lệch bao nhiêu so với một chiều dày khác — trả về KHOẢNG CÁCH, không phải chiều dày.</summary>
    public KhoangCachMm LechSoVoi(ChieuDayMm chuan) => new(GiaTri - chuan.GiaTri);

    public override string ToString() => GiaTri.ToString("F3", CultureInfo.InvariantCulture) + " mm";
}

// ══════════════ G.1.4 — bảng mã cảnh báo có metadata ══════════════

public enum HanhDongKhiCanhBao { ChiGhiNhan, DungCuoiChuKy, DungNgay }

public sealed record ThongTinCanhBao(
    int Ma, string MoTa, string ViecNenLam, HanhDongKhiCanhBao HanhDong)
{
    public string Nhom => (Ma / 10000) switch
    {
        1 => "Chuyển động",
        2 => "Cảm biến",
        3 => "Khí / cơ cấu",
        4 => "Hệ thống",
        5 => "Truyền thông",
        _ => "Không rõ",
    };
}

/// <summary>
/// Tra metadata từ mã số — MỘT chỗ duy nhất, không rải `switch` khắp nơi.
/// Danh mục là dữ liệu tĩnh nên tra cứu là O(1) và không đọc file lúc chạy.
/// </summary>
public static class DanhMucCanhBao
{
    private static readonly Dictionary<int, ThongTinCanhBao> Bang =
        new ThongTinCanhBao[]
        {
            new(MaCanhBao.TrucQuaThoiGian,     "Trục quá thời gian di chuyển",
                "Kiểm tra cơ khí có kẹt không, kiểm tra servo đã bật chưa", HanhDongKhiCanhBao.DungNgay),
            new(MaCanhBao.TrucChuaVeGoc,       "Ra lệnh đi khi trục chưa về gốc",
                "Bấm Khởi tạo để về gốc trước",                            HanhDongKhiCanhBao.DungNgay),
            new(MaCanhBao.CamBienKhongPhanHoi, "Cảm biến chiều dày không phản hồi",
                "Kiểm tra cáp nối tiếp và nguồn cấp cho cảm biến",         HanhDongKhiCanhBao.DungNgay),
            new(MaCanhBao.KepKhongXacNhan,     "Kẹp không lên tín hiệu xác nhận",
                "Kiểm tra áp suất khí và cảm biến kẹp",                    HanhDongKhiCanhBao.DungCuoiChuKy),
        }.ToDictionary(t => t.Ma);

    public static IReadOnlyCollection<ThongTinCanhBao> TatCa => Bang.Values;

    public static ThongTinCanhBao Tra(int ma)
        => Bang.TryGetValue(ma, out var t)
            ? t
            : new ThongTinCanhBao(ma, $"Cảnh báo chưa khai báo (mã {ma})",
                                  "Báo cho bộ phận phần mềm", HanhDongKhiCanhBao.DungNgay);

    public static bool DaKhaiBao(int ma) => Bang.ContainsKey(ma);
}

// ══════════════ G.2.2 — quy đổi mm ↔ xung ══════════════

public sealed class BoQuyDoi
{
    private readonly double _xungMoiMm;

    public BoQuyDoi(double xungMoiMm)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(xungMoiMm);
        _xungMoiMm = xungMoiMm;
    }

    public double XungMoiMm => _xungMoiMm;

    /// <summary>
    /// mm → xung. LÀM TRÒN chứ không cắt: cắt luôn về một phía sẽ tích luỹ sai
    /// số theo một chiều sau hàng nghìn chu kỳ. Làm tròn thì sai số dao động
    /// quanh 0 và không trôi.
    /// </summary>
    public long SangXung(double mm) => (long)Math.Round(mm * _xungMoiMm, MidpointRounding.AwayFromZero);

    public double SangMm(long xung) => xung / _xungMoiMm;
}

// ══════════════ G.2.3 — tách khung từ dòng byte ══════════════

/// <summary>
/// Nhận từng MẢNH byte tuỳ ý, phát ra từng KHUNG hoàn chỉnh giữa STX và ETX.
/// Ba tình huống bắt buộc chịu được: một khung tới làm nhiều mảnh, nhiều khung
/// tới trong một mảnh, và mảnh cụt ở cuối phải được GIỮ LẠI cho lần sau.
/// </summary>
public sealed class BoTachKhung(byte stx = 0x02, byte etx = 0x03, int soByteToiDa = 4096)
{
    private readonly List<byte> _dem = [];
    private bool _dangTrongKhung;

    public int SoByteDangGiu => _dem.Count;
    public int SoKhungBoVi   { get; private set; }   // khung quá dài bị bỏ

    public IReadOnlyList<byte[]> Nap(ReadOnlySpan<byte> manh)
    {
        var ketQua = new List<byte[]>();

        foreach (byte b in manh)
        {
            if (b == stx)
            {
                _dangTrongKhung = true;
                _dem.Clear();            // STX mới → bỏ phần dở dang, khung cũ hỏng
                continue;
            }

            if (!_dangTrongKhung) continue;   // rác ngoài khung: bỏ qua, không tích luỹ

            if (b == etx)
            {
                ketQua.Add([.. _dem]);
                _dem.Clear();
                _dangTrongKhung = false;
                continue;
            }

            _dem.Add(b);

            // Chống phình bộ nhớ khi thiết bị hỏng và không bao giờ gửi ETX.
            if (_dem.Count > soByteToiDa)
            {
                _dem.Clear();
                _dangTrongKhung = false;
                SoKhungBoVi++;
            }
        }

        return ketQua;
    }

    public IReadOnlyList<string> NapVanBan(ReadOnlySpan<byte> manh)
        => [.. Nap(manh).Select(k => Encoding.ASCII.GetString(k))];
}

// ══════════════ G.2.4 — tổng kiểm ══════════════

public static class TongKiem
{
    /// <summary>XOR mọi byte. Bắt được lỗi một bit; không bắt được hoán vị byte.</summary>
    public static byte Xor(ReadOnlySpan<byte> duLieu)
    {
        byte t = 0;
        foreach (byte b in duLieu) t ^= b;
        return t;
    }

    /// <summary>Khung = [dữ liệu…][1 byte tổng kiểm]. Xác minh byte cuối.</summary>
    public static bool XacMinh(ReadOnlySpan<byte> khungCoTongKiem)
    {
        if (khungCoTongKiem.Length < 2) return false;
        var duLieu = khungCoTongKiem[..^1];
        return Xor(duLieu) == khungCoTongKiem[^1];
    }

    public static byte[] Dong(ReadOnlySpan<byte> duLieu)
    {
        var ra = new byte[duLieu.Length + 1];
        duLieu.CopyTo(ra);
        ra[^1] = Xor(duLieu);
        return ra;
    }
}

// ══════════════ G.2.5 — cửa sổ trượt ══════════════

public sealed record ThongKeDo(bool DuMau, double TrungBinh, double DoLech, int SoMau);

public sealed class CuaSoTruotDo
{
    private readonly Queue<double> _mau = new();
    private readonly int _soMauCan;

    public CuaSoTruotDo(int soMauCan)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(soMauCan);
        _soMauCan = soMauCan;
    }

    public void Them(double giaTri)
    {
        _mau.Enqueue(giaTri);
        while (_mau.Count > _soMauCan) _mau.Dequeue();
    }

    public void XoaHet() => _mau.Clear();

    public ThongKeDo Tinh()
    {
        if (_mau.Count < _soMauCan) return new ThongKeDo(false, 0, 0, _mau.Count);

        double tb = _mau.Average();
        double phuongSai = _mau.Sum(v => (v - tb) * (v - tb)) / _mau.Count;
        return new ThongKeDo(true, Math.Round(tb, 4), Math.Round(Math.Sqrt(phuongSai), 4), _mau.Count);
    }
}

// ══════════════ Bộ tự kiểm ══════════════

public static class KiemMienVaLogic2
{
    public static Task Chay()
    {
        // ---------- G.1.1 ----------
        Kiem.MoBai("G.1.1", "Đơn vị không lẫn được");
        var viTri = ViTriMm.Tu(120.0);
        var dich  = KhoangCachMm.Tu(5.0);
        Kiem.Bang(viTri + dich, ViTriMm.Tu(125.0), "vị trí + khoảng cách = vị trí");
        Kiem.Bang(ViTriMm.Tu(125.0) - ViTriMm.Tu(120.0), KhoangCachMm.Tu(5.0),
                  "vị trí − vị trí = KHOẢNG CÁCH, không phải vị trí");
        var lech = ChieuDayMm.Tu(2.12).LechSoVoi(ChieuDayMm.Tu(2.00)).TriTuyetDoi;
        Kiem.Gan(lech.GiaTri, 0.12, 1e-9, "chiều dày lệch so với chuẩn → khoảng cách 0,12 mm");

        // ★ Bẫy thật, và phép kiểm này của sách đã dính nó ở lần chạy đầu:
        //   2.12 - 2.00 = 0.12000000000000011, KHÔNG phải 0.12.
        //   Hai giá trị in ra đều là "0.120 mm" nên thông báo hỏng trông vô lý
        //   ("mong đợi 0.120 mm, thực tế 0.120 mm") — đúng kiểu lỗi làm mất
        //   hàng giờ nếu không biết trước. Đây chính là quy tắc 6 ở Bảng F.3.
        Kiem.Dung(lech != KhoangCachMm.Tu(0.12),
                  "★ so sánh BẰNG NHAU trên số thực TRƯỢT dù hai số 'trông giống nhau'");
        Kiem.Dung(Math.Abs(lech.GiaTri - 0.12) < 1e-9,
                  "★ so theo DUNG SAI thì đúng — luôn so số thực theo dung sai");
        Kiem.Bang(viTri.ToString(), "120.000 mm", "hiển thị dùng dấu chấm bất biến, kèm đơn vị");
        Kiem.Dung(viTri == ViTriMm.Tu(120.0), "hai vị trí cùng giá trị thì bằng nhau");
        Kiem.Dung(!viTri.Equals(ChieuDayMm.Tu(120.0)),
                  "★ 120 mm VỊ TRÍ khác 120 mm CHIỀU DÀY — hai kiểu không so sánh lẫn được");

        // Điều quan trọng nhất của bài này KHÔNG kiểm được lúc chạy, vì nó xảy
        // ra lúc BIÊN DỊCH. Bỏ dấu chú thích hai dòng dưới, chương trình sẽ
        // không biên dịch được — và đó chính là điều bài muốn đạt:
        //   ViTriMm sai1 = 10.0;                       // CS0029
        //   var     sai2 = viTri + ChieuDayMm.Tu(2.0); // CS0019
        Kiem.Dung(true, "(lỗi gán thẳng double và cộng nhầm kiểu bị chặn LÚC BIÊN DỊCH — xem chú thích)");

        // ---------- G.1.4 ----------
        Kiem.MoBai("G.1.4", "Bảng mã cảnh báo có metadata");
        var tt = DanhMucCanhBao.Tra(MaCanhBao.TrucQuaThoiGian);
        Kiem.Bang(tt.Nhom, "Chuyển động", "suy ra nhóm từ dải mã");
        Kiem.Bang(tt.HanhDong, HanhDongKhiCanhBao.DungNgay, "trục quá giờ thì dừng ngay");
        Kiem.Dung(tt.ViecNenLam.Length > 10, "có hướng dẫn việc nên làm cho người vận hành");
        Kiem.Bang(DanhMucCanhBao.Tra(MaCanhBao.CamBienKhongPhanHoi).Nhom, "Cảm biến", "dải 20000 → Cảm biến");
        Kiem.Bang(DanhMucCanhBao.Tra(MaCanhBao.KepKhongXacNhan).HanhDong,
                  HanhDongKhiCanhBao.DungCuoiChuKy, "kẹp lỗi thì dừng cuối chu kỳ, không dừng giữa chừng");
        Kiem.Dung(!DanhMucCanhBao.DaKhaiBao(99999), "mã lạ chưa khai báo");
        var la = DanhMucCanhBao.Tra(99999);
        Kiem.Dung(la.MoTa.Contains("chưa khai báo", StringComparison.Ordinal),
                  "★ mã lạ vẫn tra được, KHÔNG ném — máy không sập vì một mã chưa khai báo");
        Kiem.Bang(la.HanhDong, HanhDongKhiCanhBao.DungNgay,
                  "★ mã lạ mặc định là DỪNG NGAY — chọn phía an toàn khi không biết");
        Kiem.Dung(DanhMucCanhBao.TatCa.Select(t => t.Ma).Distinct().Count() == DanhMucCanhBao.TatCa.Count,
                  "không có mã trùng trong danh mục");

        // ---------- G.2.2 ----------
        Kiem.MoBai("G.2.2", "Quy đổi mm ↔ xung");
        var qd = new BoQuyDoi(xungMoiMm: 1000.0);
        Kiem.Bang(qd.SangXung(25.0), 25000L, "25 mm = 25 000 xung");
        Kiem.Gan(qd.SangMm(25000L), 25.0, 1e-9, "đổi ngược lại ra đúng 25 mm");
        Kiem.Bang(qd.SangXung(-3.5), -3500L, "số âm đổi đúng");
        Kiem.Gan(qd.SangMm(qd.SangXung(12.3456)), 12.3456, 1.0 / 1000.0,
                 "đổi xuôi rồi ngược sai lệch không quá một xung");
        Kiem.Bang(qd.SangXung(0.0005), 1L, "★ làm tròn ra xa 0, không cắt về 0");
        Kiem.Bang(qd.SangXung(-0.0005), -1L, "làm tròn đối xứng cho số âm");
        Kiem.Nem<ArgumentOutOfRangeException>(() => _ = new BoQuyDoi(0).XungMoiMm,
                                              "hệ số 0 bị chặn ngay ở hàm dựng");

        // ---------- G.2.3 ----------
        Kiem.MoBai("G.2.3", "Tách khung từ dòng byte");
        byte STX = 0x02, ETX = 0x03;

        // (1) MỘT khung tới làm BA mảnh
        var bt = new BoTachKhung();
        Kiem.Bang(bt.NapVanBan([STX, (byte)'2']).Count, 0, "mảnh 1/3: chưa đủ khung → chưa phát gì");
        Kiem.Bang(bt.NapVanBan([(byte)'.', (byte)'0']).Count, 0, "mảnh 2/3: vẫn chưa đủ");
        var ra1 = bt.NapVanBan([(byte)'1', ETX]);
        Kiem.Bang(ra1.Count, 1, "mảnh 3/3: phát ra ĐÚNG MỘT khung");
        Kiem.Bang(ra1[0], "2.01", "★ ghép lại đúng nội dung khung bị cắt làm ba");

        // (2) HAI khung trong MỘT mảnh
        var bt2 = new BoTachKhung();
        var ra2 = bt2.NapVanBan([STX, (byte)'1', ETX, STX, (byte)'2', ETX]);
        Kiem.Bang(ra2.Count, 2, "★ hai khung trong một mảnh → phát ra hai khung");
        Kiem.Bang(ra2[0] + "|" + ra2[1], "1|2", "đúng thứ tự và đúng nội dung");

        // (3) mảnh cụt ở cuối phải được GIỮ LẠI
        var bt3 = new BoTachKhung();
        var ra3 = bt3.NapVanBan([STX, (byte)'9', ETX, STX, (byte)'8']);
        Kiem.Bang(ra3.Count, 1, "một khung trọn + một khung dở → phát 1");
        Kiem.Bang(bt3.SoByteDangGiu, 1, "★ phần dư ĐƯỢC GIỮ LẠI, không bị vứt");
        Kiem.Bang(bt3.NapVanBan([(byte)'7', ETX])[0], "87", "★ lần sau ghép tiếp ra đúng '87'");

        // (4) rác ngoài khung và khung quá dài
        var bt4 = new BoTachKhung(soByteToiDa: 8);
        Kiem.Bang(bt4.NapVanBan([(byte)'r', (byte)'á', (byte)'c']).Count, 0, "byte ngoài khung bị bỏ qua");
        bt4.Nap([STX]);
        bt4.Nap(Encoding.ASCII.GetBytes("123456789"));
        Kiem.Bang(bt4.SoKhungBoVi, 1, "★ thiết bị hỏng gửi mãi không có ETX → bỏ khung, không phình bộ nhớ");
        Kiem.Bang(bt4.SoByteDangGiu, 0, "bộ đệm đã được giải phóng");

        // ---------- G.2.4 ----------
        Kiem.MoBai("G.2.4", "Tổng kiểm");
        byte[] duLieu = Encoding.ASCII.GetBytes("2.010");
        byte[] khung  = TongKiem.Dong(duLieu);
        Kiem.Bang(khung.Length, duLieu.Length + 1, "khung = dữ liệu + 1 byte tổng kiểm");
        Kiem.Dung(TongKiem.XacMinh(khung), "khung nguyên vẹn → xác minh đạt");

        byte[] hong = [.. khung];
        hong[2] ^= 0x01;                       // lật đúng MỘT bit
        Kiem.Dung(!TongKiem.XacMinh(hong), "★ lật một bit bất kỳ → xác minh trượt");

        byte[] hong2 = [.. khung];
        (hong2[0], hong2[1]) = (hong2[1], hong2[0]);   // hoán vị hai byte
        Kiem.Dung(TongKiem.XacMinh(hong2),
                  "★ XOR KHÔNG bắt được hoán vị byte — biết giới hạn của công cụ mình dùng");
        Kiem.Dung(!TongKiem.XacMinh([0x01]), "khung quá ngắn → không hợp lệ, không ném");

        // ---------- G.2.5 ----------
        Kiem.MoBai("G.2.5", "Cửa sổ trượt và lọc nhiễu");
        var cs = new CuaSoTruotDo(soMauCan: 4);
        cs.Them(10); cs.Them(10); cs.Them(10);
        Kiem.Dung(!cs.Tinh().DuMau, "chưa đủ 4 mẫu → báo CHƯA ĐỦ, không trả số bừa");
        Kiem.Bang(cs.Tinh().SoMau, 3, "cho biết đang có mấy mẫu");

        cs.Them(50);
        var tk = cs.Tinh();
        Kiem.Dung(tk.DuMau, "đủ 4 mẫu → tính được");
        Kiem.Gan(tk.TrungBinh, 20.0, 1e-9, "[10,10,10,50] → trung bình 20");
        Kiem.Gan(tk.DoLech, 17.3205, 1e-3, "★ độ lệch chuẩn 17,3 — con số TỐ CÁO rằng 50 là bất thường");

        cs.Them(10); cs.Them(10);
        Kiem.Gan(cs.Tinh().TrungBinh, 20.0, 1e-9, "cửa sổ trượt: [10,50,10,10] vẫn trung bình 20");
        cs.Them(10); cs.Them(10);   // cần HAI mẫu nữa mới đẩy được 50 ra khỏi cửa sổ 4 mẫu
        Kiem.Gan(cs.Tinh().TrungBinh, 10.0, 1e-9, "giá trị 50 trôi hẳn ra khỏi cửa sổ → trung bình về 10");
        Kiem.Gan(cs.Tinh().DoLech, 0.0, 1e-9, "độ lệch 0 → bốn mẫu giống nhau");

        return Task.CompletedTask;
    }
}
