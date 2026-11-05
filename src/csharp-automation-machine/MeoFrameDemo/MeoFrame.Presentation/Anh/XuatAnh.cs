using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using MeoFrame.Domain;
using MeoFrame.Presentation.ViewModels;
using MeoFrame.Presentation.Views;

namespace MeoFrame.Presentation.Anh;

// Dựng các hình của Chương 8 và Chương 10 ra PNG, không cần mở cửa sổ:
//   dotnet run --project MeoFrame.Presentation -- --xuat-anh <thư mục>
// Mọi màn hình đều là XAML thật của project này; số tương phản được tính, không gõ tay.
internal static class XuatAnh
{
    private const double BeRong = 860;   // bằng khung nội dung của sách, nên chữ trong ảnh giữ đúng cỡ khi đọc

    public static void Chay(string thuMuc)
    {
        Directory.CreateDirectory(thuMuc);
        Luu(ManHinh(MainViewModel.MauBinhThuong(), coSoChiDan: true), thuMuc, "man_hinh_binh_thuong", 1920, 1080, 1);
        Luu(ManHinh(MainViewModel.MauCoCanhBao(), coSoChiDan: false), thuMuc, "man_hinh_co_canh_bao", 1920, 1080, 1);
        Luu(SoSanhNenMau(), thuMuc, "nen_mau_so_sanh");
        Luu(BangMauTuongPhan(), thuMuc, "bang_mau_tuong_phan");
        Luu(VienTrangThai(), thuMuc, "vien_trang_thai");
        Luu(DatVaThuc(), thuMuc, "dat_thuc");
        Luu(CanSo(), thuMuc, "can_so");
        Luu(TrangThaiNut(), thuMuc, "trang_thai_nut");
        Luu(ChipKetNoi(), thuMuc, "chip_ket_noi");
        Luu(BannerCanhBao(), thuMuc, "alarm_banner", BeRong, 132);
        Luu(TiLeHienThi(), thuMuc, "ti_le_hien_thi");
    }

    // ---------- Hạ tầng dựng ảnh ----------

    private static void Luu(FrameworkElement e, string thuMuc, string ten, double rong = BeRong, double cao = double.NaN, double tiLe = 2)
    {
        e.Measure(new Size(rong, double.IsNaN(cao) ? double.PositiveInfinity : cao));
        double caoThat = double.IsNaN(cao) ? e.DesiredSize.Height : cao;
        e.Arrange(new Rect(0, 0, rong, caoThat));
        e.UpdateLayout();
        Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);
        e.UpdateLayout();
        var anh = new RenderTargetBitmap((int)Math.Ceiling(rong * tiLe), (int)Math.Ceiling(caoThat * tiLe),
                                         96 * tiLe, 96 * tiLe, PixelFormats.Pbgra32);
        anh.Render(e);
        GhiPng(anh, System.IO.Path.Combine(thuMuc, ten + ".png"));
    }

    private static void GhiPng(BitmapSource anh, string duongDan)
    {
        var bo = new PngBitmapEncoder();
        bo.Frames.Add(BitmapFrame.Create(anh));
        using var tep = File.Create(duongDan);
        bo.Save(tep);
    }

    private static SolidColorBrush Mau(string hex) => (SolidColorBrush)new BrushConverter().ConvertFromString(hex)!;

    private static TextBlock Chu(string s, double co = 14, string mau = "#FFFFFF", FontWeight? dam = null, double duoi = 0) => new()
    {
        Text = s, FontFamily = new FontFamily("Segoe UI"), FontSize = co, Foreground = Mau(mau),
        FontWeight = dam ?? FontWeights.Normal, Margin = new Thickness(0, 0, 0, duoi), TextWrapping = TextWrapping.Wrap,
    };

    private static Border Hop(UIElement ben, string nen = "#1E1E1E", double dem = 12, string? vien = null, double day = 0, double bo = 4) => new()
    {
        Child = ben, Background = Mau(nen), Padding = new Thickness(dem), CornerRadius = new CornerRadius(bo),
        BorderBrush = vien is null ? Brushes.Transparent : Mau(vien), BorderThickness = new Thickness(day),
    };

    private static StackPanel Doc(params UIElement[] con)
    {
        var p = new StackPanel();
        foreach (var c in con) p.Children.Add(c);
        return p;
    }

    private static Grid HaiCot(UIElement trai, UIElement phai, double khe = 20)
    {
        var g = new Grid();
        g.ColumnDefinitions.Add(new ColumnDefinition());
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(khe) });
        g.ColumnDefinitions.Add(new ColumnDefinition());
        Grid.SetColumn(trai, 0);
        Grid.SetColumn(phai, 2);
        g.Children.Add(trai);
        g.Children.Add(phai);
        return g;
    }

    private static Border NenHinh(UIElement ben) => new() { Child = ben, Background = Brushes.White, Padding = new Thickness(0) };

    // ---------- Màn hình vận hành (mục 10.1.8) ----------

    private static FrameworkElement ManHinh(MainViewModel vm, bool coSoChiDan)
    {
        var goc = new Grid { Width = 1920, Height = 1080 };
        goc.Children.Add(new OperatorScreen { DataContext = vm });
        if (coSoChiDan)
        {
            // Năm câu hỏi của mục 10.1.8, đánh dấu đúng chỗ trả lời trên màn hình
            var nen = new Canvas();
            Dau(nen, 1, 300, 6);      // trạng thái máy
            Dau(nen, 2, 312, 194);    // bước của từng trạm
            Dau(nen, 3, 1360, 194);   // sản lượng, nhịp
            Dau(nen, 4, 332, 116);    // cảnh báo
            Dau(nen, 5, 1132, 232);   // phôi
            goc.Children.Add(nen);
        }
        return goc;
    }

    private static void Dau(Canvas nen, int so, double x, double y)
    {
        var o = new Grid { Width = 44, Height = 44 };
        o.Children.Add(new Ellipse { Fill = Mau("#4DA3FF"), Stroke = Brushes.White, StrokeThickness = 2 });
        o.Children.Add(new TextBlock
        {
            Text = so.ToString(), FontFamily = new FontFamily("Segoe UI"), FontSize = 26, FontWeight = FontWeights.Bold,
            Foreground = Mau("#10233A"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
        });
        Canvas.SetLeft(o, x);
        Canvas.SetTop(o, y);
        nen.Children.Add(o);
    }

    // ---------- Nền trung tính: cùng một cảnh báo trên hai màn hình (mục 10.2.1) ----------

    private static FrameworkElement SoSanhNenMau()
    {
        var trai = Doc(Chu("Mọi thứ đều có màu", 15, "#333333", FontWeights.SemiBold, 6), ManHinhNho(nhieuMau: true));
        var phai = Doc(Chu("Nền trung tính: màu chỉ dành cho bất thường", 15, "#333333", FontWeights.SemiBold, 6), ManHinhNho(nhieuMau: false));
        return NenHinh(HaiCot(trai, phai));
    }

    private static Border ManHinhNho(bool nhieuMau)
    {
        string Nen(string mau, string xam) => nhieuMau ? mau : xam;
        var p = new StackPanel();
        p.Children.Add(Hop(Chu("ĐANG CHẠY", 15, nhieuMau ? "#FFFFFF" : "#FFFFFF", FontWeights.SemiBold), Nen("#0066FF", "#2A2A2A"), 6, bo: 2));
        p.Children.Add(Dem(6));

        var luoi = new UniformGrid2(2);
        luoi.Them(TheNho("Trạm Cấp phôi", "Bước 2/4", Nen("#00A651", "#2A2A2A")));
        luoi.Them(TheNho("Trạm Gắp", "Bước 5/9", Nen("#00B3B3", "#2A2A2A")));
        luoi.Them(TheNho("Trạm Kiểm", "Bước 3/7", Nen("#8A2BE2", "#2A2A2A")));
        luoi.Them(TheNho("Trạm Xuất", "Bước 1/3", Nen("#FF8C00", "#2A2A2A")));
        p.Children.Add(luoi.Luoi);
        p.Children.Add(Dem(6));

        p.Children.Add(Chu("Áp suất khí nén dưới ngưỡng", 15, nhieuMau ? "#FF2020" : "#FF5252", FontWeights.Bold));
        p.Children.Add(Dem(6));

        var nut = new Grid();
        nut.ColumnDefinitions.Add(new ColumnDefinition());
        nut.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(6) });
        nut.ColumnDefinitions.Add(new ColumnDefinition());
        var a = Hop(Chu("Bắt đầu", 14), Nen("#00C853", "#3A3A3A"), 6);
        var b = Hop(Chu("Dừng", 14), Nen("#E91E63", "#3A3A3A"), 6);
        Grid.SetColumn(b, 2);
        nut.Children.Add(a);
        nut.Children.Add(b);
        p.Children.Add(nut);
        return Hop(p, Nen("#FFD54F", "#1E1E1E"), 10);
    }

    private static Border TheNho(string ten, string buoc, string nen) =>
        Hop(Doc(Chu(ten, 14, "#FFFFFF", FontWeights.SemiBold), Chu(buoc, 13, "#FFFFFF")), nen, 8);

    private static Border Dem(double cao) => new() { Height = cao };

    private sealed class UniformGrid2
    {
        public Grid Luoi { get; } = new();
        private readonly int _cot;
        private int _dem;
        public UniformGrid2(int cot)
        {
            _cot = cot;
            for (int i = 0; i < cot; i++) Luoi.ColumnDefinitions.Add(new ColumnDefinition());
        }
        public void Them(FrameworkElement e)
        {
            int hang = _dem / _cot;
            while (Luoi.RowDefinitions.Count <= hang) Luoi.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetRow(e, hang);
            Grid.SetColumn(e, _dem % _cot);
            e.Margin = new Thickness(_dem % _cot == 0 ? 0 : 3, hang == 0 ? 0 : 6, _dem % _cot == _cot - 1 ? 0 : 3, 0);
            Luoi.Children.Add(e);
            _dem++;
        }
    }

    // ---------- Bảng màu và tương phản tính được (mục 10.2.1, 10.2.2) ----------

    private static double TuongPhan(string a, string b)
    {
        static double Kenh(byte c)
        {
            double v = c / 255.0;
            return v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
        }
        static double DoSang(string hex)
        {
            var c = Mau(hex).Color;
            return (0.2126 * Kenh(c.R)) + (0.7152 * Kenh(c.G)) + (0.0722 * Kenh(c.B));
        }
        double x = DoSang(a), y = DoSang(b);
        return (Math.Max(x, y) + 0.05) / (Math.Min(x, y) + 0.05);
    }

    private static FrameworkElement BangMauTuongPhan()
    {
        var toi = CotMau("Nền tối #1E1E1E", "#1E1E1E", "#FFFFFF",
            ("Giá trị chính", "#FFFFFF"), ("Nhãn phụ", "#A0A0A0"), ("Chữ xám đậm", "#606060"),
            ("Critical, bản cũ", "#CC0000"), ("Critical", "#FF5252"), ("Major", "#FF8C00"), ("Minor", "#FFD700"),
            ("Normal, bản cũ", "#228B22"), ("Normal", "#3CB043"));
        var sang = CotMau("Nền sáng #F5F5F5", "#F5F5F5", "#212121",
            ("Giá trị chính", "#212121"), ("Nhãn phụ", "#616161"), ("Vàng giữ nguyên", "#FFD700"),
            ("Critical", "#CC0000"), ("Major, bản cũ", "#FF8C00"), ("Major", "#A85A00"), ("Minor, lần thử đầu", "#B8860B"),
            ("Minor", "#8A6D00"), ("Normal", "#1B7A2E"));
        return NenHinh(HaiCot(toi, sang, 12));
    }

    private static Border CotMau(string tieuDe, string nen, string mauChu, params (string Ten, string Hex)[] hang)
    {
        var g = new Grid();
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        g.ColumnDefinitions.Add(new ColumnDefinition());
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
        for (int i = 0; i < hang.Length; i++)
        {
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var (ten, hex) = hang[i];
            double tp = TuongPhan(hex, nen);
            bool dat = tp >= 4.5;
            Dat(g, Chu(hex, 14, hex, FontWeights.Bold), i, 0, new Thickness(0, 5, 12, 5));
            Dat(g, Chu(ten, 13, mauChu), i, 1, new Thickness(0, 5, 8, 5));
            Dat(g, Chu($"{tp:0.00}:1".Replace('.', ','), 13, mauChu, dat ? FontWeights.Normal : FontWeights.Bold), i, 2, new Thickness(0, 5, 8, 5));
            var dau = Chu(dat ? "✓" : "✗", 15, dat ? (nen == "#1E1E1E" ? "#3CB043" : "#1B7A2E") : (nen == "#1E1E1E" ? "#FF5252" : "#CC0000"), FontWeights.Bold);
            dau.FontFamily = new FontFamily("Segoe UI Symbol");
            dau.HorizontalAlignment = HorizontalAlignment.Center;
            Dat(g, dau, i, 3, new Thickness(0, 4, 0, 4));
        }
        return Hop(Doc(Chu(tieuDe, 15, mauChu, FontWeights.SemiBold, 8), g), nen, 14);
    }

    private static void Dat(Grid g, UIElement e, int hang, int cot, Thickness le)
    {
        Grid.SetRow(e, hang);
        Grid.SetColumn(e, cot);
        if (e is FrameworkElement f) { f.Margin = le; f.VerticalAlignment = VerticalAlignment.Center; }
        g.Children.Add(e);
    }

    // ---------- Viền trạng thái (mục 10.2.3) ----------

    private static FrameworkElement VienTrangThai()
    {
        var doi = HaiCot(
            Doc(Chu("Tô cả nền: chữ phụ khó đọc đúng lúc cần đọc", 14, "#333333", FontWeights.SemiBold, 6),
                TheTram("#CC0000", null)),
            Doc(Chu("Tô viền: phần ruột giữ nguyên", 14, "#333333", FontWeights.SemiBold, 6),
                TheTram("#2A2A2A", "#FF5252")));

        var nam = new Grid { Margin = new Thickness(0, 16, 0, 0) };
        (string Nhan, string? Vien)[] ds =
        {
            ("Bình thường", null), ("Đang có lỗi", "#FF5252"), ("Cần chú ý", "#FFD700"),
            ("Đang xử lý", "#4DA3FF"), ("Cần người thao tác", "#3CB043"),
        };
        for (int i = 0; i < ds.Length; i++)
        {
            nam.ColumnDefinitions.Add(new ColumnDefinition());
            var o = Hop(Doc(Chu("Trạm Gắp", 13, "#FFFFFF", FontWeights.SemiBold), Chu("Bước 5/9", 12, "#A0A0A0")),
                        "#2A2A2A", ds[i].Vien is null ? 12 : 9, ds[i].Vien, ds[i].Vien is null ? 0 : 3);
            var cot = Doc(new Border { Background = Mau("#1E1E1E"), Padding = new Thickness(8), Child = o },
                          Chu(ds[i].Nhan, 13, "#333333"));
            ((TextBlock)cot.Children[1]).Margin = new Thickness(0, 4, 0, 0);
            ((TextBlock)cot.Children[1]).TextAlignment = TextAlignment.Center;
            Grid.SetColumn(cot, i);
            cot.Margin = new Thickness(i == 0 ? 0 : 4, 0, i == ds.Length - 1 ? 0 : 4, 0);
            nam.Children.Add(cot);
        }
        return NenHinh(Doc(doi, nam));
    }

    private static Border TheTram(string nen, string? vien)
    {
        var the = Hop(Doc(Chu("Trạm Gắp", 18, "#FFFFFF", FontWeights.SemiBold, 2),
                          Chu("Bước 5/9: dừng, chờ khí nén", 14, "#A0A0A0", null, 2),
                          Chu("Trục Z   24,98 mm", 14, "#A0A0A0")),
                      nen, vien is null ? 15 : 12, vien, vien is null ? 0 : 3);
        return new Border { Background = Mau("#1E1E1E"), Padding = new Thickness(10), Child = the };
    }

    // ---------- Giá trị đặt và giá trị thực (mục 10.2.3) ----------

    private static FrameworkElement DatVaThuc()
    {
        var sai = Hop(Doc(Chu("Nhiệt độ", 15, "#A0A0A0", null, 4), Chu("250 °C", 36, "#FFFFFF", FontWeights.Medium, 6),
                          Chu("Đặt hay thực? Người đọc phải đoán.", 13, "#A0A0A0")), "#1E1E1E", 16);

        var o = new Border
        {
            Background = Mau("#111111"), BorderBrush = Mau("#6E6E6E"), BorderThickness = new Thickness(1),
            Padding = new Thickness(10, 2, 10, 2), MinWidth = 96, CornerRadius = new CornerRadius(3),
            Child = new TextBlock { Text = "250", FontFamily = new FontFamily("Segoe UI"), FontSize = 32, Foreground = Mau("#FFFFFF"), TextAlignment = TextAlignment.Right },
        };
        var dong = new Grid();
        dong.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        dong.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
        dong.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        dong.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var cot1 = Doc(Chu("Đặt (ô nhập)", 13, "#A0A0A0", null, 4), o);
        var thuc = Chu("180", 36, "#FFFFFF", FontWeights.Medium);
        thuc.TextAlignment = TextAlignment.Right;
        thuc.MinWidth = 96;
        var cot2 = Doc(Chu("Thực (chỉ đọc)", 13, "#A0A0A0", null, 4), thuc);
        var donVi = Chu("°C", 18, "#A0A0A0");
        donVi.VerticalAlignment = VerticalAlignment.Bottom;
        donVi.Margin = new Thickness(8, 0, 0, 8);
        Grid.SetColumn(cot1, 0);
        Grid.SetColumn(cot2, 2);
        Grid.SetColumn(donVi, 3);
        dong.Children.Add(cot1);
        dong.Children.Add(cot2);
        dong.Children.Add(donVi);
        var dung = Hop(Doc(Chu("Nhiệt độ", 15, "#A0A0A0", null, 4), dong), "#1E1E1E", 16);

        return NenHinh(HaiCot(
            Doc(Chu("Sai: một con số nhập nhằng", 14, "#333333", FontWeights.SemiBold, 6), sai),
            Doc(Chu("Đúng: hai giá trị, cùng một chỗ", 14, "#333333", FontWeights.SemiBold, 6), dung)));
    }

    // ---------- Căn số (mục 10.2.6b) ----------

    private static FrameworkElement CanSo()
    {
        string[] noi = { "9.5 mm", "12.75 mm", "108.2 mm" };
        var trai = new StackPanel();
        foreach (var s in noi) trai.Children.Add(Chu(s, 26, "#FFFFFF", FontWeights.Medium, 2));

        double[] so = { 9.5, 12.75, 108.2 };
        var phai = new Grid { HorizontalAlignment = HorizontalAlignment.Left };
        phai.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
        phai.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        for (int i = 0; i < so.Length; i++)
        {
            phai.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var t = Chu(so[i].ToString("F2", System.Globalization.CultureInfo.InvariantCulture), 26, "#FFFFFF", FontWeights.Medium, 2);
            t.TextAlignment = TextAlignment.Right;
            System.Windows.Documents.Typography.SetNumeralAlignment(t, FontNumeralAlignment.Tabular);
            Grid.SetRow(t, i);
            phai.Children.Add(t);
            var d = Chu("mm", 18, "#A0A0A0");
            d.Margin = new Thickness(10, 7, 0, 0);
            Grid.SetRow(d, i);
            Grid.SetColumn(d, 1);
            phai.Children.Add(d);
        }
        return NenHinh(HaiCot(
            Doc(Chu("Căn trái, đơn vị nối vào số", 14, "#333333", FontWeights.SemiBold, 6), Hop(trai, "#1E1E1E", 16)),
            Doc(Chu("Căn phải, đơn vị một cột riêng", 14, "#333333", FontWeights.SemiBold, 6), Hop(phai, "#1E1E1E", 16))));
    }

    // ---------- Trạng thái của một nút (mục 10.2.7b) ----------

    private static FrameworkElement TrangThaiNut()
    {
        var kieu = (Style)System.Windows.Application.Current.Resources["NutThuong"];
        Button Nut(object noiDung, bool bat = true, object? tag = null) => new()
        {
            Style = kieu, Content = noiDung, IsEnabled = bat, Tag = tag, FontSize = 16, MinHeight = 48, MinWidth = 150, Padding = new Thickness(8),
        };

        var ban = new StackPanel();
        ban.Children.Add(new TextBlock { Text = "Đang về gốc…", FontFamily = new FontFamily("Segoe UI"), FontSize = 16, Foreground = Mau("#D0D0D0"), TextAlignment = TextAlignment.Center });
        ban.Children.Add(new Border { Height = 3, Width = 84, Background = Mau("#4DA3FF"), HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 4, 0, 0) });

        var hang1 = new Grid();
        (string Nhan, Button N)[] ds =
        {
            ("Bình thường", Nut("Về gốc")),
            ("Đang nhấn: phải thấy đổi ngay", Nut("Về gốc", true, "DangNhan")),
            ("Đang bận: đổi nhãn, chặn bấm lại", Nut(ban, false)),
            ("Làm mờ: tạm thời chưa đủ điều kiện", Nut("Về gốc", false)),
        };
        for (int i = 0; i < ds.Length; i++)
        {
            hang1.ColumnDefinitions.Add(new ColumnDefinition());
            var nhan = Chu(ds[i].Nhan, 12, "#333333");
            nhan.Margin = new Thickness(0, 6, 0, 0);
            nhan.TextAlignment = TextAlignment.Center;
            var cot = Doc(new Border { Background = Mau("#1E1E1E"), Padding = new Thickness(14), Child = ds[i].N }, nhan);
            cot.Margin = new Thickness(i == 0 ? 0 : 4, 0, i == ds.Length - 1 ? 0 : 4, 0);
            Grid.SetColumn(cot, i);
            hang1.Children.Add(cot);
        }

        var den = new Ellipse { Width = 18, Height = 18, Fill = Mau("#3CB043"), Margin = new Thickness(0, 0, 8, 0), VerticalAlignment = VerticalAlignment.Center };
        var trangThai = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 16, 0), VerticalAlignment = VerticalAlignment.Center };
        trangThai.Children.Add(den);
        trangThai.Children.Add(Chu("Bơm đang bật", 16, "#FFFFFF"));
        var dungHang = new StackPanel { Orientation = Orientation.Horizontal };
        dungHang.Children.Add(trangThai);
        dungHang.Children.Add(Nut("Tắt bơm"));

        var hang2 = HaiCot(
            Doc(Chu("Sai: nhãn nói trạng thái hay nói hành động?", 14, "#333333", FontWeights.SemiBold, 6),
                new Border { Background = Mau("#1E1E1E"), Padding = new Thickness(14), Child = Nut("Bơm: BẬT") }),
            Doc(Chu("Đúng: đèn nói trạng thái, nút nói hành động", 14, "#333333", FontWeights.SemiBold, 6),
                new Border { Background = Mau("#1E1E1E"), Padding = new Thickness(14), Child = dungHang }));
        hang2.Margin = new Thickness(0, 18, 0, 0);

        return NenHinh(Doc(hang1, hang2));
    }

    // ---------- Chip kết nối (mục 10.2.11) và Alarm Banner (mục 10.3.3) ----------

    private static FrameworkElement ChipKetNoi()
    {
        var p = new StackPanel { Orientation = Orientation.Horizontal };
        foreach (var (ten, tt) in new[] { ("PLC", ConnectionState.Connected), ("Camera", ConnectionState.Reconnecting), ("Máy quét", ConnectionState.Disconnected) })
            p.Children.Add(new ConnectionChip { DataContext = new ChipModel(ten, tt), FontSize = 15, Margin = new Thickness(0, 0, 12, 0) });
        return Hop(p, "#1E1E1E", 14, bo: 0);
    }

    private static FrameworkElement BannerCanhBao()
    {
        var vm = new AlarmBannerViewModel();
        var luc = new DateTimeOffset(2026, 11, 5, 14, 32, 9, TimeSpan.Zero);
        vm.ActiveAlarms.Add(new AlarmModel(70012, "Trạm Gắp", AlarmSeverity.Critical, "Trạm Gắp: áp suất khí nén dưới ngưỡng, đã dừng trục", luc));
        vm.ActiveAlarms.Add(new AlarmModel(20007, "Trạm Kiểm", AlarmSeverity.Major, "Trạm Kiểm: camera mất 3 khung hình liên tiếp", luc.AddSeconds(-27)));
        vm.ActiveAlarms.Add(new AlarmModel(60003, "Trạm Cấp phôi", AlarmSeverity.Minor, "Trạm Cấp phôi: sắp hết khay, còn 2 khay", luc.AddMinutes(-3)) { Lifecycle = AlarmLifecycle.Acknowledged });
        vm.ActiveAlarms.Add(new AlarmModel(40021, "Hệ thống", AlarmSeverity.Warning, "Hệ thống: nhật ký đã xoay vòng sang tệp mới", luc.AddMinutes(-9)) { Lifecycle = AlarmLifecycle.Acknowledged });
        return Hop(new AlarmBanner { DataContext = vm }, "#1E1E1E", 8, bo: 0);
    }

    // ---------- Tỉ lệ hiển thị 150 %, không khai báo DPI (mục 8.1.6) ----------

    private static FrameworkElement TiLeHienThi()
    {
        var man = new OperatorScreen { DataContext = MainViewModel.MauBinhThuong() };
        man.Measure(new Size(1920, 1080));
        man.Arrange(new Rect(0, 0, 1920, 1080));
        man.UpdateLayout();
        Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);
        man.UpdateLayout();

        // Việc hệ điều hành làm: ứng dụng tin màn hình chỉ rộng 1280×720, vẽ ở cỡ đó, rồi ảnh bị kéo lên 1920×1080
        var nho = new RenderTargetBitmap(1280, 720, 96, 96, PixelFormats.Pbgra32);
        nho.Render(man);
        var toanCanh = new Image { Source = nho, Width = BeRong, Stretch = Stretch.Uniform };
        RenderOptions.SetBitmapScalingMode(toanCanh, BitmapScalingMode.Linear);

        // Một vùng nhỏ để nhìn rõ độ nét: bản vẽ lại đúng ở 150 % và bản kéo giãn từ 100 %
        var net = new RenderTargetBitmap(1920, 1620, 144, 144, PixelFormats.Pbgra32);
        net.Render(man);
        var banNet = new CroppedBitmap(net, new Int32Rect(30, 21, 420, 100));
        var banMo = new TransformedBitmap(new CroppedBitmap(nho, new Int32Rect(20, 14, 280, 67)), new ScaleTransform(1.5, 1.5));

        Image AnhPhong(BitmapSource s, BitmapScalingMode che)
        {
            var i = new Image { Source = s, Width = 420, Stretch = Stretch.Uniform };
            RenderOptions.SetBitmapScalingMode(i, che);
            return i;
        }

        var sub = HaiCot(
            Doc(Chu("Vẽ lại đúng ở 150 % (có khai báo DPI)", 13, "#333333", FontWeights.SemiBold, 4), AnhPhong(banNet, BitmapScalingMode.NearestNeighbor)),
            Doc(Chu("Vẽ ở 100 % rồi kéo giãn (không khai báo)", 13, "#333333", FontWeights.SemiBold, 4), AnhPhong(banMo, BitmapScalingMode.NearestNeighbor)));
        sub.Margin = new Thickness(0, 12, 0, 0);

        return NenHinh(Doc(
            Chu("Màn hình đặt 150 %, không khai báo DPI: phần dưới và phần phải bị cắt, nút Dừng không còn trên màn hình", 13, "#333333", FontWeights.SemiBold, 4),
            toanCanh, sub));
    }
}
