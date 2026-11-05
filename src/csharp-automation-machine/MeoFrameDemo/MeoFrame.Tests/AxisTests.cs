using MeoFrame.Domain;
using Moq;
using Xunit;

namespace MeoFrame.Tests;

// Code 18.1 và Code 18.2, Chương 18
public class AxisTests
{
    private static Axis CreateAxis(IMotionDriver driver) =>
        new(axisId: 1, name: "X", driver, maxVelocity: 500);

    [Fact]
    public void TryMoveAbs_KhiChuaConnect_TraVeFalse()
    {
        var mockDriver = new Mock<IMotionDriver>();
        var axis = CreateAxis(mockDriver.Object);

        bool ok = axis.TryMoveAbs(position: 100, velocity: 50, out string error);

        Assert.False(ok);
        Assert.Contains("chưa kết nối", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryMoveAbs_KhiAlarmDangBat_TraVeFalse()
    {
        var mockDriver = new Mock<IMotionDriver>();
        var axis = CreateAxis(mockDriver.Object);
        axis.SimulateConnect();
        axis.SimulateAlarm();

        bool ok = axis.TryMoveAbs(position: 100, velocity: 50, out string error);

        Assert.False(ok);
        Assert.Contains("báo lỗi", error, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(1,   true)]    // velocity = 1 (tối thiểu hợp lệ)
    [InlineData(500, true)]    // velocity = 500 (đúng bằng max)
    [InlineData(0,   false)]   // velocity = 0 (không hợp lệ)
    [InlineData(501, false)]   // velocity = 501 (vượt max)
    [InlineData(-10, false)]   // velocity âm
    [InlineData(double.NaN, false)]   // velocity NaN (số tính hỏng): dòng hay bị quên nhất
    public void TryMoveAbs_KiemTraGioiHanVelocity(double velocity, bool expectedOk)
    {
        var mockDriver = new Mock<IMotionDriver>();
        // Khi driver được gọi, giả định thành công
        mockDriver
            .Setup(d => d.TryMoveAbs(It.IsAny<int>(), It.IsAny<double>(),
                                      It.IsAny<double>(), out It.Ref<string>.IsAny))
            .Returns(true);

        var axis = CreateAxis(mockDriver.Object);   // maxVelocity = 500
        axis.SimulateConnect();
        axis.SimulateServoOn();   // ServoOn=true để chỉ velocity check ảnh hưởng kết quả

        bool ok = axis.TryMoveAbs(100, velocity, out _);

        Assert.Equal(expectedOk, ok);
    }
}
