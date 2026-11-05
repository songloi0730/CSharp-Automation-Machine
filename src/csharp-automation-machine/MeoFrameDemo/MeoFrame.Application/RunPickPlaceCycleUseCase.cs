using MeoFrame.Domain;

namespace MeoFrame.Application;

public sealed record CycleResult(int CycleNumber, double AxisPosition, bool Success);

// Chu kỳ máy rút gọn, đủ để tập debug ở mục 2.3: vị trí trôi dần theo số chu kỳ, và tới chu kỳ 101 thì
// thành số âm. Đặt breakpoint có điều kiện "axisPosition < 0" trong MoveAxis là dừng đúng lần đó.
public sealed class RunPickPlaceCycleUseCase
{
    private const double StartPositionMm = 50.0;
    private const double DriftPerCycleMm = 0.5;
    private const double VelocityMmPerS  = 200.0;

    private readonly Axis _axisX;
    private int _cycleCount;

    public RunPickPlaceCycleUseCase(Axis axisX)
    {
        ArgumentNullException.ThrowIfNull(axisX);
        _axisX = axisX;
    }

    public int CycleCount => _cycleCount;
    public string LastError { get; private set; } = string.Empty;

    public CycleResult RunOneCycle()
    {
        _cycleCount++;
        double axisPosition = StartPositionMm - (_cycleCount * DriftPerCycleMm);
        bool success = MoveAxis(_axisX, axisPosition, VelocityMmPerS);
        return new CycleResult(_cycleCount, axisPosition, success);
    }

    private bool MoveAxis(Axis currentAxis, double axisPosition, double velocity)
    {
        if (!currentAxis.TryMoveAbs(axisPosition, velocity, out string error))
        {
            LastError = error;
            return false;
        }
        return true;
    }
}
