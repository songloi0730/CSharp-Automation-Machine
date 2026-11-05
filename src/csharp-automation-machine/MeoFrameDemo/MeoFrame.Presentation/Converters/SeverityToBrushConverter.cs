using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using MeoFrame.Domain;

namespace MeoFrame.Presentation.Converters;

// Code 10.4, Chương 10
public sealed class SeverityToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not AlarmSeverity severity)
            return DependencyProperty.UnsetValue;

        return severity switch
        {
            AlarmSeverity.Critical => System.Windows.Application.Current?.Resources["BrushAlarmCritical"]
                                      ?? Brushes.DarkRed,
            AlarmSeverity.Major    => System.Windows.Application.Current?.Resources["BrushAlarmMajor"]
                                      ?? Brushes.DarkOrange,
            AlarmSeverity.Minor    => System.Windows.Application.Current?.Resources["BrushAlarmMinor"]
                                      ?? Brushes.Goldenrod,
            _                      => Brushes.LightGray // Warning (Ch15): chỉ thông tin, không dùng màu alarm
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException("Chỉ dùng một chiều: AlarmSeverity -> Brush.");
}
