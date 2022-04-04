// See https://aka.ms/new-console-template for more information
using TqkLibrary.Scrcpy;

while (true)
{
    Console.WriteLine($"{DateTime.Now:mm:ss.fff} Start");
    using (Scrcpy scrcpy = new Scrcpy("a29bc285"/*"5793d2f39905"*/))
    {
        Console.WriteLine($"{DateTime.Now:mm:ss.fff} Connect");
        scrcpy.Connect();
        Console.WriteLine($"{DateTime.Now:mm:ss.fff} Connected");
        await Task.Delay(10000);
        Console.WriteLine($"{DateTime.Now:mm:ss.fff} Stop");
        scrcpy.Stop();
        Console.WriteLine($"{DateTime.Now:mm:ss.fff} Stopped");
    }
    Console.WriteLine($"{DateTime.Now:mm:ss.fff} Disposesed");
    GC.Collect();
    await Task.Delay(5000);
}