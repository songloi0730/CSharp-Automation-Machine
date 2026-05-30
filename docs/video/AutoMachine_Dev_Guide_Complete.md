# Hướng Dẫn Toàn Diện: Xây Dựng Phần Mềm Máy Tự Động Hoá
## Dành cho kỹ sư điện chuyển sang lập trình C# / .NET

> **Lời mở đầu:** Tài liệu này hướng dẫn từng bước toàn bộ quy trình xây dựng, kiểm thử, bảo mật, và vận hành một phần mềm máy tự động hoá công nghiệp. Không yêu cầu kinh nghiệm lập trình trước — nhưng yêu cầu hiểu biết về máy móc và điện công nghiệp (bạn đã có).

---

# PHẦN I — NỀN TẢNG TƯ DUY

---

## Chương 1: Tư duy lập trình cho kỹ sư điện

### 1.1 Bạn đã biết nhiều hơn bạn nghĩ

Là kỹ sư điện, bạn đã quen với:

| Kiến thức điện | Tương đương trong lập trình |
|---------------|---------------------------|
| Ladder Logic (PLC) | Điều kiện `if/else`, vòng lặp |
| Function Block (FBD) | Hàm (Function), Class |
| Sequential Function Chart (SFC) | State Machine, async/await |
| Datablock PLC | Class, Struct, Properties |
| Tag PLC | Biến (Variable) |
| Alarm PLC | Exception, Event |
| HMI screen | View, ViewModel (WPF) |

Lập trình phần mềm = PLC nhưng mạnh hơn, linh hoạt hơn, và không bị giới hạn phần cứng.

### 1.2 Sự khác biệt cần chú ý

**PLC:** Scan cycle cố định, deterministic, real-time cứng
**Phần mềm PC:** Không deterministic, OS có thể preempt, cần xử lý concurrency

**PLC:** Lỗi → watchdog reset, continue
**Phần mềm:** Lỗi → Exception → cần catch và xử lý đúng cách

**PLC:** Không cần quan tâm memory
**Phần mềm:** Cần quản lý object lifecycle, tránh memory leak

### 1.3 Mental model: Phần mềm máy là gì?

Hãy nghĩ phần mềm máy tự động hoá gồm 3 lớp:

```
┌─────────────────────────────────────────────┐
│  PRESENTATION LAYER — "Mặt người nhìn"       │
│  WPF, XAML, ViewModel, Alarm display         │
├─────────────────────────────────────────────┤
│  BUSINESS LOGIC LAYER — "Não máy"            │
│  Machine sequence, recipe, production logic  │
├─────────────────────────────────────────────┤
│  HARDWARE LAYER — "Tay chân máy"             │
│  Motion controller, Camera, I/O, Serial/TCP  │
└─────────────────────────────────────────────┘
```

Mỗi lớp được tách biệt — thay đổi một lớp không ảnh hưởng lớp kia. Đây là nguyên tắc QUAN TRỌNG nhất.

---

## Chương 2: Quy trình dự án tổng thể

### 2.1 Vòng đời một dự án máy

```
Phase 1: KICKOFF (1–2 tuần)
├── Nhận spec từ khách hàng
├── Phân tích yêu cầu phần cứng & phần mềm
├── Lập danh sách I/O, danh sách trục
└── Ký biên bản xác nhận yêu cầu

Phase 2: DESIGN (1–2 tuần)
├── Thiết kế solution structure
├── Thiết kế database schema
├── Thiết kế màn hình (wireframe)
├── Xác định alarm list
└── Review với khách hàng → confirm

Phase 3: DEVELOPMENT (4–12 tuần tuỳ độ phức tạp)
├── Sprint 1: Core framework, hardware drivers
├── Sprint 2: Machine sequence, recipe
├── Sprint 3: UI screens
├── Sprint 4: Production features (alarm, log, report)
└── Sprint 5: Integration & bug fix

Phase 4: TESTING (2–4 tuần)
├── Unit test từng module
├── Integration test
├── FAT — Factory Acceptance Test (test tại xưởng mình)
└── SAT — Site Acceptance Test (test tại khách hàng)

Phase 5: DEPLOYMENT & HANDOVER
├── Cài đặt tại khách hàng
├── Training operator
├── Bàn giao tài liệu
└── Warranty period (thường 12 tháng)
```

### 2.2 Quản lý yêu cầu (Requirements)

Trước khi viết 1 dòng code, phải có tài liệu:

**Functional Requirements (FR)** — Máy làm gì:
```
FR-001: Hệ thống đọc barcode sản phẩm khi phôi đến vị trí A
FR-002: Hệ thống di chuyển phôi từ A đến B trong ≤ 3 giây
FR-003: Camera kiểm tra bề mặt, pass nếu score ≥ 95
FR-004: Phân loại OK → băng tải 1, NG → băng tải 2
```

**Non-Functional Requirements (NFR)** — Máy làm tốt như thế nào:
```
NFR-001: UPH ≥ 1200 sản phẩm/giờ
NFR-002: Thời gian khởi động ≤ 15 giây
NFR-003: Uptime ≥ 99.5% trong 8 giờ ca
NFR-004: Dữ liệu production lưu 365 ngày
NFR-005: Hỗ trợ 3 ngôn ngữ: VI, EN, ZH
```

> **Quy tắc vàng:** Nếu khách hàng không có spec, hãy tự viết rồi đưa họ ký. Mọi thay đổi sau đó là change request — có phí.

---

# PHẦN II — THIẾT LẬP MÔI TRƯỜNG & CÔNG CỤ

---

## Chương 3: Công cụ cần thiết

### 3.1 IDE và ngôn ngữ

```
Visual Studio 2022 Community (miễn phí)
├── Cài .NET 8 SDK
├── Cài workload: .NET desktop development
├── Extension: ReSharper hoặc Rider (trả phí, rất đáng)
└── Extension: GitLens, CodeMaid

Ngôn ngữ: C# 12 / .NET 8
```

### 3.2 Source Control — Git

Git là bắt buộc. Không dùng USB để share code.

```bash
# Cài Git for Windows
# Tạo tài khoản GitHub hoặc GitLab (miễn phí)

# Các lệnh Git cần biết ngay:
git init                    # Khởi tạo repo mới
git clone <url>             # Copy repo về máy
git add .                   # Stage tất cả thay đổi
git commit -m "message"     # Lưu snapshot
git push origin main        # Đẩy lên server
git pull                    # Lấy code mới nhất
git branch feature/alarm    # Tạo nhánh mới
git checkout feature/alarm  # Chuyển nhánh
git merge feature/alarm     # Gộp nhánh vào main
```

**Quy tắc commit message:**
```
feat: thêm màn hình production report
fix: sửa lỗi axis timeout khi homing
refactor: tách AlarmService thành interface
docs: cập nhật README cài đặt
test: thêm unit test cho ParameterService
```

### 3.3 Git Branching Strategy (cho nhóm nhỏ 1–3 người)

```
main          ← Code đang chạy tại khách hàng (LUÔN STABLE)
  └─ develop  ← Code đang phát triển, tích hợp hàng ngày
       ├─ feature/alarm-module     ← Tính năng mới
       ├─ feature/production-report
       └─ fix/motion-timeout-bug   ← Sửa lỗi
```

**Quy tắc:**
- KHÔNG commit thẳng vào `main`
- Mỗi tính năng = 1 branch riêng
- Trước khi merge vào `main` phải test xong
- Tag version khi release: `git tag v1.0.0`

### 3.4 Công cụ quản lý task

Cho dự án 1 người hoặc nhóm nhỏ:
- **Notion** (miễn phí, đơn giản) — quản lý task + tài liệu
- **GitHub Issues** — nếu dùng GitHub, built-in miễn phí
- **Trello** — board Kanban đơn giản

Cột Kanban tối thiểu:
```
Backlog | In Progress | Review | Done
```

### 3.5 Công cụ thiết kế UI

- **Figma** (miễn phí) — wireframe và mockup trước khi code
- **WPF Snoop** — debug WPF visual tree khi chạy

### 3.6 Công cụ Database

- **DB Browser for SQLite** — xem và edit file .db (miễn phí)
- **DBeaver** — universal database tool (miễn phí)

---

## Chương 4: Cấu trúc Solution và Project

### 4.1 Tổ chức thư mục

```
AutoMachine/                        ← Thư mục gốc
├── src/                            ← Source code
│   ├── AM.Core.Abstractions/
│   ├── AM.Core/
│   ├── AM.Infrastructure/
│   ├── AM.Data/
│   ├── AM.CommonTools/
│   ├── AM.Hardware.Motion/
│   ├── AM.Hardware.Vision/
│   ├── AM.Hardware.IO/
│   ├── AM.Hardware.Communication/
│   ├── AM.Services/
│   ├── AM.Modules.*/               ← Các module UI
│   ├── AM.UI.Controls/
│   ├── AM.Application.Shell/
│   └── AM.WorkStation.{MachineName}/
│
├── tests/                          ← Tất cả test projects
│   ├── AM.Core.Tests/
│   ├── AM.Services.Tests/
│   └── AM.Integration.Tests/
│
├── docs/                           ← Tài liệu
│   ├── requirements/
│   ├── design/
│   └── manual/
│
├── tools/                          ← Scripts tiện ích
│   ├── setup.ps1                   ← Cài đặt dependencies
│   └── deploy.ps1                  ← Deploy script
│
├── .github/                        ← CI/CD workflows
│   └── workflows/
│       ├── build.yml
│       └── release.yml
│
├── .gitignore
├── README.md
└── AutoMachine.slnx
```

### 4.2 File .gitignore quan trọng

```gitignore
# Build outputs
bin/
obj/
*.user

# Visual Studio
.vs/
*.suo
*.vsix

# Database files (không commit data thật)
*.db
*.db-shm
*.db-wal

# Log files
logs/
*.log

# Cấu hình máy thật (sensitive)
hardware.json
machine.json

# NuGet packages (tự restore)
packages/
**/packages/

# Secrets (KHÔNG BAO GIỜ commit)
appsettings.Production.json
secrets.json
```

---

# PHẦN III — LẬP TRÌNH C# CĂN BẢN CHO KỸ SƯ ĐIỆN

---

## Chương 5: C# nhanh cho người có nền PLC

### 5.1 Kiểu dữ liệu — so sánh PLC vs C#

```csharp
// PLC: BOOL   →  C#: bool
bool doorClosed = true;
bool partPresent = false;

// PLC: INT/DINT  →  C#: int (32-bit)
int productionCount = 0;
int axisPosition = 12345; // unit: pulse

// PLC: REAL/LREAL  →  C#: double
double positionMm = 123.45;
double velocity = 50.0; // mm/s

// PLC: STRING  →  C#: string
string serialNumber = "SN20240101001";
string recipeName = "Product_A_v2";

// PLC: TIME  →  C#: TimeSpan
TimeSpan cycleTime = TimeSpan.FromSeconds(4.2);

// PLC: DATE_AND_TIME  →  C#: DateTime
DateTime productionTime = DateTime.Now;
```

### 5.2 Điều kiện và vòng lặp

```csharp
// IF/ELSE — giống PLC
if (axisPosition > 100.0 && doorClosed)
{
    StartMotion();
}
else if (axisPosition == 0.0)
{
    HomeAxis();
}
else
{
    RaiseAlarm(AlarmCode.POSITION_INVALID);
}

// SWITCH — giống CASE/OF trong PLC
switch (machineState)
{
    case MachineState.Idle:
        WaitForStart();
        break;
    case MachineState.Running:
        ExecuteSequence();
        break;
    case MachineState.Error:
        HandleError();
        break;
    default:
        break;
}

// FOR loop — lặp có số lần cố định
for (int i = 0; i < 8; i++)
{
    CheckIO(i);
}

// WHILE loop — lặp có điều kiện
while (!homeComplete && !timeout)
{
    await Task.Delay(10); // chờ 10ms
    CheckStatus();
}

// FOREACH — lặp qua danh sách
foreach (var axis in allAxes)
{
    await axis.HomeAsync(cancellationToken);
}
```

### 5.3 Class — tương đương Function Block trong PLC

```csharp
// PLC Function Block:
// FUNCTION_BLOCK FB_Axis
//   VAR_INPUT: Enable, JogPos, JogNeg
//   VAR_OUTPUT: InPosition, Error, ErrorID
// END_FUNCTION_BLOCK

// C# tương đương:
public class LinearAxis : IMotionController
{
    // Properties = VAR (có thể đọc từ bên ngoài)
    public double CurrentPosition { get; private set; }
    public bool IsHomed { get; private set; }
    public bool HasError { get; private set; }
    public int ErrorCode { get; private set; }

    // Private fields = VAR nội bộ
    private readonly string _axisName;
    private readonly ILogger<LinearAxis> _logger;

    // Constructor = khởi tạo FB (chạy 1 lần)
    public LinearAxis(string axisName, ILogger<LinearAxis> logger)
    {
        _axisName = axisName;
        _logger = logger;
    }

    // Methods = function trong FB
    public async Task<bool> HomeAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Axis {Name}: Starting home", _axisName);
        // ... thực thi home
        IsHomed = true;
        return true;
    }

    public async Task<bool> MoveAbsAsync(double position, double velocity,
                                          CancellationToken ct = default)
    {
        if (!IsHomed)
            throw new AlarmException(AlarmCode.MOTION_NOT_HOMED, _axisName);

        // ... thực thi motion
        CurrentPosition = position;
        return true;
    }
}
```

### 5.4 Interface — "Bản hợp đồng" giữa các module

```csharp
// Interface = định nghĩa "ai làm gì" không quan tâm "làm như thế nào"
// Tương tự: bạn không cần biết PLC LTDMC hay GTS, chỉ cần biết nó có thể Home, Move

public interface IMotionController
{
    Task<bool> HomeAsync(int axisIndex, CancellationToken ct = default);
    Task<bool> MoveAbsAsync(int axisIndex, double position, double velocity,
                            CancellationToken ct = default);
    Task<double> GetPositionAsync(int axisIndex);
    bool IsMotionComplete(int axisIndex);
}

// Implementation 1: dùng thư viện LTDMC thật
public class LtdmcController : IMotionController
{
    public async Task<bool> HomeAsync(int axisIndex, CancellationToken ct)
    {
        // Gọi hàm LTDMC thật
        LTDMC.GT_Home(axisIndex, ...);
        return true;
    }
    // ...
}

// Implementation 2: giả lập để test không cần HW
public class SimulatedController : IMotionController
{
    private readonly double[] _positions = new double[8];

    public async Task<bool> HomeAsync(int axisIndex, CancellationToken ct)
    {
        await Task.Delay(500, ct); // giả lập thời gian home
        _positions[axisIndex] = 0;
        return true;
    }
    // ...
}

// Machine sequence KHÔNG biết đang dùng loại nào — chỉ dùng interface
public class MachineSequence
{
    private readonly IMotionController _motion; // chỉ biết interface

    public MachineSequence(IMotionController motion) // inject vào
    {
        _motion = motion;
    }
}
```

### 5.5 Async/Await — chạy nhiều việc không block

```csharp
// PLC: Máy vẫn scan trong khi đợi timer
// C#: async/await cho phép đợi mà không block thread

// ❌ SAI — block UI thread, màn hình đứng
private void StartHome_Click(object sender, RoutedEventArgs e)
{
    Thread.Sleep(5000); // Block 5 giây, màn hình đơ
    axis.Home();        // KHÔNG làm thế này
}

// ✅ ĐÚNG — async, UI vẫn responsive
private async void StartHome_Click(object sender, RoutedEventArgs e)
{
    _startButton.IsEnabled = false;
    try
    {
        await _motion.HomeAsync(_cancellationToken); // chờ mà không block
        MessageBox.Show("Home complete!");
    }
    finally
    {
        _startButton.IsEnabled = true;
    }
}

// Chạy song song nhiều trục cùng lúc
public async Task HomeAllAxesAsync(CancellationToken ct)
{
    // Chạy song song — giống khi bạn enable nhiều trục cùng lúc trong PLC
    await Task.WhenAll(
        _motion.HomeAsync(0, ct), // X axis
        _motion.HomeAsync(1, ct), // Y axis
        _motion.HomeAsync(2, ct)  // Z axis
    );
}

// CancellationToken — nút dừng
// Khi operator bấm Stop → token bị cancel → tất cả await đang chờ sẽ throw OperationCanceledException
private CancellationTokenSource _cts = new();

public void Stop()
{
    _cts.Cancel(); // trigger cancel
    _cts = new();  // tạo token mới cho lần chạy tiếp
}
```

### 5.6 Exception Handling — xử lý lỗi

```csharp
// Tạo custom exception cho alarm
public class AlarmException : Exception
{
    public int AlarmCode { get; }
    public string Station { get; }

    public AlarmException(int alarmCode, string station, string? message = null)
        : base(message ?? $"Alarm {alarmCode} at {station}")
    {
        AlarmCode = alarmCode;
        Station = station;
    }
}

// Sequence luôn dùng pattern này
public async Task RunAsync(CancellationToken ct)
{
    while (!ct.IsCancellationRequested)
    {
        try
        {
            await ExecuteOneCycleAsync(ct);
            _productionService.RecordOK();
        }
        catch (AlarmException ex)
        {
            // Lỗi có thể xử lý được — raise alarm, chờ operator
            await _alarmService.RaiseAsync(ex.AlarmCode, ex.Station);
            _logger.LogError("Alarm {Code} at {Station}", ex.AlarmCode, ex.Station);
            await WaitForAlarmClearAsync(ct);
        }
        catch (OperationCanceledException)
        {
            // Operator bấm Stop — bình thường, không phải lỗi
            _logger.LogInformation("Sequence stopped by operator");
            break;
        }
        catch (Exception ex)
        {
            // Lỗi không mong đợi — log và raise critical alarm
            _logger.LogCritical(ex, "Unexpected error in sequence");
            await _alarmService.RaiseAsync(AlarmCode.SYSTEM_CRITICAL, "SEQUENCE");
            break;
        }
    }
}
```

---

# PHẦN IV — KIẾN TRÚC VÀ DESIGN PATTERNS

---

## Chương 6: Dependency Injection (DI) — Trái tim của kiến trúc

### 6.1 DI là gì và tại sao cần

```csharp
// ❌ KHÔNG DI — MachineSequence tự tạo dependencies
// Vấn đề: không thể test, không thể thay LTDMC bằng simulator
public class MachineSequence
{
    private LtdmcController _motion = new LtdmcController(); // CỨNG
    private CognexCamera _camera = new CognexCamera();       // CỨNG
}

// ✅ DI — inject từ bên ngoài
// Lợi ích: test dễ, swap implementation dễ
public class MachineSequence
{
    private readonly IMotionController _motion;   // CHỈ biết interface
    private readonly ICameraDevice _camera;

    public MachineSequence(IMotionController motion, ICameraDevice camera)
    {
        _motion = motion;  // Ai cấu hình DI sẽ quyết định inject cái gì
        _camera = camera;
    }
}
```

### 6.2 Đăng ký DI trong Shell (Bootstrapper)

```csharp
// AM.Application.Shell/Bootstrapper.cs
public class Bootstrapper : PrismApplication
{
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        bool useSimulation = Configuration.GetValue<bool>("UseSimulation");

        if (useSimulation)
        {
            // TEST MODE: Dùng simulator — không cần phần cứng thật
            containerRegistry.RegisterSingleton<IMotionController, SimulatedController>();
            containerRegistry.RegisterSingleton<ICameraDevice, SimulatedCamera>();
            containerRegistry.RegisterSingleton<IIoModule, SimulatedIOModule>();
        }
        else
        {
            // PRODUCTION MODE: Dùng hardware thật
            containerRegistry.RegisterSingleton<IMotionController, LtdmcController>();
            containerRegistry.RegisterSingleton<ICameraDevice, CognexCamera>();
            containerRegistry.RegisterSingleton<IIoModule, IOC0640Module>();
        }

        // Services — giống nhau dù hardware nào
        containerRegistry.RegisterSingleton<IAlarmService, AlarmService>();
        containerRegistry.RegisterSingleton<IParameterService, ParameterService>();
        containerRegistry.RegisterSingleton<IProductionService, ProductionService>();
        containerRegistry.RegisterSingleton<ILogService, LogService>();

        // WorkStation — inject tự động từ DI
        containerRegistry.RegisterSingleton<IMachineSequence, MachineSequence>();
    }
}
```

### 6.3 Lifetimes quan trọng

```csharp
// Singleton — tạo 1 lần, dùng mãi (hardware, services)
containerRegistry.RegisterSingleton<IAlarmService, AlarmService>();

// Transient — tạo mới mỗi lần yêu cầu (dialog, popup ViewModel)
containerRegistry.Register<AlarmDetailViewModel>();

// Scoped — tạo 1 lần trong 1 scope (ít dùng trong WPF)
```

---

## Chương 7: MVVM Pattern cho WPF

### 7.1 MVVM là gì

```
Model       = Dữ liệu thuần (RecipeModel, AlarmInfo, ProductionRecord)
View        = XAML (.xaml file) — chỉ giao diện, không logic
ViewModel   = Cầu nối Model ↔ View — xử lý logic hiển thị

Rule: View KHÔNG biết ViewModel cụ thể
      ViewModel KHÔNG biết View
      → Thay View không ảnh hưởng ViewModel và ngược lại
```

### 7.2 ViewModel cơ bản với CommunityToolkit.Mvvm

```csharp
// ProductionViewModel.cs
public partial class ProductionViewModel : ObservableObject
{
    private readonly IProductionService _productionService;
    private readonly IAlarmService _alarmService;

    // [ObservableProperty] tự sinh code INotifyPropertyChanged
    [ObservableProperty]
    private int _totalOK;

    [ObservableProperty]
    private int _totalNG;

    [ObservableProperty]
    private double _yield;

    [ObservableProperty]
    private string _currentRecipe = "—";

    [ObservableProperty]
    private bool _isMachineRunning;

    // Computed property — tự cập nhật khi TotalOK/TotalNG thay đổi
    public int TotalCount => TotalOK + TotalNG;

    // [RelayCommand] tự sinh ICommand
    [RelayCommand(CanExecute = nameof(CanStartMachine))]
    private async Task StartMachine()
    {
        try
        {
            IsMachineRunning = true;
            await _machineSequence.RunAsync(_cts.Token);
        }
        finally
        {
            IsMachineRunning = false;
        }
    }

    private bool CanStartMachine() => !IsMachineRunning;

    [RelayCommand]
    private async Task StopMachine()
    {
        _cts.Cancel();
    }
}
```

```xml
<!-- ProductionView.xaml — binding với ViewModel -->
<UserControl ...>
  <Grid>
    <TextBlock Text="{Binding TotalOK}"
               Style="{StaticResource LiveValueTextStyle}"/>
    <TextBlock Text="{Binding CurrentRecipe}"
               Style="{StaticResource LabelTextStyle}"/>

    <Button Content="START"
            Command="{Binding StartMachineCommand}"
            Style="{StaticResource Button.PrimaryStyle}"/>

    <Button Content="STOP"
            Command="{Binding StopMachineCommand}"
            Style="{StaticResource Button.DangerStyle}"/>
  </Grid>
</UserControl>
```

---
# PHẦN V — KIỂM THỬ (TESTING)

---

## Chương 8: Triết lý kiểm thử cho phần mềm máy

### 8.1 Tại sao phải test? — Bài học từ thực tế

> Phần mềm máy tự động hoá lỗi không chỉ gây mất data — nó có thể làm hỏng sản phẩm, hỏng máy, hoặc gây nguy hiểm người. Chi phí sửa lỗi sau khi máy đã giao gấp 100 lần so với phát hiện khi đang code.

**Tháp kiểm thử (Test Pyramid):**

```
         /\
        /  \    E2E Test (ít nhất)
       /    \   — Test toàn bộ luồng
      /------\
     /        \  Integration Test (vừa)
    /          \  — Test nhiều module ghép nhau
   /------------\
  /              \  Unit Test (nhiều nhất)
 /                \  — Test từng hàm/class độc lập
/──────────────────\
```

**Nguyên tắc FIRST cho unit test:**
- **F**ast — Chạy nhanh (< 1 giây mỗi test)
- **I**solated — Không phụ thuộc nhau
- **R**epeatable — Kết quả như nhau mỗi lần
- **S**elf-validating — Pass/Fail rõ ràng, không cần nhìn output
- **T**imely — Viết cùng lúc hoặc trước khi code (TDD)

### 8.2 Setup project test

```xml
<!-- tests/AM.Services.Tests/AM.Services.Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <!-- Test framework -->
    <PackageReference Include="xunit" Version="2.9.*"/>
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.*"/>
    <!-- Mocking library -->
    <PackageReference Include="Moq" Version="4.20.*"/>
    <!-- Fluent assertions (đọc test dễ hơn) -->
    <PackageReference Include="FluentAssertions" Version="6.12.*"/>
    <!-- Project đang test -->
    <ProjectReference Include="../../src/AM.Services/AM.Services.csproj"/>
  </ItemGroup>
</Project>
```

---

## Chương 9: Unit Testing

### 9.1 Cấu trúc test: Arrange — Act — Assert (AAA)

```csharp
// Giống PLC debug: Setup → Chạy → Verify kết quả
// tests/AM.Services.Tests/AlarmServiceTests.cs

public class AlarmServiceTests
{
    // [Fact] = 1 test case không có input biến đổi
    [Fact]
    public async Task RaiseAlarm_WhenValidCode_ShouldAddToActiveAlarms()
    {
        // ARRANGE — chuẩn bị môi trường
        var mockRepository = new Mock<IAlarmRepository>();
        var mockLogger = new Mock<ILogger<AlarmService>>();
        var alarmService = new AlarmService(mockRepository.Object, mockLogger.Object);

        // ACT — thực hiện hành động cần test
        await alarmService.RaiseAsync(AlarmCode.MOTION_TIMEOUT, "AXIS_X");

        // ASSERT — kiểm tra kết quả
        var activeAlarms = alarmService.GetActiveAlarms();
        activeAlarms.Should().ContainSingle(a =>
            a.Code == AlarmCode.MOTION_TIMEOUT &&
            a.Station == "AXIS_X");
    }

    // [Theory] + [InlineData] = test nhiều input khác nhau
    [Theory]
    [InlineData(AlarmCode.MOTION_TIMEOUT, AlarmLevel.High)]
    [InlineData(AlarmCode.ESTOP_PRESSED, AlarmLevel.Critical)]
    [InlineData(AlarmCode.TEMPERATURE_WARNING, AlarmLevel.Medium)]
    public async Task RaiseAlarm_ShouldAssignCorrectLevel(int alarmCode, AlarmLevel expectedLevel)
    {
        // Arrange
        var mockRepository = new Mock<IAlarmRepository>();
        var service = new AlarmService(mockRepository.Object,
                                       Mock.Of<ILogger<AlarmService>>());
        // Act
        await service.RaiseAsync(alarmCode, "TEST");

        // Assert
        var alarm = service.GetActiveAlarms().First(a => a.Code == alarmCode);
        alarm.Level.Should().Be(expectedLevel);
    }

    [Fact]
    public async Task ClearAlarm_WhenAlarmActive_ShouldMoveToHistory()
    {
        // Arrange
        var service = CreateService();
        await service.RaiseAsync(AlarmCode.MOTION_TIMEOUT, "AXIS_X");

        // Act
        await service.ClearAsync(AlarmCode.MOTION_TIMEOUT, "AXIS_X");

        // Assert
        service.GetActiveAlarms().Should().BeEmpty();
        // Verify repository được gọi để lưu history
        _mockRepository.Verify(r =>
            r.SaveHistoryAsync(It.IsAny<AlarmHistoryEntity>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
```

### 9.2 Mock — giả lập dependencies

```csharp
// Moq: tạo giả lập cho interface
public class ParameterServiceTests
{
    private readonly Mock<IParameterRepository> _mockRepo;
    private readonly ParameterService _service;

    public ParameterServiceTests()
    {
        _mockRepo = new Mock<IParameterRepository>();
        _service = new ParameterService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetParameter_WhenExists_ShouldReturnValue()
    {
        // Giả lập repository trả về giá trị cụ thể
        _mockRepo.Setup(r => r.GetAsync("axis.x.velocity", default))
                 .ReturnsAsync(new ParameterEntity { Key = "axis.x.velocity", Value = "500" });

        var result = await _service.GetAsync<double>("axis.x.velocity");

        result.Should().Be(500.0);
    }

    [Fact]
    public async Task GetParameter_WhenKeyNotFound_ShouldReturnDefault()
    {
        // Giả lập repository trả về null (key không tồn tại)
        _mockRepo.Setup(r => r.GetAsync(It.IsAny<string>(), default))
                 .ReturnsAsync((ParameterEntity?)null);

        var result = await _service.GetAsync<double>("nonexistent.key", defaultValue: 0.0);

        result.Should().Be(0.0);
    }

    [Fact]
    public async Task SetParameter_ShouldCallRepository()
    {
        await _service.SetAsync("axis.x.velocity", 600.0);

        // Verify repository được gọi đúng cách
        _mockRepo.Verify(r =>
            r.SaveAsync(It.Is<ParameterEntity>(e =>
                e.Key == "axis.x.velocity" && e.Value == "600"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
```

### 9.3 Test cho Machine Sequence Logic

```csharp
public class MachineSequenceTests
{
    private readonly Mock<IMotionController> _mockMotion;
    private readonly Mock<ICameraDevice> _mockCamera;
    private readonly Mock<IAlarmService> _mockAlarm;
    private readonly MachineSequence _sequence;

    public MachineSequenceTests()
    {
        _mockMotion = new Mock<IMotionController>();
        _mockCamera = new Mock<ICameraDevice>();
        _mockAlarm = new Mock<IAlarmService>();
        _sequence = new MachineSequence(_mockMotion.Object,
                                         _mockCamera.Object,
                                         _mockAlarm.Object);
    }

    [Fact]
    public async Task RunCycle_WhenCameraPassess_ShouldRecordOK()
    {
        // Arrange
        var mockProduction = new Mock<IProductionService>();
        _mockCamera.Setup(c => c.RunInspectionAsync(default))
                   .ReturnsAsync(new VisionResult { Passed = true, Score = 98.5 });
        _mockMotion.Setup(m => m.MoveAbsAsync(It.IsAny<int>(),
                                               It.IsAny<double>(),
                                               It.IsAny<double>(), default))
                   .ReturnsAsync(true);

        // Act
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await _sequence.RunOneCycleAsync(cts.Token);

        // Assert
        mockProduction.Verify(p => p.RecordResultAsync(
            It.Is<ProductionResult>(r => r.IsOK == true)), Times.Once);
    }

    [Fact]
    public async Task RunCycle_WhenMotionTimeout_ShouldRaiseAlarm()
    {
        // Arrange: motion throw exception sau 10ms
        _mockMotion.Setup(m => m.MoveAbsAsync(It.IsAny<int>(),
                                               It.IsAny<double>(),
                                               It.IsAny<double>(), default))
                   .ThrowsAsync(new AlarmException(AlarmCode.MOTION_TIMEOUT, "AXIS_X"));

        // Act
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await _sequence.RunOneCycleAsync(cts.Token);

        // Assert: alarm service được gọi
        _mockAlarm.Verify(a =>
            a.RaiseAsync(AlarmCode.MOTION_TIMEOUT, "AXIS_X", default),
            Times.Once);
    }
}
```

### 9.4 Test coverage và ngưỡng tối thiểu

```xml
<!-- Cấu hình coverage trong .csproj -->
<PropertyGroup>
  <!-- Fail build nếu coverage < ngưỡng -->
  <ThresholdType>line</ThresholdType>
  <Threshold>70</Threshold>
</PropertyGroup>
```

**Ngưỡng coverage khuyến nghị cho máy tự động hoá:**

| Module | Coverage tối thiểu | Lý do |
|--------|-------------------|-------|
| AlarmService | 90% | An toàn — phải chắc chắn |
| ParameterService | 85% | Ảnh hưởng toàn bộ machine |
| MachineSequence logic | 80% | Core business |
| UI ViewModel | 70% | Presentation logic |
| Hardware drivers | 50% | Khó test không có HW thật |

---

## Chương 10: Integration Testing

### 10.1 Test nhiều module cùng nhau

```csharp
// tests/AM.Integration.Tests/ProductionFlowTests.cs
// Test toàn bộ luồng: Start → Run → Record → Alarm

public class ProductionFlowIntegrationTests : IAsyncLifetime
{
    private IServiceProvider _services;
    private IMachineSequence _sequence;
    private IAlarmService _alarmService;
    private IProductionService _productionService;

    public async Task InitializeAsync()
    {
        // Setup với Simulator — không cần hardware thật
        var services = new ServiceCollection();
        services.AddSingleton<IMotionController, SimulatedController>();
        services.AddSingleton<ICameraDevice, SimulatedCamera>();
        services.AddSingleton<IIoModule, SimulatedIOModule>();
        services.AddSingleton<IAlarmService, AlarmService>();
        services.AddSingleton<IProductionService, ProductionService>();
        services.AddSingleton<IMachineSequence, MachineSequence>();
        services.AddDbContext<MachineDbContext>(opt =>
            opt.UseSqlite("Data Source=:memory:")); // In-memory DB

        _services = services.BuildServiceProvider();
        _sequence = _services.GetRequiredService<IMachineSequence>();
        _alarmService = _services.GetRequiredService<IAlarmService>();
        _productionService = _services.GetRequiredService<IProductionService>();

        // Initialize hardware simulators
        await _sequence.InitializeAsync(CancellationToken.None);
    }

    [Fact]
    public async Task FullCycle_5Times_ShouldRecord5OKProducts()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        int cycleCount = 0;

        // Chạy 5 chu kỳ
        while (cycleCount < 5 && !cts.Token.IsCancellationRequested)
        {
            await _sequence.RunOneCycleAsync(cts.Token);
            cycleCount++;
        }

        // Kiểm tra kết quả
        var stats = await _productionService.GetCurrentBatchStatsAsync();
        stats.TotalOK.Should().Be(5);
        stats.TotalNG.Should().Be(0);
        _alarmService.GetActiveAlarms().Should().BeEmpty();
    }

    public async Task DisposeAsync()
    {
        await _sequence.DisposeAsync();
    }
}
```

### 10.2 Test với Database thật (SQLite)

```csharp
[Fact]
public async Task SaveProductionRecord_ShouldPersistToDatabase()
{
    // Arrange: dùng SQLite in-memory
    var options = new DbContextOptionsBuilder<MachineDbContext>()
        .UseSqlite("Data Source=:memory:")
        .Options;

    using var context = new MachineDbContext(options);
    await context.Database.EnsureCreatedAsync();

    var repo = new ProductionRepository(context);
    var service = new ProductionService(repo, Mock.Of<ILogger<ProductionService>>());

    // Act
    await service.RecordResultAsync(new ProductionResult
    {
        SerialNumber = "SN001",
        IsOK = true,
        Timestamp = DateTime.Now,
        RecipeName = "Recipe_A"
    });

    // Assert: verify data thật trong DB
    var saved = await context.ProductionRecords
        .FirstOrDefaultAsync(r => r.SerialNumber == "SN001");
    saved.Should().NotBeNull();
    saved!.IsOK.Should().BeTrue();
}
```

---

## Chương 11: FAT và SAT — Acceptance Testing

### 11.1 FAT — Factory Acceptance Test

FAT là bài kiểm tra tại xưởng bạn trước khi giao máy. Khách hàng (hoặc đại diện) tham gia xác nhận.

**Chuẩn bị cho FAT:**

```
FAT Checklist — Machine: {TênMáy} — Date: {Ngày}
Customer: _______________ Witness: _______________

SECTION 1: SAFETY
□ 1.1 E-Stop button hoạt động — máy dừng trong < 500ms
□ 1.2 Safety gate interlock — mở cửa, máy dừng ngay
□ 1.3 Tower light: Green=Run, Yellow=Warn, Red=Alarm
□ 1.4 Manual jog chỉ hoạt động khi cửa đóng
□ 1.5 Alarm log ghi đầy đủ khi E-Stop

SECTION 2: INITIALIZATION
□ 2.1 Khởi động phần mềm trong ≤ {time} giây
□ 2.2 Kết nối tất cả hardware: OK/FAIL
□ 2.3 Home tất cả trục: OK/FAIL
□ 2.4 Load recipe mặc định: OK/FAIL

SECTION 3: PRODUCTION CYCLE
□ 3.1 Chạy 10 chu kỳ liên tiếp không lỗi
□ 3.2 Cycle time đo thực tế: ___ giây (yêu cầu ≤ ___ giây)
□ 3.3 UPH thực tế: ___ (yêu cầu ≥ ___)
□ 3.4 Vision detection: 20 phôi OK đúng, 5 phôi NG phát hiện đúng

SECTION 4: ALARM HANDLING
□ 4.1 Trigger từng alarm trong danh sách → hiển thị đúng
□ 4.2 Acknowledge alarm → ghi log timestamp đúng
□ 4.3 Reset sau alarm → machine về đúng trạng thái
□ 4.4 Alarm history lưu đúng, export được

SECTION 5: DATA & REPORTING
□ 5.1 Production record lưu đủ fields
□ 5.2 Export CSV/Excel production data
□ 5.3 Barcode / serial number tracking đúng
□ 5.4 Traceability: từ SN tra được ảnh và data

SECTION 6: USER & SECURITY
□ 6.1 Login với đúng role → đúng quyền
□ 6.2 Login sai 3 lần → lock account
□ 6.3 Session timeout → auto lock
□ 6.4 Admin thay đổi parameter → audit log ghi lại

SECTION 7: PERFORMANCE
□ 7.1 Chạy 2 giờ liên tục: không crash, không memory leak
□ 7.2 CPU usage khi running: < 30% (đo bằng Task Manager)
□ 7.3 RAM usage: stable, không tăng liên tục
□ 7.4 Database size sau 2 giờ: hợp lý

SECTION 8: MULTI-LANGUAGE
□ 8.1 Đổi sang EN: tất cả text hiển thị đúng
□ 8.2 Đổi sang ZH: tất cả text hiển thị đúng
□ 8.3 Alarm message đúng ngôn ngữ

FAT Result: □ PASS  □ CONDITIONAL PASS  □ FAIL
Outstanding items: ___________________
Signatures: _________________ / _________________
```

### 11.2 SAT — Site Acceptance Test

SAT tại site khách hàng sau khi lắp máy. Thực hiện tất cả bài FAT + thêm:

```
SAT Additional Tests:

□ 9.1 Kết nối mạng nội bộ khách hàng: ping OK
□ 9.2 Xuất báo cáo ra server khách hàng: OK
□ 9.3 Chạy 8 giờ liên tục — full production shift
□ 9.4 UPH trung bình thực tế đạt spec
□ 9.5 Dữ liệu đồng bộ lên MES (nếu có)
□ 9.6 Backup/restore cấu hình: OK
□ 9.7 Operator training hoàn thành (ký nhận)
□ 9.8 Tài liệu bàn giao đầy đủ (ký nhận)
```

### 11.3 Stress Test và Soak Test

```
Stress Test — Đẩy đến giới hạn:
□ Chạy tối đa UPH trong 30 phút
□ Trigger 100 alarm liên tiếp
□ Import recipe 50 lần liên tiếp
□ Mở/đóng 20 popup cùng lúc (nếu multi-user)

Soak Test — Chạy dài hạn:
□ Chạy 72 giờ liên tục với production bình thường
□ Theo dõi: CPU, RAM, disk space, database size mỗi giờ
□ Không crash, không memory leak, data chính xác
```

---

# PHẦN VI — BẢO MẬT PHẦN MỀM

---

## Chương 12: Bảo mật trong phần mềm máy công nghiệp

### 12.1 Các mối đe doạ thực tế

| Mối đe doạ | Hậu quả | Biện pháp |
|-----------|---------|----------|
| Thay đổi recipe trái phép | Sản phẩm lỗi hàng loạt | RBAC, audit log |
| Đọc trộm dữ liệu sản xuất | Lộ IP, năng suất | Encrypt DB, HTTPS |
| Cài phần mềm độc hại | Ransomware, mất data | Application whitelist |
| Truy cập vật lý vào máy | Thao túng trực tiếp | Auto-lock, USB block |
| Tấn công mạng nội bộ | Control từ xa | Firewall, VPN |

### 12.2 Authentication — Xác thực người dùng

```csharp
// UserService.cs — không bao giờ lưu password dạng plaintext
public class UserService : IUserService
{
    public async Task<LoginResult> LoginAsync(string username, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(username);

        if (user == null)
            return LoginResult.Failed("Tài khoản không tồn tại");

        // BCrypt verify — dù database bị lấy, password vẫn an toàn
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            await RecordFailedLoginAsync(username);

            if (await GetFailedLoginCountAsync(username) >= 3)
            {
                await LockAccountAsync(username);
                return LoginResult.Failed("Tài khoản bị khoá sau 3 lần sai. Liên hệ Admin.");
            }

            return LoginResult.Failed("Sai mật khẩu");
        }

        // Tạo session token
        var token = GenerateSessionToken(user);
        await _auditLogger.LogAsync("LOGIN", username, "Login successful");

        return LoginResult.Success(token, user.Role);
    }

    public async Task<string> HashPasswordAsync(string plainPassword)
    {
        // BCrypt với work factor 12 — đủ chậm để brute-force khó
        return BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: 12);
    }
}
```

### 12.3 Authorization — RBAC (Role-Based Access Control)

```csharp
// PermissionService.cs
public class PermissionService : IPermissionService
{
    private static readonly Dictionary<UserRole, HashSet<Permission>> _permissions = new()
    {
        [UserRole.Operator] = new()
        {
            Permission.ViewProduction,
            Permission.AcknowledgeAlarm,
            Permission.StartMachine,
            Permission.StopMachine,
        },
        [UserRole.Technician] = new()
        {
            Permission.ViewProduction,
            Permission.AcknowledgeAlarm,
            Permission.StartMachine,
            Permission.StopMachine,
            Permission.ManualJog,         // Thêm quyền jog
            Permission.ViewIODetail,
            Permission.ViewLog,
        },
        [UserRole.Engineer] = new()
        {
            // Kế thừa tất cả Technician +
            Permission.EditParameter,
            Permission.LoadRecipe,
            Permission.CameraCalibration,
            Permission.ViewEngineeringScreen,
        },
        [UserRole.Admin] = new()
        {
            // Tất cả quyền
            Permission.All,
        },
    };

    public bool HasPermission(UserInfo user, Permission permission)
    {
        if (!_permissions.TryGetValue(user.Role, out var perms))
            return false;

        return perms.Contains(Permission.All) || perms.Contains(permission);
    }
}
```

```csharp
// Dùng trong ViewModel — check permission trước khi enable button
public class ParameterViewModel : ObservableObject
{
    private readonly IPermissionService _permission;
    private readonly IUserService _userService;

    public bool CanEditParameter =>
        _permission.HasPermission(_userService.CurrentUser, Permission.EditParameter);

    [RelayCommand(CanExecute = nameof(CanEditParameter))]
    private async Task SaveParameter()
    {
        // Chỉ chạy được khi CanEditParameter = true
        await _parameterService.SaveAsync(_editingParameter);
        await _auditLogger.LogAsync("PARAMETER_CHANGED",
            _userService.CurrentUser.Username,
            $"Changed {_editingParameter.Key} = {_editingParameter.Value}");
    }
}
```

### 12.4 Audit Log — Nhật ký thao tác

```csharp
// AuditLogger.cs — ghi tất cả hành động quan trọng
public class AuditLogger : IAuditLogger
{
    public async Task LogAsync(string action, string username, string details,
                               CancellationToken ct = default)
    {
        var entry = new AuditLogEntity
        {
            Timestamp = DateTime.UtcNow,
            Action = action,
            Username = username,
            Details = details,
            ComputerName = Environment.MachineName,
            SessionId = _sessionManager.CurrentSessionId,
        };

        await _auditRepository.InsertAsync(entry, ct);
    }
}

// Các hành động phải audit:
// - LOGIN / LOGOUT
// - PARAMETER_CHANGED (key, old value, new value)
// - RECIPE_LOADED (tên recipe)
// - ALARM_ACKNOWLEDGED (alarm code)
// - ALARM_CLEARED (alarm code)
// - USER_CREATED / USER_MODIFIED / USER_DELETED
// - INTERLOCK_OVERRIDE (ai override, lý do)
// - MACHINE_STARTED / MACHINE_STOPPED
```

### 12.5 Bảo vệ dữ liệu cấu hình

```csharp
// ConfigurationProtector.cs — mã hoá thông tin nhạy cảm
public class ConfigurationProtector
{
    // KHÔNG lưu plaintext trong file config
    // Dùng Windows DPAPI — chỉ decrypt được trên cùng máy, cùng user

    public string Protect(string plaintext)
    {
        var data = Encoding.UTF8.GetBytes(plaintext);
        var encrypted = ProtectedData.Protect(data, null, DataProtectionScope.LocalMachine);
        return Convert.ToBase64String(encrypted);
    }

    public string Unprotect(string ciphertext)
    {
        var data = Convert.FromBase64String(ciphertext);
        var decrypted = ProtectedData.Unprotect(data, null, DataProtectionScope.LocalMachine);
        return Encoding.UTF8.GetString(decrypted);
    }
}

// appsettings.json — không lưu password thật
{
  "Database": {
    "ConnectionString": "Data Source=C:\\ProgramData\\AutoMachine\\machine.db"
  },
  "Hardware": {
    "ControllerIP": "ENCRYPTED:Q29udHJvbGxlcklQ..."  // mã hoá bằng DPAPI
  }
}
```

### 12.6 Bảo mật SQLite Database

```csharp
// Dùng SQLite với encryption (SQLCipher)
// NuGet: Microsoft.EntityFrameworkCore.Sqlite + SQLitePCLRaw.bundle_sqlcipher

var optionsBuilder = new DbContextOptionsBuilder<MachineDbContext>();
optionsBuilder.UseSqlite($"Data Source={dbPath};Password={GetDbKey()}");

// DB key lưu trong Windows Credential Manager — không hardcode
private string GetDbKey()
{
    using var credential = new WindowsCredential("AutoMachine_DBKey");
    return credential.Password ?? throw new InvalidOperationException("DB key not configured");
}
```

---

# PHẦN VII — CI/CD (CONTINUOUS INTEGRATION / CONTINUOUS DELIVERY)

---

## Chương 13: CI/CD cho phần mềm máy

### 13.1 CI/CD là gì — giải thích đơn giản

```
Không có CI/CD:
Developer viết code → commit → "works on my machine" → giao cho khách hàng → lỗi

Có CI/CD:
Developer commit → Tự động: build + test + check → Nếu pass → tạo installer → sẵn sàng deploy

Lợi ích:
- Phát hiện lỗi ngay sau commit, không phải sau 1 tuần
- Mọi người luôn có build mới nhất để test
- Deploy nhất quán — không phụ thuộc vào "tay nghề" ai build
- Lịch sử build rõ ràng — biết version nào có gì
```

### 13.2 Setup GitHub Actions (miễn phí)

```yaml
# .github/workflows/build-and-test.yml
name: Build and Test

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: windows-latest   # Cần Windows vì WPF

    steps:
    - name: Checkout code
      uses: actions/checkout@v4

    - name: Setup .NET 8
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'

    - name: Restore NuGet packages
      run: dotnet restore AutoMachine.slnx

    - name: Build solution
      run: dotnet build AutoMachine.slnx
             --configuration Release
             --no-restore
             --warnaserror    # Warning cũng là lỗi

    - name: Run unit tests
      run: dotnet test tests/AM.Core.Tests/
             --configuration Release
             --no-build
             --collect:"XPlat Code Coverage"
             --results-directory ./coverage

    - name: Run service tests
      run: dotnet test tests/AM.Services.Tests/
             --configuration Release
             --no-build
             --collect:"XPlat Code Coverage"
             --results-directory ./coverage

    - name: Generate coverage report
      uses: danielpalme/ReportGenerator-GitHub-Action@5
      with:
        reports: 'coverage/**/coverage.cobertura.xml'
        targetdir: 'coveragereport'
        reporttypes: 'HtmlInline;Cobertura'

    - name: Upload coverage report
      uses: actions/upload-artifact@v4
      with:
        name: coverage-report
        path: coveragereport/

    - name: Check coverage threshold
      run: |
        $coverage = [xml](Get-Content coverage/**/coverage.cobertura.xml)
        $lineRate = [double]$coverage.coverage.'line-rate'
        if ($lineRate -lt 0.70) {
          Write-Error "Coverage $($lineRate * 100)% is below 70% threshold"
          exit 1
        }
      shell: pwsh
```

### 13.3 CD — Tạo installer tự động

```yaml
# .github/workflows/release.yml
name: Create Release

on:
  push:
    tags:
      - 'v*'   # Trigger khi push tag như v1.0.0, v1.2.3

jobs:
  release:
    runs-on: windows-latest

    steps:
    - uses: actions/checkout@v4

    - name: Setup .NET 8
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'

    - name: Get version from tag
      id: version
      run: echo "VERSION=${GITHUB_REF#refs/tags/v}" >> $GITHUB_OUTPUT
      shell: bash

    - name: Build Release
      run: |
        dotnet publish src/AM.Application.Shell/AM.Application.Shell.csproj `
          --configuration Release `
          --runtime win-x64 `
          --self-contained true `
          --output ./publish `
          /p:Version=${{ steps.version.outputs.VERSION }} `
          /p:AssemblyVersion=${{ steps.version.outputs.VERSION }}

    - name: Run all tests before release
      run: dotnet test AutoMachine.slnx --configuration Release --no-build

    - name: Create installer with Inno Setup
      run: |
        iscc /DMyAppVersion="${{ steps.version.outputs.VERSION }}" `
             /DMyAppOutput="./installer" `
             tools/setup.iss
      shell: pwsh

    - name: Create GitHub Release
      uses: softprops/action-gh-release@v2
      with:
        files: |
          installer/AutoMachine_Setup_v${{ steps.version.outputs.VERSION }}.exe
        body: |
          ## Version ${{ steps.version.outputs.VERSION }}
          ### Changes
          - (fill in changelog)
          ### Installation
          Run the installer as Administrator.
          ### Requirements
          - Windows 10/11 64-bit
          - .NET 8 Runtime (bundled in installer)
```

### 13.4 Code Quality Gates — Chặn code xấu

```yaml
# Thêm vào build.yml — kiểm tra code quality trước khi merge
    - name: Code Analysis (built-in Roslyn analyzers)
      run: |
        dotnet build --configuration Release `
          /p:RunAnalyzersDuringBuild=true `
          /p:TreatWarningsAsErrors=true

    - name: Check for hardcoded secrets (GitLeaks)
      uses: gitleaks/gitleaks-action@v2
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

```xml
<!-- Directory.Build.props — áp dụng cho toàn bộ solution -->
<Project>
  <PropertyGroup>
    <!-- Bật tất cả Roslyn analyzers -->
    <AnalysisMode>All</AnalysisMode>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>

    <!-- Nullable reference types — bắt null exception khi compile -->
    <Nullable>enable</Nullable>

    <!-- Implicit usings -->
    <ImplicitUsings>enable</ImplicitUsings>

    <!-- Version info -->
    <Authors>YourCompany</Authors>
    <Company>YourCompany</Company>
  </PropertyGroup>

  <!-- Security analyzers -->
  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="8.*">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
    <PackageReference Include="SonarAnalyzer.CSharp" Version="9.*">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
  </ItemGroup>
</Project>
```

### 13.5 Versioning — Đánh số phiên bản

```
Format: MAJOR.MINOR.PATCH[-PRERELEASE]

MAJOR: Thay đổi breaking (không tương thích với version cũ)
MINOR: Tính năng mới, tương thích ngược
PATCH: Sửa lỗi

Ví dụ:
1.0.0       ← Release đầu tiên tại SAT
1.0.1       ← Sửa lỗi nhỏ trong tuần đầu
1.1.0       ← Thêm tính năng báo cáo mới
2.0.0       ← Redesign màn hình chính (breaking)

Tag git:
git tag v1.0.0 -m "Initial release - SAT passed 2024-01-15"
git push origin v1.0.0
```

---

# PHẦN VIII — HIỆU NĂNG (PERFORMANCE)

---

## Chương 14: Kiểm tra và tối ưu hiệu năng

### 14.1 Các vấn đề hiệu năng thường gặp

| Vấn đề | Triệu chứng | Nguyên nhân |
|--------|------------|-------------|
| UI freeze | Màn hình đứng vài giây | Block UI thread |
| Memory leak | RAM tăng dần theo giờ | Event handler không unsubscribe |
| Database slow | Query chậm sau nhiều ngày | Không có index, không cleanup |
| High CPU idle | Fan laptop chạy mạnh khi idle | Update quá nhanh, animation loop |
| Startup slow | Khởi động > 20 giây | Load tất cả cùng lúc, không lazy |

### 14.2 Profiling với Visual Studio

```
// Cách profile ứng dụng:
1. Menu: Debug → Performance Profiler
2. Chọn: CPU Usage + Memory Usage
3. Click Start
4. Thực hiện thao tác cần kiểm tra
5. Stop → Xem flame graph

// Những chỉ số cần chú ý:
CPU Hotspot:    Hàm nào tốn nhiều CPU nhất?
Memory:         Đối tượng nào chiếm nhiều RAM?
                Có object nào không bị GC collect?
Thread:         UI thread có bị block không?
```

### 14.3 Benchmark với BenchmarkDotNet

```csharp
// Đo hiệu năng chính xác từng hàm
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class AlarmServiceBenchmarks
{
    private AlarmService _service;
    private List<AlarmInfo> _testAlarms;

    [GlobalSetup]
    public void Setup()
    {
        _service = new AlarmService(...);
        _testAlarms = Enumerable.Range(1, 1000)
            .Select(i => new AlarmInfo { Code = i, Station = "TEST" })
            .ToList();
    }

    [Benchmark]
    public void AddRemove1000Alarms()
    {
        foreach (var alarm in _testAlarms)
            _service.Raise(alarm);
        foreach (var alarm in _testAlarms)
            _service.Clear(alarm.Code, alarm.Station);
    }

    [Benchmark]
    public IEnumerable<AlarmInfo> GetActiveAlarms_WithFilter()
    {
        return _service.GetActiveAlarms(AlarmLevel.High);
    }
}

// Chạy: dotnet run -c Release
// Kết quả: thời gian ms, memory allocated
```

### 14.4 Memory Leak — Vấn đề phổ biến nhất

```csharp
// ❌ LỖI PHỔ BIẾN: Event handler không unsubscribe
public class AxisViewModel
{
    public AxisViewModel(IMotionController motion)
    {
        // Subscribe event
        motion.PositionChanged += OnPositionChanged; // LEAK!
        // MotionController giữ reference đến AxisViewModel
        // AxisViewModel không bao giờ bị GC
    }
    // Không có unsubscribe → memory leak
}

// ✅ ĐÚNG: Implement IDisposable và unsubscribe
public class AxisViewModel : ObservableObject, IDisposable
{
    private readonly IMotionController _motion;

    public AxisViewModel(IMotionController motion)
    {
        _motion = motion;
        _motion.PositionChanged += OnPositionChanged;
    }

    public void Dispose()
    {
        _motion.PositionChanged -= OnPositionChanged; // Unsubscribe
    }

    private void OnPositionChanged(double position)
    {
        Position = position;
    }
}

// Hoặc dùng WeakEventManager (WPF)
WeakEventManager<IMotionController, PositionEventArgs>
    .AddHandler(_motion, nameof(IMotionController.PositionChanged), OnPositionChanged);
```

### 14.5 Database Performance

```csharp
// ❌ N+1 Query problem — query trong loop
foreach (var alarm in alarms)
{
    var history = await _context.AlarmHistory  // 1 query mỗi alarm!
        .Where(h => h.AlarmCode == alarm.Code)
        .ToListAsync();
}

// ✅ Join query — 1 query duy nhất
var alarmsWithHistory = await _context.Alarms
    .Include(a => a.History)  // JOIN trong 1 query
    .Where(a => a.IsActive)
    .ToListAsync();

// ✅ Index cho các column hay filter/sort
public class MachineDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Index cho column thường xuyên query
        modelBuilder.Entity<ProductionRecordEntity>()
            .HasIndex(p => p.Timestamp);          // Query theo ngày
        modelBuilder.Entity<ProductionRecordEntity>()
            .HasIndex(p => p.SerialNumber);        // Lookup theo SN
        modelBuilder.Entity<AlarmHistoryEntity>()
            .HasIndex(a => new { a.AlarmCode, a.Station }); // Composite index
    }
}

// ✅ Cleanup old data — tránh DB phình to
public async Task CleanupOldDataAsync(int retentionDays = 365)
{
    var cutoff = DateTime.UtcNow.AddDays(-retentionDays);
    await _context.ProductionRecords
        .Where(r => r.Timestamp < cutoff)
        .ExecuteDeleteAsync();  // EF Core 7+ bulk delete
}
```

### 14.6 UI Performance

```csharp
// ❌ Update quá nhanh từ hardware
timer.Interval = TimeSpan.FromMilliseconds(1); // 1000 Hz — quá nhiều
timer.Tick += (s, e) => {
    PositionX = _motion.GetPosition(0); // Trigger 1000 PropertyChanged/giây
};

// ✅ Throttle update rate
private DateTime _lastUiUpdate = DateTime.MinValue;

// Gọi từ hardware callback
public void OnHardwarePositionUpdate(double newPosition)
{
    var now = DateTime.Now;
    if ((now - _lastUiUpdate).TotalMilliseconds < 100) // max 10 Hz
        return;

    _lastUiUpdate = now;
    Application.Current.Dispatcher.InvokeAsync(() =>
    {
        PositionX = newPosition;
    }, DispatcherPriority.Background); // Background priority — không block UI
}
```

---

# PHẦN IX — LOGGING VÀ MONITORING

---

## Chương 15: Logging chiến lược

### 15.1 Cấu hình log4net

```xml
<!-- log4net.config -->
<log4net>
  <!-- File log hàng ngày, giữ 30 ngày -->
  <appender name="RollingFileAppender" type="log4net.Appender.RollingFileAppender">
    <file value="C:\ProgramData\AutoMachine\Logs\app" />
    <appendToFile value="true" />
    <rollingStyle value="Date" />
    <datePattern value="_yyyy-MM-dd'.log'" />
    <staticLogFileName value="false" />
    <preserveLogFileNameExtension value="true" />
    <maxSizeRollBackups value="30" />
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="%date{yyyy-MM-dd HH:mm:ss.fff} [%thread] %-5level %logger - %message%newline" />
    </layout>
  </appender>

  <!-- Console appender cho debug -->
  <appender name="ConsoleAppender" type="log4net.Appender.ConsoleAppender">
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="%date{HH:mm:ss.fff} %-5level %logger{1} - %message%newline" />
    </layout>
  </appender>

  <!-- Logger riêng cho motion — verbose hơn -->
  <logger name="AM.Hardware.Motion">
    <level value="DEBUG" />
    <appender-ref ref="MotionFileAppender" />
  </logger>

  <root>
    <level value="INFO" />
    <appender-ref ref="RollingFileAppender" />
  </root>
</log4net>
```

### 15.2 Quy tắc log đúng cách

```csharp
// Sử dụng log level đúng:
_logger.LogTrace("Axis position: {Pos}", position);        // Cực kỳ chi tiết (dev only)
_logger.LogDebug("Starting home sequence for axis {Axis}", axisName); // Debug
_logger.LogInformation("Recipe {Name} loaded successfully", recipeName); // Bình thường
_logger.LogWarning("Temperature {Temp}°C approaching limit {Limit}°C", temp, limit); // Cảnh báo
_logger.LogError(ex, "Failed to connect to motion controller"); // Lỗi có exception
_logger.LogCritical("Database corruption detected, shutting down"); // Nguy hiểm

// ❌ SAI — log string concatenation (tốn memory nếu level không active)
_logger.LogDebug("Position is " + position.ToString()); // BAD

// ✅ ĐÚNG — structured logging với template
_logger.LogDebug("Position is {Position:F2} mm", position); // GOOD

// ❌ SAI — log exception message mà không có stack trace
catch (Exception ex) { _logger.LogError(ex.Message); }

// ✅ ĐÚNG — log cả exception object
catch (Exception ex) { _logger.LogError(ex, "Failed to move axis"); }
```

### 15.3 Structured Logging — query log được

```csharp
// Structured log với Serilog (bổ sung log4net)
Log.Logger = new LoggerConfiguration()
    .WriteTo.File(new JsonFormatter(), "logs/structured-.json",
                  rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Log với structured data
_logger.LogInformation(
    "Production cycle completed. {@CycleResult}",
    new {
        SerialNumber = sn,
        IsOK = result.Passed,
        CycleTime = stopwatch.Elapsed.TotalSeconds,
        Score = result.Score,
        Station = "ST01"
    });

// Kết quả JSON dễ query:
// {"@t":"2024-01-15T08:30:00","@mt":"Production cycle...",
//  "SerialNumber":"SN001","IsOK":true,"CycleTime":3.82,"Score":98.5}
```

---

# PHẦN X — DEPLOYMENT VÀ VẬN HÀNH

---

## Chương 16: Đóng gói và cài đặt

### 16.1 Tạo installer với Inno Setup (miễn phí)

```pascal
; tools/setup.iss
[Setup]
AppName=AutoMachine
AppVersion={#MyAppVersion}
AppPublisher=YourCompany
DefaultDirName={autopf}\AutoMachine
DefaultGroupName=AutoMachine
OutputDir=installer
OutputBaseFilename=AutoMachine_Setup_v{#MyAppVersion}
SetupIconFile=src\AM.Application.Shell\app.ico
Compression=lzma2/max
SolidCompression=yes
PrivilegesRequired=admin
MinVersion=10.0

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create desktop shortcut"; GroupDescription: "Additional Icons:"

[Files]
; Main application (self-contained, không cần .NET install riêng)
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\AutoMachine"; Filename: "{app}\AM.Application.Shell.exe"
Name: "{group}\Uninstall AutoMachine"; Filename: "{uninstallexe}"
Name: "{commondesktop}\AutoMachine"; Filename: "{app}\AM.Application.Shell.exe"; Tasks: desktopicon

[Run]
; Chạy lần đầu sau cài đặt
Filename: "{app}\AM.Application.Shell.exe"; Description: "Launch AutoMachine"; Flags: postinstall nowait skipifsilent

[UninstallDelete]
; Xoá data khi uninstall (hỏi user trước)
Type: filesandordirs; Name: "{app}\Logs"
```

### 16.2 Cấu hình máy tính công nghiệp

```powershell
# tools/setup.ps1 — chạy một lần khi setup máy mới
# Chạy với quyền Administrator

# 1. Tắt Windows Update tự động (tránh restart đột ngột)
Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU" `
  -Name "NoAutoUpdate" -Value 1 -Type DWORD

# 2. Tắt Sleep/Hibernate
powercfg /change standby-timeout-ac 0
powercfg /change hibernate-timeout-ac 0
powercfg /change monitor-timeout-ac 0

# 3. Autostart phần mềm khi Windows khởi động
$startupPath = [System.Environment]::GetFolderPath("CommonStartup")
$shortcut = (New-Object -ComObject WScript.Shell).CreateShortcut(
    "$startupPath\AutoMachine.lnk")
$shortcut.TargetPath = "C:\Program Files\AutoMachine\AM.Application.Shell.exe"
$shortcut.Save()

# 4. Firewall — chỉ cho phép port cần thiết
netsh advfirewall firewall add rule name="AutoMachine" `
  dir=in action=allow program="C:\Program Files\AutoMachine\AM.Application.Shell.exe"

# 5. Tạo scheduled task backup database hàng ngày
$action = New-ScheduledTaskAction -Execute "powershell.exe" `
  -Argument "-File `"C:\Program Files\AutoMachine\tools\backup.ps1`""
$trigger = New-ScheduledTaskTrigger -Daily -At "02:00"
Register-ScheduledTask -TaskName "AutoMachine_DailyBackup" `
  -Action $action -Trigger $trigger -RunLevel Highest
```

### 16.3 Backup và Recovery

```powershell
# tools/backup.ps1 — chạy hàng ngày lúc 2:00 sáng
$date = Get-Date -Format "yyyyMMdd"
$backupDir = "D:\Backup\AutoMachine\$date"
New-Item -ItemType Directory -Force -Path $backupDir

# Backup database
Copy-Item "C:\ProgramData\AutoMachine\machine.db" "$backupDir\machine.db"
Copy-Item "C:\ProgramData\AutoMachine\audit.db"   "$backupDir\audit.db"

# Backup config
Copy-Item "C:\ProgramData\AutoMachine\Config\*" "$backupDir\Config\" -Recurse

# Backup recipe
Copy-Item "C:\ProgramData\AutoMachine\Products\*" "$backupDir\Products\" -Recurse

# Nén và xoá backup cũ hơn 30 ngày
Compress-Archive -Path $backupDir -DestinationPath "$backupDir.zip"
Remove-Item $backupDir -Recurse -Force

Get-ChildItem "D:\Backup\AutoMachine\*.zip" |
  Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-30) } |
  Remove-Item -Force

Write-Host "Backup completed: $backupDir.zip"
```

---

# PHẦN XI — TIÊU CHUẨN VÀ QUY CHUẨN

---

## Chương 17: Tiêu chuẩn áp dụng

### 17.1 Tiêu chuẩn phần mềm

| Tiêu chuẩn | Áp dụng | Nội dung chính |
|-----------|---------|----------------|
| **IEC 62061** | Máy an toàn | Safety function software requirements |
| **IEC 61511** | Process safety | SIL requirements cho phần mềm |
| **ISO 13849** | Control safety | PLr (Performance Level) |
| **SEMI S2** | Semiconductor equipment | EHS requirements |
| **ISA-101** | HMI design | UI/UX standards |
| **ISA-18.2** | Alarm management | Alarm philosophy |
| **EEMUA 191/201** | Alarm management | Alarm rate, priority |

### 17.2 Coding Standards

```csharp
// Quy tắc đặt tên (Microsoft C# Conventions):

// Namespace: PascalCase
namespace AutoMachine.Services.Alarm

// Class, Interface, Enum: PascalCase
public class AlarmService : IAlarmService
public interface IMotionController
public enum MachineState

// Method, Property: PascalCase
public async Task RaiseAsync()
public double CurrentPosition { get; }

// Private field: _camelCase với underscore prefix
private readonly IAlarmService _alarmService;
private int _cycleCount;

// Local variable, parameter: camelCase
var alarmInfo = new AlarmInfo();
public void RaiseAlarm(int alarmCode, string stationName)

// Constant: PascalCase hoặc UPPER_SNAKE_CASE (internal)
public const int MaxAlarmCount = 1000;
private const string DEFAULT_RECIPE = "Default";

// Async method: luôn có suffix Async
public async Task<bool> HomeAsync(CancellationToken ct)
```

### 17.3 Document Headers

```csharp
// Mỗi file phải có header (có thể dùng snippet trong VS)
// -----------------------------------------------------------------
// File:    AlarmService.cs
// Project: AM.Services
// Author:  [Tên bạn]
// Created: 2024-01-15
// Purpose: Manages alarm lifecycle: raise, acknowledge, clear, history
// -----------------------------------------------------------------
```

### 17.4 Code Review Checklist

Trước khi merge Pull Request, reviewer kiểm tra:

```
□ Code compile không có warning
□ Unit test pass 100%
□ Coverage không giảm so với trước
□ Không có hardcoded string (dùng resource key)
□ Không có hardcoded color/number (dùng constant/resource)
□ Async/await dùng đúng (có CancellationToken)
□ Exception được catch đúng cấp
□ Không có Console.WriteLine (dùng ILogger)
□ Không có TODO/HACK/FIXME tồn đọng
□ Interface không bị thay đổi nếu không cần
□ Tài liệu XML comment trên public API
□ Log message có structured data, không string concat
□ Không leak sensitive data vào log
```

---

# PHẦN XII — TÀI LIỆU DỰ ÁN

---

## Chương 18: Tài liệu cần có

### 18.1 Danh sách tài liệu bắt buộc

```
docs/
├── requirements/
│   ├── FRS.md          ← Functional Requirements Specification
│   ├── alarm-list.xlsx ← Danh sách tất cả alarm: code, message, level, action
│   └── io-list.xlsx    ← Danh sách I/O: địa chỉ, tên, loại, chức năng
│
├── design/
│   ├── solution-structure.md  ← Kiến trúc solution (đã có)
│   ├── database-schema.md     ← Sơ đồ bảng database
│   ├── state-machine.md       ← Machine state diagram
│   └── wireframes/            ← Hình ảnh thiết kế màn hình
│
├── test/
│   ├── test-plan.md           ← Kế hoạch test
│   ├── FAT-checklist.xlsx     ← Checklist FAT
│   └── SAT-checklist.xlsx     ← Checklist SAT
│
└── manual/
    ├── operator-manual.pdf    ← Hướng dẫn vận hành cho operator
    ├── engineer-manual.pdf    ← Hướng dẫn cài đặt và bảo trì
    └── changelog.md           ← Lịch sử thay đổi version
```

### 18.2 README.md chuẩn

```markdown
# AutoMachine — [Tên Dự Án]

## Mô tả
Phần mềm điều khiển máy [mô tả ngắn]. Version hiện tại: v1.0.0

## Yêu cầu hệ thống
- Windows 10/11 64-bit
- .NET 8.0 Runtime
- RAM: ≥ 4GB
- Disk: ≥ 20GB

## Cài đặt Development
```bash
git clone https://github.com/yourcompany/automachine.git
cd automachine
dotnet restore
dotnet build
```

## Cấu hình
Chỉnh `src/AM.Application.Shell/appsettings.json`:
- `UseSimulation: true` — không cần phần cứng thật
- `UseSimulation: false` — kết nối hardware thật

## Chạy test
```bash
dotnet test
```

## Build Release
```bash
dotnet publish -c Release -r win-x64 --self-contained
```

## Cấu trúc project
Xem `docs/design/solution-structure.md`

## Liên hệ
- Dev: [email]
- Hotline: [số điện thoại]
```

---

# PHẦN XIII — ROADMAP HỌC TẬP

---

## Chương 19: Lộ trình 6 tháng cho kỹ sư điện

### Tháng 1 — Nền tảng C#
```
Week 1: Cài VS2022, .NET 8. Học C# cơ bản (biến, điều kiện, vòng lặp)
Week 2: Class, Interface, Inheritance. So sánh với PLC FB
Week 3: Async/await, Task, CancellationToken
Week 4: LINQ (query dữ liệu), Collections
→ Milestone: Viết được class AxisController đơn giản với mock hardware
```

### Tháng 2 — WPF và MVVM
```
Week 1: WPF cơ bản: Window, Grid, TextBlock, Button
Week 2: Data Binding, INotifyPropertyChanged
Week 3: CommunityToolkit.Mvvm: [ObservableProperty], [RelayCommand]
Week 4: Prism cơ bản: Module, Region, EventAggregator
→ Milestone: Màn hình hiển thị position axis, nút Start/Stop hoạt động
```

### Tháng 3 — Kiến trúc và DI
```
Week 1: Dependency Injection, DryIoc
Week 2: Repository pattern, Entity Framework Core + SQLite
Week 3: Exception handling, Logging với log4net
Week 4: Unit testing với xUnit + Moq
→ Milestone: AlarmService có unit test, kết nối database
```

### Tháng 4 — Hardware Integration
```
Week 1: Tích hợp motion controller (LTDMC hoặc simulator)
Week 2: Tích hợp camera (Basler/HIK SDK hoặc simulator)
Week 3: Tích hợp I/O card
Week 4: Machine state machine với Stateless
→ Milestone: Chạy được sequence đơn giản với simulator
```

### Tháng 5 — Features nâng cao
```
Week 1: Multi-language support (XML resource)
Week 2: Recipe management
Week 3: Production reporting (Excel export với NPOI)
Week 4: CI/CD với GitHub Actions
→ Milestone: Có CI pipeline tự động build và test
```

### Tháng 6 — Dự án hoàn chỉnh
```
Week 1-2: Build một máy đơn giản hoàn chỉnh (sequence + UI + alarm + report)
Week 3: Performance tuning, memory leak check
Week 4: Viết FAT checklist, thực hiện FAT tự test
→ Milestone: Dự án đầu tiên sẵn sàng demo cho khách hàng
```

### Tài nguyên học tập

| Loại | Tên | Link | Chi phí |
|------|-----|------|---------|
| Video | C# Fundamentals | Microsoft Learn | Miễn phí |
| Video | WPF with MVVM | Tim Corey (YouTube) | Miễn phí |
| Video | .NET 8 Course | freeCodeCamp | Miễn phí |
| Sách | C# in Depth | Jon Skeet | ~$45 |
| Sách | Clean Code | Robert C. Martin | ~$35 |
| Sách | Pro WPF 4.5 | Matthew MacDonald | ~$60 |
| Community | Stack Overflow | stackoverflow.com | Miễn phí |
| Community | .NET Discord | discord.gg/dotnet | Miễn phí |

---

## Phụ lục A — Quick Reference Card

### Git hàng ngày
```bash
git pull                          # Lấy code mới nhất
git checkout -b feature/ten-tinh-nang  # Tạo nhánh mới
git add .                         # Stage changes
git commit -m "feat: mô tả"       # Commit
git push origin feature/ten-tinh-nang # Push lên server
```

### Chạy test nhanh
```bash
dotnet test --filter "ClassName=AlarmServiceTests"  # Test 1 class
dotnet test --collect:"XPlat Code Coverage"         # Test + coverage
```

### Build release
```bash
dotnet publish -c Release -r win-x64 --self-contained -o ./publish
```

### Xem log nhanh
```powershell
# Xem 50 dòng log cuối
Get-Content "C:\ProgramData\AutoMachine\Logs\app_$(Get-Date -f yyyy-MM-dd).log" -Tail 50

# Xem log real-time
Get-Content "*.log" -Wait -Tail 20
```

---

*Tài liệu này là điểm khởi đầu — thực hành mới là cách học tốt nhất.*
*Bắt đầu từ một màn hình đơn giản, một service nhỏ. Từng bước một.*
