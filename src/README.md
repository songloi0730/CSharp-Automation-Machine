# src — mã nguồn ví dụ của các sách

Mỗi cuốn sách có **một thư mục con riêng** ở đây. Đặt tên thư mục theo tên kho sách, chữ thường,
nối bằng dấu gạch ngang — để thêm sách mới về sau chỉ là thêm một thư mục ngang hàng, không phải
sắp xếp lại gì.

| Thư mục | Sách | Nền tảng |
|---|---|---|
| [`csharp-automation-machine/`](csharp-automation-machine/) | *C# cho Automation Machine* | .NET 9 |

> Sách khác (VisionPro, PLC, Thiết kế bản vẽ điện, Đấu nối thiết bị…) khi có mã ví dụ thì thêm
> thư mục con mới ở đây, kèm một `README.md` riêng theo cùng khuôn.

## Quy ước chung cho mọi thư mục con

- **Mã trong sách và mã ở đây phải khớp nhau.** Mọi đoạn mã in trong sách đều lấy từ một project
  có thật ở đây, đã biên dịch và chạy — không có đoạn nào chỉ tồn tại trên giấy.
- **Chạy được ngay, không cần phần cứng.** Tất cả dùng bản giả lập.
- **Không có tên dự án, máy móc, công ty hay phần mềm tham khảo nào** trong mã. Tên phần cứng
  (hãng card, hãng cảm biến) thì được giữ nguyên vì nó giúp người mới tra cứu thiết bị.
- Mỗi project là một thư mục có `.csproj` riêng, chạy bằng `dotnet run` từ trong thư mục đó.
