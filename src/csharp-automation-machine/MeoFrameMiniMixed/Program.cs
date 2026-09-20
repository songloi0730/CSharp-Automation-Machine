// -------------------------------------------------------
// File:    Program.cs
// Project: MeoFrameMiniMixed — pha trộn: thiết bị gọi thẳng, trình tự giữ IStep của chương trình mẫu
// Purpose: Cùng một cỗ máy gắp-đặt như MeoFrameMini, nhưng viết theo lối
//          CHẶN (blocking) + một luồng chạy riêng — đúng lối mà phần lớn
//          phần mềm máy ngoài thực tế đang dùng. Đọc kèm Chương 7 mục 7.6.
// -------------------------------------------------------
using System.Diagnostics;

// ══════════════════════════════════════════════════════════════════
// TẦNG 1 — KIỂU DỮ LIỆU MIỀN. Giống hệt bản async: không đổi một chữ.
// ══════════════════════════════════════════════════════════════════

public readonly record struct PressureReading(double Bar, TimeSpan At);

public enum MachineState { Idle, Homing, Running, Alarm }

public static class AlarmCodes
{
    public const int AirPressureLow = 30001;
    public const int AxisTimeout    = 10001;
}

public sealed class AlarmException : Exception
{
    public AlarmException(int alarmCode, string station, string message) : base(message)
    {
        AlarmCode = alarmCode;
        Station   = station;
    }

    public int    AlarmCode { get; }
    public string Station   { get; }
}

// ══════════════════════════════════════════════════════════════════
// TẦNG 2 — INTERFACE. Khác bản async ĐÚNG MỘT ĐIỂM: không có Task,
//          không có CancellationToken. Chữ ký gọn hơn hẳn.
// ══════════════════════════════════════════════════════════════════

/// <summary>
/// Thay cho CancellationToken: một cờ dừng dùng chung, phải TỰ KIỂM TRA ở mọi
/// vòng lặp. Đây chính là chỗ lối viết chặn đắt hơn — xem Chương 5 mục 5.3.1.
/// </summary>
public sealed class StopFlag
{
    private volatile bool _stop;          // volatile: luồng khác đọc thấy ngay

    public bool IsStopRequested => _stop;
    public void Request() => _stop = true;

    /// <summary>Ném lỗi nếu đã có yêu cầu dừng — bản thủ công của ThrowIfCancellationRequested.</summary>
    public void ThrowIfStopRequested()
    {
        if (_stop) throw new OperationCanceledException("Người vận hành bấm Dừng");
    }
}

// ══════════════════════════════════════════════════════════════════
// TẦNG 3 — BẢN GIẢ LẬP. Thread.Sleep thay cho await Task.Delay.
// ══════════════════════════════════════════════════════════════════

public sealed class PressureSensor
{
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private double _bar;

    public PressureSensor(double startBar = 6.00, double dropPerRead = 0.08)
    {
        _bar        = startBar;
        DropPerRead = dropPerRead;
    }

    public double DropPerRead { get; init; }

    public PressureReading Read()
    {
        _bar -= DropPerRead;
        return new PressureReading(_bar, _clock.Elapsed);
    }
}

public sealed class Axis
{
    private readonly StopFlag _stop;
    private double _positionMm;
    private bool   _isHomed;

    public Axis(string name, StopFlag stop, double speedMmPerStep = 40.0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(stop);
        Name           = name;
        _stop          = stop;
        SpeedMmPerStep = speedMmPerStep;
    }

    public string Name           { get; }
    public double SpeedMmPerStep { get; init; }
    public double PositionMm     => _positionMm;
    public bool   IsHomed        => _isHomed;

    public void Home()
    {
        MoveTo(0.0);
        _isHomed = true;
    }

    public void MoveTo(double targetMm)
    {
        while (Math.Abs(_positionMm - targetMm) > 0.001)
        {
            _stop.ThrowIfStopRequested();          // PHẢI nhớ gọi — không ai nhắc
            var delta = Math.Clamp(targetMm - _positionMm, -SpeedMmPerStep, SpeedMmPerStep);
            _positionMm += delta;
            Thread.Sleep(5);                       // chặn nguyên luồng này 5 ms
        }
    }
}

public sealed class Gripper
{
    public bool IsGripping { get; private set; }

    public void Grip()
    {
        Thread.Sleep(20);
        IsGripping = true;
    }

    public void Release()
    {
        Thread.Sleep(20);
        IsGripping = false;
    }
}

// ══════════════════════════════════════════════════════════════════
// TẦNG 4 — LỚP NGHIỆP VỤ. Logic giống hệt; chỉ bỏ async/await/ct.
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
    private readonly PressureSensor _sensor;
    private PressureReading _last;

    public PressureMonitor(PressureSensor sensor)
    {
        ArgumentNullException.ThrowIfNull(sensor);
        _sensor = sensor;
    }

    public double          MinimumBar { get; init; } = 5.00;
    public PressureReading Last       => _last;
    public bool            IsTooLow   => _last.Bar < MinimumBar;

    public event EventHandler<PressureEventArgs>? PressureTooLow;

    public PressureReading Poll()
    {
        _last = _sensor.Read();
        if (IsTooLow)
            PressureTooLow?.Invoke(this, new PressureEventArgs(_last, MinimumBar));
        return _last;
    }
}

public interface IStep
{
    string Name { get; }
    void Execute();
}

public sealed class StepMoveTo : IStep
{
    private readonly Axis     _axis;
    private readonly double   _targetMm;
    private readonly StopFlag _stop;

    public StepMoveTo(Axis axis, double targetMm, string name, StopFlag stop)
    {
        ArgumentNullException.ThrowIfNull(axis);
        ArgumentNullException.ThrowIfNull(stop);
        _axis     = axis;
        _targetMm = targetMm;
        _stop     = stop;
        Name      = name;
    }

    public string Name { get; }

    public void Execute()
    {
        // Hạn giờ phải TỰ dựng bằng đồng hồ + luồng canh, vì không có
        // CancellationTokenSource.CancelAfter để nhờ.
        var sw      = Stopwatch.StartNew();
        var timeout = TimeSpan.FromSeconds(5);

        var worker = new Thread(() => _axis.MoveTo(_targetMm)) { IsBackground = true };
        worker.Start();

        while (worker.IsAlive)
        {
            if (sw.Elapsed > timeout)
                throw new AlarmException(AlarmCodes.AxisTimeout, _axis.Name,
                    $"Trục {_axis.Name} quá thời gian khi đi tới {_targetMm:F1} mm");
            _stop.ThrowIfStopRequested();
            Thread.Sleep(2);
        }
    }
}

public sealed class StepGrip : IStep
{
    private readonly Gripper  _gripper;
    private readonly bool     _grip;

    public StepGrip(Gripper gripper, bool grip, string name)
    {
        ArgumentNullException.ThrowIfNull(gripper);
        _gripper = gripper;
        _grip    = grip;
        Name     = name;
    }

    public string Name { get; }

    public void Execute()
    {
        if (_grip) _gripper.Grip();
        else       _gripper.Release();
    }
}

public sealed class MachineController
{
    private readonly Axis                 _axis;
    private readonly PressureMonitor      _pressure;
    private readonly IReadOnlyList<IStep> _steps;
    private readonly StopFlag             _stop;

    public MachineController(Axis axis, PressureMonitor pressure,
                             IReadOnlyList<IStep> steps, StopFlag stop)
    {
        ArgumentNullException.ThrowIfNull(axis);
        ArgumentNullException.ThrowIfNull(pressure);
        ArgumentNullException.ThrowIfNull(steps);
        ArgumentNullException.ThrowIfNull(stop);
        _axis     = axis;
        _pressure = pressure;
        _steps    = steps;
        _stop     = stop;
    }

    public MachineState State      { get; private set; } = MachineState.Idle;
    public int          CycleCount { get; private set; }
    public string?      AlarmText  { get; private set; }

    public event EventHandler<string>? Reported;

    private void Report(string text) => Reported?.Invoke(this, text);

    public void Run(int maxCycles)
    {
        State = MachineState.Homing;
        Report("Về gốc…");
        _axis.Home();
        Report($"Đã về gốc, vị trí {_axis.PositionMm:F1} mm");

        State = MachineState.Running;

        while (CycleCount < maxCycles && !_stop.IsStopRequested)
        {
            try
            {
                _pressure.Poll();
                if (_pressure.IsTooLow)
                    throw new AlarmException(AlarmCodes.AirPressureLow, "AIR",
                        $"Áp suất {_pressure.Last.Bar:F2} bar < ngưỡng {_pressure.MinimumBar:F2} bar");

                foreach (var step in _steps)
                    step.Execute();

                CycleCount++;
                Report($"Chu kỳ {CycleCount,2} xong · áp suất {_pressure.Last.Bar:F2} bar");
            }
            catch (AlarmException ex)
            {
                State     = MachineState.Alarm;
                AlarmText = $"[{ex.AlarmCode}] {ex.Station}: {ex.Message}";
                Report($"CẢNH BÁO {AlarmText}");
                return;
            }
            catch (OperationCanceledException)
            {
                Report("Dừng theo yêu cầu người vận hành");
                State = MachineState.Idle;
                return;
            }
        }

        State = MachineState.Idle;
        Report($"Dừng bình thường sau {CycleCount} chu kỳ");
    }
}

// ══════════════════════════════════════════════════════════════════
// TẦNG 5 — COMPOSITION ROOT. Chu trình chạy trên MỘT LUỒNG RIÊNG,
//          vì Run() chặn — luồng chính phải rảnh để còn nhận lệnh Dừng.
// ══════════════════════════════════════════════════════════════════

public static class Program
{
    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var stop = new StopFlag();

        var sensor  = new PressureSensor(startBar: 6.00, dropPerRead: 0.08);
        var axisX   = new Axis("X", stop, speedMmPerStep: 40.0);
        var gripper = new Gripper();

        var pressure = new PressureMonitor(sensor) { MinimumBar = 5.00 };

        var steps = new IStep[]
        {
            new StepMoveTo(axisX, 120.0, "Tới vị trí gắp", stop),
            new StepGrip(gripper, grip: true,  "Gắp"),
            new StepMoveTo(axisX,  20.0, "Tới vị trí đặt", stop),
            new StepGrip(gripper, grip: false, "Nhả"),
        };

        var machine = new MachineController(axisX, pressure, steps, stop);

        machine.Reported        += (_, text) => Console.WriteLine($"  {text}");
        pressure.PressureTooLow += (_, e) =>
            Console.WriteLine($"  ! áp suất thấp: {e.Reading.Bar:F2} < {e.MinimumBar:F2} bar");

        Console.WriteLine("=== MeoFrameMiniMixed — pha trộn ===");

        // Run() chặn cho tới khi xong, nên phải đẩy nó sang luồng riêng.
        var cycleThread = new Thread(() => machine.Run(maxCycles: 20))
        {
            IsBackground = true,
            Name         = "CycleThread",
        };
        cycleThread.Start();
        cycleThread.Join();          // ở đây chờ cho xong; ứng dụng thật thì KHÔNG chờ

        Console.WriteLine();
        Console.WriteLine($"Trạng thái cuối : {machine.State}");
        Console.WriteLine($"Số chu kỳ chạy  : {machine.CycleCount}");
        Console.WriteLine($"Cảnh báo        : {machine.AlarmText ?? "(không có)"}");
    }
}
