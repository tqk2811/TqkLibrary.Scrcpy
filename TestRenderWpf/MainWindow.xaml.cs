using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TqkLibrary.AdbDotNet;
using TqkLibrary.AudioPlayer.Sdl2;
using TqkLibrary.AudioPlayer.Sdl2.Enums;
using TqkLibrary.AudioPlayer.XAudio2;
using TqkLibrary.Scrcpy;
using TqkLibrary.Scrcpy.Configs;
using TqkLibrary.Scrcpy.Enums;
using TqkLibrary.Scrcpy.ListSupport;
using TqkLibrary.Scrcpy.Wpf;
namespace TestRenderWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Scrcpy? scrcpy;
        Adb? adb;
        string? deviceId;
        readonly MainWindowVM mainWindowVM;
        readonly ScrcpyConfig scrcpyConfig = new ScrcpyConfig()
        {
            //HwType = FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA,
            IsUseD3D11ForUiRender = true,
            IsUseD3D11ForConvert = true,
            GpuThreadX = 1,
            GpuThreadY = 4,
            IsForceUiGpuFlush = false,
            ConnectionTimeout = 10000,
            ServerConfig = new ScrcpyServerConfig()
            {
                IsControl = true,
                VideoConfig = new VideoConfig()
                {
                    MaxFps = 24
                },
                AudioConfig = new AudioConfig()
                {
#if AudioTest
                    IsAudio = true,
#endif
                },
            },
        };
        SdlDevice? _sdlDevice;
        XAudio2Engine? _audio2Engine;
        XAudio2MasterVoice? _audio2MasterVoice;
        XAudio2SourceVoice? _audio2SourceVoice;
        ScrcpyAudioStream? _audioStream;
        Task? _audioTask;
        public MainWindow()
        {
            InitializeComponent();
            mainWindowVM = this.DataContext as MainWindowVM ?? throw new InvalidOperationException();
        }

        // Parse "--key=value" / "--flag" command-line args (from launchSettings.json) into a map.
        static Dictionary<string, string> ParseArgs()
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var arg in Environment.GetCommandLineArgs().Skip(1))
            {
                if (!arg.StartsWith("--")) continue;
                string body = arg.Substring(2);
                int eq = body.IndexOf('=');
                if (eq >= 0)
                    result[body.Substring(0, eq)] = body.Substring(eq + 1);
                else
                    result[body] = string.Empty;
            }
            return result;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            deviceId = Adb.Devices().Where(x => x.DeviceState == DeviceState.Device).FirstOrDefault()?.DeviceId;
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                MessageBox.Show("No device");
                return;
            }
            adb = new Adb(deviceId);
            scrcpy = new Scrcpy(deviceId);
            scrcpy.OnDisconnect += Scrcpy_OnDisconnect;
            mainWindowVM.Control = new ControlChain(scrcpy.Control);
            mainWindowVM.ScrcpyUiView = scrcpy.InitScrcpyUiView();
            var s = await scrcpy.ListSupportAsync(new ListSupportQuery()
            {
                ListEncoders = true,
                ListDisplays = true,
                ListCameras = true,
                ListCameraSizes = true,
                ListApps = true,
            });

            // --- Flex display test options (passed via launchSettings.json commandLineArgs) ---
            // e.g. --new-display=1280x720/320 --flex --auto-resize --start-app=com.android.chrome
            //
            // NOTE: This flex-display path has NOT been validated end-to-end yet, because no Android 14+
            // device was available for testing (only a Redmi Note 9S, Android 12 / API 31).
            // Reason it cannot be verified on Android 12:
            //   --new-display + flex needs the virtual display to be "trusted" and own its focus, but
            //   scrcpy only sets those flags on newer Android (see NewDisplayCapture.startNew):
            //     - TRUSTED | OWN_DISPLAY_GROUP | ALWAYS_UNLOCKED  -> only from API 33 (Android 13)
            //     - OWN_FOCUS | DEVICE_DISPLAY_GROUP               -> only from API 34 (Android 14)
            //   On Android 12 the virtual display IS created and resize_display IS sent, but apps
            //   (e.g. Chrome) cannot reliably launch/focus on it, so the feature can't be confirmed here.
            //   Test with an Android 14+ device or AVD.
            var args = ParseArgs();
            if (args.TryGetValue("new-display", out var newDisplay) && !string.IsNullOrWhiteSpace(newDisplay))
                scrcpyConfig.ServerConfig!.NewDisplay = newDisplay;
            if (args.ContainsKey("flex"))
                scrcpyConfig.ServerConfig!.VideoConfig!.FlexDisplay = true;
            scrcpyControl.AutoResizeFlexDisplay = args.ContainsKey("auto-resize");

#if CameraTest
            scrcpyConfig.ServerConfig.VideoSource = VideoSource.Camera;
            var cam = s.CameraInfos
                .First(x => 
                    !x.IsHighSpeed && 
                    x.Size.Width < 1600 && 
                    x.CameraFacing == CameraFacing.Back
                    );
            scrcpyConfig.ServerConfig.CameraConfig = new CameraConfig()
            {
                CameraId = cam.CameraId,
                CameraSize = cam.Size,
                Camerafps = cam.FpsMin,
                CameraHighSpeed = cam.IsHighSpeed,
                CameraFacing = cam.CameraFacing,
            };
#endif

            if (!scrcpy.Connect(scrcpyConfig))
            {
                MessageBox.Show("Connect Failed");
            }
            else if (args.TryGetValue("start-app", out var startApp) && !string.IsNullOrWhiteSpace(startApp))
            {
                // launch the app on the (virtual) display; keep it in the textbox for re-launch
                mainWindowVM.AppPackageName = startApp;
                await Task.Delay(800);// let the virtual display register before starting the app
                mainWindowVM.Control?.StartApp(startApp);
            }

            if (scrcpyConfig?.ServerConfig?.AudioConfig?.IsAudio == true)
            {
                _audioStream = scrcpy.GetAudioStream();
                // Keep the task so Window_Closed can wait for the loop to finish before disposing
                // scrcpy. StartNew on an async method returns Task<Task>, so Unwrap() to get the task
                // that completes when RunReadAudio actually returns.
                _audioTask = Task.Factory.StartNew(RunReadAudio, TaskCreationOptions.LongRunning).Unwrap();
            }
        }

        private async void Scrcpy_OnDisconnect(ScrcpyDisconnectSource disconnectSource)
        {
            if (windowClosed) return;
            await adb!.WaitFor(WaitForType.Device).ExecuteAsync();
            scrcpy!.Connect(scrcpyConfig);
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        bool windowClosed = false;
        private void Window_Closed(object sender, EventArgs e)
        {
            windowClosed = true;

            // Stop the session first: this shuts down the scrcpy sockets, so the background audio
            // read loop (RunReadAudio -> ScrcpyAudioStream.Read) sees IsConnected == false, Read
            // returns 0, and the loop exits. We must WAIT for that thread to finish BEFORE disposing
            // scrcpy below; otherwise it would call into an already-disposed Scrcpy (its IsConnected
            // throws ObjectDisposedException on a background thread -> the whole app crashes).
            scrcpy?.Stop();
            try { _audioTask?.Wait(TimeSpan.FromSeconds(3)); }
            catch { /* ignore audio thread teardown errors */ }

            mainWindowVM.ScrcpyUiView?.Dispose();
            mainWindowVM.ScrcpyUiView = null;
            mainWindowVM.Control = null;
            scrcpy?.Dispose();
            _audioStream?.Dispose();
        }

        async Task RunReadAudio()
        {
            var stream = _audioStream!;
            _sdlDevice = new SdlDevice(
                stream.SampleRate,
                (byte)stream.Channels,
                stream.Format switch
                {
                    AVSampleFormat.S16 => SdlAudioFormat.AUDIO_S16,
                    _ => throw new NotSupportedException()
                });

            byte[] readBuf = new byte[stream.SampleRate * stream.Channels * stream.SampleSizeBytes / 10]; // ~100ms

            while (scrcpy!.IsConnected)
            {
                int read = stream.Read(readBuf, 0, readBuf.Length);
                if (read <= 0)
                    break;

                byte[] buffer = new byte[read];
                Array.Copy(readBuf, 0, buffer, 0, read);
                SdlSourceQueueResult result;
                while (true)
                {
                    result = _sdlDevice.QueueAudio(buffer, 1);
                    if (result == SdlSourceQueueResult.Success)
                        break;
                    if (result == SdlSourceQueueResult.QueueFailed)
                    {
                        await Task.Delay(10);
                        continue;
                    }
                }
            }
        }
    }
}
