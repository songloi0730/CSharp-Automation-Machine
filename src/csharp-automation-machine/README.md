# Mã ví dụ — sách *C# cho Automation Machine*

Sáu project .NET 9, tất cả **chạy được ngay không cần phần cứng**. Mỗi project tương ứng với một
phần cụ thể trong sách.

| Project | Dùng cho | Chạy |
|---|---|---|
| [`MeoFrameMini/`](MeoFrameMini/) | Chương 7 mục 7.5 — bản `async` + interface | `dotnet run` |
| [`MeoFrameMiniSync/`](MeoFrameMiniSync/) | Mục 7.6 — cùng cỗ máy, viết theo lối **chặn luồng** | `dotnet run` |
| [`MeoFrameMiniDirect/`](MeoFrameMiniDirect/) | Mục 7.7 — **bỏ cả interface**, và cái giá phải trả | `dotnet run` |
| [`MeoFrameMiniMixed/`](MeoFrameMiniMixed/) | Mục 7.7.4 — bản pha trộn được khuyến nghị | `dotnet run` |
| [`MeoBench/`](MeoBench/) | **Phụ lục G** — lời giải mẫu **đủ 40/40** bài thực hành | xem bên dưới |
| [`RandomProbe/`](RandomProbe/) | Mục 13.2.5c — đo hành vi thật của `Random` trên .NET 9 | `dotnet run` |

## Bốn biến thể MeoFrameMini

Cùng **một cỗ máy gắp–đặt**, viết theo bốn lối kiến trúc khác nhau. Chạy cả bốn sẽ thấy **kết quả
giống hệt nhau** — đó chính là điểm của bài: cùng một hành vi, khác nhau ở cái giá bảo trì.

```bash
cd MeoFrameMini       && dotnet run    # 350 dòng · 4 interface · async
cd MeoFrameMiniSync   && dotnet run    # 390 dòng · 4 interface · chặn luồng
cd MeoFrameMiniDirect && dotnet run    # 336 dòng · 0 interface · 10 nhánh cờ giả lập
cd MeoFrameMiniMixed  && dotnet run    # 368 dòng · 1 interface (IStep)
```

## MeoBench — lời giải mẫu Phụ lục G

Máy **MeoBench-01** (kiểm chiều dày và phân loại OK/NG), kèm bộ tự kiểm **458 phép kiểm**: 294 cho **đủ 40** bài thực hành,
62 cho **cỗ máy ghép hoàn chỉnh** (G.11), 34 cho **tách cấu hình** (G.12), 68 cho **năng lực vận
hành máy thật** (G.13). Chạy lẻ được từng nhóm — không phải làm xong hết mới biết sai ở đâu:

```bash
cd MeoBench
dotnet run                 # tất cả — 458 phép kiểm
dotnet run -- G1           # nhóm kiểu dữ liệu miền
dotnet run -- G2           # logic thuần
dotnet run -- G3           # hợp đồng thiết bị và bản giả lập
dotnet run -- G4           # lớp nghiệp vụ
dotnet run -- G5           # trình tự
dotnet run -- G6           # công thức, cấu hình, dữ liệu
dotnet run -- G7           # giao diện
dotnet run -- G8           # ráp nối và chạy máy
dotnet run -- G9           # CỖ MÁY GHÉP HOÀN CHỈNH — 62 phép kiểm
dotnet run -- G12          # tách cấu hình config/product — 34 phép kiểm
dotnet run -- G13          # quyền, jog, đèn tháp, truy xuất… — 68 phép kiểm
dotnet run -- --demo       # chạy máy 20 chu kỳ, in nhật ký
dotnet run -- --danhsach   # liệt kê đủ 40 bài
```

Bộ tự kiểm (`Kiem.cs`, khoảng 60 dòng) **cố ý không dùng xUnit**: máy tính công nghiệp ngoài hiện
trường không có Visual Studio và không chạy được `dotnet test`, nên bộ kiểm này gắn được vào một
nút trên màn hình chẩn đoán của máy thật.

## Cách dùng khi làm bài tập

Đừng mở lời giải ra trước. Trình tự có ích nhất:

1. Đọc đặc tả và tiêu chí chấm ở **Phụ lục G mục G.10**.
2. **Chép riêng phần hàm kiểm** của bài đó vào project của bạn — chúng chính là bản đặc tả viết
   bằng mã.
3. Tự viết cho tới khi các phép kiểm xanh.
4. **Chỉ khi đó** mới mở lời giải mẫu ra so — và chỗ đáng so không phải cú pháp, mà là **những
   nhánh lỗi bạn chưa nghĩ tới**.

## Yêu cầu

.NET SDK 9.0 trở lên. Không cần phần cứng, không cần thư viện ngoài nào.

Mọi project bật `TreatWarningsAsErrors=true`; `MeoBench` bật thêm `AnalysisMode=Recommended` và
biên dịch sạch **0 cảnh báo**.
