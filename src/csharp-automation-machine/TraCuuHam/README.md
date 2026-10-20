# TraCuuHam — mã của Phụ lục K

Mỗi dòng và mỗi khối mã trong **Phụ lục K (Tra cứu nhanh hàm C# hay dùng)** là một file trong `Mau/`.
Tên file là số của dòng đó: `K_1_2a.cs` là mục **K.1**, việc thứ **2**, dòng **a**. Dòng có kết quả
cụ thể trong sách thì file có thêm một câu `if (!(...)) throw` kiểm đúng kết quả đó.

Dự án dựng được ở cả ba mức mà phụ lục ghi trong ba cột cuối của bảng:

| Mức | Lệnh (chạy trong thư mục này) | Dựng gì |
|---|---|---|
| **.NET** — .NET 9 | `dotnet run -c Release` | Tất cả đoạn mẫu |
| **FW** — .NET Framework 4.8, C# 7.3 | `dotnet build -c Release -p:Muc=fw` rồi chạy `bin\Release\net48\TraCuuHam.exe` | Bỏ các đoạn chỉ có trên .NET (danh sách trong `LoaiTru.fw.props`) |
| **C# 5** — `csc.exe` cũ có sẵn trong Windows | Dựng bản FW trước (để có `Newtonsoft.Json.dll`), tạo thư mục `bin_cs5`, chép `bin\Release\net48\Newtonsoft.Json.dll` vào đó, rồi chạy `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe @cs5.rsp` và `bin_cs5\TraCuuHam.exe` | Chỉ các đoạn có ✓ ở cột C# 5 (danh sách trong `cs5.rsp`) |

Chạy xong, dòng cuối in `XONG <số> phep kiem, 0 hong`. Dòng nào sai kết quả sẽ in
`HONG <số dòng> | <lỗi>`.

## Thư mục

| Đường dẫn | Nội dung |
|---|---|
| `Mau/` | Các đoạn mẫu, mỗi file một dòng hoặc một khối của phụ lục |
| `Chung.cs` | Ba kiểu dùng chung cho các đoạn mẫu: `TrangThai`, `Phoi`, `Co.Nem<T>` (đúng nếu hành động ném đúng loại lỗi) |
| `Program.cs` | Chạy mọi phép kiểm biên dịch được ở mức hiện tại. Viết bằng C# 5; phần chỉ có trên .NET nằm trong `#if NET`, phần C# 6–7 nằm trong `#if !CS5` |
| `LoiBienDich/` | Các đoạn **cố ý sai** của mục K.17 — mỗi file sinh ra đúng một lỗi biên dịch. Không nằm trong dự án; biên dịch riêng từng file để xem thông báo lỗi, ví dụ `csc /target:library LoiBienDich\L03.cs` |

## Thử một dòng của riêng bạn

Chép một file trong `Mau/`, đổi tên lớp, sửa thân hàm `Chay()`, rồi biên dịch lại ở mức bạn cần. Muốn
biết một cú pháp có chạy trong ô script đời cũ không, dựng ở mức **C# 5**: thêm tên file mới vào cuối
`cs5.rsp` là đủ.

## Yêu cầu

- .NET SDK 9.0 trở lên.
- Mức FW: .NET Framework 4.8 (có sẵn trên Windows 10/11). SDK tự tải gói tham chiếu khi dựng.
- Hai gói NuGet, SDK tự tải: `Newtonsoft.Json` (ví dụ JSON ở K.7) và `System.IO.Ports` (cổng nối
  tiếp ở K.10 — trên .NET phải thêm gói này, .NET Framework có sẵn).
- Mức C# 5 cần Windows 64 bit (đường dẫn `Framework64` và thư viện WPF trong `cs5.rsp`).
