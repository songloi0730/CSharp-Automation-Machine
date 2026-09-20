// -------------------------------------------------------
// File:    Program.cs
// Project: MeoFrameMiniDirect — bản KHÔNG interface, KHÔNG Task
// Purpose: Cùng cỗ máy gắp-đặt, viết theo lối mà 3/13 dự án khảo sát đang
//          dùng: gọi thẳng class cụ thể, chặn luồng, giả lập bằng cờ bool.
//          Đọc kèm Chương 7 mục 7.7 — mục đó phân tích cái giá phải trả.
// -------------------------------------------------------
using System.Diagnostics;

// ══════════════════════════════════════════════════════════════════
// TẦNG 1 — KIỂU DỮ LIỆU MIỀN. Vẫn giữ nguyên: đây là phần không ai bỏ.
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

public sealed class StopFlag
{
    private volatile bool _stop;

    public bool IsStopRequested => _stop;
    public void Request() => _stop = true;

    public void ThrowIfStopRequested()
    {
        if (_stop) throw new OperationCanceledException("Người vận hành bấm Dừng");
    }
}

// ══════════════════════════════════════════════════════════════════
// TẦNG 2 — KHÔNG CÒN. Không có interface nào cả.
// Hệ quả: muốn chạy không cần phần cứng thì phải nhét CỜ GIẢ LẬP vào
// chính lớp thiết bị, và mỗi hàm mọc thêm một nhánh rẽ.
// ══════════════════════════════════════════════════════════════════

public sealed class PressureSensor
{
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly bool _simulate;
    private double _bar;

    public PressureSensor(bool simulate, double startBar = 6.00, double dropPerRead = 0.08)
    {
        _simulate   = simulate;
        _bar        = startBar;
        DropPerRead = dropPerRead;
    }

    public double DropPerRead { get; init; }

    public PressureReading Read()
    {
        if (_simulate)
        {
            _bar -= DropPerRead;
            return new PressureReading(_bar, _clock.Elapsed);
        }

        // Đường chạy thật: gọi SDK của hãng. Trong bản demo này không có
        // phần cứng nên chỉ để lại chỗ trống — và đó chính là vấn đề:
        // logic giả lập và logic thật nằm chung một hàm, cùng một file.
        throw new InvalidOperationException("Chưa nối cảm biến thật");
    }
}

public sealed class Axis
{
    private readonly StopFlag _stop;
    private readonly bool     _simulate;
    private double _positionMm;
    private bool   _isHomed;

    public Axis(string name, StopFlag stop, bool simulate, double speedMmPerStep = 40.0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(stop);
        Name           = name;
        _stop          = stop;
        _simulate      = simulate;
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
        if (_simulate)
        {
            while (Math.Abs(_positionMm - targetMm) > 0.001)
            {
                _stop.ThrowIfStopRequested();
                var delta = Math.Clamp(targetMm - _positionMm, -SpeedMmPerStep, SpeedMmPerStep);
                _positionMm += delta;
                Thread.Sleep(5);
            }
            return;
        }

        throw new InvalidOperationException("Chưa nối card chuyển động thật");
    }
}

public sealed class Gripper
{
    private readonly bool _simulate;

    public Gripper(bool simulate) => _simulate = simulate;

    public bool IsGripping { get; private set; }

    public void Grip()
    {
        if (_simulate) { Thread.Sleep(20); IsGripping = true; return; }
        throw new InvalidOperationException("Chưa nối van kẹp thật");
    }

    public void Release()
    {
        if (_simulate) { Thread.Sleep(20); IsGripping = false; return; }
        throw new InvalidOperationException("Chưa nối van kẹp thật");
    }
}

// ══════════════════════════════════════════════════════════════════
// TẦNG 4 — LỚP NGHIỆP VỤ, nay dính chặt vào lớp thiết bị cụ thể.
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
    private readonly PressureSensor _sensor;   // ← lớp CỤ THỂ, không phải interface
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

public sealed class MachineController
{
    private readonly Axis            _axis;      // ← lớp cụ thể
    private readonly Gripper         _gripper;   // ← lớp cụ thể
    private readonly PressureMonitor _pressure;
    private readonly StopFlag        _stop;

    public MachineController(Axis axis, Gripper gripper, PressureMonitor pressure, StopFlag stop)
    {
        ArgumentNullException.ThrowIfNull(axis);
        ArgumentNullException.ThrowIfNull(gripper);
        ArgumentNullException.ThrowIfNull(pressure);
        ArgumentNullException.ThrowIfNull(stop);
        _axis     = axis;
        _gripper  = gripper;
        _pressure = pressure;
        _stop     = stop;
    }

    public MachineState State      { get; private set; } = MachineState.Idle;
    public int          CycleCount { get; private set; }
    public string?      AlarmText  { get; private set; }

    public event EventHandler<string>? Reported;

    private void Report(string text) => Reported?.Invoke(this, text);

    // Không có IStep nên không giữ được danh sách bước. Chu trình trở thành
    // một dãy lời gọi viết cứng — xem Chương 12 mục 12.1.1.
    private void RunOneCycle()
    {
        MoveWithTimeout(120.0);      // bước 1: tới vị trí gắp
        _gripper.Grip();             // bước 2: gắp
        MoveWithTimeout(20.0);       // bước 3: tới vị trí đặt
        _gripper.Release();          // bước 4: nhả
    }

    // Hạn giờ phải lặp lại ở mọi chỗ gọi chuyển động, vì không còn lớp Step
    // để gói nó vào một chỗ.
    private void MoveWithTimeout(double targetMm)
    {
        var sw      = Stopwatch.StartNew();
        var timeout = TimeSpan.FromSeconds(5);

        var worker = new Thread(() => _axis.MoveTo(targetMm)) { IsBackground = true };
        worker.Start();

        while (worker.IsAlive)
        {
            if (sw.Elapsed > timeout)
                throw new AlarmException(AlarmCodes.AxisTimeout, _axis.Name,
                    $"Trục {_axis.Name} quá thời gian khi đi tới {targetMm:F1} mm");
            _stop.ThrowIfStopRequested();
            Thread.Sleep(2);
        }
    }

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

                RunOneCycle();

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
// TẦNG 5 — KHÔNG CÒN LÀ "COMPOSITION ROOT" NỮA.
// Nó vẫn tạo đối tượng, nhưng việc chọn thật/giả nay nằm ở THAM SỐ
// truyền vào từng lớp, không phải ở việc chọn lớp nào để tạo.
// ══════════════════════════════════════════════════════════════════

public static class Program
{
    private const bool UseSimulation = true;     // đổi thành false khi có phần cứng

    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var stop = new StopFlag();

        var sensor   = new PressureSensor(UseSimulation, startBar: 6.00, dropPerRead: 0.08);
        var axisX    = new Axis("X", stop, UseSimulation, speedMmPerStep: 40.0);
        var gripper  = new Gripper(UseSimulation);
        var pressure = new PressureMonitor(sensor) { MinimumBar = 5.00 };

        var machine = new MachineController(axisX, gripper, pressure, stop);

        machine.Reported        += (_, text) => Console.WriteLine($"  {text}");
        pressure.PressureTooLow += (_, e) =>
            Console.WriteLine($"  ! áp suất thấp: {e.Reading.Bar:F2} < {e.MinimumBar:F2} bar");

        Console.WriteLine("=== MeoFrameMiniDirect — bản không interface, không Task ===");

        var cycleThread = new Thread(() => machine.Run(maxCycles: 20))
        {
            IsBackground = true,
            Name         = "CycleThread",
        };
        cycleThread.Start();
        cycleThread.Join();

        Console.WriteLine();
        Console.WriteLine($"Trạng thái cuối : {machine.State}");
        Console.WriteLine($"Số chu kỳ chạy  : {machine.CycleCount}");
        Console.WriteLine($"Cảnh báo        : {machine.AlarmText ?? "(không có)"}");
    }
}
