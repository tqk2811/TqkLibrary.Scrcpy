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

namespace TestRenderWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Scrcpy scrcpy;
        ScrcpyUiView scrcpyUiView;
        bool lastVisible;
        TimeSpan lastRender;
        System.Drawing.Size videoSize;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string deviceId = Adb.Devices().Where(x => x.DeviceState == DeviceState.Device).FirstOrDefault().DeviceId;
            scrcpy = new Scrcpy(deviceId);
            scrcpyUiView = scrcpy.InitScrcpyUiView();

            //Task.Run(() =>
            //{
            //    scrcpy.Connect(new ScrcpyConfig()
            //    {
            //        HwType = FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA,
            //        IsUseD3D11Shader = true,
            //        IsControl = true,
            //        MaxFps = 24,
            //        ConnectionTimeout = 99999999
            //    });
            //});
            scrcpy.Connect(new ScrcpyConfig()
            {
                HwType = FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA,
                IsUseD3D11Shader = true,
                IsControl = true,
                MaxFps = 24,
                ConnectionTimeout = 99999999
            });
            host_SizeChanged(null, null);
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            scrcpy.Stop();
            scrcpyUiView.Dispose();
            scrcpyUiView = null;
            scrcpy.Dispose();
        }

        private void DoRender(IntPtr surface, bool isNewSurface)
        {
            scrcpyUiView.DoRender(surface, isNewSurface);
        }


        private void host_Loaded(object sender, RoutedEventArgs e)
        {
            InteropImage.WindowOwner = (new System.Windows.Interop.WindowInteropHelper(this)).Handle;
            InteropImage.OnRender = this.DoRender;
            InteropImage.RequestRender();
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            RenderingEventArgs args = (RenderingEventArgs)e;

            // It's possible for Rendering to call back twice in the same frame 
            // so only render when we haven't already rendered in this frame.
            if (this.lastRender != args.RenderingTime)
            {
                var videoSize = scrcpy?.ScreenSize;
                if(videoSize != this.videoSize)
                {
                    host_SizeChanged(null, null);
                }
                InteropImage.RequestRender();
                this.lastRender = args.RenderingTime;
            }
        }

        private void host_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double dpiScale = 1.0; // default value for 96 dpi

            // determine DPI
            // (as of .NET 4.6.1, this returns the DPI of the primary monitor, if you have several different DPIs)
            var hwndTarget = PresentationSource.FromVisual(this).CompositionTarget as HwndTarget;
            if (hwndTarget != null)
            {
                dpiScale = hwndTarget.TransformToDevice.M11;
            }

            double surfWidth = host.ActualWidth < 0 ? 0 : Math.Ceiling(host.ActualWidth * dpiScale);
            double surfHeight = host.ActualHeight < 0 ? 0 : Math.Ceiling(host.ActualHeight * dpiScale);
            var videoSize = scrcpy?.ScreenSize;

            if (videoSize.HasValue && videoSize.Value.Width != 0 && videoSize.Value.Height != 0)
            {
                this.videoSize = videoSize.Value;
                double rate = Math.Min(surfWidth / videoSize.Value.Width, surfHeight / videoSize.Value.Width);
                surfWidth = videoSize.Value.Width * rate;
                surfHeight = videoSize.Value.Height * rate;
            }
            // Notify the D3D11Image of the pixel size desired for the DirectX rendering.
            // The D3DRendering component will determine the size of the new surface it is given, at that point.
            InteropImage.SetPixelSize((int)surfWidth, (int)surfHeight);



            // Stop rendering if the D3DImage isn't visible - currently just if width or height is 0
            // TODO: more optimizations possible (scrolled off screen, etc...)
            bool isVisible = (surfWidth != 0 && surfHeight != 0);

            if (lastVisible != isVisible)
            {
                lastVisible = isVisible;
                if (lastVisible)
                {
                    CompositionTarget.Rendering += CompositionTarget_Rendering;
                }
                else
                {
                    CompositionTarget.Rendering -= CompositionTarget_Rendering;
                }
            }
        }
    }
}
