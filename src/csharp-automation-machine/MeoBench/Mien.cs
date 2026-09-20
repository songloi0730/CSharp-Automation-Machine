// -------------------------------------------------------
// File:    Mien.cs
// Project: MeoBench
// Purpose: Lời giải mẫu G.1.2, G.1.3, G.1.5 — kiểu dữ liệu miền.
// -------------------------------------------------------
namespace MeoBench;

// ══════════════ G.1.3 — enum trạng thái, không phải cờ bool ══════════════

/// <summary>Trạng thái toàn máy. Số gán tường minh và KHÔNG được đánh số lại.</summary>
public enum TrangThaiMay
{
    ChuaKhoiTao = 0,
    DangVeGoc   = 1,
    SanSang     = 2,
    DangChay    = 3,
    TamDung     = 4,
    BaoDong     = 5,
}

/// <summary>Kết luận cho một phôi. Có ChuaDo và LoiDo nên không dùng bool được.</summary>
public enum KetLuanDo
{
    ChuaDo     = 0,
    Dat        = 1,
    DuoiNguong = 2,
    TrenNguong = 3,
    LoiDo      = 4,
}

// ══════════════ G.1.2 — bản ghi một lần đo ══════════════

/// <summary>
/// Một lần đo. Là GIÁ TRỊ (không có danh tính) nên dùng readonly record struct:
/// so sánh theo từng trường, bất biến, không cấp phát trên heap.
/// </summary>
public readonly record struct KetQuaDo(
    int         SoHieuPhoi,
    double      ChieuDayMm,
    DateTime    ThoiDiem,
    KetLuanDo   KetLuan)
{
    public bool Dat => KetLuan == KetLuanDo.Dat;

    public override string ToString()
        => $"#{SoHieuPhoi} {ChieuDayMm.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)} mm {KetLuan}";
}

// ══════════════ G.1.5 — ngoại lệ mang mã cảnh báo ══════════════

/// <summary>Mã cảnh báo chia dải theo loại. Dải đã chốt, không đổi về sau.</summary>
public static class MaCanhBao
{
    public const int TrucQuaThoiGian      = 10001;  // 10000–10999 chuyển động
    public const int TrucChuaVeGoc        = 10002;
    public const int CamBienKhongPhanHoi  = 20001;  // 20000–20999 cảm biến
    public const int KepKhongXacNhan      = 30001;  // 30000–30999 khí / cơ cấu
}

/// <summary>
/// Ngoại lệ mang mã cảnh báo và vị trí phát sinh.
/// LƯU Ý: KHÔNG đặt tên thuộc tính là Source — Exception đã có sẵn Source,
/// trùng tên sẽ cho lỗi biên dịch CS0114. Sách dùng ViTri.
/// </summary>
public sealed class AlarmException : Exception
{
    public AlarmException(int ma, string viTri, string thongDiep) : base(thongDiep)
    {
        Ma    = ma;
        ViTri = viTri;
    }

    public int    Ma    { get; }
    public string ViTri { get; }

    public string DongHienThi => $"CẢNH BÁO [{Ma}] {ViTri}: {Message}";
}

// ══════════════ Đồng hồ tiêm được (G.6.5 — cần cho G.4.3 kiểm thử được) ══════════════

public interface IDongHo { DateTime BayGio { get; } }

public sealed class DongHoHeThong : IDongHo
{
    public DateTime BayGio => DateTime.Now;
}

public sealed class DongHoGia(DateTime batDau) : IDongHo
{
    private DateTime _hienTai = batDau;
    public DateTime BayGio => _hienTai;
    public void Tien(TimeSpan khoang) => _hienTai += khoang;
}

// ══════════════ Bộ tự kiểm ══════════════

public static class KiemMien
{
    public static Task Chay()
    {
        // ---------- G.1.2 ----------
        Kiem.MoBai("G.1.2", "Bản ghi một lần đo");
        var t = new DateTime(2026, 9, 20, 8, 0, 0);
        var a = new KetQuaDo(1, 2.005, t, KetLuanDo.Dat);
        var b = new KetQuaDo(1, 2.005, t, KetLuanDo.Dat);
        var c = new KetQuaDo(2, 2.005, t, KetLuanDo.Dat);
        Kiem.Dung(a == b, "hai bản ghi cùng giá trị thì bằng nhau");
        Kiem.Dung(a != c, "khác số hiệu phôi thì khác nhau");
        Kiem.Bang(a.GetHashCode(), b.GetHashCode(), "cùng giá trị thì cùng mã băm");
        Kiem.Dung(a.Dat, "thuộc tính Dat suy ra từ KetLuan");
        Kiem.Bang(a.ToString(), "#1 2.005 mm Dat", "ToString dùng dấu chấm thập phân bất biến");

        // ---------- G.1.3 ----------
        Kiem.MoBai("G.1.3", "Enum trạng thái");
        Kiem.Bang((int)TrangThaiMay.SanSang, 2, "SanSang giữ nguyên giá trị 2");
        Kiem.Bang((int)KetLuanDo.ChuaDo, 0, "ChuaDo là giá trị mặc định 0");
        Kiem.Bang(default(KetLuanDo), KetLuanDo.ChuaDo, "biến chưa gán mang nghĩa 'chưa đo'");
        Kiem.Bang(Enum.GetValues<TrangThaiMay>().Length, 6, "đúng 6 trạng thái");
        Kiem.Dung(!Enum.IsDefined((TrangThaiMay)99), "giá trị lạ không hợp lệ");

        // ---------- G.1.5 ----------
        Kiem.MoBai("G.1.5", "Ngoại lệ mang mã cảnh báo");
        var ex = new AlarmException(MaCanhBao.TrucQuaThoiGian, "TRUC_Z", "quá thời gian khi đi tới 25,0 mm");
        Kiem.Bang(ex.Ma, 10001, "mã cảnh báo giữ nguyên");
        Kiem.Bang(ex.ViTri, "TRUC_Z", "vị trí phát sinh giữ nguyên");
        Kiem.Bang(ex.DongHienThi,
                  "CẢNH BÁO [10001] TRUC_Z: quá thời gian khi đi tới 25,0 mm",
                  "dòng hiển thị đúng khuôn");
        Kiem.Dung(ex is Exception, "kế thừa Exception, bắt được ở vòng lặp chu trình");
        Kiem.Dung(MaCanhBao.TrucQuaThoiGian / 10000 == 1, "mã chuyển động nằm ở dải 10000");
        Kiem.Dung(MaCanhBao.CamBienKhongPhanHoi / 10000 == 2, "mã cảm biến nằm ở dải 20000");

        return Task.CompletedTask;
    }
}
