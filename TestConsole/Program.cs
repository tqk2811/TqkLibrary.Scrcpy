using System.Drawing;
using TqkLibrary.Scrcpy;
using TqkLibrary.AdbDotNet;

var env = System.Environment.GetEnvironmentVariables();
var h = 1080 % 16;

int first_row = 1783;
Dictionary<char, Point> keyMaps = new Dictionary<char, Point>()
{
    { 'q', new Point(51,first_row) },
    { 'w', new Point(160,first_row)},
    { 'e', new Point(263,first_row)},
    { 'r', new Point(381,first_row)},
    { 't', new Point(485,first_row)},
    { 'y', new Point(592,first_row)},
    { 'u', new Point(700,first_row)},
    { 'i', new Point(800,first_row)},
    { 'o', new Point(920,first_row)},
    { 'p', new Point(1026,first_row)},
};


var imgs = Path.Combine(Directory.GetCurrentDirectory(), "Imgs");
Directory.CreateDirectory(imgs);

Directory.GetFiles(imgs, "*.png")
    .ToList()
    .ForEach(File.Delete);

var devices = Adb.Devices().Where(x => x.DeviceState == DeviceState.Device).ToList();

string deviceId = devices.First().DeviceId;
//string deviceId = "a29bc285";
//string deviceId = "5793d2f39905";


int i = 0;
ScrcpyConfig config = new ScrcpyConfig()
{
    HwType = FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA,
    IsUseD3D11ForUiRender = true,
    ServerConfig = new ScrcpyServerConfig()
    {
        IsControl = true,
        IsAudio = true,
        MaxFps = 24,
        ClipboardAutosync = false,
        SCID = new Random(DateTime.Now.Millisecond).Next()
    },
    ConnectionTimeout = 10000,
};
while (true)
{
    Console.WriteLine($"{DateTime.Now:mm:ss.fff} Start");
    using (Scrcpy scrcpy = new Scrcpy(deviceId))
    {
        scrcpy.Control.OnClipboardReceived += Control_OnClipboardReceived;
        scrcpy.Control.OnSetClipboardAcknowledgement += Control_OnSetClipboardAcknowledgement;
        Console.WriteLine($"{DateTime.Now:mm:ss.fff} Connect");
        if (scrcpy.Connect(config))
        {
            Console.WriteLine($"{DateTime.Now:mm:ss.fff} Connected");
            //await Task.Delay(3000);


            //await TapKeyboard(scrcpy, "qwertyuiop");
            //await Task.Delay(500);
            //await scrcpy.Control.KeyAsync(AndroidKeyCode.KEYCODE_ENTER);
            //await Task.Delay(3000);
            ////scrcpy.Control.SetClipboard("test clipboard", true);
            //await scrcpy.Control.SwipeAsync(500, 2000, 500, 500, 1000);
            //string text = await scrcpy.Control.GetClipboardAsync();
            //Console.WriteLine($"GetClipboardAsync: {text}");

            //while (true)
            //    await Task.Delay(3000);
            //Console.WriteLine($"{DateTime.Now:mm:ss.fff} GetScreenShot");
            await Task.Delay(1000);
            while (true)
            {
                //scrcpy.Control.SetClipboard("Phạm Đức Long Click mất cũng chừng đó thôi =))", true, 222233);

                using Bitmap bitmap = scrcpy.GetScreenShot();
                Console.WriteLine($"{DateTime.Now:mm:ss.fff} GetScreenShoted");
                bitmap.Save($"{imgs}\\{i++:00000}.png");
                await Task.Delay(500);

                //scrcpy.Control.GetClipboard();
                //Console.WriteLine($"{DateTime.Now:mm:ss.fff} GetClipboardAsync");
                //string abc = await scrcpy.Control.GetClipboardAsync().ConfigureAwait(false);
                //Console.WriteLine($"{DateTime.Now:mm:ss.fff} GetClipboardAsync: {abc}");
                //break;
            }
            //await Task.Delay(3000);

            //while (true) await Task.Delay(3000);

            //Console.WriteLine($"{DateTime.Now:mm:ss.fff} Stop");
            scrcpy.Stop();
            //Console.WriteLine($"{DateTime.Now:mm:ss.fff} Stopped");
        }
        else
        {

        }
    }
    Console.WriteLine($"{DateTime.Now:mm:ss.fff} Disposesed");
    GC.Collect();
    GC.WaitForPendingFinalizers();
    Console.WriteLine($"{DateTime.Now:mm:ss.fff} GC cleared");
    //await Task.Delay(5000);
}

void Control_OnSetClipboardAcknowledgement(IControl control, long data)
{
    Console.WriteLine($"Control_OnSetClipboardAcknowledgement: {data}");
}

void Control_OnClipboardReceived(IControl control, string data)
{
    Console.WriteLine($"Control_OnClipboardReceived: {data}");
}

async Task TapKeyboard(Scrcpy scrcpy, string text)
{
    foreach (var c in text)
    {
        await scrcpy.Control.TapAsync(keyMaps[c], 100);
        await Task.Delay(50);
    }
}