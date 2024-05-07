using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TqkLibrary.Scrcpy;
using TqkLibrary.AdbDotNet;
using System.Windows.Interop;
using System.Timers;
using TqkLibrary.Scrcpy.Wpf;
using TqkLibrary.Scrcpy.Enums;
using TqkLibrary.Scrcpy.Configs;
using TqkLibrary.Scrcpy.ListSupport;
using TqkLibrary.AudioPlayer.Sdl2;
using TqkLibrary.AudioPlayer.XAudio2;
using System.Threading;
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
            HwType = FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA,
            IsUseD3D11ForUiRender = true,
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
        AVFrame? _aVFrame;
        public MainWindow()
        {
            InitializeComponent();
            mainWindowVM = this.DataContext as MainWindowVM ?? throw new InvalidOperationException();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            deviceId = Adb.Devices().Where(x => x.DeviceState == DeviceState.Device).FirstOrDefault()?.DeviceId;
            if(string.IsNullOrWhiteSpace(deviceId))
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
            });

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

            if (scrcpyConfig?.ServerConfig?.AudioConfig?.IsAudio == true)
            {
                _aVFrame = new AVFrame();
                _ = Task.Factory.StartNew(RunReadAudio, TaskCreationOptions.LongRunning);
            }
        }

        private async void Scrcpy_OnDisconnect()
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
            scrcpy?.Stop();
            mainWindowVM.ScrcpyUiView?.Dispose();
            mainWindowVM.ScrcpyUiView = null;
            mainWindowVM.Control = null;
            scrcpy?.Dispose();
            _aVFrame?.Dispose();
        }

        void RunReadAudio()
        {
            long last_pts = 0;
            while (scrcpy!.IsConnected)
            {
                last_pts = scrcpy!.ReadAudioFrame(_aVFrame!, last_pts, 1000);
                if (last_pts > 0) break;
            }

            _sdlDevice = new SdlDevice(_aVFrame!.Handle);
            _sdlDevice.QueueAudio(_aVFrame.Handle);
            while (scrcpy.IsConnected)
            {
                long current_pts = scrcpy!.ReadAudioFrame(_aVFrame!, last_pts, 10);
                if (current_pts > 0)
                {
                    last_pts = current_pts;
                    _sdlDevice.QueueAudio(_aVFrame.Handle);
                }

                if (_sdlDevice.GetQueuedAudioSize() > 320 * 1024)
                {
                    _sdlDevice.ClearQueuedAudio();
                }
            }
        }
    }
}
