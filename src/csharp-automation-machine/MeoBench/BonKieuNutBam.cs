// -------------------------------------------------------
// File:    BonKieuNutBam.cs
// Project: MeoBench
// Purpose: Mục G.15 — bốn kiểu viết CÙNG MỘT nút bấm, dựng lại từ hình dạng
//          quan sát được trong mã máy thật (đã tổng quát hoá, không tên thật).
//          Mục đích không phải chê kiểu nào, mà để ĐO xem kiểm thử với tới đâu.
// -------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

namespace MeoBench;

/// <summary>Kết quả một thao tác người dùng — thứ phép kiểm cần nhìn vào.</summary>
public readonly record struct KetQuaThaoTac(bool ChoPhep, string LyDo);

/// <summary>Cổng ra vào tối giản cho ví dụ G.15.</summary>
public interface ICongRa
{
    bool DangBat(string ten);
    void Dat(string ten, bool bat);
}

/// <summary>Bản giả lập cổng ra — đếm cả số lần bị từ chối.</summary>
public sealed class CongRaGiaLap : ICongRa
{
    private readonly Dictionary<string, bool> _o = new(StringComparer.Ordinal);
    public int SoLanDat { get; private set; }

    public bool DangBat(string ten) => _o.GetValueOrDefault(ten);

    public void Dat(string ten, bool bat)
    {
        _o[ten] = bat;
        SoLanDat++;
    }
}

/// <summary>Hỏi người dùng một câu có/không. Trong máy thật là hộp thoại; ở đây cắm được bản giả.</summary>
public interface IHoiNguoiDung
{
    bool XacNhan(string cauHoi);
}

/// <summary>Bản giả: trả lời theo kịch bản, và ghi lại câu đã hỏi.</summary>
public sealed class HoiTheoKichBan(bool traLoi) : IHoiNguoiDung
{
    public List<string> DaHoi { get; } = [];

    public bool XacNhan(string cauHoi)
    {
        DaHoi.Add(cauHoi);
        return traLoi;
    }
}

// ══════════════════════════════════════════════════════════════════
// KIỂU 1 — làm thẳng trong nút. Đây là hình dạng hay gặp nhất.
// ══════════════════════════════════════════════════════════════════

/// <summary>
/// Kiểu 1: toàn bộ luật nằm trong thân hàm xử lý sự kiện, kể cả <b>danh sách tín hiệu đặc biệt
/// viết cứng</b> và <b>hộp thoại hỏi</b>. Quan sát được trong mã thật gần như nguyên dạng này.
/// <para/>
/// Kiểm thử được tới đâu: <b>không</b>. Muốn kiểm phải dựng được một cửa sổ thật, bấm được một
/// nút thật, và bắt được một hộp thoại thật. Ba thứ đó không chạy trong máy chủ tích hợp liên tục.
/// </summary>
public static class Kieu1LamThangTrongNut
{
    // Luật an toàn sống dưới dạng ký tự, trong file giao diện.
    [SuppressMessage("Performance", "CA1861", Justification = "Cố ý tái hiện hình dạng mã thật")]
    public static void KhiBamNutBat(ICongRa io, string tenTinHieu)
    {
        // if (ten == "OUT_53" || ten == "OUT_54" || ... 10 chuỗi)
        //     if (MessageBox.Show("Bật tín hiệu bắt tay?") != OK) return;
        // io.Dat(ten, !io.DangBat(ten));
        //
        // Không viết ra được ở đây mà vẫn kiểm thử được — và đó CHÍNH LÀ điều mục G.15 muốn nói.
        // Phiên bản chạy được nằm ở Kiểu 4 bên dưới.
        ArgumentNullException.ThrowIfNull(io);
        io.Dat(tenTinHieu, !io.DangBat(tenTinHieu));
    }
}

// ══════════════════════════════════════════════════════════════════
// KIỂU 4 — tách quyết định ra khỏi nút. Cùng hành vi, kiểm thử được.
// ══════════════════════════════════════════════════════════════════

/// <summary>
/// Kiểu 4: nút bấm chỉ còn <b>ba dòng</b> — hỏi lớp quyết định, và làm theo.
/// Mọi luật (tín hiệu nào cần xác nhận, khi nào bị chặn) nằm ở đây, không nằm trong cửa sổ.
/// <para/>
/// Kiểm thử được tới đâu: <b>toàn bộ</b>, không cần cửa sổ nào.
/// </summary>
public sealed class QuyetDinhBatTinHieu
{
    private readonly ICongRa _io;
    private readonly IHoiNguoiDung _hoi;
    private readonly IAnToanChiDoc _anToan;
    private readonly HashSet<string> _canXacNhan;

    public QuyetDinhBatTinHieu(ICongRa io, IHoiNguoiDung hoi, IAnToanChiDoc anToan,
                               IEnumerable<string> tinHieuCanXacNhan)
    {
        ArgumentNullException.ThrowIfNull(io);
        ArgumentNullException.ThrowIfNull(hoi);
        ArgumentNullException.ThrowIfNull(anToan);
        ArgumentNullException.ThrowIfNull(tinHieuCanXacNhan);
        _io = io;
        _hoi = hoi;
        _anToan = anToan;
        // Danh sách tới từ CẤU HÌNH, không viết cứng trong mã giao diện (mục G.12).
        _canXacNhan = new HashSet<string>(tinHieuCanXacNhan, StringComparer.Ordinal);
    }

    /// <summary>Thử đảo một tín hiệu. Trả về lý do khi từ chối — không bao giờ chỉ trả <c>false</c>.</summary>
    public KetQuaThaoTac ThuDao(string tenTinHieu)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenTinHieu);

        // Cửa 1 — an toàn. Đọc, không ghi (mục 15.2.2b).
        if (_anToan.DungKhanDangNhan)
            return new(false, "Nút dừng khẩn đang nhấn — không thao tác được cổng ra");
        if (_anToan.ManChanBiChe)
            return new(false, "Màn chắn đang bị che — không thao tác được cổng ra");

        // Cửa 2 — tín hiệu đặc biệt thì phải hỏi.
        if (_canXacNhan.Contains(tenTinHieu)
            && !_hoi.XacNhan($"{tenTinHieu} là tín hiệu bắt tay với máy bên cạnh. Vẫn đảo?"))
            return new(false, "Người vận hành đã huỷ");

        _io.Dat(tenTinHieu, !_io.DangBat(tenTinHieu));
        return new(true, string.Empty);
    }
}

/// <summary>
/// Thân hàm nút bấm sau khi tách — đây là TẤT CẢ những gì còn lại trong file giao diện.
/// Ba dòng, không luật nào, không hộp thoại nào gọi thẳng.
/// </summary>
public static class Kieu4UyHetChoLopKhac
{
    /// <summary>
    /// Trả về thông báo cần hiển thị (chuỗi rỗng = không cần báo gì).
    /// <para/>
    /// Trong dự án thật hàm này tên là <c>btnBat_Click</c> — do trình thiết kế đặt, và tên đó
    /// vi phạm chính luật CA1707 của sách (không gạch dưới). Đó là một lý do nữa để trong thân
    /// hàm ấy <b>không có gì đáng kiểm thử</b>: nó thuộc về công cụ, không thuộc về bạn.
    /// </summary>
    public static string KhiBamNutBat(QuyetDinhBatTinHieu quyetDinh, string tenTinHieu)
    {
        ArgumentNullException.ThrowIfNull(quyetDinh);
        var kq = quyetDinh.ThuDao(tenTinHieu);
        return kq.ChoPhep ? string.Empty : kq.LyDo;
    }
}
