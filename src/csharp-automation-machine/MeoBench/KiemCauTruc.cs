// -------------------------------------------------------
// File:    KiemCauTruc.cs
// Project: MeoBench
// Purpose: Mục G.15 — ĐO xem phép kiểm với tới đâu khi cấu trúc đổi.
//          Cùng một hành vi, hai cách tổ chức, và số phép kiểm viết được
//          cho mỗi cách là con số nói lên khác biệt rõ nhất.
// -------------------------------------------------------

namespace MeoBench;

/// <summary>Phép kiểm cho mục G.15 — cấu trúc và khả năng kiểm thử.</summary>
public static class KiemCauTruc
{
    /// <summary>Chạy toàn bộ.</summary>
    public static Task Chay()
    {
        KieuUyQuyen();
        NutBamConLaiGi();
        return Task.CompletedTask;
    }

    private static QuyetDinhBatTinHieu Dung(out CongRaGiaLap io, out HoiTheoKichBan hoi,
                                            out AnToanGiaLap at, bool traLoiHoi = true)
    {
        io = new CongRaGiaLap();
        hoi = new HoiTheoKichBan(traLoiHoi);
        at = new AnToanGiaLap();
        // Danh sách tín hiệu bắt tay tới từ CẤU HÌNH, không viết cứng trong mã giao diện.
        return new QuyetDinhBatTinHieu(io, hoi, at, ["OUT_BAT_TAY_1", "OUT_BAT_TAY_2"]);
    }

    // ── Kiểu 4: tách quyết định ra — kiểm được TOÀN BỘ luật ──────────
    private static void KieuUyQuyen()
    {
        Kiem.MoBai("G.15.1", "Tách quyết định khỏi nút bấm — luật nào cũng kiểm được");

        // 1. Tín hiệu thường: đảo ngay, không hỏi.
        var qd = Dung(out var io, out var hoi, out _);
        var kq = qd.ThuDao("OUT_THUONG");
        Kiem.Dung(kq.ChoPhep, "tín hiệu thường được đảo");
        Kiem.Bang(io.SoLanDat, 1, "đã ghi xuống cổng ra đúng một lần");
        Kiem.Bang(hoi.DaHoi.Count, 0, "★ KHÔNG hỏi người dùng cho tín hiệu thường");

        // 2. Tín hiệu bắt tay: phải hỏi, và câu hỏi phải NÓI RÕ vì sao.
        qd = Dung(out io, out hoi, out _);
        kq = qd.ThuDao("OUT_BAT_TAY_1");
        Kiem.Dung(kq.ChoPhep, "người dùng đồng ý → vẫn đảo");
        Kiem.Bang(hoi.DaHoi.Count, 1, "★ có hỏi trước khi đảo tín hiệu bắt tay");
        Kiem.Dung(hoi.DaHoi[0].Contains("bắt tay", StringComparison.Ordinal),
            "★ câu hỏi nói RÕ LÝ DO, không phải 'Bạn có chắc không?'");

        // 3. Người dùng huỷ → không được ghi gì xuống cổng ra.
        qd = Dung(out io, out hoi, out _, traLoiHoi: false);
        kq = qd.ThuDao("OUT_BAT_TAY_1");
        Kiem.Dung(!kq.ChoPhep, "người dùng huỷ → từ chối");
        Kiem.Bang(io.SoLanDat, 0, "★ huỷ thì KHÔNG ghi gì xuống cổng ra");
        Kiem.Dung(kq.LyDo.Length > 0, "từ chối luôn kèm lý do đọc được");

        // 4. An toàn chặn trước mọi thứ — kể cả trước khi kịp hỏi.
        qd = Dung(out io, out hoi, out var at);
        at.DungKhanDangNhan = true;
        kq = qd.ThuDao("OUT_BAT_TAY_1");
        Kiem.Dung(!kq.ChoPhep, "dừng khẩn đang nhấn → từ chối");
        Kiem.Bang(hoi.DaHoi.Count, 0, "★ an toàn chặn TRƯỚC, không hỏi người dùng làm gì");
        Kiem.Bang(io.SoLanDat, 0, "và không ghi gì xuống cổng ra");
        Kiem.Dung(kq.LyDo.Contains("dừng khẩn", StringComparison.OrdinalIgnoreCase),
            "lý do nói đúng cửa nào chặn");

        qd = Dung(out io, out _, out at);
        at.ManChanBiChe = true;
        Kiem.Dung(!qd.ThuDao("OUT_THUONG").ChoPhep, "màn chắn bị che → cũng từ chối");

        // 5. Đảo hai lần thì về trạng thái cũ.
        qd = Dung(out io, out _, out _);
        _ = qd.ThuDao("OUT_THUONG");
        Kiem.Dung(io.DangBat("OUT_THUONG"), "lần 1: bật");
        _ = qd.ThuDao("OUT_THUONG");
        Kiem.Dung(!io.DangBat("OUT_THUONG"), "lần 2: tắt — đúng nghĩa 'đảo'");
    }

    // ── Nút bấm còn lại gì sau khi tách ──────────────────────────────
    private static void NutBamConLaiGi()
    {
        Kiem.MoBai("G.15.2", "Sau khi tách, nút bấm còn lại gì");

        var qd = Dung(out var io, out _, out var at);
        Kiem.Bang(Kieu4UyHetChoLopKhac.KhiBamNutBat(qd, "OUT_THUONG"), "",
            "★ thao tác được → nút không phải hiện thông báo nào");

        at.DungKhanDangNhan = true;
        string tb = Kieu4UyHetChoLopKhac.KhiBamNutBat(qd, "OUT_THUONG");
        Kiem.Dung(tb.Length > 0, "★ bị chặn → nút nhận đúng câu để hiển thị");
        Kiem.Dung(tb.Contains("dừng khẩn", StringComparison.OrdinalIgnoreCase),
            "và câu đó tới TỪ LỚP QUYẾT ĐỊNH, không do giao diện tự nghĩ ra");

        // Điểm cốt lõi của cả mục: nút bấm không còn quyết định gì.
        // Nó nhận một chuỗi và hiển thị. Không luật, không hộp thoại, không hạn giờ.
        Kiem.Bang(io.SoLanDat, 1, "★ toàn bộ kịch bản trên chạy mà KHÔNG cần một cửa sổ nào");
    }
}
