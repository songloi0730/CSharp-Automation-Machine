namespace MeoFrame.Domain;

// Code 4.1a, Chương 4: Axis chỉ biết tới ba dòng này, không biết SDK hãng nào.
public interface IMotionDriver
{
    bool TryMoveAbs(int axisId, double position, double velocity, out string error);
    bool TryHome(int axisId, out string error);
    double ReadPosition(int axisId);
}
