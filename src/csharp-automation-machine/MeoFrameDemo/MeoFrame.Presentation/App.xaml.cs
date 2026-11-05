using System.Windows;
using MeoFrame.Application;
using MeoFrame.Domain;
using MeoFrame.Infrastructure;
using MeoFrame.Presentation.Anh;
using MeoFrame.Presentation.ViewModels;

namespace MeoFrame.Presentation;

// Phải viết đủ System.Windows.Application: trong namespace MeoFrame.Presentation, chữ "Application" trần
// sẽ trỏ vào namespace MeoFrame.Application và báo CS0118 (xem mục 2.2.2).
public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // dotnet run -- --xuat-anh <thư mục>: dựng các hình của Chương 8 và Chương 10 ra PNG rồi thoát
        int viTri = Array.IndexOf(e.Args, "--xuat-anh");
        if (viTri >= 0 && viTri + 1 < e.Args.Length)
        {
            XuatAnh.Chay(e.Args[viTri + 1]);
            Shutdown();
            return;
        }

        // Composition Root: nơi DUY NHẤT biết lớp cụ thể (mục 7.4.3)
        var axisX = new Axis(axisId: 1, name: "X", new SimulatedMotionDriver(), maxVelocity: 500);
        axisX.Connect();
        axisX.EnableServo();
        var useCase = new RunPickPlaceCycleUseCase(axisX);

        // --mau-canh-bao: mở màn hình với cảnh báo mẫu, để nhìn Alarm Banner trong Live Visual Tree (mục 10.3.3)
        MainViewModel viewModel = e.Args.Contains("--mau-canh-bao") ? MainViewModel.MauCoCanhBao() : new MainViewModel(useCase);
        var window = new MainWindow { DataContext = viewModel };
        window.Show();

        // launchSettings.json truyền cờ này, để bấm F5 là chu kỳ chạy ngay và tập được debug ở mục 2.3
        if (e.Args.Contains("--tu-chay"))
            viewModel.StartCommand.Execute(null);
    }
}
