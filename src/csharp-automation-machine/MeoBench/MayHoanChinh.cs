// -------------------------------------------------------
// File:    MayHoanChinh.cs
// Project: MeoBench
// Purpose: GHÉP THẬT SỰ — nối mọi thứ 40 bài sinh ra thành một cỗ máy.
//
// Vì sao cần file này: đo trên chính dự án cho thấy 40 bài sinh ra 82 kiểu
// công khai, nhưng bản ghép ở G.8.5 chỉ dùng tới 19. Sáu mươi ba kiểu còn lại
// "sống trong phép kiểm" mà chưa bao giờ vào máy. Từng mảnh chạy đúng KHÔNG
// có nghĩa là cỗ máy chạy đúng — đúng điều Chương 7 mục 7.4 nói: cái khó nằm
// ở chỗ các mảnh gặp nhau.
//
// File này cũng bổ sung BA thứ mà cả 40 bài chưa đụng tới nhưng máy thật nào
// cũng cần:
//   (1) đường ra khỏi trạng thái báo động: xác nhận cảnh báo rồi Reset
//   (2) tín hiệu an toàn CHỈ ĐỌC và việc khoá lệnh theo nó (mục 15.2.2b)
//   (3) tắt máy sạch: chốt số liệu ca, đẩy nốt nhật ký, trả tài nguyên
// -------------------------------------------------------
using System.Globalization;

namespace MeoBench;

// ══════════════ (2) Tín hiệu an toàn — phần mềm CHỈ ĐỌC ══════════════

/// <summary>
/// Phần mềm KHÔNG thực hiện chức năng an toàn; nó chỉ ĐỌC kết quả của mạch an
/// toàn phần cứng để hiển thị, ghi log và khoá lệnh (mục 15.2.2b, vai trò 1–4).
/// Không có phương thức nào đặt các tín hiệu này — cố ý.
/// </summary>
public interface IAnToanChiDoc
{
    bool DungKhanDangNhan { get; }
    bool ManChanBiChe     { get; }
}

public sealed class AnToanGiaLap : IAnToanChiDoc
{
    public bool DungKhanDangNhan { get; set; }   // set chỉ dùng khi giả lập
    public bool ManChanBiChe     { get; set; }
}

// ══════════════ Cỗ máy hoàn chỉnh ══════════════

public enum NguonCamBien { GiaLap, NoiTiep }

public sealed record CauHinhHoanChinh
{
    public CauHinhMay    May             { get; init; } = new();
    public string        ThuMucDuLieu    { get; init; } = "";
    public string        MaCa            { get; init; } = "CA-A";
    public double        ApSuatNguongBar { get; init; } = 5.00;

    /// <summary>Đổi MỘT dòng này là đổi cả nguồn dữ liệu đo — đó là chỗ interface trả công.</summary>
    public NguonCamBien  NguonCamBien    { get; init; } = NguonCamBien.GiaLap;

    /// <summary>Để trống thì dùng công thức truyền thẳng vào; có đường dẫn thì NẠP TỪ FILE.</summary>
    public string?       DuongDanCongThuc{ get; init; }
}

/// <summary>
/// Nguồn byte giả lập một cảm biến RS-232 thật: sinh khung STX + số + tổng kiểm
/// + ETX, rồi CẮT NGẪU NHIÊN thành nhiều mảnh — đúng cách cổng nối tiếp thật
/// giao dữ liệu (mục 14.1.5b).
/// </summary>
public sealed class NguonNoiTiepGiaLap(int hatGiong, int soLanDoMoiPhoi = 3, double tamMm = 2.000)
    : INguonByte
{
    private readonly Random _r = new(hatGiong);
    private readonly Queue<byte> _cho = new();
    private int    _soKhungDaGui;
    private int    _phoiHienTai = -1;
    private double _chieuDayThatCuaPhoi;

    public Task<byte[]> DocManhAsync(CancellationToken ct = default)
    {
        if (_cho.Count == 0)
        {
            // MỘT PHÔI CÓ MỘT CHIỀU DÀY THẬT — mỗi khung chỉ khác nhau bằng nhiễu.
            // Quay xúc xắc "phôi dày" theo TỪNG KHUNG là sai mô hình: ba lần đo
            // cùng một phôi sẽ trung bình mất khuyết tật đi. Đây đúng là lỗi đã
            // ghi ở mục G.10.3 — và nó tái phát ở đây khi viết nguồn nối tiếp.
            int phoi = _soKhungDaGui / soLanDoMoiPhoi;
            if (phoi != _phoiHienTai)
            {
                _phoiHienTai = phoi;
                _chieuDayThatCuaPhoi = tamMm + (_r.NextDouble() < 0.15 ? 0.120 : 0.0);
            }
            _soKhungDaGui++;

            double v = _chieuDayThatCuaPhoi + (_r.NextDouble() - 0.5) * 0.004;
            byte[] than = System.Text.Encoding.ASCII.GetBytes(
                Math.Round(v, 4).ToString("F4", CultureInfo.InvariantCulture));
            _cho.Enqueue(0x02);
            foreach (byte b in than) _cho.Enqueue(b);
            _cho.Enqueue(TongKiem.Xor(than));
            _cho.Enqueue(0x03);
        }
        int lay = Math.Min(_cho.Count, _r.Next(1, 5));      // cắt thành mảnh 1–4 byte
        var manh = new byte[lay];
        for (int i = 0; i < lay; i++) manh[i] = _cho.Dequeue();
        return Task.FromResult(manh);
    }
}

public sealed class MayHoanChinh : IDisposable
{
    private readonly CauHinhHoanChinh _ch;
    private readonly IDongHo          _dongHo;
    private readonly IAnToanChiDoc    _anToan;
    private readonly VaoRaGiaLap      _io;
    private readonly BoiCanh          _bc;
    private readonly IReadOnlyList<IBuoc> _buocs;

    private SemaphoreSlim _congTamDung = new(1, 1);
    private bool _daHuy;

    public MayHoanChinh(CauHinhHoanChinh ch, IDongHo dongHo, IAnToanChiDoc anToan,
                        CongThuc congThuc, Func<double> docApSuat)
    {
        ArgumentNullException.ThrowIfNull(ch);
        ArgumentNullException.ThrowIfNull(dongHo);
        ArgumentNullException.ThrowIfNull(anToan);
        ArgumentNullException.ThrowIfNull(congThuc);

        // Nạp công thức TỪ FILE nếu cấu hình có đường dẫn — file hỏng thì
        // dùng công thức truyền vào làm dự phòng và ghi rõ lý do (G.6.2).
        string? lyDoCongThuc = null;
        if (!string.IsNullOrWhiteSpace(ch.DuongDanCongThuc))
            congThuc = KhoCongThuc.Nap(ch.DuongDanCongThuc, congThuc, out lyDoCongThuc);

        var loiCt = congThuc.KiemTra();
        if (loiCt.Count > 0)
            throw new ArgumentException("Công thức sai: " + string.Join("; ", loiCt), nameof(congThuc));

        _ch = ch; _dongHo = dongHo; _anToan = anToan;
        CongThuc = congThuc;

        // ── Nhật ký có cấu trúc (G.8.2) + hai cửa ra: bộ nhớ và bảng giao diện
        NhatKy    = new NhatKy(dongHo);
        CuaRaBoNho = new CuaRaBoNho();
        BangLog    = new BangLogVM(soDongGiuToiDa: 2000);
        NhatKy.ThemCuaRa(CuaRaBoNho);
        NhatKy.ThemCuaRa(new CuaRaNoiVaoBang(BangLog));

        if (lyDoCongThuc is not null)
            NhatKy.Ghi(MucLog.CanhBao, "CONGTHUC", "{LyDo}", ("LyDo", lyDoCongThuc));
        NhatKy.Ghi(MucLog.ThongTin, "CONGTHUC", "Dùng công thức {Ten}, dải {Duoi}–{Tren} mm",
                   ("Ten", congThuc.Ten), ("Duoi", congThuc.GioiHanDuoiMm), ("Tren", congThuc.GioiHanTrenMm));

        // ── Vào-ra theo TÊN (G.3.3) + kẹp CÓ xác nhận (G.4.2)
        var banDo = new BanDoTinHieu(
            vao: new Dictionary<string, int> { [CumKep.TinHieuVaoDaKep] = 3, ["CB_PHOI"] = 0 },
            ra:  new Dictionary<string, int> { [CumKep.TinHieuRaKep] = 1, ["DEN_XANH"] = 2 });
        var loiBanDo = banDo.KiemTra();
        if (loiBanDo.Count > 0)
            throw new ArgumentException("Bản đồ tín hiệu sai: " + string.Join("; ", loiBanDo));
        _io = new VaoRaGiaLap(banDo);
        var kep = new KepKemGiaLap(_io, hanGioMs: 500, nhipHoiMs: 2);

        // ── Thiết bị + quy đổi (G.2.2) + cảm biến có hạn giờ (G.3.2)
        QuyDoi   = new BoQuyDoi(xungMoiMm: 1000.0);
        TrucZ    = new TrucGiaLap("Z", mmMoiBuoc: 10.0);
        TrucX    = new TrucGiaLap("X", mmMoiBuoc: 40.0);
        // ĐÂY là chỗ interface trả công: đổi một dòng cấu hình thì cả cỗ máy
        // chuyển từ cảm biến giả lập sang driver nối tiếp thật, không lớp nào
        // phía trên phải sửa một chữ.
        CamBien = new CamBienGiaLap(ch.May.HatGiongGiaLap, congThuc.SoLanDoMoiPhoi);
        ICamBienChieuDay nguonDo = ch.NguonCamBien switch
        {
            NguonCamBien.NoiTiep => new DriverCamBienNoiTiep(
                                        new NguonNoiTiepGiaLap(ch.May.HatGiongGiaLap, congThuc.SoLanDoMoiPhoi)),
            _                    => CamBien,
        };
        var cbCoHanGio = new CamBienCoHanGio(nguonDo, "CB_DAY", hanGioMs: 800);

        var cdZ = new ChuyenDong(TrucZ, -1.0,  60.0, ch.May.HanGioTrucMs);
        var cdX = new ChuyenDong(TrucX, -1.0, 400.0, ch.May.HanGioTrucMs);

        _bc = new BoiCanh
        {
            TrucZ = cdZ, TrucX = cdX, Kep = kep,
            CumDo = new CumDo(cdZ, cbCoHanGio, dongHo,
                              ch.May.ViTriDoMm, ch.May.ViTriAnToanMm,
                              congThuc.GioiHanDuoiMm, congThuc.GioiHanTrenMm,
                              congThuc.SoLanDoMoiPhoi),
            MangOkMm = ch.May.MangOkMm, MangNgMm = ch.May.MangNgMm, ChoMm = ch.May.ViTriChoMm,
        };
        _buocs = ChuKy.BayBuoc();

        // ── Dữ liệu sản xuất (G.6.3, G.6.4) + cửa sổ trượt (G.2.5)
        SoGhi  = new SoGhiKetQua(Path.Combine(ch.ThuMucDuLieu, "ketqua"), dongHo);
        BoDem  = new BoDemCa(Path.Combine(ch.ThuMucDuLieu, "ca", "ca.json"), dongHo, ch.MaCa);
        CuaSo  = new CuaSoTruotDo(soMauCan: 10);

        // ── Giám sát khí nén (G.4.4) — chỉ đo và kêu, máy tự quyết định phản ứng
        GiamSatKhi = new GiamSatKhiNen(docApSuat, ch.ApSuatNguongBar);
        GiamSatKhi.ApSuatXuongThap += (_, e) =>
        {
            _yeuCauDungCuoiChuKy = true;
            NhatKy.Ghi(MucLog.CanhBao, "KHI", "Áp suất {Bar} bar dưới ngưỡng {Nguong} bar",
                       ("Bar", e.BarDoDuoc), ("Nguong", e.BarNguong));
        };
        GiamSatKhi.ApSuatHoiPhuc += (_, e) =>
            NhatKy.Ghi(MucLog.ThongTin, "KHI", "Áp suất hồi phục {Bar} bar", ("Bar", e.BarDoDuoc));

        // ── Bắt tay hai dây (G.8.4)
        BatTay = new BatTayHaiDay(() => _coPhoiChoSan, () => _maySauSanSang,
                                  v => _io.Ghi("DEN_XANH", v));

        // ── Giao diện (G.7.1, G.7.4)
        ManHinh   = new ManHinhVM(new DieuPhoiTrucTiep());
        BangCanhBao = new BangCanhBaoVM();
    }

    // ── Thành phần lộ ra để kiểm thử và để giao diện đọc ──
    public CongThuc       CongThuc    { get; private set; }
    public NhatKy         NhatKy      { get; }
    public CuaRaBoNho     CuaRaBoNho  { get; }
    public BangLogVM      BangLog     { get; }
    public BangCanhBaoVM  BangCanhBao { get; }
    public ManHinhVM      ManHinh     { get; }
    public SoGhiKetQua    SoGhi       { get; }
    public BoDemCa        BoDem       { get; }
    public CuaSoTruotDo   CuaSo       { get; }
    public GiamSatKhiNen  GiamSatKhi  { get; }
    public BatTayHaiDay   BatTay      { get; }
    public BoQuyDoi       QuyDoi      { get; }
    public TrucGiaLap     TrucZ       { get; }
    public TrucGiaLap     TrucX       { get; }
    public CamBienGiaLap  CamBien     { get; }
    public VaoRaGiaLap    VaoRa       => _io;

    public TrangThaiMay TrangThai { get; private set; } = TrangThaiMay.ChuaKhoiTao;
    public bool DangTamDung { get; private set; }
    public int  SoPhoi => BoDem.HienTai.Tong;

    private bool _yeuCauDungCuoiChuKy;
    private bool _coPhoiChoSan = true;
    private bool _maySauSanSang = true;

    public void DatNguonPhoi(bool coPhoi) => _coPhoiChoSan = coPhoi;
    public void DatMaySau(bool sanSang)   => _maySauSanSang = sanSang;

    // ── Chuyển trạng thái: BẮT BUỘC đi qua bảng của G.5.4 ──
    private bool ThuChuyen(LenhMay lenh)
    {
        if (!BangChuyen.ThuChuyen(TrangThai, lenh, out var den))
        {
            NhatKy.Ghi(MucLog.CanhBao, "MAY", "Lệnh {Lenh} bị từ chối ở trạng thái {TrangThai}",
                       ("Lenh", lenh), ("TrangThai", TrangThai));
            return false;
        }
        var cu = TrangThai;
        TrangThai = den;
        NhatKy.Ghi(MucLog.ThongTin, "MAY", "Trạng thái {Cu} → {Moi} (lệnh {Lenh})",
                   ("Cu", cu), ("Moi", den), ("Lenh", lenh));
        ManHinh.CapNhatTrangThai(den, BangCanhBao.CoCanhBao ? BangCanhBao.DangHoatDong[0].DongHienThi : null);
        return true;
    }

    /// <summary>Nút nào bấm được lúc này — giao diện hỏi hàm này, không tự đoán (G.7.5).</summary>
    public TrangThaiNut TrangThaiNut(LenhMay lenh)
    {
        var tn = NutManHinhChinh.Tinh(TrangThai, lenh);
        if (tn.BatDuoc && lenh is LenhMay.BatDau or LenhMay.ChayTiep && !_anToan.DungKhanDangNhan == false)
            return new TrangThaiNut(false, "Dừng khẩn đang nhấn");
        if (tn.BatDuoc && lenh is LenhMay.BatDau or LenhMay.ChayTiep && _anToan.ManChanBiChe)
            return new TrangThaiNut(false, "Màn chắn sáng đang bị che");
        return tn;
    }

    public async Task KhoiTaoAsync(CancellationToken ct = default)
    {
        if (!ThuChuyen(LenhMay.KhoiTao)) return;
        try
        {
            await _bc.TrucZ.VeGocAsync(ct).ConfigureAwait(false);
            await _bc.TrucX.VeGocAsync(ct).ConfigureAwait(false);
            ThuChuyen(LenhMay.VeGocXong);
        }
        catch (AlarmException ex)
        {
            GhiNhanCanhBao(ex);
            ThuChuyen(LenhMay.Loi);
            throw;
        }
    }

    public async Task ChayAsync(int soChuKy, CancellationToken ct = default)
    {
        // Khoá lệnh theo tín hiệu an toàn — vai trò thứ 4 ở mục 15.2.2
        if (_anToan.DungKhanDangNhan || _anToan.ManChanBiChe)
        {
            NhatKy.Ghi(MucLog.CanhBao, "ANTOAN", "Từ chối chạy: dừng khẩn={Emg}, màn chắn bị che={Man}",
                       ("Emg", _anToan.DungKhanDangNhan), ("Man", _anToan.ManChanBiChe));
            return;
        }
        if (!ThuChuyen(LenhMay.BatDau)) return;

        _yeuCauDungCuoiChuKy = false;

        for (int i = 0; i < soChuKy; i++)
        {
            // Cổng tạm dừng ở RANH GIỚI CHU KỲ và giữa các bước (G.5.5)
            await _congTamDung.WaitAsync(ct).ConfigureAwait(false);
            _congTamDung.Release();

            if (_yeuCauDungCuoiChuKy)
            {
                NhatKy.Ghi(MucLog.CanhBao, "MAY", "Dừng cuối chu kỳ theo yêu cầu của giám sát khí nén");
                break;
            }

            GiamSatKhi.Kiem();                      // đo áp suất mỗi chu kỳ
            if (!BatTay.Nhip())                     // đói hoặc bị chặn thì chờ
            {
                NhatKy.Ghi(MucLog.ThongTin, "BATTAY", "Chưa chuyển được phôi, lý do {LyDo}",
                           ("LyDo", BatTay.LyDoDungHienTai));
                await Task.Delay(5, ct).ConfigureAwait(false);
                i--;                                 // chưa tính là một chu kỳ
                continue;
            }

            _bc.SoHieuPhoi = SoPhoi + 1;
            _bc.KetQua     = default;

            try
            {
                int chiSo = 0;
                foreach (var buoc in _buocs)
                {
                    await _congTamDung.WaitAsync(ct).ConfigureAwait(false);
                    _congTamDung.Release();
                    ct.ThrowIfCancellationRequested();

                    chiSo++;
                    ManHinh.CapNhatBuoc(chiSo, _buocs.Count, buoc.Ten);
                    await buoc.ThucThiAsync(_bc, ct).ConfigureAwait(false);
                }

                GhiNhanPhoi(_bc.KetQua);
            }
            catch (AlarmException ex)
            {
                GhiNhanCanhBao(ex);
                ThuChuyen(LenhMay.Loi);
                return;
            }
            catch (OperationCanceledException)
            {
                NhatKy.Ghi(MucLog.ThongTin, "MAY", "Dừng theo yêu cầu người vận hành");
                ThuChuyen(LenhMay.Dung);
                throw;
            }
#pragma warning disable CA1031 // bắt rộng rồi NÉM LẠI, chỉ để đưa máy về trạng thái an toàn
            catch (Exception ex)
#pragma warning restore CA1031
            {
                NhatKy.Ghi(MucLog.Loi, "MAY", "Lỗi không lường trước {Loai}: {ThongDiep}",
                           ("Loai", ex.GetType().Name), ("ThongDiep", ex.Message));
                ThuChuyen(LenhMay.Loi);
                throw;
            }
        }

        ThuChuyen(LenhMay.Dung);
    }

    private void GhiNhanPhoi(KetQuaDo kq)
    {
        BoDem.Dem(kq);
        SoGhi.Ghi(kq, CongThuc.Ten);
        CuaSo.Them(kq.ChieuDayMm);
        ManHinh.CapNhatSanLuong(BoDem.HienTai.Tong, BoDem.HienTai.Ok, BoDem.HienTai.Ng);

        var tk = CuaSo.Tinh();
        NhatKy.Ghi(MucLog.ThongTin, "DO", "Phôi {SoHieu} dày {Day} mm, kết luận {KetLuan}",
                   ("SoHieu", kq.SoHieuPhoi), ("Day", kq.ChieuDayMm), ("KetLuan", kq.KetLuan));
        if (tk.DuMau && tk.DoLech > 0.05)
            NhatKy.Ghi(MucLog.CanhBao, "DO", "Độ lệch chuẩn {DoLech} mm đang cao — kiểm tra quá trình",
                       ("DoLech", tk.DoLech));
    }

    private void GhiNhanCanhBao(AlarmException ex)
    {
        BangCanhBao.Phat(ex);
        var tt = DanhMucCanhBao.Tra(ex.Ma);
        NhatKy.Ghi(MucLog.Loi, ex.ViTri, "Cảnh báo {Ma} ({Nhom}): {MoTa}. Việc nên làm: {Viec}",
                   ("Ma", ex.Ma), ("Nhom", tt.Nhom), ("MoTa", ex.Message), ("Viec", tt.ViecNenLam));
    }

    // ── (1) Đường RA khỏi báo động: xác nhận rồi Reset ──

    public bool XacNhanCanhBao(int ma)
    {
        bool xong = BangCanhBao.XacNhan(ma);
        NhatKy.Ghi(MucLog.ThongTin, "CANHBAO", "Xác nhận mã {Ma}: {KetQua}",
                   ("Ma", ma), ("KetQua", xong ? "thành công" : "không có mã này"));
        return xong;
    }

    /// <summary>Chỉ reset được khi KHÔNG còn cảnh báo chưa xác nhận.</summary>
    public bool Reset()
    {
        if (BangCanhBao.CoCanhBao)
        {
            NhatKy.Ghi(MucLog.CanhBao, "MAY", "Không reset được: còn {SoCanhBao} cảnh báo chưa xác nhận",
                       ("SoCanhBao", BangCanhBao.DangHoatDong.Count));
            return false;
        }
        return ThuChuyen(LenhMay.Reset);
    }

    public void TamDung()
    {
        if (DangTamDung || TrangThai != TrangThaiMay.DangChay) return;
        DangTamDung = true;
        _congTamDung.Wait();
        ThuChuyen(LenhMay.TamDung);
    }

    public void ChayTiep()
    {
        if (!DangTamDung) return;
        DangTamDung = false;
        _congTamDung.Release();
        ThuChuyen(LenhMay.ChayTiep);
    }

    // ── (3) Tắt máy sạch ──

    public void TatMay()
    {
        NhatKy.Ghi(MucLog.ThongTin, "MAY",
                   "Tắt máy. Ca {MaCa}: {Tong} phôi, {Ok} đạt, {Ng} không đạt, tỷ lệ {TyLe} %",
                   ("MaCa", BoDem.HienTai.MaCa), ("Tong", BoDem.HienTai.Tong),
                   ("Ok", BoDem.HienTai.Ok), ("Ng", BoDem.HienTai.Ng), ("TyLe", BoDem.TyLeDat));
        DonFileCu.Don(Path.Combine(_ch.ThuMucDuLieu, "ketqua"), "KetQua_", soNgayGiu: 30, _dongHo);
        _io.Ghi("DEN_XANH", false);
    }

    public void Dispose()
    {
        if (_daHuy) return;
        _congTamDung.Dispose();
        _congTamDung = null!;
        _daHuy = true;
    }
}

/// <summary>Kẹp dùng vào-ra thật, tự xác nhận bằng cảm biến — và tự "đóng van" khi giả lập.</summary>
public sealed class KepKemGiaLap : IKep
{
    private readonly CumKep      _that;
    private readonly VaoRaGiaLap _io;

    public KepKemGiaLap(VaoRaGiaLap io, int hanGioMs, int nhipHoiMs)
    {
        ArgumentNullException.ThrowIfNull(io);
        _io = io;
        _that = new CumKep(io, hanGioMs, nhipHoiMs);
    }

    public bool DangKep => _that.DangKep;

    public async Task KepAsync(CancellationToken ct = default)
    {
        var vanDong = Task.Run(async () =>
        {
            await Task.Delay(8, ct).ConfigureAwait(false);       // van mất 8 ms để đóng
            _io.EpTinHieuVao(CumKep.TinHieuVaoDaKep, true);
        }, ct);
        await _that.KepAsync(ct).ConfigureAwait(false);
        await vanDong.ConfigureAwait(false);
    }

    public async Task NhaAsync(CancellationToken ct = default)
    {
        var vanMo = Task.Run(async () =>
        {
            await Task.Delay(8, ct).ConfigureAwait(false);
            _io.EpTinHieuVao(CumKep.TinHieuVaoDaKep, false);
        }, ct);
        await _that.NhaAsync(ct).ConfigureAwait(false);
        await vanMo.ConfigureAwait(false);
    }
}

/// <summary>Nối đường ống log (G.8.2) vào bảng hiển thị (G.7.4) — đúng chiều: bảng là CỬA RA.</summary>
public sealed class CuaRaNoiVaoBang(BangLogVM bang) : ICuaRaLog
{
    public void Nhan(BanGhiLog ban)
    {
        ArgumentNullException.ThrowIfNull(ban);
        bang.Nhan(new DongLog(ban.ThoiDiem, ban.Muc, ban.Nguon, ban.DungCau()));
    }
}
