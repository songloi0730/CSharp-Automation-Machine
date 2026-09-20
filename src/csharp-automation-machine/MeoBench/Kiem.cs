// -------------------------------------------------------
// File:    Kiem.cs
// Project: MeoBench — lời giải mẫu cho Phụ lục G
// Purpose: Bộ tự kiểm tối giản, KHÔNG phụ thuộc thư viện ngoài.
//          Cố ý không dùng xUnit: máy tính công nghiệp ngoài hiện trường
//          không có Visual Studio và không chạy được `dotnet test`
//          (xem Chương 18 mục 18.6.4). Cùng một bộ kiểm này có thể gắn
//          vào một nút trên màn hình chẩn đoán của máy thật.
// -------------------------------------------------------
namespace MeoBench;

public static class Kiem
{
    private static int _dat, _hong;
    private static string _baiHienTai = "";

    public static void MoBai(string ma, string ten)
    {
        _baiHienTai = ma;
        Console.WriteLine();
        Console.WriteLine($"── {ma} · {ten} ──");
    }

    public static void Dung(bool dieuKien, string moTa)
        => Ghi(dieuKien, moTa, dieuKien ? "" : "điều kiện sai");

    public static void Bang<T>(T thucTe, T mongDoi, string moTa)
        => Ghi(EqualityComparer<T>.Default.Equals(thucTe, mongDoi), moTa,
               $"mong đợi <{mongDoi}>, thực tế <{thucTe}>");

    public static void Gan(double thucTe, double mongDoi, double dungSai, string moTa)
        => Ghi(Math.Abs(thucTe - mongDoi) <= dungSai, moTa,
               $"mong đợi {mongDoi} ± {dungSai}, thực tế {thucTe}");

    public static void Nem<TEx>(Action hanhDong, string moTa) where TEx : Exception
    {
        try { hanhDong(); Ghi(false, moTa, $"không ném gì, mong đợi {typeof(TEx).Name}"); }
        catch (TEx) { Ghi(true, moTa, ""); }
        catch (Exception ex) { Ghi(false, moTa, $"ném {ex.GetType().Name}, mong đợi {typeof(TEx).Name}"); }
    }

    public static async Task NemAsync<TEx>(Func<Task> hanhDong, string moTa) where TEx : Exception
    {
        try { await hanhDong(); Ghi(false, moTa, $"không ném gì, mong đợi {typeof(TEx).Name}"); }
        catch (TEx) { Ghi(true, moTa, ""); }
        catch (Exception ex) { Ghi(false, moTa, $"ném {ex.GetType().Name}, mong đợi {typeof(TEx).Name}"); }
    }

    /// <summary>Bắt ngoại lệ để kiểm nội dung của nó, trả về null nếu không ném.</summary>
    public static async Task<TEx?> BatAsync<TEx>(Func<Task> hanhDong) where TEx : Exception
    {
        try { await hanhDong(); return null; }
        catch (TEx ex) { return ex; }
    }

    private static void Ghi(bool dat, string moTa, string chiTiet)
    {
        if (dat) { _dat++; Console.WriteLine($"   ĐẠT   {moTa}"); }
        else     { _hong++; Console.WriteLine($"   HỎNG  {moTa}  →  {chiTiet}"); }
    }

    public static int TongKet()
    {
        Console.WriteLine();
        Console.WriteLine(new string('═', 58));
        Console.WriteLine($"  TỔNG: {_dat} đạt, {_hong} hỏng");
        Console.WriteLine(new string('═', 58));
        return _hong == 0 ? 0 : 1;
    }
}
