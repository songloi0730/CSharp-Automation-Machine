// -------------------------------------------------------
// File:    Program.cs
// Project: MeoFrameMini — chương trình mẫu của sách "C# cho Automation Machine"
// Purpose: Một cỗ máy gắp-đặt thu nhỏ, CHẠY ĐƯỢC THẬT, không cần phần cứng.
//          Đọc kèm Chương 7 mục 7.4 (thứ tự viết mã).
// -------------------------------------------------------
using System.Diagnostics;

// ══════════════════════════════════════════════════════════════════
// TẦNG 1 — KIỂU DỮ LIỆU MIỀN. Không phụ thuộc vào bất cứ thứ gì.
// ══════════════════════════════════════════════════════════════════

/// <summary>Một lần đọc áp suất khí nén tại một thời điểm.</summary>
public readonly record struct PressureReading(double Bar, TimeSpan At);

/// <summary>Trạng thái tổng của máy. Chỉ MachineController được phép đổi.</summary>
public enum MachineState { Idle, Homing, Running, Alarm }

/// <summary>Mã cảnh báo — dải 30xxx dành cho cảm biến (xem Chương 15).</summary>
public static class AlarmCodes
{
    public const int AirPressureLow = 30001;
    public const int AxisTimeout    = 10001;
}

/// <summary>Lỗi có thể lường trước, người vận hành xử lý được.</summary>
public sealed class AlarmException : Exception
{
    // Lưu ý: KHÔNG đặt tên thuộc tính là Source — Exception đã có sẵn Source,
    // đặt trùng sẽ bị lỗi biên dịch CS0114. Dùng Station theo quy ước của sách.
    public AlarmException(int alarmCode, string station, string message) : base(message)
    {
        AlarmCode = alarmCode;
        Station   = station;
    }

    public int    AlarmCode { get; }
    public string Station   { get; }
}

// ══════════════════════════════════════════════════════════════════
// TẦNG 2 — INTERFACE NĂNG LỰC. Chỉ phụ thuộc tầng 1.
// ══════════════════════════════════════════════════════════════════

public interface IPressureSensor
{
    Task<PressureReading> ReadAsync(CancellationToken ct = default);
}

public interface IAxis
{
    string Name     { get; }
    double PositionMm { get; }
    bool   IsHomed  { get; }

    Task HomeAsync(CancellationToken ct = default);
    Task MoveToAsync(double targetMm, CancellationToken ct = default);
}

public interface IGripper
{
    bool IsGripping { get; }
    Task GripAsync(CancellationToken ct = default);
    Task ReleaseAsync(CancellationToken ct = default);
}

// ══════════════════════════════════════════════════════════════════
// TẦNG 3 — BẢN GIẢ LẬP. Phụ thuộc tầng 2. Tới đây đã CHẠY được.
// ══════════════════════════════════════════════════════════════════

public sealed class SimulatedPressureSensor : IPressureSensor
{
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private double _bar;                         // field: trạng thái riêng

    public SimulatedPressureSensor(double startBar = 6.00, double dropPerRead = 0.08)
    {
        _bar        = startBar;
        DropPerRead = dropPerRead;
    }

    /// <summary>Mỗi lần đọc thì tụt bấy nhiêu — giả lập rò rỉ để thấy được cảnh báo.</summary>
    public double DropPerRead { get; init; }

    public Task<PressureReading> ReadAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        _bar -= DropPerRead;
        return Task.FromResult(new PressureReading(_bar, _clock.Elapsed));
    }
}

public sealed class SimulatedAxis : IAxis
{
    private double _positionMm;
    private bool   _isHomed;

    public SimulatedAxis(string name, double speedMmPerStep = 40.0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name           = name;
        SpeedMmPerStep = speedMmPerStep;
    }

    public string Name           { get; }
    public double SpeedMmPerStep { get; init; }
    public double PositionMm     => _positionMm;      // thuộc tính chỉ đọc
    public bool   IsHomed        => _isHomed;

    public async Task HomeAsync(CancellationToken ct = default)
    {
        await MoveToAsync(0.0, ct).ConfigureAwait(false);
        _isHomed = true;
    }

    public async Task MoveToAsync(double targetMm, CancellationToken ct = default)
    {
        // Giả lập trục chạy dần tới đích, mỗi vòng một bước.
        while (Math.Abs(_positionMm - targetMm) > 0.001)
        {
            ct.ThrowIfCancellationRequested();
            var delta = Math.Clamp(targetMm - _positionMm, -SpeedMmPerStep, SpeedMmPerStep);
            _positionMm += delta;
            await Task.Delay(5, ct).ConfigureAwait(false);
        }
    }
}

public sealed class SimulatedGripper : IGripper
{
    public bool IsGripping { get; private set; }     // property có set riêng tư

    public async Task GripAsync(CancellationToken ct = default)
    {
        await Task.Delay(20, ct).ConfigureAwait(false);
        IsGripping = true;
    }

    public async Task ReleaseAsync(CancellationToken ct = default)
    {
        await Task.Delay(20, ct).ConfigureAwait(false);
        IsGripping = false;
    }
}

// ══════════════════════════════════════════════════════════════════
// TẦNG 4 — LỚP NGHIỆP VỤ. Luật của máy nằm ở đây.
// ══════════════════════════════════════════════════════════════════

public sealed class PressureEventArgs : EventArgs
{
    public PressureEventArgs(PressureReading reading, double minimumBar)
    {
        Reading    = reading;
        MinimumBar = minimumBar;
    }

    public PressureReading Reading    { get; }
    public double          MinimumBar { get; }
}

public sealed class PressureMonitor
{
    private readonly IPressureSensor _sensor;     // field readonly — gán một lần
    private PressureReading _last;                // field thường  — đổi liên tục

    public PressureMonitor(IPressureSensor sensor)
    {
        ArgumentNullException.ThrowIfNull(sensor);
        _sensor = sensor;
    }

    public double           MinimumBar { get; init; } = 5.00;          // cấu hình
    public PressureReading  Last       => _last;                       // chỉ đọc
    public bool             IsTooLow   => _last.Bar < MinimumBar;      // tính toán

    public event EventHandler<PressureEventArgs>? PressureTooLow;

    public async Task<PressureReading> PollAsync(CancellationToken ct = default)
    {
        _last = await _sensor.ReadAsync(ct).ConfigureAwait(false);
        if (IsTooLow)
            PressureTooLow?.Invoke(this, new PressureEventArgs(_last, MinimumBar));
        return _last;
    }
}

/// <summary>Một bước trong chu trình. Mọi bước đều cùng hình dạng này.</summary>
public interface IStep
{
    string Name { get; }
    Task ExecuteAsync(CancellationToken ct = default);
}

public sealed class StepMoveTo : IStep
{
    private readonly IAxis  _axis;
    private readonly double _targetMm;

    public StepMoveTo(IAxis axis, double targetMm, string name)
    {
        ArgumentNullException.ThrowIfNull(axis);
        _axis     = axis;
        _targetMm = targetMm;
        Name      = name;
    }

    public string Name { get; }

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        using var toCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        toCts.CancelAfter(TimeSpan.FromSeconds(5));            // hạn giờ: BẮT BUỘC
        try
        {
            await _axis.MoveToAsync(_targetMm, toCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new AlarmException(AlarmCodes.AxisTimeout, _axis.Name,
                $"Trục {_axis.Name} quá thời gian khi đi tới {_targetMm:F1} mm");
        }
    }
}

public sealed class StepGrip : IStep
{
    private readonly IGripper _gripper;
    private readonly bool     _grip;

    public StepGrip(IGripper gripper, bool grip, string name)
    {
        ArgumentNullException.ThrowIfNull(gripper);
        _gripper = gripper;
        _grip    = grip;
        Name     = name;
    }

    public string Name { get; }

    public Task ExecuteAsync(CancellationToken ct = default) =>
        _grip ? _gripper.GripAsync(ct) : _gripper.ReleaseAsync(ct);
}

public sealed class MachineController
{
    private readonly IAxis           _axis;
    private readonly PressureMonitor _pressure;
    private readonly IReadOnlyList<IStep> _steps;

    public MachineController(IAxis axis, PressureMonitor pressure, IReadOnlyList<IStep> steps)
    {
        ArgumentNullException.ThrowIfNull(axis);
        ArgumentNullException.ThrowIfNull(pressure);
        ArgumentNullException.ThrowIfNull(steps);
        _axis     = axis;
        _pressure = pressure;
        _steps    = steps;
    }

    public MachineState State      { get; private set; } = MachineState.Idle;
    public int          CycleCount { get; private set; }
    public string?      AlarmText  { get; private set; }

    public event EventHandler<string>? Reported;

    private void Report(string text) => Reported?.Invoke(this, text);

    public async Task RunAsync(int maxCycles, CancellationToken ct = default)
    {
        State = MachineState.Homing;
        Report("Về gốc…");
        await _axis.HomeAsync(ct).ConfigureAwait(false);
        Report($"Đã về gốc, vị trí {_axis.PositionMm:F1} mm");

        State = MachineState.Running;

        while (CycleCount < maxCycles && !ct.IsCancellationRequested)
        {
            try
            {
                // Điều kiện tiên quyết: kiểm TRƯỚC mỗi chu kỳ, không phải một lần lúc khởi động.
                await _pressure.PollAsync(ct).ConfigureAwait(false);
                if (_pressure.IsTooLow)
                    throw new AlarmException(AlarmCodes.AirPressureLow, "AIR",
                        $"Áp suất {_pressure.Last.Bar:F2} bar < ngưỡng {_pressure.MinimumBar:F2} bar");

                foreach (var step in _steps)
                    await step.ExecuteAsync(ct).ConfigureAwait(false);

                CycleCount++;
                Report($"Chu kỳ {CycleCount,2} xong · áp suất {_pressure.Last.Bar:F2} bar");
            }
            catch (AlarmException ex)
            {
                State     = MachineState.Alarm;
                AlarmText = $"[{ex.AlarmCode}] {ex.Station}: {ex.Message}";
                Report($"CẢNH BÁO {AlarmText}");
                return;                       // dừng chu trình, chờ người xử lý
            }
        }

        State = MachineState.Idle;
        Report($"Dừng bình thường sau {CycleCount} chu kỳ");
    }
}

// ══════════════════════════════════════════════════════════════════
// TẦNG 5 — COMPOSITION ROOT. Nơi DUY NHẤT biết class cụ thể.
// ══════════════════════════════════════════════════════════════════

public static class Program
{
    public static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // --- tạo thiết bị (bản giả lập) ---
        IPressureSensor sensor  = new SimulatedPressureSensor(startBar: 6.00, dropPerRead: 0.08);
        IAxis           axisX   = new SimulatedAxis("X", speedMmPerStep: 40.0);
        IGripper        gripper = new SimulatedGripper();

        // --- lớp nghiệp vụ ---
        var pressure = new PressureMonitor(sensor) { MinimumBar = 5.00 };

        var steps = new IStep[]
        {
            new StepMoveTo(axisX, 120.0, "Tới vị trí gắp"),
            new StepGrip(gripper, grip: true,  "Gắp"),
            new StepMoveTo(axisX,  20.0, "Tới vị trí đặt"),
            new StepGrip(gripper, grip: false, "Nhả"),
        };

        var machine = new MachineController(axisX, pressure, steps);

        // --- đăng ký nghe: giao diện (ở đây là Console) chỉ NGHE, không điều khiển ---
        machine.Reported     += (_, text) => Console.WriteLine($"  {text}");
        pressure.PressureTooLow += (_, e) =>
            Console.WriteLine($"  ! áp suất thấp: {e.Reading.Bar:F2} < {e.MinimumBar:F2} bar");

        // --- chạy ---
        Console.WriteLine("=== MeoFrameMini — máy gắp-đặt thu nhỏ ===");
        await machine.RunAsync(maxCycles: 20).ConfigureAwait(false);

        Console.WriteLine();
        Console.WriteLine($"Trạng thái cuối : {machine.State}");
        Console.WriteLine($"Số chu kỳ chạy  : {machine.CycleCount}");
        Console.WriteLine($"Cảnh báo        : {machine.AlarmText ?? "(không có)"}");
    }
}
