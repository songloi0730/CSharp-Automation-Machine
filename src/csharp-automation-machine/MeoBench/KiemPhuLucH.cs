// -------------------------------------------------------
// File:    KiemPhuLucH.cs
// Project: MeoBench
// Purpose: Kiểm MỌI khẳng định hành vi nêu trong Phụ lục H
//          (boxing, closure, thực thi trì hoãn, yield, hiệp biến, Span, [Flags]).
//          Sách không được nói điều gì mà mã không chứng minh được.
// -------------------------------------------------------

using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace MeoBench;

/// <summary>
/// Phép kiểm cho Phụ lục H. Khác các nhóm G khác: nhóm này không dựng chi tiết máy nào,
/// nó xác nhận rằng những điều sách khẳng định về <b>ngôn ngữ C#</b> đúng trên .NET 9.
/// </summary>
public static class KiemPhuLucH
{
    private static readonly string[] TenFileKhongCo = ["khong-ton-tai-abc.txt"];

    /// <summary>Đo số byte cấp phát trung bình mỗi lần chạy <paramref name="viec"/>.</summary>
    private static long ByteMoiLan(Action viec, int soLan = 1000)
    {
        viec();                                     // nạp trước, bỏ qua lần đầu
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long truoc = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < soLan; i++) viec();
        return (GC.GetAllocatedBytesForCurrentThread() - truoc) / soLan;
    }

    /// <summary>Chạy toàn bộ phép kiểm của Phụ lục H.</summary>
    [SuppressMessage("Performance", "CA1848", Justification = "Bản mẫu, không phải mã sản xuất")]
    public static Task Chay()
    {
        ChayH1Boxing();
        ChayH2Closure();
        ChayH3TriHoan();
        ChayH4Yield();
        ChayH5Variance();
        ChayH6Span();
        ChayH7Flags();
        return Task.CompletedTask;
    }

    // ---------------------------------------------------------------- H.1
    private static void ChayH1Boxing()
    {
        Kiem.MoBai("H.1", "Boxing — cái giá của bảng tag kiểu object");

        var bangObject = new Dictionary<string, object>();
        var bangSo = new Dictionary<string, double>();

        long aObj = ByteMoiLan(() => bangObject["Tram3.ViTriZ"] = 125.4);
        long aSo = ByteMoiLan(() => bangSo["Tram3.ViTriZ"] = 125.4);

        Kiem.Dung(aObj >= 24, $"Dictionary<string,object>: ghi double cấp phát {aObj} byte (box)");
        Kiem.Bang(aSo, 0L, "Dictionary<string,double>: ghi double cấp phát 0 byte");

        // Lời khuyên cũ "nội suy chuỗi box đối số" đã hết đúng từ C# 10 / .NET 6.
        int so = 42;
        double d = 3.14159;
        long aNoiSuy = ByteMoiLan(() => { _ = $"Truc {so} tai {d}"; });
        long aFormat = ByteMoiLan(() => { _ = string.Format(CultureInfo.InvariantCulture, "Truc {0} tai {1}", so, d); });

        // Bất biến ỔN ĐỊNH: string.Format luôn cấp phát nhiều hơn, vì nó box đối số.
        Kiem.Dung(aFormat > aNoiSuy,
            $"string.Format ({aFormat} byte) vẫn cấp phát nhiều hơn nội suy chuỗi ({aNoiSuy} byte)");

        // Chênh lệch KHÔNG ổn định, và lý do đáng học hơn bản thân con số:
        //   • mã đã lên tier-1 (JIT tối ưu): nội suy = 64 byte = đúng chuỗi kết quả, 0 box
        //   • mã còn ở tier-0 (QuickJit):    nội suy = 88 byte — thừa đúng MỘT box 24 byte
        // Chạy `DOTNET_TieredCompilation=0 dotnet run -- H` sẽ thấy con số về 64.
        // Vậy nên phép kiểm chỉ chốt hai đầu, không chốt một con số.
        long chuoiKetQua = 64;   // 19 ký tự: 22 byte header + 2×19, làm tròn bội số 8
        Kiem.Dung(aNoiSuy == chuoiKetQua || aNoiSuy == chuoiKetQua + 24,
            $"Nội suy = {aNoiSuy} byte: hoặc đúng chuỗi kết quả (tier-1), hoặc thừa 1 box (tier-0)");
    }

    // ---------------------------------------------------------------- H.2
    private static void ChayH2Closure()
    {
        Kiem.MoBai("H.2", "Closure — biến bị chia sẻ, không được sao chép");

        var tuFor = new List<Func<int>>();
        for (int i = 0; i < 8; i++) tuFor.Add(() => i);
        Kiem.Dung(tuFor.TrueForAll(f => f() == 8),
            "for: cả 8 lambda cùng trỏ một biến, đều trả về 8 — đây LÀ lỗi, không phải tính năng");

        var tuForSua = new List<Func<int>>();
        for (int i = 0; i < 8; i++) { int truc = i; tuForSua.Add(() => truc); }
        Kiem.Bang(tuForSua[0](), 0, "for + biến sao chép: lambda đầu trả 0");
        Kiem.Bang(tuForSua[7](), 7, "for + biến sao chép: lambda cuối trả 7");

        var tuForeach = new List<Func<int>>();
        foreach (int i in Enumerable.Range(0, 8)) tuForeach.Add(() => i);
        Kiem.Bang(tuForeach[0](), 0, "foreach: biến MỚI mỗi vòng (C# 5.0+) — không cần tự chép");
    }

    // ---------------------------------------------------------------- H.3
    private static void ChayH3TriHoan()
    {
        Kiem.MoBai("H.3", "Thực thi trì hoãn — LINQ chưa chạy khi bạn tưởng");

        var ds = new List<int> { 1, 2, 3 };
        var truyVan = ds.Where(x => x > 1);
        ds.Add(9);
        Kiem.Bang(truyVan.Count(), 3, "Truy vấn thấy cả phần tử thêm SAU khi nó được viết");

        int soLanChay = 0;
        var q2 = ds.Select(x => { soLanChay++; return x; });
        _ = q2.Count();
        _ = q2.ToList();
        Kiem.Bang(soLanChay, ds.Count * 2, "Duyệt hai lần thì thân lambda chạy hai lượt đầy đủ");

        // Ngoại lệ nổ ở chỗ DUYỆT, không ở chỗ dựng truy vấn.
        IEnumerable<string> q3 = [];
        bool nemLucDung = false;
        try { q3 = TenFileKhongCo.Select(File.ReadAllText); }
#pragma warning disable CA1031 // đang chứng minh rằng KHÔNG có gì để bắt
        catch (Exception) { nemLucDung = true; }
#pragma warning restore CA1031
        Kiem.Dung(!nemLucDung, "try bọc quanh chuỗi LINQ KHÔNG bắt được gì — chưa có gì chạy");

        bool nemLucDuyet = false;
        try { foreach (string _ in q3) { } }
        catch (FileNotFoundException) { nemLucDuyet = true; }
        Kiem.Dung(nemLucDuyet, "Ngoại lệ nổ ở dòng foreach, cách dòng gây lỗi rất xa");
    }

    // ---------------------------------------------------------------- H.4
    private static void ChayH4Yield()
    {
        Kiem.MoBai("H.4", "yield return — thân hàm chạy muộn");

        int thanChay = 0;
        IEnumerable<int> Sinh()
        {
            thanChay++;
            yield return 1;
            yield return 2;
        }

        var day = Sinh();
        Kiem.Bang(thanChay, 0, "Gọi hàm iterator KHÔNG chạy thân hàm — nên kiểm tham số ở đây nổ muộn");
        _ = day.First();
        Kiem.Bang(thanChay, 1, "Thân hàm chạy ở lần duyệt đầu tiên");
    }

    // ---------------------------------------------------------------- H.5
    private static void ChayH5Variance()
    {
        Kiem.MoBai("H.5", "in/out — hiệp biến và nghịch biến");

        List<TrucThu> trucs = [new TrucThu()];
        IEnumerable<IThietBiThu> raNgoai = trucs;          // nhờ IEnumerable<out T>
        Kiem.Bang(raNgoai.Count(), 1,
            "Hiệp biến: List<TrucThu> dùng được ở chỗ cần IEnumerable<IThietBiThu>");

        var chung = new XuLyChungThu();
        IXuLyThu<IThietBiThu> rong = chung;
        IXuLyThu<TrucThu> hep = rong;                      // nhờ IXuLyThu<in T>
        hep.Nhan(new TrucThu());
        Kiem.Bang(chung.SoLan, 1,
            "Nghịch biến: bộ xử lý kiểu rộng cắm được vào chỗ cần kiểu hẹp");
    }

    // ---------------------------------------------------------------- H.6
    private static void ChayH6Span()
    {
        Kiem.MoBai("H.6", "Span<T> — cửa sổ nhìn vào bộ nhớ, không chép");

        byte[] khung = [0x02, 0x01, 0x03, 0x00, 0x01, 0xE2, 0x40, 0x00, 0x03];

        long aSpan = ByteMoiLan(() =>
            _ = BinaryPrimitives.ReadInt32BigEndian(khung.AsSpan(3, 4)));

        long aChep = ByteMoiLan(() =>
        {
            byte[] than = new byte[4];
            Array.Copy(khung, 3, than, 0, 4);
            _ = BinaryPrimitives.ReadInt32BigEndian(than);
        });

        Kiem.Bang(aSpan, 0L, "AsSpan + đọc: 0 byte cấp phát");
        Kiem.Dung(aChep > 0, $"Array.Copy + đọc: {aChep} byte mỗi khung — nhân với 100 khung/giây");

        // Cửa sổ nhìn vào dữ liệu gốc, không phải bản sao.
        Span<byte> cuaSo = khung.AsSpan(3, 4);
        cuaSo[0] = 0xFF;
        Kiem.Bang(khung[3], (byte)0xFF, "Ghi qua Span sửa THẲNG mảng gốc — nó là cửa sổ, không phải bản sao");
        khung[3] = 0x00;
    }

    // ---------------------------------------------------------------- H.7
    private static void ChayH7Flags()
    {
        Kiem.MoBai("H.7", "[Flags] enum — nhiều cờ trong một con số");

        var co = CoTrucThu.DaVeGoc | CoTrucThu.ChamGioiHan | CoTrucThu.ServoBat;

        Kiem.Bang(co.ToString(), "DaVeGoc, ChamGioiHan, ServoBat",
            "[Flags] làm ToString() liệt kê tên cờ — đó là toàn bộ công dụng của chú thích này");
        Kiem.Bang((int)co, 21, "Giá trị là 1|4|16 = 21");
        Kiem.Dung(co.HasFlag(CoTrucThu.Khong), "HasFlag(Khong) LUÔN true — đừng dùng để kiểm 'không cờ nào'");
        Kiem.Dung(co == CoTrucThu.Khong == false, "So sánh với Khong mới là cách kiểm đúng");

        const CoTrucThu CanCoDeChay = CoTrucThu.DaVeGoc | CoTrucThu.ServoBat;
        const CoTrucThu CamChay = CoTrucThu.LoiServo | CoTrucThu.ChamGioiHan;

        Kiem.Dung((co & CanCoDeChay) == CanCoDeChay, "Đủ điều kiện cần: đã về gốc và servo đã bật");
        Kiem.Dung((co & CamChay) != 0, "Nhưng đang chạm giới hạn — điều kiện cấm");
        Kiem.Dung(!((co & CanCoDeChay) == CanCoDeChay && (co & CamChay) == 0),
            "Kết luận: KHÔNG được chạy — hai hằng số cờ thay cho một chuỗi && dài");

        var khongFlags = (KhongFlagsThu)21;
        Kiem.Bang(khongFlags.ToString(), "21",
            "Enum KHÔNG có [Flags]: ToString chỉ in số — không đọc được trong nhật ký");
    }

    // --- kiểu phụ trợ chỉ dùng cho phép kiểm ---
    private interface IThietBiThu;

    private sealed class TrucThu : IThietBiThu;

    private interface IXuLyThu<in T> { void Nhan(T ban); }

    private sealed class XuLyChungThu : IXuLyThu<IThietBiThu>
    {
        public int SoLan { get; private set; }
        public void Nhan(IThietBiThu ban) => SoLan++;
    }

    [Flags]
    private enum CoTrucThu
    {
        Khong = 0,
        DaVeGoc = 1 << 0,
        DangChay = 1 << 1,
        ChamGioiHan = 1 << 2,
        LoiServo = 1 << 3,
        ServoBat = 1 << 4,
    }

    private enum KhongFlagsThu { A = 1, B = 2, C = 4 }
}
