using System.Drawing;
using TqkLibrary.Scrcpy;

//string deviceId = "a29bc285";
string deviceId = "5793d2f39905";


int i = 0;
while (true)
{
    Console.WriteLine($"{DateTime.Now:mm:ss.fff} Start");
    using (Scrcpy scrcpy = new Scrcpy(deviceId))
    {
        Console.WriteLine($"{DateTime.Now:mm:ss.fff} Connect");
        scrcpy.Connect();
        Console.WriteLine($"{DateTime.Now:mm:ss.fff} Connected");
        await Task.Delay(3000);

        Console.WriteLine($"{DateTime.Now:mm:ss.fff} GetScreenShot");
        using Bitmap bitmap = scrcpy.GetScreenShot();
        if (bitmap != null)
        {
            Console.WriteLine($"{DateTime.Now:mm:ss.fff} GetScreenShoted");
            bitmap.Save($"{i++:0000}.png");
        }
        else
        {

        }
        await Task.Delay(3000);

        Console.WriteLine($"{DateTime.Now:mm:ss.fff} Stop");
        scrcpy.Stop();
        Console.WriteLine($"{DateTime.Now:mm:ss.fff} Stopped");
    }
    Console.WriteLine($"{DateTime.Now:mm:ss.fff} Disposesed");
    GC.Collect();
    await Task.Delay(5000);
}