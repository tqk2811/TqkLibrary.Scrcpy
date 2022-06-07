using TqkLibrary.AdbDotNet;
using TqkLibrary.Scrcpy;
using TqkLibrary.Media.Images;
using System.Drawing;
using System.Diagnostics;

namespace TestAutoConsole
{
    public static class Program
    {
        static Scrcpy scrcpy;
        public static void Main(string[] args)
        {
            Dictionary<string, Rectangle> Crops = new Dictionary<string, Rectangle>()
            {
                { "ytb_Home", new Rectangle(0,2080,1080,280) },
                { "ytb_Library", new Rectangle(0,2080,1080,280) }
            };

            var devices = Adb.Devices(DeviceState.Device).ToList();
            string deviceId = devices.First();
            ScrcpyConfig config = new ScrcpyConfig()
            {
                //HwType = FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA,
                IsUseD3D11Shader = true,
                IsControl = true,
                MaxFps = 24
            };
            scrcpy = new Scrcpy(deviceId);
            
            var imageManaged = new ImageTemplateHelper(Path.Combine(Directory.GetCurrentDirectory(), "Images"));
            var cropManaged = new ImageCropHelper(Crops);
            var waiter = new WaitImageHelper(
                () =>
                {
                    Stopwatch stopWatch = new Stopwatch();
                    try
                    {
                        stopWatch.Start();
                        return scrcpy.GetScreenShot();
                    }
                    finally
                    {
                        stopWatch.Stop();
                        Console.WriteLine($"GetScreenShot: {stopWatch.ElapsedMilliseconds}ms");
                    }
                },
                imageManaged.GetImage,
                cropManaged.GetCrop,
                () => new List<string>(),
                () => 0.90,
                () => 60000);
            waiter.LogCallback += Waiter_LogCallback;
            waiter.DelayStep = 10;
            
            scrcpy.Connect(config);
            Task.Delay(500).Wait();
            while (true)
            {
                Stopwatch stopWatch = new Stopwatch();

                stopWatch.Start();
                waiter.WaitUntil("ytb_Home").AndTapFirst(Tap).WithThrow().StartAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                stopWatch.Stop();
                Console.WriteLine($"ytb_Home: {stopWatch.ElapsedMilliseconds}ms");
                
                Task.Delay(1000).Wait();

                stopWatch.Restart();
                waiter.WaitUntil("ytb_Library").AndTapFirst(Tap).WithThrow().StartAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                stopWatch.Stop();
                Console.WriteLine($"ytb_Library: {stopWatch.ElapsedMilliseconds}ms");

                Task.Delay(1000).Wait();
            }
        }
        static bool Tap(int index, OpenCvFindResult result, string[] images)
        {
            scrcpy.Control.Tap(result.Point);
            return false;
        }
        private static void Waiter_LogCallback(string obj)
        {
            Console.WriteLine(obj);
        }
    }
}



