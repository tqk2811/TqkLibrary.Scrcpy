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

namespace TestRenderWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Scrcpy scrcpy;
        Adb adb;
        string deviceId;
        readonly MainWindowVM mainWindowVM;
        public MainWindow()
        {
            InitializeComponent();
            mainWindowVM = this.DataContext as MainWindowVM;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            deviceId = Adb.Devices().Where(x => x.DeviceState == DeviceState.Device).FirstOrDefault().DeviceId;
            adb = new Adb(deviceId);
            scrcpy = new Scrcpy(deviceId);
            scrcpy.OnDisconnect += Scrcpy_OnDisconnect;
            mainWindowVM.Control = new ControlChain(scrcpy.Control);
            mainWindowVM.ScrcpyUiView = scrcpy.InitScrcpyUiView();
            if (!scrcpy.Connect(new ScrcpyConfig()
            {
                //HwType = FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA,
                HwType = FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_NONE,
                IsUseD3D11ForUiRender = true,
                ConnectionTimeout = 10000,
                ServerConfig = new ScrcpyServerConfig()
                {
                    IsControl = true,
                    MaxFps = 24,
                },
            }))
            {
                MessageBox.Show("Connect Failed");
            }
        }

        private async void Scrcpy_OnDisconnect()
        {
            if (windowClosed) return;
            await adb.WaitFor(WaitForType.Device).ExecuteAsync();
            scrcpy.Connect(new ScrcpyConfig()
            {
                //HwType = FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA,
                HwType = FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_NONE,
                IsUseD3D11ForUiRender = true,
                ConnectionTimeout = 3000,
                ServerConfig = new ScrcpyServerConfig()
                {
                    IsControl = true,
                    MaxFps = 24,
                },
            });
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        bool windowClosed = false;
        private void Window_Closed(object sender, EventArgs e)
        {
            windowClosed = true;
            scrcpy.Stop();
            mainWindowVM.ScrcpyUiView.Dispose();
            mainWindowVM.ScrcpyUiView = null;
            mainWindowVM.Control = null;
            scrcpy.Dispose();
        }
    }
}
