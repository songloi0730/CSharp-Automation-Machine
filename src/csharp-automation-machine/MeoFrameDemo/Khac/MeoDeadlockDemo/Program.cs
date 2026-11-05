// Mục 19.2.2, Chương 19: đúng cặp lock lồng nhau ngược thứ tự trong sách, chạy thật và treo thật.
// Chạy bằng F5 trong Visual Studio, đợi vài giây, Debug > Break All, rồi mở Debug > Windows > Parallel Stacks.
object alarmLock = new();
object axisLock = new();

var luongUi = new Thread(() =>
{
    // Luồng UI (cập nhật HMI)
    lock (alarmLock)
    {
        Thread.Sleep(200);
        lock (axisLock) { /* đọc vị trí trục để hiển thị */ }
    }
}) { Name = "Luong UI" };

var luongPlc = new Thread(() =>
{
    // Luồng nền (đọc PLC theo chu kỳ): khoá NGƯỢC thứ tự
    lock (axisLock)
    {
        Thread.Sleep(200);
        lock (alarmLock) { /* ghi alarm mới phát hiện */ }
    }
}) { Name = "Luong doc PLC" };

luongUi.Start();
luongPlc.Start();
Console.WriteLine("Hai luồng đã chạy. Nếu dòng dưới không bao giờ hiện ra thì chúng đang chờ nhau.");
luongUi.Join();
luongPlc.Join();
Console.WriteLine("Xong.");
