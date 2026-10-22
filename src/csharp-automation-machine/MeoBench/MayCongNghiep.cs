// -------------------------------------------------------
// File:    MayCongNghiep.cs
// Project: MeoBench
// Purpose: Mục G.16 — ráp yêu cầu công nghiệp của Phụ lục L vào cỗ máy G.11.
//
// Cỗ máy G.11 đã chạy đúng chu kỳ. File này KHÔNG viết lại vòng chạy; nó bọc
// cỗ máy và cài yêu cầu vào đúng những chỗ mà yêu cầu công nghiệp sống:
//   (1) lúc khởi động: cờ phiên chạy — biết lần trước tắt bất thường (19.5.3)
//   (2) trước nút Bắt đầu: bộ điều kiện chạy, đủ mọi lý do một lần (15.2.3b)
//   (3) ở ranh giới chu kỳ: ổ đĩa, nhật ký, hiệu lực mẫu chuẩn (19.6.3, 13.4.9)
//   (4) trong chu kỳ: một bước chèn thêm — phôi NG phải thật sự rời máy (13.4.9)
//   (5) lúc tắt: đóng cờ phiên ở bước CUỐI CÙNG (19.5.3)
// Cộng thêm: mẫu chuẩn không bị đếm, đèn tháp và còi tính ở một chỗ (15.1.9),
// nhật ký ra tệp không làm sập máy khi ổ đầy (19.6.3), gói chẩn đoán (19.7).
// -------------------------------------------------------
using System.Globalization;
using System.Text;

namespace MeoBench;

/// <summary>Cấu hình riêng của lớp yêu cầu công nghiệp.</summary>
public sealed record CauHinhCongNghiep
{
    /// <summary>Phiên bản phần mềm — ghi vào cờ phiên và gói chẩn đoán.</summary>
    public string   PhienBan          { get; init; } = "1.0.0";

    /// <summary>Thời gian tối đa từ lúc nhả phôi NG tới lúc cảm biến máng thấy nó.</summary>
    public TimeSpan ThoiGianChoPhoiNg { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>Lượng dữ liệu máy ghi mỗi ngày — đo trên máy thật, không đoán (mục 19.6.3).</summary>
    public long     ByteMoiNgay       { get; init; } = 2L * 1024 * 1024 * 1024;
}

/// <summary>
/// Cảm biến ở máng NG, bản giả lập: phôi nhả ra thì 20 ms sau cảm biến sáng trong 50 ms — lúc phôi
/// rơi ngang qua. <see cref="MoPhongPhoiKet"/> giả lập phôi mắc lại trên đường trượt.
/// </summary>
public sealed class MangNgGiaLap(IDongHoDonDieu dongHo)
{
    private const long TreMs = 20, RongMs = 50;
    private long? _lucNha;

    /// <summary>Giả lập phôi mắc trên đường trượt — cảm biến không bao giờ sáng.</summary>
    public bool MoPhongPhoiKet { get; set; }

    /// <summary>Số phôi đã thật sự rơi vào máng.</summary>
    public int SoPhoiDaRoi { get; private set; }

    /// <summary>Tín hiệu cảm biến lúc này.</summary>
    public bool CoPhoi => _lucNha is long t && dongHo.MiliGiay - t is >= TreMs and < TreMs + RongMs;

    /// <summary>Thế giới giả lập nhận tin: phần mềm vừa nhả một phôi phía trên máng.</summary>
    public void PhoiDuocNha()
    {
        if (MoPhongPhoiKet) return;
        _lucNha = dongHo.MiliGiay;
        SoPhoiDaRoi++;
    }
}

/// <summary>
/// Bước chèn sau "Nhả phôi" (mục 13.4.9): phôi NG phải THẬT SỰ rơi vào máng. Canh ngay tại đây,
/// trong chu kỳ, vì việc canh phải bắt đầu đúng lúc nhả — canh muộn thì sườn lên của cảm biến có
/// thể đã trôi qua, và máy báo động oan.
/// </summary>
public sealed class BuocXacNhanPhoiNg(XacNhanBoNG xacNhan, Func<bool> camBienMang,
                                      Action? baoGiaLapDaNha = null) : IBuoc
{
    /// <summary>Mã bước — nằm giữa 60 (Nhả phôi) và 70 (Về vị trí chờ).</summary>
    public const int MaBuoc = 65;

    /// <inheritdoc/>
    public int    Ma  => MaBuoc;

    /// <inheritdoc/>
    public string Ten => "Xác nhận phôi NG rời máy";

    /// <inheritdoc/>
    public async Task ThucThiAsync(BoiCanh bc, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(bc);
        if (bc.KetQua.Dat) return;                      // phôi đạt vào máng OK — không canh

        xacNhan.BatDauCho($"P{bc.SoHieuPhoi}");
        baoGiaLapDaNha?.Invoke();                       // CHỈ bản giả lập; máy thật để null
        while (xacNhan.CapNhat(camBienMang()) == TrangThaiBoNG.ChoXacNhan)
            await Task.Delay(2, ct).ConfigureAwait(false);

        if (xacNhan.TrangThai == TrangThaiBoNG.QuaHan)
            throw new AlarmException(MaCanhBao.PhoiNgKhongRoiMay, "MANG_NG",
                $"phôi {xacNhan.MaPhoi} ({bc.KetQua.KetLuan}, {bc.KetQua.ChieuDayMm:F3} mm) không thấy rơi vào máng "
                + $"sau {xacNhan.ThoiGianCho.TotalSeconds:0.#} s");
    }
}

/// <summary>Cửa ra nhật ký ghi ra tệp qua <see cref="GhiKhongSap"/> — ổ đầy thì ngừng ghi, không ném.</summary>
public sealed class CuaRaTepKhongSap(GhiKhongSap ghi) : ICuaRaLog
{
    /// <inheritdoc/>
    public void Nhan(BanGhiLog ban)
    {
        ArgumentNullException.ThrowIfNull(ban);
        ghi.Ghi(string.Create(CultureInfo.InvariantCulture,
            $"{ban.ThoiDiem:yyyy-MM-dd HH:mm:ss.fff} [{ban.Muc}] {ban.Nguon}: {ban.DungCau()}"));
    }
}

/// <summary>Cỗ máy G.11 cộng các yêu cầu công nghiệp của Phụ lục L.</summary>
public sealed class MayCongNghiep : IDisposable
{
    /// <summary>Mã điều kiện "đã qua mẫu chuẩn" — chạy mẫu chuẩn thì bỏ qua đúng điều kiện này.</summary>
    public const string MaDieuKienMauChuan = "DL-01";

    private readonly CauHinhHoanChinh  _ch;
    private readonly CauHinhCongNghiep _cn;
    private readonly IDongHo           _dongHo;
    private readonly DenThapVaCoi      _denCoi = new();
    private readonly GhiKhongSap       _ghiTep;
    private bool _canXacNhanSauSuCo;
    private bool _daHuy;

    /// <summary>Ráp cỗ máy. <paramref name="ghiTepNhatKy"/> để null thì ghi ra tệp thật.</summary>
    public MayCongNghiep(CauHinhHoanChinh ch, CauHinhCongNghiep cn, IDongHo dongHo,
                         IDongHoDonDieu dongHoDonDieu, IAnToanChiDoc anToan, CongThuc congThuc,
                         Func<double> docApSuat, IThongTinODia oDia, Action<string>? ghiTepNhatKy = null)
    {
        ArgumentNullException.ThrowIfNull(ch);
        ArgumentNullException.ThrowIfNull(cn);
        ArgumentNullException.ThrowIfNull(dongHo);
        ArgumentNullException.ThrowIfNull(dongHoDonDieu);
        ArgumentNullException.ThrowIfNull(anToan);
        ArgumentNullException.ThrowIfNull(docApSuat);
        ArgumentNullException.ThrowIfNull(oDia);
        _ch = ch; _cn = cn; _dongHo = dongHo;

        May = new MayHoanChinh(ch, dongHo, anToan, congThuc, docApSuat);

        // Nhật ký ra TỆP — cửa ra thứ ba của cùng đường ống; ổ đầy thì ngừng ghi, máy không sập
        ThuMucNhatKy = Path.Combine(ch.ThuMucDuLieu, "nhatky");
        _ghiTep = new GhiKhongSap(ghiTepNhatKy ?? GhiDongVaoTep);
        May.NhatKy.ThemCuaRa(new CuaRaTepKhongSap(_ghiTep));

        Phien    = new PhienChay(Path.Combine(ch.ThuMucDuLieu, "phien_chay.co"), dongHo);
        MauChuan = new CongMauChuan(dongHo);
        ODia     = new CanhODia(oDia) { ByteMoiNgay = cn.ByteMoiNgay };
        DangNhap = new PhienDangNhap(dongHo);
        BoNG     = new XacNhanBoNG(dongHoDonDieu) { ThoiGianCho = cn.ThoiGianChoPhoiNg };
        MangNg   = new MangNgGiaLap(dongHoDonDieu);

        // (4) Chèn bước canh phôi NG ngay sau "Nhả phôi" (mã 60) — vòng chạy không phải sửa
        May.ThemBuocSau(60, new BuocXacNhanPhoiNg(BoNG, () => MangNg.CoPhoi, MangNg.PhoiDuocNha));

        // (3) Cổng ở ranh giới chu kỳ
        May.CongTruocChuKy = XetTruocChuKy;

        // (2) Bộ điều kiện chạy — khai báo MỘT lần; nút Bắt đầu và màn hình cùng hỏi nó
        DieuKien = new BoDieuKienChay();
        DieuKien.Them("AT-01", NhomDieuKien.AnToan, "Nút dừng khẩn đang nhấn",
                      "Nhả nút dừng khẩn rồi reset mạch an toàn", () => !anToan.DungKhanDangNhan);
        DieuKien.Them("AT-02", NhomDieuKien.AnToan, "Màn chắn sáng đang bị che",
                      "Đưa tay và vật ra khỏi vùng màn chắn", () => !anToan.ManChanBiChe);
        DieuKien.Them("TB-01", NhomDieuKien.ThietBi, "Máy chưa ở trạng thái Sẵn sàng",
                      "Xác nhận cảnh báo, Reset, rồi Khởi tạo để về gốc",
                      () => May.TrangThai == TrangThaiMay.SanSang);
        DieuKien.Them("TB-02", NhomDieuKien.ThietBi, "Áp suất khí nén dưới ngưỡng",
                      "Kiểm nguồn khí và van điều áp", () => docApSuat() >= ch.ApSuatNguongBar);
        DieuKien.Them("TB-03", NhomDieuKien.ThietBi, "Phôi NG trước chưa được xác nhận đã rời máy",
                      "Tìm phôi trên đường trượt, lấy ra, rồi bấm Xác nhận phôi NG", () => BoNG.ChoChuKyMoi);
        DieuKien.Them("TB-04", NhomDieuKien.ThietBi, "Giấy phép thư viện hãng không dùng được",
                      "Kiểm khoá cứng và hạn giấy phép (mục 17.3.9)",
                      () => XetGiayPhep.Xet(GiayPhep, dongHo.BayGio).Muc != MucGiayPhep.KhongDung);
        DieuKien.Them(MaDieuKienMauChuan, NhomDieuKien.DuLieu, "Chưa qua mẫu chuẩn, hoặc kết quả đã hết hiệu lực",
                      "Chạy đủ các mẫu chuẩn", () => MauChuan.ChoPhepSanXuat().ChoPhep);
        DieuKien.Them("DL-02", NhomDieuKien.DuLieu, "Ổ dữ liệu sắp đầy, hoặc nhật ký đã ngừng ghi vì ổ đầy",
                      "Giải phóng ổ dữ liệu, rồi bấm Ghi nhật ký lại nếu nhật ký đã ngừng", ODiaChoNhanPhoi);
        DieuKien.Them("ND-01", NhomDieuKien.NguoiDung, "Chưa đăng nhập",
                      "Đăng nhập mức Vận hành trở lên", () => DangNhap.MucHienTai() >= MucNguoiDung.VanHanh);
        DieuKien.Them("ND-02", NhomDieuKien.NguoiDung, "Lần trước máy tắt bất thường, chưa ai xác nhận đã kiểm máy",
                      "Kiểm phôi còn trong máy rồi bấm Xác nhận đã kiểm", () => !_canXacNhanSauSuCo);
    }

    /// <summary>Cỗ máy G.11 bên trong.</summary>
    public MayHoanChinh   May      { get; }
    /// <summary>Cờ phiên chạy.</summary>
    public PhienChay      Phien    { get; }
    /// <summary>Bộ điều kiện chạy.</summary>
    public BoDieuKienChay DieuKien { get; }
    /// <summary>Cổng mẫu chuẩn.</summary>
    public CongMauChuan   MauChuan { get; }
    /// <summary>Canh ổ dữ liệu.</summary>
    public CanhODia       ODia     { get; }
    /// <summary>Người đang đăng nhập.</summary>
    public PhienDangNhap  DangNhap { get; }
    /// <summary>Theo dõi phôi NG đã rời máy chưa.</summary>
    public XacNhanBoNG    BoNG     { get; }
    /// <summary>Cảm biến máng NG giả lập.</summary>
    public MangNgGiaLap   MangNg   { get; }
    /// <summary>Thư mục tệp nhật ký.</summary>
    public string         ThuMucNhatKy { get; }

    /// <summary>Giấy phép thư viện hãng — máy thật đọc từ API của hãng (mục 17.3.9).</summary>
    public TrangThaiGiayPhep GiayPhep { get; set; } = new("thư viện đo (giả lập)", CoMat: true, HetHan: null);

    /// <summary>Lần khởi động này phát hiện lần trước không tắt đúng trình tự.</summary>
    public bool LanTruocTatBatThuong { get; private set; }

    /// <summary>Số dòng nhật ký không ghi được ra tệp từ lúc ổ đầy.</summary>
    public int SoDongNhatKyBoMat => _ghiTep.SoDongBoMat;

    private NhatKy NhatKy => May.NhatKy;

    // ── (1) Khởi động ──

    /// <summary>Gọi MỘT lần, sớm nhất có thể — ngay khi nhật ký đã có chỗ ghi.</summary>
    public KetQuaMoPhien KhoiDong()
    {
        var kq = Phien.Mo(Environment.ProcessId, _cn.PhienBan);
        LanTruocTatBatThuong = kq.LanTruocTatBatThuong;
        _canXacNhanSauSuCo   = kq.LanTruocTatBatThuong;
        if (kq.LanTruocTatBatThuong)
            NhatKy.Ghi(MucLog.CanhBao, "PHIEN",
                       "Lần trước máy tắt bất thường (phiên mở lúc {LucMo}) — phải kiểm máy trước khi chạy",
                       ("LucMo", kq.PhienTruoc?.BatDauLuc));
        else
            NhatKy.Ghi(MucLog.ThongTin, "PHIEN", "Khởi động phiên bản {PhienBan}", ("PhienBan", _cn.PhienBan));
        BaoTruoc();
        return kq;
    }

    /// <summary>
    /// Về gốc. Sau một lần tắt bất thường thì CHƯA được cho trục chạy khi chưa có người xác nhận đã
    /// kiểm máy: phôi còn kẹt trong tay kẹp có thể rơi ngay ở lần chuyển động đầu tiên (mục 15.2.6).
    /// </summary>
    public async Task<KetQuaKiemQuyen> KhoiTaoAsync(CancellationToken ct = default)
    {
        if (_canXacNhanSauSuCo)
        {
            const string lyDo = "Lần trước máy tắt bất thường — kiểm máy và xác nhận trước khi cho trục chạy";
            NhatKy.Ghi(MucLog.CanhBao, "PHIEN", "Từ chối về gốc: {LyDo}", ("LyDo", lyDo));
            return new KetQuaKiemQuyen(false, lyDo);
        }
        await May.KhoiTaoAsync(ct).ConfigureAwait(false);
        return new KetQuaKiemQuyen(true, null);
    }

    // ── (2) Nút Bắt đầu ──

    /// <summary>Trả về MỌI lý do chưa chạy được; danh sách rỗng nghĩa là máy đã chạy.</summary>
    public async Task<IReadOnlyList<LyDoChuaChay>> BatDauAsync(int soChuKy, CancellationToken ct = default)
    {
        BaoTruoc();
        var chua = DieuKien.XetTatCa();
        if (chua.Count > 0)
        {
            foreach (var l in chua)
                NhatKy.Ghi(MucLog.CanhBao, "DIEUKIEN", "Chưa chạy được [{Ma}] {MoTa} — {CachKhacPhuc}",
                           ("Ma", l.Ma), ("MoTa", l.MoTa), ("CachKhacPhuc", l.CachKhacPhuc));
            return chua;
        }
        NhatKy.Ghi(MucLog.ThongTin, "MAY", "{Nguoi} bấm Bắt đầu", ("Nguoi", DangNhap.Ten));
        await May.ChayAsync(soChuKy, ct).ConfigureAwait(false);
        return [];
    }

    /// <summary>Chạy MỘT mẫu chuẩn đã đặt lên máy. Mẫu đi đủ chu kỳ nhưng không được đếm.</summary>
    public async Task<KetQuaChayMau> ChayMauChuanAsync(string maMau, CancellationToken ct = default)
    {
        var chua = DieuKien.XetTatCa().Where(l => l.Ma != MaDieuKienMauChuan).ToList();
        if (chua.Count > 0)
            return new KetQuaChayMau(maMau, false, "Chưa chạy được: " + string.Join("; ", chua.Select(l => l.MoTa)));

        May.CheDoMauChuan = true;
        try { await May.ChayAsync(1, ct).ConfigureAwait(false); }
        finally { May.CheDoMauChuan = false; }

        if (May.KetQuaGanNhat is not KetQuaDo kq)
            return new KetQuaChayMau(maMau, false, $"Mẫu chưa đi hết chu kỳ — máy ở trạng thái {May.TrangThai}");

        var ketQua = MauChuan.GhiKetQua(maMau, kq.Dat, kq.Dat ? null : kq.KetLuan.ToString());
        NhatKy.Ghi(ketQua.KhopMongDoi ? MucLog.ThongTin : MucLog.CanhBao, "MAUCHUAN",
                   "Mẫu {Ma}: {GhiChu}", ("Ma", maMau), ("GhiChu", ketQua.GhiChu));
        return ketQua;
    }

    // ── (3) Ranh giới chu kỳ — chỉ những thứ có thể đổi GIỮA CA ──

    private KetQuaKiemQuyen XetTruocChuKy()
    {
        if (!ODiaChoNhanPhoi())
            return new KetQuaKiemQuyen(false, "Ổ dữ liệu sắp đầy hoặc nhật ký đã ngừng ghi — ngừng nhận phôi mới");
        if (!May.CheDoMauChuan)
        {
            var mau = MauChuan.ChoPhepSanXuat();       // hết hiệu lực giữa ca thì dừng ở đây
            if (!mau.ChoPhep) return mau;
        }
        return new KetQuaKiemQuyen(true, null);
    }

    private bool ODiaChoNhanPhoi()
        => !_ghiTep.DangNgung && ODia.Xet(_ch.ThuMucDuLieu).Muc != MucODia.NgungNhanPhoi;

    /// <summary>Báo trước những thứ CHƯA chặn máy nhưng sắp chặn: ổ đĩa vơi dần, giấy phép sắp hết.</summary>
    private void BaoTruoc()
    {
        var od = ODia.Xet(_ch.ThuMucDuLieu);
        if (od.Muc != MucODia.Du)
            NhatKy.Ghi(MucLog.CanhBao, "ODIA", "{ThongBao}", ("ThongBao", od.ThongBao));
        var gp = XetGiayPhep.Xet(GiayPhep, _dongHo.BayGio);
        if (gp.Muc != MucGiayPhep.Tot)
            NhatKy.Ghi(MucLog.CanhBao, "GIAYPHEP", "{ThongBao}", ("ThongBao", gp.ThongBao));
    }

    /// <summary>Ổ đã có chỗ: ghi nhật ký ra tệp lại, và ghi rõ đã mất bao nhiêu dòng.</summary>
    public void GhiNhatKyLai()
    {
        if (!_ghiTep.DangNgung) return;
        _ghiTep.GhiLai();
        NhatKy.Ghi(MucLog.CanhBao, "ODIA", "Ghi nhật ký ra tệp lại; {SoDong} dòng đã mất trong lúc ổ đầy",
                   ("SoDong", _ghiTep.SoDongBoMat));
    }

    // ── Cảnh báo, đèn tháp và còi (mục 15.1.9) ──

    /// <summary>Đèn tháp và còi lúc này — tính ở MỘT chỗ từ trạng thái máy và cảnh báo chưa xác nhận.</summary>
    public TinHieuThap DenVaCoi() => _denCoi.Tinh(May.TrangThai, DanhSachChuaXacNhan());

    /// <summary>Tắt còi: làm im các cảnh báo đang có — KHÔNG xác nhận chúng.</summary>
    public void TatCoi()
    {
        _denCoi.TatCoi(DanhSachChuaXacNhan());
        NhatKy.Ghi(MucLog.ThongTin, "CANHBAO", "Tắt còi ({SoCanhBao} cảnh báo vẫn chờ xác nhận)",
                   ("SoCanhBao", May.BangCanhBao.DangHoatDong.Count));
    }

    /// <summary>Xác nhận một cảnh báo — lần sau mã này xuất hiện lại thì còi kêu lại.</summary>
    public bool XacNhanCanhBao(int ma)
    {
        bool xong = May.XacNhanCanhBao(ma);
        if (xong) _denCoi.Quen(ma.ToString(CultureInfo.InvariantCulture));
        return xong;
    }

    private List<CanhBaoChuaXacNhan> DanhSachChuaXacNhan()
        => May.BangCanhBao.DangHoatDong
              .Select(a => new CanhBaoChuaXacNhan(a.Ma.ToString(CultureInfo.InvariantCulture), MucUuTien(a.Ma)))
              .ToList();

    private static MucUuTienCanhBao MucUuTien(int ma) => DanhMucCanhBao.Tra(ma).HanhDong switch
    {
        HanhDongKhiCanhBao.DungNgay      => MucUuTienCanhBao.Cao,
        HanhDongKhiCanhBao.DungCuoiChuKy => MucUuTienCanhBao.Vua,
        _                                => MucUuTienCanhBao.Thap,
    };

    // ── Xác nhận có tên ──

    /// <summary>Quá hạn chờ phôi NG thì người vận hành phải tự tìm phôi — và xác nhận CÓ TÊN.</summary>
    public KetQuaKiemQuyen XacNhanPhoiNgBangTay()
    {
        var q = DangNhap.Kiem(MucNguoiDung.VanHanh, "xác nhận phôi NG");
        if (!q.ChoPhep) return q;
        if (BoNG.TrangThai != TrangThaiBoNG.QuaHan)
            return new KetQuaKiemQuyen(false, "Không có phôi NG nào đang chờ xác nhận tay");
        BoNG.XacNhanTay(DangNhap.Ten);
        NhatKy.Ghi(MucLog.CanhBao, "MANG_NG", "{Nguoi} xác nhận tay: phôi {Ma} đã được lấy khỏi đường trượt",
                   ("Nguoi", DangNhap.Ten), ("Ma", BoNG.MaPhoi));
        return q;
    }

    /// <summary>Sau một lần tắt bất thường: người vận hành kiểm máy rồi xác nhận — có tên (mục 15.2.6).</summary>
    public KetQuaKiemQuyen XacNhanDaKiemSauSuCo()
    {
        var q = DangNhap.Kiem(MucNguoiDung.VanHanh, "xác nhận đã kiểm máy");
        if (!q.ChoPhep) return q;
        _canXacNhanSauSuCo = false;
        NhatKy.Ghi(MucLog.CanhBao, "PHIEN", "{Nguoi} xác nhận đã kiểm máy sau lần tắt bất thường",
                   ("Nguoi", DangNhap.Ten));
        return q;
    }

    // ── Gói chẩn đoán (mục 19.7) ──

    /// <summary>Một nút: gom nhật ký, công thức và thông tin máy thành một tệp zip.</summary>
    public KetQuaGoiChanDoan TaoGoiChanDoan(string duongDanZip)
    {
        string thongTin = string.Join(Environment.NewLine,
            "Phiên bản: " + _cn.PhienBan,
            "Trạng thái: " + May.TrangThai,
            "Công thức: " + May.CongThuc.Ten,
            "Lần trước tắt bất thường: " + (LanTruocTatBatThuong ? "có" : "không"));
        var tepNhatKy  = Directory.Exists(ThuMucNhatKy) ? Directory.GetFiles(ThuMucNhatKy, "*.log") : [];
        var tepCauHinh = _ch.DuongDanCongThuc is { } ct && File.Exists(ct) ? new[] { ct } : [];
        var kq = new GoiChanDoan(_dongHo).Tao(duongDanZip, thongTin, tepNhatKy, tepCauHinh);
        NhatKy.Ghi(MucLog.ThongTin, "CHANDOAN", "Tạo gói chẩn đoán {Tep} ({SoTep} tệp)",
                   ("Tep", Path.GetFileName(duongDanZip)), ("SoTep", kq.SoTep));
        return kq;
    }

    // ── (5) Tắt máy ──

    /// <summary>Phần tắt máy của G.11 trước, rồi đóng cờ phiên ở bước CUỐI CÙNG.</summary>
    public void TatMay()
    {
        if (BoNG.TrangThai is TrangThaiBoNG.ChoXacNhan or TrangThaiBoNG.QuaHan)
            NhatKy.Ghi(MucLog.CanhBao, "MANG_NG", "Tắt máy khi phôi {Ma} chưa được xác nhận đã rời máy",
                       ("Ma", BoNG.MaPhoi));
        if (_ghiTep.SoDongBoMat > 0)
            NhatKy.Ghi(MucLog.CanhBao, "ODIA", "{SoDong} dòng nhật ký đã không ghi được ra tệp",
                       ("SoDong", _ghiTep.SoDongBoMat));
        May.TatMay();
        NhatKy.Ghi(MucLog.ThongTin, "PHIEN", "Đóng phiên — tắt đúng trình tự");
        Phien.DongBinhThuong();    // BƯỚC CUỐI: chết sau dòng này mới không còn là "tắt bất thường"
    }

    /// <summary>
    /// Trả tài nguyên. Dispose KHÔNG phải tắt máy: không gọi <see cref="TatMay"/> thì cờ phiên còn
    /// nguyên — đúng điều lần khởi động sau cần biết.
    /// </summary>
    public void Dispose()
    {
        if (_daHuy) return;
        May.Dispose();
        _daHuy = true;
    }

    private void GhiDongVaoTep(string dong)
    {
        Directory.CreateDirectory(ThuMucNhatKy);
        string tep = Path.Combine(ThuMucNhatKy,
            string.Create(CultureInfo.InvariantCulture, $"NhatKy_{_dongHo.BayGio:yyyyMMdd}.log"));
        File.AppendAllText(tep, dong + Environment.NewLine, Encoding.UTF8);
    }
}
