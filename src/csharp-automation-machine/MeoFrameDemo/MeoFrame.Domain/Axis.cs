namespace MeoFrame.Domain;

// Code 4.1b và 4.1c, Chương 4, thêm hai hàm nối thiết bị và các helper cho phép kiểm (mục 18.2.2).
public sealed class Axis
{
    // Định danh: chỉ đọc, gán trong constructor
    public int    AxisId { get; }
    public string Name   { get; }

    // Cấu hình: đọc từ file lúc khởi tạo
    public double MaxVelocity { get; }

    // Trạng thái runtime: ai cũng ĐỌC được, chỉ Axis tự GHI (đóng gói)
    public bool   Connected { get; private set; }
    public bool   ServoOn   { get; private set; }
    public bool   Alarm     { get; private set; }
    public double Position  { get; private set; }

    private readonly IMotionDriver _driver;   // phụ thuộc interface, KHÔNG phải SDK hãng cụ thể

    public Axis(int axisId, string name, IMotionDriver driver, double maxVelocity)
    {
        ArgumentNullException.ThrowIfNull(driver);
        AxisId = axisId;
        Name = name;
        _driver = driver;
        MaxVelocity = maxVelocity;
    }

    public void Connect() => Connected = true;
    public void EnableServo() => ServoOn = Connected;

    // Hành vi: chặn "dùng sai" sớm, trả Try-result thay vì throw trong hot path
    public bool TryMoveAbs(double position, double velocity, out string error)
    {
        error = string.Empty;
        if (!Connected)                          { error = "Axis chưa kết nối."; return false; }
        if (Alarm)                               { error = "Axis đang báo lỗi."; return false; }
        if (!ServoOn)                            { error = "Servo đang OFF.";    return false; }
        // IsFinite trước: NaN làm mọi phép so sánh thành sai, nên "<= 0 || > Max" để nó lọt qua (Phụ lục F mục F.4b)
        if (!double.IsFinite(velocity) || velocity <= 0 || velocity > MaxVelocity)
                                                 { error = $"Vận tốc vượt giới hạn {MaxVelocity}."; return false; }

        bool ok = _driver.TryMoveAbs(AxisId, position, velocity, out error);
        if (ok) Position = _driver.ReadPosition(AxisId);
        return ok;
    }

    // Helper chỉ dùng trong phép kiểm: đặt trạng thái cần thiết mà không cần phần cứng
    internal void SimulateConnect() => Connected = true;
    internal void SimulateServoOn() => ServoOn = true;
    internal void SimulateAlarm() => Alarm = true;
}
