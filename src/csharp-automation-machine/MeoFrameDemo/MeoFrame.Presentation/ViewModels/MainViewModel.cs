using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeoFrame.Application;
using MeoFrame.Domain;

namespace MeoFrame.Presentation.ViewModels;

public enum StationStatus { Normal, Error, Attention, Processing, NeedsOperator }

// Một dòng của bảng "mỗi trạm một dòng", mục 10.1.8
public sealed record StationRow(string Time, string Station, string Step, string StepName, string Part, StationStatus Status);

public sealed record ChipModel(string Name, ConnectionState State);

public sealed record KeyItem(string Text, bool IsActive);

public sealed class AlarmBannerViewModel
{
    public ObservableCollection<AlarmModel> ActiveAlarms { get; } = new();
    public bool HasAlarms => ActiveAlarms.Count > 0;
    public bool HasNoAlarms => ActiveAlarms.Count == 0;
}

public partial class MainViewModel : ObservableObject
{
    private readonly RunPickPlaceCycleUseCase? _useCase;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(50) };

    [ObservableProperty] private string _machineStateText = "SẴN SÀNG";
    [ObservableProperty] private bool   _isFault;
    [ObservableProperty] private bool   _isRunning;
    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(StartCommand))] private bool _canStart = true;
    [ObservableProperty] private bool   _canInitialize = true;
    [ObservableProperty] private bool   _canAcknowledge;
    [ObservableProperty] private string _modeText = "Chạy thật";
    [ObservableProperty] private string _recipeText = "SP-A12";
    [ObservableProperty] private string _lotText = "L2611-07";
    [ObservableProperty] private string _userText = "Vận hành: Ca 2";
    [ObservableProperty] private string _clockText = "14:32:08";

    [ObservableProperty] private string _totalCount = "0";
    [ObservableProperty] private string _ngCount = "0";
    [ObservableProperty] private string _ngRate = "0,0";
    [ObservableProperty] private string _lastTact = "0,00";
    [ObservableProperty] private string _averageTact = "0,00";

    [ObservableProperty] private string _axisZTarget = "25,00";
    [ObservableProperty] private string _axisZActual = "24,98";
    [ObservableProperty] private string _axisXTarget = "380,00";
    [ObservableProperty] private string _axisXActual = "379,99";
    [ObservableProperty] private string _airPressure = "0,52";
    [ObservableProperty] private string _doorText = "Đóng";
    [ObservableProperty] private string _emergencyText = "Bình thường";

    public AlarmBannerViewModel AlarmBanner { get; } = new();
    public ObservableCollection<StationRow> Stations { get; } = new();
    public ObservableCollection<ChipModel> Chips { get; } = new();
    public ObservableCollection<KeyItem> FunctionKeys { get; } = new()
    {
        new("F1 · Chạy tự động", true), new("F2 · Vận hành tay", false), new("F4 · Bảo trì cơ cấu", false),
        new("F5 · Công thức", false), new("F6 · Kỹ thuật", false), new("F8 · Thiết lập", false),
    };

    public MainViewModel(RunPickPlaceCycleUseCase? useCase = null)
    {
        _useCase = useCase;
        _timer.Tick += OnTimerTick;
        Chips.Add(new ChipModel("PLC", ConnectionState.Connected));
        Chips.Add(new ChipModel("Camera", ConnectionState.Connected));
        Chips.Add(new ChipModel("Máy quét", ConnectionState.Connected));
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private void Start()
    {
        CanStart = false;
        CanInitialize = false;
        IsRunning = true;
        MachineStateText = "ĐANG CHẠY";
        _timer.Start();
    }

    [RelayCommand]
    private void Stop()
    {
        CanStart = true;
        CanInitialize = true;
        IsRunning = false;
        MachineStateText = "SẴN SÀNG";
        _timer.Stop();
    }

    // Mỗi nhịp đồng hồ chạy một chu kỳ. Mục 2.3 đặt breakpoint trong chuỗi gọi bắt đầu từ đây.
    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (_useCase is null) return;
        CycleResult result = _useCase.RunOneCycle();
        TotalCount = result.CycleNumber.ToString();
    }

    // Dữ liệu mẫu để dựng ảnh: máy chạy bình thường, màn hình yên tĩnh
    public static MainViewModel MauBinhThuong()
    {
        var vm = new MainViewModel
        {
            MachineStateText = "ĐANG CHẠY", IsRunning = true, CanStart = false, CanInitialize = false, TotalCount = "1 284", NgCount = "37", NgRate = "2,9",
            LastTact = "4,21", AverageTact = "4,30",
        };
        vm.Stations.Add(new("14:32:07", "Trạm Cấp phôi", "2/4", "Chờ cảm biến có phôi",   "P-001285", StationStatus.Normal));
        vm.Stations.Add(new("14:32:05", "Trạm Gắp",      "5/9", "Hạ đầu hút xuống phôi",  "P-001284", StationStatus.Normal));
        vm.Stations.Add(new("14:32:06", "Trạm Kiểm",     "3/7", "Chụp ảnh mặt trên",      "P-001283", StationStatus.Normal));
        vm.Stations.Add(new("14:32:01", "Trạm Xuất",     "1/3", "Chờ máy sau sẵn sàng",   "P-001282", StationStatus.Normal));
        return vm;
    }

    // Cùng màn hình đó khi có lỗi: chỉ những gì bất thường mới có màu
    public static MainViewModel MauCoCanhBao()
    {
        var vm = new MainViewModel
        {
            MachineStateText = "LỖI VẬN HÀNH", IsFault = true, CanStart = false, CanInitialize = false, CanAcknowledge = true, TotalCount = "1 284", NgCount = "37", NgRate = "2,9",
            LastTact = "4,21", AverageTact = "4,30", AirPressure = "0,38",
        };
        vm.AlarmBanner.ActiveAlarms.Add(new AlarmModel(70012, "Trạm Gắp", AlarmSeverity.Critical,
            "Trạm Gắp: áp suất khí nén dưới ngưỡng, đã dừng trục", new DateTimeOffset(2026, 11, 5, 14, 32, 9, TimeSpan.Zero)));
        vm.AlarmBanner.ActiveAlarms.Add(new AlarmModel(20007, "Trạm Kiểm", AlarmSeverity.Major,
            "Trạm Kiểm: camera mất 3 khung hình liên tiếp", new DateTimeOffset(2026, 11, 5, 14, 31, 42, TimeSpan.Zero)));
        vm.Chips[1] = new ChipModel("Camera", ConnectionState.Reconnecting);
        vm.Stations.Add(new("14:32:07", "Trạm Cấp phôi", "2/4", "Chờ cảm biến có phôi",   "P-001285", StationStatus.Normal));
        vm.Stations.Add(new("14:32:09", "Trạm Gắp",      "5/9", "Dừng: chờ khí nén",      "P-001284", StationStatus.Error));
        vm.Stations.Add(new("14:32:06", "Trạm Kiểm",     "3/7", "Chụp ảnh mặt trên",      "P-001283", StationStatus.Attention));
        vm.Stations.Add(new("14:32:01", "Trạm Xuất",     "1/3", "Chờ máy sau sẵn sàng",   "P-001282", StationStatus.Normal));
        return vm;
    }
}
