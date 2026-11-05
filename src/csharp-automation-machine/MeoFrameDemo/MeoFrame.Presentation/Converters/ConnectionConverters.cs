using System.Globalization;
using System.Windows.Data;
using MeoFrame.Domain;

namespace MeoFrame.Presentation.Converters;

// Mục 10.2.11, Chương 10
public sealed class ConnectionToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value switch
        {
            ConnectionState.Connected    => System.Windows.Application.Current.Resources["BrushConnectionOk"],
            ConnectionState.Reconnecting => System.Windows.Application.Current.Resources["BrushConnectionWarn"],
            _                            => System.Windows.Application.Current.Resources["BrushConnectionLost"],
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException("Chip kết nối chỉ hiển thị, không binding ngược.");
}

public sealed class ConnectionToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value switch
        {
            ConnectionState.Connected    => "OK",
            ConnectionState.Reconnecting => "Đang kết nối lại...",
            _                            => "Mất kết nối",
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException("Chip kết nối chỉ hiển thị, không binding ngược.");
}
