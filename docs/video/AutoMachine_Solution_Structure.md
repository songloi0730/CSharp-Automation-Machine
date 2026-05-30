# AutoMachine Framework — Solution Structure Hoàn Chỉnh
### Dành cho phần mềm máy tự động hoá | C# / .NET 8 / WPF + Prism

> **Triết lý thiết kế:** Phần "máy móc" (hardware I/O, motion, vision, protocol) là bất biến qua các dự án. Phần "quy trình" (sequence/workflow của từng máy) là thứ duy nhất thay đổi. Mọi thứ còn lại tái sử dụng 100%.

---

## 1. Tổng quan cấu trúc Solution

```
AutoMachine.slnx
│
├── 📦 Core Layer            — Không phụ thuộc gì, dùng được ở mọi nơi
│   ├── AM.Core
│   └── AM.Core.Abstractions
│
├── 📦 Infrastructure Layer  — Hardware, DB, Logging, I18n
│   ├── AM.Infrastructure
│   ├── AM.Data
│   └── AM.CommonTools
│
├── 📦 Services Layer        — Business logic, không UI
│   └── AM.Services
│
├── 📦 Hardware Drivers      — Wrapper cho từng loại thiết bị
│   ├── AM.Hardware.Motion
│   ├── AM.Hardware.Vision
│   ├── AM.Hardware.IO
│   └── AM.Hardware.Communication
│
├── 📦 Machine Sequence      — ⭐ PHẦN DUY NHẤT THAY ĐỔI KHI LÀM MÁY MỚI
│   └── AM.WorkStation.{MachineName}
│
├── 📦 Modules (Prism)       — Các module UI độc lập, load theo cấu hình
│   ├── AM.Modules.Alarm
│   ├── AM.Modules.Parameter
│   ├── AM.Modules.Production
│   ├── AM.Modules.IO
│   ├── AM.Modules.Motion
│   ├── AM.Modules.Vision
│   ├── AM.Modules.Identity
│   ├── AM.Modules.Logging
│   ├── AM.Modules.Diagnostics
│   └── AM.Modules.SecsGem       (optional — bật/tắt qua config)
│
├── 📦 UI Shared             — Controls, Resources, Themes
│   ├── AM.UI.Controls
│   ├── AM.UI.Resources
│   └── AM.UI.Shared
│
└── 🚀 Application Shell     — Entry point, DI container, Module catalog
    └── AM.Application.Shell
```

---

## 2. Chi tiết từng Project

### 2.1 AM.Core.Abstractions
**Mục đích:** Định nghĩa interface & base class. Không reference bất cứ thứ gì.

```
AM.Core.Abstractions/
├── Interfaces/
│   ├── Hardware/
│   │   ├── IMotionController.cs        — MoveAbs, MoveRel, Home, GetPos, IsInMotion
│   │   ├── IAxisGroup.cs               — Multi-axis coordinated motion
│   │   ├── ICameraDevice.cs            — Grab, TriggerSoftware, SetExposure
│   │   ├── ILightController.cs         — SetIntensity, SetChannel
│   │   ├── IIoModule.cs                — ReadDI, WriteDO, ReadAI, WriteAO
│   │   └── ICommunicationDevice.cs     — Connect, Send, Receive (TCP/Serial/EtherCAT)
│   │
│   ├── Services/
│   │   ├── IAlarmService.cs            — Raise, Clear, Subscribe
│   │   ├── IParameterService.cs        — Get<T>, Set<T>, Load, Save
│   │   ├── IProductionService.cs       — StartBatch, EndBatch, RecordResult
│   │   ├── ILogService.cs              — Log, LogStructured
│   │   ├── IUserService.cs             — Login, Logout, CheckPermission
│   │   └── ILocalizationService.cs     — GetString, ChangeLanguage
│   │
│   ├── Machine/
│   │   ├── IMachineSequence.cs         — Initialize, Run, Pause, Stop, Reset, Home
│   │   ├── IMachineState.cs            — State machine definition
│   │   ├── IWorkStation.cs             — Multi-workstation coordination
│   │   └── IRecipe.cs                  — Product-specific parameters
│   │
│   └── Events/                         — Prism EventAggregator events
│       ├── AlarmChangedEvent.cs
│       ├── MachineStateChangedEvent.cs
│       ├── ProductionDataEvent.cs
│       └── LanguageChangedEvent.cs
```

---

### 2.2 AM.Core
**Mục đích:** Base class, enums, common value objects, state machine foundation.

```
AM.Core/
├── Enums/
│   ├── MachineState.cs         — Idle, Initializing, Running, Paused, Error, EStop, Homing
│   ├── AlarmLevel.cs           — Info, Warning, Error, Critical, EStop
│   ├── AxisState.cs            — Ready, Moving, Homing, Error, Disabled
│   ├── InspectionResult.cs     — Pass, Fail, NG, Skip
│   ├── ProductionResult.cs     — OK, NG, Rework, Scrap
│   └── UserRole.cs             — Operator, Engineer, Admin, SuperAdmin
│
├── Models/
│   ├── AlarmInfo.cs            — Code, Message, Level, Timestamp, Station
│   ├── AxisPosition.cs         — Value, Unit, IsAbsolute
│   ├── ProductionRecord.cs     — SerialNumber, Result, Timestamp, Data
│   ├── RecipeBase.cs           — Id, Name, Version, CreatedBy, ModifiedDate
│   └── UserInfo.cs             — Id, Name, Role, LoginTime
│
├── StateMachine/
│   ├── MachineStateMachine.cs  — Stateless FSM wrapper
│   ├── StateTransition.cs      — From, To, Trigger, Guard, Action
│   └── StateMachineBuilder.cs  — Fluent builder
│
└── Constants/
    ├── AlarmCodes.cs           — const int định nghĩa tất cả mã lỗi
    ├── ParameterKeys.cs        — const string key cho parameter
    └── RegionNames.cs          — Prism region names (UI shell regions)
```

---

### 2.3 AM.Infrastructure
**Mục đích:** Cross-cutting concerns — Logging, I18n, Security, Configuration.

```
AM.Infrastructure/
├── Localization/
│   ├── LocalizationService.cs          — Đọc XML ngôn ngữ, notify khi đổi
│   ├── LanguageManager.cs              — Quản lý danh sách ngôn ngữ
│   └── Resources/
│       ├── Languages/
│       │   ├── vi-VN.xml               — Tiếng Việt (default)
│       │   ├── en-US.xml               — English
│       │   ├── zh-CN.xml               — 简体中文
│       │   └── zh-TW.xml               — 繁體中文
│       └── LanguageExtension.cs        — XAML Markup Extension {lang:Text Key=...}
│
├── Logging/
│   ├── LogService.cs                   — Wrapper log4net + structured log
│   ├── LogServiceExtensions.cs         — DI extension method
│   └── log4net.config                  — Rolling file, max 30 days
│
├── Security/
│   ├── PasswordHasher.cs               — BCrypt hashing
│   ├── TokenManager.cs                 — JWT-based session token
│   └── AuditLogger.cs                  — Ghi lại thao tác operator
│
├── Configuration/
│   ├── AppSettings.cs                  — appsettings.json binding
│   ├── HardwareConfig.cs               — Cấu hình phần cứng (axes, IOs, cameras)
│   └── MachineConfig.cs                — Thông tin máy (name, serial, location)
│
└── Persistence/
    ├── AppDbContext.cs                  — EF Core SQLite
    ├── Migrations/                      — EF migrations
    └── GenericRepository.cs             — Repository pattern
```

---

### 2.4 AM.Data
**Mục đích:** Entity definitions & Database access.

```
AM.Data/
├── Entities/
│   ├── AlarmEntity.cs
│   ├── AlarmHistoryEntity.cs
│   ├── ProductionRecordEntity.cs
│   ├── ParameterEntity.cs
│   ├── UserEntity.cs
│   ├── RecipeEntity.cs
│   └── AuditLogEntity.cs
│
├── Repositories/
│   ├── IAlarmRepository.cs
│   ├── AlarmRepository.cs
│   ├── IProductionRepository.cs
│   ├── ProductionRepository.cs
│   ├── IParameterRepository.cs
│   └── ParameterRepository.cs
│
└── DbContexts/
    ├── MachineDbContext.cs              — SQLite main DB (params, production, alarms)
    └── AuditDbContext.cs                — Separate audit trail DB
```

---

### 2.5 AM.CommonTools
**Mục đích:** Utility classes dùng chung — không có dependency vào core framework.

```
AM.CommonTools/
├── Extensions/
│   ├── StringExtensions.cs             — ToDouble, ToBool, SafeTrim
│   ├── CollectionExtensions.cs         — Batch, SafeForEach
│   ├── DateTimeExtensions.cs           — ToDisplayString, ToIso8601
│   └── TaskExtensions.cs               — TimeoutAfter, FireAndForget
│
├── IO/
│   ├── FileHelper.cs                   — EnsureDirectory, SafeWriteJson, SafeReadJson
│   ├── CsvHelper.cs                    — Export production data to CSV
│   ├── ExcelExporter.cs                — NPOI-based Excel report generation
│   └── ImageHelper.cs                  — Save/load/compress camera images
│
├── Math/
│   ├── StatisticsHelper.cs             — Mean, StdDev, Cpk, Ppk
│   ├── CoordinateTransform.cs          — 2D/3D coordinate conversion
│   └── UnitConverter.cs                — mm/inch/pulse conversions
│
└── Diagnostics/
    ├── PerformanceMonitor.cs           — Cycle time tracking
    ├── SystemInfoHelper.cs             — CPU, RAM, Disk usage
    └── DiagnosticSnapshot.cs           — Export full system snapshot
```

---

### 2.6 AM.Hardware.Motion
**Mục đích:** Wrapper cho motion controller (LTDMC, GTS, EtherCAT, etc.)

```
AM.Hardware.Motion/
├── Abstractions/
│   ├── AxisBase.cs                     — Base class cho mọi loại axis
│   └── MotionControllerBase.cs         — Template method pattern
│
├── Controllers/
│   ├── LtdmcController.cs              — Leadshine LTDMC
│   ├── GtsController.cs                — Googol GTS-400/800
│   ├── EtherCatController.cs           — Generic EtherCAT (SOEM)
│   └── SimulatedController.cs          — ⭐ Software simulation — test không cần HW
│
├── Axis/
│   ├── LinearAxis.cs                   — Trục thẳng
│   ├── RotaryAxis.cs                   — Trục quay
│   └── MultiAxisGroup.cs               — Điều phối nhiều trục cùng lúc
│
├── Motion/
│   ├── MotionProfile.cs                — Vel, Acc, Dec, Jerk
│   ├── MotionSequence.cs               — Danh sách điểm chạy tuần tự
│   └── TeachPoint.cs                   — Tọa độ dạy tay, có tên
│
└── Monitoring/
    ├── PositionMonitor.cs              — Theo dõi vị trí real-time
    └── MotionErrorMonitor.cs           — Detect & raise alarm khi lỗi
```

---

### 2.7 AM.Hardware.Vision
**Mục đích:** Wrapper camera + vision processing (Cognex VisionPro, Halcon, OpenCV).

```
AM.Hardware.Vision/
├── Cameras/
│   ├── CognexGigECamera.cs             — Cognex GigE
│   ├── BaslerCamera.cs                 — Basler Pylon
│   ├── HIKCamera.cs                    — HIK Robot / MVS
│   └── SimulatedCamera.cs              — Load ảnh từ folder để test
│
├── Lighting/
│   ├── SchottLightController.cs
│   ├── CCSLightController.cs
│   └── SimulatedLightController.cs
│
├── VisionTools/
│   ├── VPToolRunner.cs                 — Chạy Cognex VisionPro .vpp file
│   ├── HalconProcedureRunner.cs        — Chạy Halcon procedure
│   ├── VisionResult.cs                 — Pass/Fail + measurements + image
│   └── CalibrationManager.cs           — Quản lý calibration data
│
└── ImageStorage/
    ├── ImageSaveService.cs             — Lưu ảnh theo folder/date/SN
    └── ImageCleanupService.cs          — Tự động xóa ảnh cũ
```

---

### 2.8 AM.Hardware.IO
**Mục đích:** Digital/Analog I/O, relay, tower light.

```
AM.Hardware.IO/
├── Modules/
│   ├── IOC0640Module.cs                — IOC-0640 DIO card
│   ├── MitsubishiQModule.cs            — Mitsubishi Q series
│   ├── OmronNxModule.cs                — Omron NX EtherCAT
│   └── SimulatedIOModule.cs            — Simulation
│
├── TowerLight/
│   ├── TowerLightService.cs            — Điều khiển đèn tháp
│   ├── TowerLightProfile.cs            — Green=Run, Yellow=Warn, Red=Alarm
│   └── BuzzerService.cs                — Còi báo
│
└── Safety/
    ├── EStopMonitor.cs                 — Giám sát nút dừng khẩn cấp
    └── SafetyGateMonitor.cs            — Giám sát cửa an toàn
```

---

### 2.9 AM.Hardware.Communication
**Mục đích:** TCP, Serial, EtherNet/IP, SECS/GEM, Modbus.

```
AM.Hardware.Communication/
├── Serial/
│   ├── SerialPortWrapper.cs
│   └── SerialProtocolBase.cs
│
├── Tcp/
│   ├── TcpClientWrapper.cs
│   ├── TcpServerWrapper.cs
│   └── TcpProtocolBase.cs
│
├── Modbus/
│   ├── ModbusTcpClient.cs
│   └── ModbusRtuClient.cs
│
└── SecsGem/
    ├── SecsGemService.cs               — SECS/GEM host communication
    ├── SecsGemConfig.cs
    └── MessageHandlers/
        ├── S1F13Handler.cs             — Establish Communication
        ├── S2F41Handler.cs             — Remote Command
        └── S6F11Handler.cs             — Event Report
```

---

### 2.10 AM.Services
**Mục đích:** Business logic services — không biết gì về UI.

```
AM.Services/
├── Alarm/
│   ├── AlarmService.cs                 — Raise, Clear, GetActive, GetHistory
│   ├── AlarmDictionaryService.cs       — Load alarm text từ DB/XML
│   └── AlarmServiceExtensions.cs       — DI registration
│
├── Parameter/
│   ├── ParameterService.cs             — Get/Set typed parameters, versioning
│   ├── RecipeService.cs                — Load/Save/Switch recipe
│   └── ParameterValidationService.cs   — Validate range/constraint
│
├── Production/
│   ├── ProductionService.cs            — Batch management, UPH, yield
│   ├── StatisticsService.cs            — Cpk, SPC, trend
│   └── ProductionReportService.cs      — Xuất báo cáo Excel/CSV
│
├── Identity/
│   ├── UserService.cs                  — Login/logout, session management
│   └── PermissionService.cs            — Role-based access control
│
├── Hardware/
│   ├── HardwareManagerService.cs       — Khởi tạo & quản lý tất cả HW
│   ├── HardwareHealthMonitor.cs        — Periodic health check
│   └── IOMappingService.cs             — Map IO số → IO tên có nghĩa
│
└── Sync/
    └── StationSyncService.cs           — Đồng bộ trạng thái giữa các station
```

---

### 2.11 ⭐ AM.WorkStation.{MachineName}   ← PHẦN DUY NHẤT THAY ĐỔI
**Mục đích:** Quy trình chạy máy cụ thể. Khi làm máy mới, chỉ tạo project này.

```
AM.WorkStation.SampleMachine/
│
├── AM.WorkStation.SampleMachine.csproj
│
├── MachineSequence.cs              — ⭐ Entry point, quản lý vòng lặp chính
│   // Implements IMachineSequence
│   // Initialize() → khởi tạo tất cả HW
│   // Run()        → vòng lặp sản xuất
│   // Pause()      → dừng tạm
│   // Stop()       → dừng an toàn
│   // Home()       → về gốc
│   // Reset()      → reset sau lỗi
│
├── Steps/                          — Mỗi file = 1 bước trong quy trình
│   ├── Step00_Initialize.cs        — Khởi tạo hardware, load recipe
│   ├── Step01_Home.cs              — Chạy về gốc tất cả trục
│   ├── Step02_WaitForPart.cs       — Chờ phôi (sensor/robot signal)
│   ├── Step03_ReadBarcode.cs       — Đọc mã vạch/QR
│   ├── Step04_LoadPart.cs          — Di chuyển phôi vào vị trí xử lý
│   ├── Step05_Process.cs           — ⭐ QUY TRÌNH CHÍNH (hàn, ép, kiểm tra...)
│   ├── Step06_Inspect.cs           — Kiểm tra kết quả (vision/measurement)
│   ├── Step07_Sort.cs              — Phân loại OK/NG
│   ├── Step08_Unload.cs            — Xuất phôi
│   └── Step09_RecordData.cs        — Ghi dữ liệu production
│
├── SubRoutines/                    — Chương trình con tái sử dụng trong máy
│   ├── ManualJog.cs                — Jog thủ công từng trục
│   ├── CameraCalibration.cs        — Quy trình calibration camera
│   └── SafetyCheck.cs              — Kiểm tra an toàn trước khi chạy
│
├── Recipe/
│   ├── SampleMachineRecipe.cs      — Recipe class cho máy này
│   └── RecipeValidator.cs          — Validate recipe trước khi load
│
└── Config/
    ├── AxisMap.cs                  — Map tên trục → index controller
    ├── IOMap.cs                    — Map tên IO → địa chỉ IO
    └── StationConfig.cs            — Cấu hình đặc thù của máy
```

#### Ví dụ MachineSequence.cs (template)
```csharp
public class MachineSequence : IMachineSequence
{
    // Inject tất cả services qua DI — KHÔNG new trực tiếp
    public MachineSequence(
        IMotionController motion,
        ICameraDevice camera,
        IIoModule io,
        IAlarmService alarmService,
        IParameterService paramService,
        IProductionService productionService,
        ILogger<MachineSequence> logger) { ... }

    public async Task<bool> InitializeAsync(CancellationToken ct)
    {
        // 1. Khởi tạo motion controller
        // 2. Khởi tạo camera
        // 3. Khởi tạo IO
        // 4. Load recipe
        // 5. Validate safety
    }

    public async Task RunAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await _step02_WaitForPart.ExecuteAsync(ct);
                await _step03_ReadBarcode.ExecuteAsync(ct);
                await _step04_LoadPart.ExecuteAsync(ct);
                await _step05_Process.ExecuteAsync(ct);
                await _step06_Inspect.ExecuteAsync(ct);
                await _step07_Sort.ExecuteAsync(ct);
                await _step08_Unload.ExecuteAsync(ct);
                await _step09_RecordData.ExecuteAsync(ct);
            }
            catch (AlarmException ex)
            {
                await _alarmService.RaiseAsync(ex.AlarmCode, ct);
                await WaitForAlarmClearAsync(ct);
            }
        }
    }
}
```

---

### 2.12 AM.Modules.* (Prism Modules)
**Mỗi module có cấu trúc chuẩn:**

```
AM.Modules.Alarm/
├── AlarmModule.cs                  — Prism IModule: RegisterTypes + OnInitialized
├── ViewModels/
│   ├── AlarmListViewModel.cs       — DataGrid hiện active alarms
│   ├── AlarmHistoryViewModel.cs    — Lịch sử alarm + filter + export
│   └── AlarmConfigViewModel.cs     — Cấu hình alarm dictionary
├── Views/
│   ├── AlarmListView.xaml
│   ├── AlarmHistoryView.xaml
│   └── AlarmConfigView.xaml
└── Converters/
    └── AlarmLevelToColorConverter.cs
```

**Danh sách module và chức năng:**

| Module | Chức năng chính |
|--------|----------------|
| `AM.Modules.Alarm` | Hiển thị & quản lý alarm, lịch sử, export |
| `AM.Modules.Parameter` | Xem/sửa tham số, quản lý recipe, import/export |
| `AM.Modules.Production` | Dashboard UPH, yield, SPC chart, báo cáo |
| `AM.Modules.IO` | Monitor DI/DO/AI/AO real-time, manual override |
| `AM.Modules.Motion` | Jog trục, teach point, motion status |
| `AM.Modules.Vision` | Live camera view, tool config, kết quả inspection |
| `AM.Modules.Identity` | Login, quản lý user, phân quyền |
| `AM.Modules.Logging` | System log viewer, filter, export |
| `AM.Modules.Diagnostics` | System health, CPU/RAM/Disk, test connectivity |
| `AM.Modules.SecsGem` | SECS/GEM debug, command builder, transaction log |

---

### 2.13 AM.Application.Shell
**Mục đích:** Entry point, DI container (DryIoc), Prism Bootstrap, MainWindow.

```
AM.Application.Shell/
├── App.xaml / App.xaml.cs          — Prism Application
├── Bootstrapper.cs                 — DryIoc + Module catalog
│
├── Views/
│   ├── MainWindow.xaml             — Shell window (menu + regions)
│   ├── SplashScreen.xaml           — Loading screen khi khởi động
│   └── ShellRegions/
│       ├── TopBarView.xaml         — Machine name, status, user, language
│       ├── SideMenuView.xaml       — Navigation menu
│       └── StatusBarView.xaml      — Alarm summary, cycle time, UPH
│
├── ViewModels/
│   ├── MainWindowViewModel.cs
│   ├── SplashScreenViewModel.cs
│   └── TopBarViewModel.cs
│
├── Services/
│   ├── IdleMonitorService.cs       — Auto-logout sau timeout
│   ├── TowerLightManager.cs        — Điều phối đèn theo machine state
│   └── StartupService.cs           — Khởi tạo sequence khi app start
│
└── CustomConfiguration/
    ├── AppParamDbContext.cs         — SQLite context cho app-level params
    ├── DefaultParameters.cs        — Giá trị mặc định khi chạy lần đầu
    └── ModuleCatalog.cs            — Quyết định module nào được load
```

---

## 3. Cấu trúc File Data (Runtime)

```
%ProgramData%\AutoMachine\{MachineName}\
│
├── Config\
│   ├── appsettings.json            — Cấu hình app (DB path, log level...)
│   ├── hardware.json               — Cấu hình phần cứng
│   └── machine.json                — Thông tin máy
│
├── Database\
│   ├── machine.db                  — SQLite: params, production, alarms
│   └── audit.db                    — SQLite: audit trail (readonly append)
│
├── Logs\
│   └── {YYYY-MM-DD}\
│       ├── app.log
│       ├── alarm.log
│       └── motion.log
│
├── Products\
│   └── {RecipeName}\
│       ├── recipe.json             — Recipe parameters
│       ├── Vision\
│       │   └── *.vpp               — VisionPro tool files
│       └── Motion\
│           └── teachpoints.json    — Teach points
│
├── Images\
│   └── {YYYY}\{MM}\{DD}\
│       ├── OK\{SN}.bmp
│       └── NG\{SN}.bmp
│
└── Reports\
    └── {YYYY}\{MM}\
        └── Production_{DD}.xlsx
```

---

## 4. Kiến trúc Dependency (Project References)

```
Shell → Modules → Services → Hardware/* → Infrastructure → Data → Core
                ↘                                         ↗
                  WorkStation → Core.Abstractions ← CommonTools
```

**Nguyên tắc dependency:**
- Các project ở tầng cao hơn KHÔNG bao giờ reference tầng thấp hơn
- `WorkStation` chỉ reference `Core.Abstractions` và `Services` — không biết implementation cụ thể
- Hardware implementation được inject vào `WorkStation` qua DI container trong `Shell`
- Modules chỉ reference `Services` và `Core.Abstractions` — không reference Hardware trực tiếp

---

## 5. Key NuGet Packages

| Package | Version | Dùng cho |
|---------|---------|---------|
| `Prism.DryIoc.Wpf` | 9.x | MVVM framework, module, event aggregator |
| `DryIoc` | 5.x | DI container |
| `Stateless` | 5.x | State machine (MachineStateMachine) |
| `Microsoft.EntityFrameworkCore.Sqlite` | 8.x | Database ORM |
| `log4net` | 2.x | Logging |
| `NPOI` | 2.x | Excel export |
| `SixLabors.ImageSharp` | 3.x | Image processing |
| `Microsoft.Xaml.Behaviors.Wpf` | 1.x | XAML behaviors |
| `CommunityToolkit.Mvvm` | 8.x | [ObservableProperty], [RelayCommand] |
| `BouncyCastle.Cryptography` | 2.x | Password encryption |
| `Serilog` | 3.x | Structured logging (optional, bổ sung log4net) |

---

## 6. Quy ước Code

### Đặt tên
```csharp
// Interface: I + PascalCase
public interface IMotionController { }

// Service: PascalCase + Service
public class AlarmService : IAlarmService { }

// ViewModel: PascalCase + ViewModel
public class AlarmListViewModel : BindableBase { }

// Step: Step{NN}_{TênBước}
public class Step05_Process : IStep { }

// Event (Prism): PascalCase + Event
public class AlarmChangedEvent : PubSubEvent<AlarmInfo> { }

// Alarm code: khu vực (2 chữ số) + mã (3 chữ số)
// VD: 10001 = Motion (10) + lỗi 001
public const int MOTION_SERVO_ERROR = 10001;
```

### Async pattern
```csharp
// Tất cả hardware call phải async + CancellationToken
Task<bool> MoveAbsAsync(double position, CancellationToken ct = default);

// Timeout wrapper
await motion.MoveAbsAsync(100.0).TimeoutAfter(TimeSpan.FromSeconds(10));
```

### Exception handling
```csharp
// Tạo AlarmException thay vì throw Exception thô
throw new AlarmException(AlarmCodes.MOTION_TIMEOUT, 
    $"Axis {axisName} timeout after {timeout}ms");

// Sequence luôn catch AlarmException — KHÔNG catch Exception chung
try { await step.ExecuteAsync(ct); }
catch (AlarmException ex) { await alarmService.RaiseAsync(ex.AlarmCode); }
catch (OperationCanceledException) { /* normal stop */ }
```

---

## 7. Hướng dẫn tạo máy mới

Khi nhận dự án máy mới, làm theo thứ tự:

1. **Copy template** `AM.WorkStation.SampleMachine` → đổi tên thành `AM.WorkStation.{MachineName}`
2. **Cập nhật** `AxisMap.cs`, `IOMap.cs`, `StationConfig.cs` theo phần cứng thực tế
3. **Tạo Recipe class** kế thừa `RecipeBase`, thêm tham số đặc thù của máy
4. **Viết Steps** — mỗi bước trong quy trình là 1 class implement `IStep`
5. **Viết MachineSequence** — gọi các step theo đúng thứ tự quy trình
6. **Register** WorkStation trong `Shell/Bootstrapper.cs`
7. **Test** từng step độc lập trước khi chạy full sequence

**Phần KHÔNG cần thay đổi khi làm máy mới:**
- Toàn bộ hardware drivers (chỉ cần cấu hình)
- Tất cả Modules UI
- Services layer
- Infrastructure & Core

---

## 8. Multithreading Model

```
UI Thread          — WPF UI, không block
  ↕ Dispatcher
ViewModel Thread   — BindableBase, PropertyChanged
  ↕ Prism EventAggregator
Service Thread     — Background services (alarm monitor, IO scan)
  ↕ CancellationToken
Sequence Thread    — Task.Run(RunAsync), dedicated per workstation
  ↕ Hardware API
Hardware Thread    — Motion/Vision callbacks (hardware SDK threads)
```

**Quy tắc:**
- Sequence chạy trên `Task.Run` với `CancellationToken`
- Mọi UI update qua `Application.Current.Dispatcher` hoặc `BindableBase`
- Hardware callback → raise event → subscribe trên service thread
- Không share state giữa sequence thread và UI thread nếu không lock

---

## 9. Internationalization (i18n)

```xml
<!-- vi-VN.xml -->
<Language code="vi-VN" name="Tiếng Việt">
  <String key="Alarm.MotionTimeout">Trục {0} timeout sau {1}ms</String>
  <String key="Production.BatchComplete">Hoàn thành lô: {0} OK / {1} NG</String>
  <String key="Param.Velocity">Tốc độ (mm/s)</String>
</Language>
```

```xaml
<!-- Dùng trong XAML -->
<TextBlock Text="{lang:Text Key='Param.Velocity'}"/>
```

```csharp
// Dùng trong code
var msg = _localization.GetString("Alarm.MotionTimeout", axisName, timeout);
```

---

*Tài liệu này là cơ sở cho mọi máy tự động hoá. Core + Infrastructure + Services + Modules = bất biến. WorkStation = thay đổi theo từng máy.*
