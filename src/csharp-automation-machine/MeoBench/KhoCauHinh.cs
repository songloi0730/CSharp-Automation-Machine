// -------------------------------------------------------
// File:    KhoCauHinh.cs
// Project: MeoBench
// Purpose: Tách CẤU HÌNH CỦA CỖ MÁY NÀY khỏi CHƯƠNG TRÌNH.
//
// Bài toán: hai mươi cỗ máy cùng loại chạy CÙNG một bản chương trình, nhưng
// mỗi cỗ có điểm dạy riêng, địa chỉ IP riêng, hành trình riêng. Nếu cấu hình
// nằm lẫn trong thư mục chương trình thì mỗi lần cập nhật phần mềm là một lần
// đè mất cấu hình của cỗ máy đó — và người đi cập nhật lúc 2 giờ sáng sẽ là
// người phát hiện ra.
//
// Đo trên 13 phần mềm máy thật: 294 chỗ đọc đường dẫn CẠNH FILE CHẠY (12/13
// dự án), chỉ 6 chỗ dùng thư mục dữ liệu của hệ điều hành, và 912 đường dẫn
// tuyệt đối gõ cứng trong mã (một dự án có 595 chỗ).
//
// Bố trí ở đây:
//   <gốc dữ liệu>/config/   cấu hình CỖ MÁY NÀY  — không bao giờ bị cập nhật đè
//   <gốc dữ liệu>/product/  công thức SẢN PHẨM   — chép được giữa các máy
//   <gốc dữ liệu>/data/     dữ liệu chạy ra      — kết quả, nhật ký, số liệu ca
// -------------------------------------------------------
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace MeoBench;

// ══════════════ Gốc dữ liệu: tìm ở đâu ══════════════

public enum KieuGocDuLieu { BienMoiTruong, DuLieuHeDieuHanh, CanhFileChay }

public sealed record KetQuaTimGoc(string DuongDan, KieuGocDuLieu Kieu, string? CanhBao);

public static class GocDuLieu
{
    public const string TenBienMoiTruong = "MEOBENCH_DATA";

    /// <summary>
    /// Thứ tự ưu tiên, và lý do của từng mức:
    ///   1. Biến môi trường — để đội triển khai đặt chỗ khác mà không sửa mã.
    ///   2. Thư mục dữ liệu của hệ điều hành — KHÔNG bị bộ cài đặt xoá khi cập nhật.
    ///   3. Cạnh file chạy — chạy được ngay khi phát triển, nhưng CÓ CẢNH BÁO,
    ///      vì đây chính là chỗ cập nhật phần mềm sẽ đè mất.
    /// </summary>
    public static KetQuaTimGoc Tim(string tenUngDung = "MeoBench", string? epDuongDan = null)
    {
        if (!string.IsNullOrWhiteSpace(epDuongDan))
            return new KetQuaTimGoc(epDuongDan, KieuGocDuLieu.BienMoiTruong, null);

        string? tuBien = Environment.GetEnvironmentVariable(TenBienMoiTruong);
        if (!string.IsNullOrWhiteSpace(tuBien))
            return new KetQuaTimGoc(tuBien, KieuGocDuLieu.BienMoiTruong, null);

        string chung = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        if (!string.IsNullOrWhiteSpace(chung))
            return new KetQuaTimGoc(Path.Combine(chung, tenUngDung), KieuGocDuLieu.DuLieuHeDieuHanh, null);

        return new KetQuaTimGoc(
            Path.Combine(AppContext.BaseDirectory, "data"), KieuGocDuLieu.CanhFileChay,
            "Gốc dữ liệu đang nằm CẠNH FILE CHẠY — bản cập nhật phần mềm có thể đè mất. "
            + $"Đặt biến môi trường {TenBienMoiTruong} để chuyển ra ngoài.");
    }
}

// ══════════════ Cấu hình CỖ MÁY NÀY — mỗi máy một bản, không chép sang máy khác ══════════════

public sealed record DiemDay(string Ten, double ViTriMm)
{
    public override string ToString()
        => $"{Ten}={ViTriMm.ToString("F3", CultureInfo.InvariantCulture)}";
}

public sealed record CauHinhMang(string DiaChiIp, int Cong, string CongNoiTiep, int TocDoBaud);

public sealed record CauHinhCoMay
{
    public string MaMay          { get; init; } = "MEOBENCH-01";
    public string BienThe        { get; init; } = "Trai";        // Trai / Phai
    public double HanhTrinhZMm   { get; init; } = 60.0;
    public double HanhTrinhXMm   { get; init; } = 400.0;
    public double XungMoiMmZ     { get; init; } = 1000.0;
    public double XungMoiMmX     { get; init; } = 1000.0;
    public int    HanGioTrucMs   { get; init; } = 3000;
    public double ApSuatNguongBar{ get; init; } = 5.00;

    public IReadOnlyList<string> KiemTra()
    {
        var loi = new List<string>();
        if (string.IsNullOrWhiteSpace(MaMay))      loi.Add("MaMay để trống");
        if (HanhTrinhZMm <= 0)                     loi.Add("HanhTrinhZMm phải lớn hơn 0");
        if (HanhTrinhXMm <= 0)                     loi.Add("HanhTrinhXMm phải lớn hơn 0");
        if (XungMoiMmZ   <= 0)                     loi.Add("XungMoiMmZ phải lớn hơn 0");
        if (XungMoiMmX   <= 0)                     loi.Add("XungMoiMmX phải lớn hơn 0");
        if (HanGioTrucMs <= 0)                     loi.Add("HanGioTrucMs phải lớn hơn 0");
        if (BienThe is not ("Trai" or "Phai"))     loi.Add($"BienThe '{BienThe}' không hợp lệ (Trai/Phai)");
        return loi;
    }
}

/// <summary>Điểm dạy — thứ KHÁC NHAU giữa từng cỗ máy dù cùng một bản vẽ.</summary>
public sealed record BoDiemDay
{
    public double ViTriDoMm     { get; init; } = 30.0;
    public double ViTriAnToanMm { get; init; }          // mặc định 0 = nâng hết
    public double MangOkMm      { get; init; } = 100.0;
    public double MangNgMm      { get; init; } = 200.0;
    public double ViTriChoMm    { get; init; }          // mặc định 0 = vị trí chờ

    public IReadOnlyList<string> KiemTraTrongHanhTrinh(CauHinhCoMay coMay)
    {
        ArgumentNullException.ThrowIfNull(coMay);
        var loi = new List<string>();
        foreach (var (ten, v, tran) in new (string, double, double)[]
                 {
                     ("ViTriDoMm",     ViTriDoMm,     coMay.HanhTrinhZMm),
                     ("ViTriAnToanMm", ViTriAnToanMm, coMay.HanhTrinhZMm),
                     ("MangOkMm",      MangOkMm,      coMay.HanhTrinhXMm),
                     ("MangNgMm",      MangNgMm,      coMay.HanhTrinhXMm),
                     ("ViTriChoMm",    ViTriChoMm,    coMay.HanhTrinhXMm),
                 })
        {
            if (v < 0 || v > tran)
                loi.Add(string.Format(CultureInfo.InvariantCulture,
                    "Điểm {0} = {1} nằm ngoài hành trình [0; {2}]", ten, v, tran));
        }
        if (Math.Abs(MangOkMm - MangNgMm) < 1e-6) loi.Add("Máng OK và máng NG trùng vị trí");
        return loi;
    }
}

// ══════════════ Kho cấu hình: nạp, tạo mặc định, KHÔNG BAO GIỜ đè ══════════════

public sealed record KetQuaNapCauHinh(
    CauHinhCoMay CoMay, BoDiemDay Diem, CauHinhMang Mang,
    IReadOnlyList<string> CanhBao, bool VuaTaoMacDinh);

public sealed class KhoCauHinh
{
    private static readonly JsonSerializerOptions TuyChon = new() { WriteIndented = true };

    public KhoCauHinh(string goc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(goc);
        Goc        = goc;
        ThuMucConfig  = Path.Combine(goc, "config");
        ThuMucProduct = Path.Combine(goc, "product");
        ThuMucData    = Path.Combine(goc, "data");
    }

    public string Goc           { get; }
    public string ThuMucConfig  { get; }
    public string ThuMucProduct { get; }
    public string ThuMucData    { get; }

    public string FileCoMay => Path.Combine(ThuMucConfig, "may.json");
    public string FileDiem  => Path.Combine(ThuMucConfig, "diem.json");
    public string FileMang  => Path.Combine(ThuMucConfig, "mang.json");

    /// <summary>Tạo đủ thư mục và file mặc định — nhưng KHÔNG đè file đã có.</summary>
    public bool TaoNeuThieu()
    {
        Directory.CreateDirectory(ThuMucConfig);
        Directory.CreateDirectory(ThuMucProduct);
        Directory.CreateDirectory(ThuMucData);

        bool coTao = false;
        coTao |= GhiNeuChuaCo(FileCoMay, new CauHinhCoMay());
        coTao |= GhiNeuChuaCo(FileDiem,  new BoDiemDay());
        coTao |= GhiNeuChuaCo(FileMang,  new CauHinhMang("192.168.0.10", 502, "COM3", 9600));

        string ctMacDinh = Path.Combine(ThuMucProduct, "SanPhamMacDinh.json");
        coTao |= GhiNeuChuaCo(ctMacDinh, new CongThuc { Ten = "SanPhamMacDinh" });
        return coTao;
    }

    private static bool GhiNeuChuaCo<T>(string duongDan, T macDinh)
    {
        if (File.Exists(duongDan)) return false;        // ★ KHÔNG BAO GIỜ đè
        File.WriteAllText(duongDan, JsonSerializer.Serialize(macDinh, TuyChon), Encoding.UTF8);
        return true;
    }

    public KetQuaNapCauHinh Nap()
    {
        bool vuaTao = TaoNeuThieu();
        var canhBao = new List<string>();

        var coMay = Doc(FileCoMay, new CauHinhCoMay(), canhBao, "may.json");
        var diem  = Doc(FileDiem,  new BoDiemDay(),    canhBao, "diem.json");
        var mang  = Doc(FileMang,  new CauHinhMang("192.168.0.10", 502, "COM3", 9600), canhBao, "mang.json");

        foreach (string l in coMay.KiemTra())
            canhBao.Add("may.json: " + l);
        foreach (string l in diem.KiemTraTrongHanhTrinh(coMay))
            canhBao.Add("diem.json: " + l);   // ★ điểm dạy phải nằm TRONG hành trình của chính máy này

        return new KetQuaNapCauHinh(coMay, diem, mang, canhBao, vuaTao);
    }

    private static T Doc<T>(string duongDan, T duPhong, List<string> canhBao, string ten)
    {
        try
        {
            var doc = JsonSerializer.Deserialize<T>(File.ReadAllText(duongDan, Encoding.UTF8));
            if (doc is not null) return doc;
            canhBao.Add($"{ten}: file rỗng — dùng mặc định");
        }
#pragma warning disable CA1031 // file cấu hình hỏng kiểu gì cũng không được làm sập máy
        catch (Exception ex)
#pragma warning restore CA1031
        {
            canhBao.Add($"{ten}: hỏng ({ex.GetType().Name}) — dùng mặc định");
        }
        return duPhong;
    }

    // ── Sản phẩm: chép được giữa các máy, khác hẳn cấu hình cỗ máy ──

    public IReadOnlyList<string> LietKeSanPham()
        => Directory.Exists(ThuMucProduct)
            ? [.. Directory.GetFiles(ThuMucProduct, "*.json")
                           .Select(Path.GetFileNameWithoutExtension)
                           .Where(s => s is not null).Select(s => s!).Order(StringComparer.Ordinal)]
            : [];

    public CongThuc NapSanPham(string ten, CongThuc duPhong, out string? lyDo)
        => KhoCongThuc.Nap(Path.Combine(ThuMucProduct, ten + ".json"), duPhong, out lyDo);

    public void LuuSanPham(CongThuc ct)
    {
        ArgumentNullException.ThrowIfNull(ct);
        Directory.CreateDirectory(ThuMucProduct);
        KhoCongThuc.Ghi(Path.Combine(ThuMucProduct, ct.Ten + ".json"), ct);
    }
}
