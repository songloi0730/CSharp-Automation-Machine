using System.Diagnostics;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine($"Runtime: {Environment.Version}");

// (a) new Random() trong vong lap chat — huyen thoai "tra ve gia tri giong nhau"
var a = new int[10];
for (int i = 0; i < 10; i++) a[i] = new Random().Next(1000);
Console.WriteLine("(a) new Random() x10 : " + string.Join(", ", a)
    + $"   -> so gia tri khac nhau: {a.Distinct().Count()}/10");

// (b) cung seed
var b = new int[5];
for (int i = 0; i < 5; i++) b[i] = new Random(42).Next(1000);
Console.WriteLine("(b) new Random(42) x5: " + string.Join(", ", b)
    + $"   -> so gia tri khac nhau: {b.Distinct().Count()}/5");

// (c) MOT Random dung chung, nhieu luong, KHONG dong bo
var shared = new Random();
int zeros = 0, total = 0;
var threads = new List<Thread>();
for (int t = 0; t < 8; t++)
{
    var th = new Thread(() =>
    {
        for (int i = 0; i < 200_000; i++)
        {
            double v = shared.NextDouble();
            Interlocked.Increment(ref total);
            if (v == 0.0) Interlocked.Increment(ref zeros);
        }
    });
    threads.Add(th); th.Start();
}
foreach (var th in threads) th.Join();
Console.WriteLine($"(c) Random dung chung, 8 luong, khong khoa : {zeros:N0} gia tri 0 tren {total:N0} lan goi");

// (d) Random.Shared — tai lieu noi an toan luong
int zeros2 = 0, total2 = 0;
threads.Clear();
for (int t = 0; t < 8; t++)
{
    var th = new Thread(() =>
    {
        for (int i = 0; i < 200_000; i++)
        {
            double v = Random.Shared.NextDouble();
            Interlocked.Increment(ref total2);
            if (v == 0.0) Interlocked.Increment(ref zeros2);
        }
    });
    threads.Add(th); th.Start();
}
foreach (var th in threads) th.Join();
Console.WriteLine($"(d) Random.Shared, 8 luong                 : {zeros2:N0} gia tri 0 tren {total2:N0} lan goi");
