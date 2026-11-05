# MeoFrameDemo

Solution đúng cấu trúc Hình 2.3 của sách *C# cho Automation Machine*, chạy được không cần phần cứng.
Các hình của Chương 2, 8, 10, 18 và 19 lấy từ chính mã này, nên có thể mở lên và đối chiếu.

| Project | Có gì | Đọc cùng |
|---|---|---|
| `MeoFrame.Domain` | `IMotionDriver`, `Axis`, `AlarmModel` | Code 4.1, Code 10.2f |
| `MeoFrame.Application` | `RunPickPlaceCycleUseCase`: vị trí giảm 0,5 mm mỗi chu kỳ, âm từ chu kỳ 101 để tập breakpoint điều kiện | Mục 2.3 |
| `MeoFrame.Infrastructure` | `SimulatedMotionDriver` | Chương 13 |
| `MeoFrame.Presentation` | Màn hình vận hành WPF 1920×1080, Alarm Banner, chip kết nối, bảng màu đã đo | Chương 10 |
| `MeoFrame.Tests` | 8 test xUnit của `Axis`, có dòng `NaN` | Code 18.1, Code 18.2 |
| `Khac/MeoFormsDemo` | Form WinForms tối giản để xem trình thiết kế | Mục 8.1.1 |
| `Khac/MeoDeadlockDemo` | Cặp `lock` ngược thứ tự, chạy là treo thật | Mục 19.2.2 |

## Chạy

```bash
dotnet test MeoFrame.Tests
dotnet run --project MeoFrame.Presentation
```

Mở `MeoFrame.sln` trong Visual Studio và bấm F5 thì `launchSettings.json` tự truyền `--tu-chay`, để chu kỳ chạy ngay và tập
được debug ở mục 2.3.

| Tham số của `MeoFrame.Presentation` | Tác dụng |
|---|---|
| `--tu-chay` | Bấm Bắt đầu thay người dùng ngay khi mở |
| `--mau-canh-bao` | Mở màn hình với cảnh báo mẫu, để xem Alarm Banner trong Live Visual Tree (mục 10.3.3) |
| `--xuat-anh <thư mục>` | Dựng Hình 8.3 và Hình 10.1 đến 10.10 ra PNG rồi thoát, tính luôn độ tương phản WCAG của Hình 10.4 |

## Hai lỗi đáng nhìn trong solution này

**`CS0118` vì tên project.** Vì có project `MeoFrame.Application`, trong namespace `MeoFrame.Presentation` chữ
`Application` trần trỏ vào namespace đó, nên `App.xaml.cs` phải viết `System.Windows.Application` (mục 2.2.2).

**Deadlock của `MeoDeadlockDemo`.** Mở `Khac/MeoDeadlockDemo/MeoDeadlockDemo.sln`, bấm F5, đợi vài giây rồi chọn
**Debug → Break All**, sau đó mở **Debug → Windows → Parallel Stacks** (mục 19.2.2). Chạy ngoài debugger thì chương trình treo
mãi, tắt bằng cách đóng cửa sổ.
