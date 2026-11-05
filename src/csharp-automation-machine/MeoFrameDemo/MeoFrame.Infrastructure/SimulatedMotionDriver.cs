using MeoFrame.Domain;

namespace MeoFrame.Infrastructure;

// Bản giả lập của IMotionDriver: chạy được khi không có card chuyển động thật.
public sealed class SimulatedMotionDriver : IMotionDriver
{
    private readonly Dictionary<int, double> _positions = new();

    public bool TryMoveAbs(int axisId, double position, double velocity, out string error)
    {
        error = string.Empty;
        _positions[axisId] = position;
        return true;
    }

    public bool TryHome(int axisId, out string error)
    {
        error = string.Empty;
        _positions[axisId] = 0;
        return true;
    }

    public double ReadPosition(int axisId) => _positions.GetValueOrDefault(axisId);
}
