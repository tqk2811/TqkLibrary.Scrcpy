using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
using TqkLibrary.Scrcpy;

namespace TqkLibrary.Scrcpy.Wpf
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="scrcpyControl"></param>
    /// <param name="data"></param>
    public delegate void OnUiChanged<T>(ScrcpyControl scrcpyControl, T data);


    /// <summary>
    /// Interaction logic for ScrcpyControl.xaml
    /// </summary>
    public partial class ScrcpyControl : UserControl
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty ScrcpyUiViewProperty = DependencyProperty.Register(
          nameof(ScrcpyUiView),
          typeof(ScrcpyUiView),
          typeof(ScrcpyControl),
          new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty ControlProperty = DependencyProperty.Register(
          nameof(Control),
          typeof(IControl),
          typeof(ScrcpyControl),
          new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty IsControlProperty = DependencyProperty.Register(
          nameof(IsControl),
          typeof(bool),
          typeof(ScrcpyControl),
          new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.None));

        public ScrcpyUiView ScrcpyUiView
        {
            get { return (ScrcpyUiView)GetValue(ScrcpyUiViewProperty); }
            set { SetValue(ScrcpyUiViewProperty, value); }
        }
        public IControl Control
        {
            get { return (IControl)GetValue(ControlProperty); }
            set { SetValue(ControlProperty, value); }
        }
        public bool IsControl
        {
            get { return (bool)GetValue(IsControlProperty); }
            set { SetValue(IsControlProperty, value); }
        }
        public event OnUiChanged<System.Drawing.Size> OnResize;

        public System.Drawing.Size VideoSize { get { return videoSize; } }
        public System.Drawing.Size RenderVideoSize { get { return drawRect.Size; } }


        bool lastVisible;
        System.Drawing.Size videoSize;
        System.Drawing.Rectangle drawRect;
        double rateVideoAndDraw;
        TimeSpan lastRender;
        public ScrcpyControl()
        {
            InitializeComponent();
        }

        private void host_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window == null) return;
            WindowInteropHelper windowInteropHelper = new WindowInteropHelper(window);

            InteropImage.WindowOwner = windowInteropHelper.Handle;
            InteropImage.OnRender = this.DoRender;
            InteropImage.RequestRender();
        }

        private void host_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double dpiScale = 1.0; // default value for 96 dpi

            // determine DPI
            // (as of .NET 4.6.1, this returns the DPI of the primary monitor, if you have several different DPIs)
            var hwndTarget = PresentationSource.FromVisual(this)?.CompositionTarget as HwndTarget;
            if (hwndTarget != null)
            {
                dpiScale = hwndTarget.TransformToDevice.M11;
            }

            double base_surfWidth = host.ActualWidth < 0 ? 0 : Math.Ceiling(host.ActualWidth * dpiScale);
            double base_surfHeight = host.ActualHeight < 0 ? 0 : Math.Ceiling(host.ActualHeight * dpiScale);
            var videoSize = ScrcpyUiView?.Scrcpy?.ScreenSize;


            double surfWidth = base_surfWidth;
            double surfHeight = base_surfHeight;
            if (videoSize.HasValue && videoSize.Value.Width != 0 && videoSize.Value.Height != 0)
            {
                this.videoSize = videoSize.Value;
                rateVideoAndDraw = Math.Min(surfWidth / videoSize.Value.Width, surfHeight / videoSize.Value.Height);
                surfWidth = videoSize.Value.Width * rateVideoAndDraw;
                surfHeight = videoSize.Value.Height * rateVideoAndDraw;
            }
            drawRect = new System.Drawing.Rectangle(
                (int)((base_surfWidth - surfWidth) / 2),
                (int)((base_surfHeight - surfHeight) / 2),
                (int)surfWidth,
                (int)surfHeight);
            // Notify the D3D11Image of the pixel size desired for the DirectX rendering.
            // The D3DRendering component will determine the size of the new surface it is given, at that point.
            InteropImage?.SetPixelSize(drawRect.Width, drawRect.Height);



            // Stop rendering if the D3DImage isn't visible - currently just if width or height is 0
            // TODO: more optimizations possible (scrolled off screen, etc...)
            bool isVisible = (surfWidth != 0 && surfHeight != 0);

            if (lastVisible != isVisible)
            {
                lastVisible = isVisible;
                if (lastVisible)
                {
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        OnResize?.Invoke(this, this.RenderVideoSize);
                    });
                    CompositionTarget.Rendering += CompositionTarget_Rendering;
                }
                else
                {
                    CompositionTarget.Rendering -= CompositionTarget_Rendering;
                }
            }
        }


        private void DoRender(IntPtr surface, bool isNewSurface)
        {
            var view = ScrcpyUiView;
            bool isNewtargetView = false;
            bool? renderResult = view?.DoRender(surface, isNewSurface,ref isNewtargetView);
            if(isNewtargetView)
            {
                host_SizeChanged(null, null);
            }
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            RenderingEventArgs args = (RenderingEventArgs)e;

            // It's possible for Rendering to call back twice in the same frame 
            // so only render when we haven't already rendered in this frame.
            if (this.lastRender != args.RenderingTime)
            {
                var videoSize = ScrcpyUiView?.Scrcpy.ScreenSize;
                if (videoSize != this.videoSize && host != null)
                {
                    host_SizeChanged(null, null);
                }
                InteropImage?.RequestRender();
                this.lastRender = args.RenderingTime;
            }
        }



        #region Control
        const long ponterid = 0xffffffff;
        bool isdown = false;
        private void img_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Keyboard.Focus(sender as Image);
            var p = GetRemotePoint(e.GetPosition((IInputElement)sender));
            var control = Control;
            if (IsControl && control != null)
            {
                Control.InjectTouchEvent(
                        AndroidMotionEventAction.ACTION_DOWN,
                        ponterid,
                        p,
                        1,
                        HandleMouse(e));

                isdown = true;
                img.CaptureMouse();
            }
        }

        private void img_MouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var p = GetRemotePoint(e.GetPosition((IInputElement)sender));
            var control = Control;
            if (IsControl && control != null)
            {
                control.InjectTouchEvent(
                          AndroidMotionEventAction.ACTION_UP,
                          ponterid,
                          p,
                          0,
                          HandleMouse(e));

                isdown = false;
                img.ReleaseMouseCapture();
            }
        }

        private void img_MouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;
            var p = GetRemotePoint(e.GetPosition((IInputElement)sender));
            var control = Control;
            if (IsControl && control != null)
            {
                if (isdown)
                    control.InjectTouchEvent(
                        AndroidMotionEventAction.ACTION_MOVE,
                        ponterid,
                        p,
                        1,
                        HandleMouse(e));
            }
        }


        private void img_MouseEnter(object sender, MouseEventArgs e)
        {
            //e.Handled = true;

        }

        private void img_MouseLeave(object sender, MouseEventArgs e)
        {
            //e.Handled = true;
        }

        private void img_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            var p = GetRemotePoint(e.GetPosition(null));
            var control = Control;
            if (IsControl && control != null)
            {
                control.InjectScrollEvent(p, e.Delta >= 0 ? 1 : -1);
            }
        }

        private void img_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (IsControl)
            {
                if (e.SystemKey == Key.None)
                {
                    AndroidKeyCode keyCode = ResolveKey(e);
                    if (keyCode != AndroidKeyCode.AKEYCODE_UNKNOWN)
                    {
                        Control?.InjectKeycode(AndroidKeyEventAction.ACTION_DOWN, keyCode);
#if DEBUG
                        Console.WriteLine($"ACTION_DOWN: {keyCode}");
#endif
                    }
                }
                else
                {

                }
            }
        }

        private void img_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (IsControl)
            {
                if (e.SystemKey == Key.None)
                {
                    AndroidKeyCode keyCode = ResolveKey(e);
                    if (keyCode != AndroidKeyCode.AKEYCODE_UNKNOWN)
                    {
                        Control?.InjectKeycode(AndroidKeyEventAction.ACTION_UP, keyCode);
#if DEBUG
                        Console.WriteLine($"ACTION_UP: {keyCode}");
#endif
                    }
                }
                else
                {

                }
            }
        }



        System.Drawing.Point GetRemotePoint(System.Windows.Point point)
        {
            double x = point.X / drawRect.Width;// (point.X - drawRect.X) / drawRect.Width;
            double y = point.Y / drawRect.Height;// (point.Y - drawRect.Y) / drawRect.Height;
            double real_x = x * videoSize.Width;
            double real_y = y * videoSize.Height;
            var outpoint = new System.Drawing.Point((int)real_x, (int)real_y);
#if DEBUG
            Debug.WriteLine($"drawRect: {drawRect}, videoSize: {videoSize}, inPoint: {point}, outPoint: {outpoint}, image_w: {InteropImage.Width}, image_h: {InteropImage.Height}");
#endif
            return outpoint;
        }


        AndroidMotionEventButton HandleMouse(MouseEventArgs e)
        {
            AndroidMotionEventButton button = AndroidMotionEventButton.None;
            if (e.LeftButton == MouseButtonState.Pressed) button |= AndroidMotionEventButton.BUTTON_PRIMARY;
            if (e.RightButton == MouseButtonState.Pressed) button |= AndroidMotionEventButton.BUTTON_SECONDARY;
            if (e.MiddleButton == MouseButtonState.Pressed) button |= AndroidMotionEventButton.BUTTON_TERTIARY;
            if (e.XButton1 == MouseButtonState.Pressed) button |= AndroidMotionEventButton.BUTTON_BACK;
            if (e.XButton2 == MouseButtonState.Pressed) button |= AndroidMotionEventButton.BUTTON_FORWARD;
            return button;
        }




        static readonly Dictionary<Key, AndroidKeyCode> special_keys = new Dictionary<Key, AndroidKeyCode>() {
            // Navigation keys and ENTER.
            // Used in all modes.
            { Key.Enter, AndroidKeyCode.AKEYCODE_ENTER },
            { Key.Escape, AndroidKeyCode.AKEYCODE_ESCAPE },
            { Key.Back, AndroidKeyCode.AKEYCODE_DEL },
            { Key.Tab, AndroidKeyCode.AKEYCODE_TAB },
            { Key.PageUp, AndroidKeyCode.AKEYCODE_PAGE_UP },
            { Key.Delete, AndroidKeyCode.AKEYCODE_FORWARD_DEL },
            { Key.Home, AndroidKeyCode.AKEYCODE_MOVE_HOME },
            { Key.End, AndroidKeyCode.AKEYCODE_MOVE_END },
            { Key.PageDown, AndroidKeyCode.AKEYCODE_PAGE_DOWN },
            { Key.Right, AndroidKeyCode.AKEYCODE_DPAD_RIGHT },
            { Key.Left, AndroidKeyCode.AKEYCODE_DPAD_LEFT },
            { Key.Down, AndroidKeyCode.AKEYCODE_DPAD_DOWN },
            { Key.Up, AndroidKeyCode.AKEYCODE_DPAD_UP },
            { Key.LeftCtrl, AndroidKeyCode.AKEYCODE_CTRL_LEFT },
            { Key.RightCtrl, AndroidKeyCode.AKEYCODE_CTRL_RIGHT },
            { Key.LeftShift, AndroidKeyCode.AKEYCODE_SHIFT_LEFT },
            { Key.RightShift, AndroidKeyCode.AKEYCODE_SHIFT_RIGHT },
        };


        // Numpad navigation keys.
        // Used in all modes, when NumLock and Shift are disabled.
        static readonly Dictionary<Key, AndroidKeyCode> kp_nav_keys = new Dictionary<Key, AndroidKeyCode>()
        {
            { Key.NumPad0, AndroidKeyCode.AKEYCODE_INSERT },
            { Key.NumPad1, AndroidKeyCode.AKEYCODE_MOVE_END },
            { Key.NumPad2, AndroidKeyCode.AKEYCODE_DPAD_DOWN },
            { Key.NumPad3, AndroidKeyCode.AKEYCODE_PAGE_DOWN },
            { Key.NumPad4, AndroidKeyCode.AKEYCODE_DPAD_LEFT },
            { Key.NumPad6, AndroidKeyCode.AKEYCODE_DPAD_RIGHT },
            { Key.NumPad7, AndroidKeyCode.AKEYCODE_MOVE_HOME },
            { Key.NumPad8, AndroidKeyCode.AKEYCODE_DPAD_UP },
            { Key.NumPad9, AndroidKeyCode.AKEYCODE_PAGE_UP },

        };

        //    static const struct sc_intmap_entry kp_nav_keys[] = {
        //    {SC_KEYCODE_KP_0,      AKEYCODE_INSERT },
        //    {SC_KEYCODE_KP_1,      AKEYCODE_MOVE_END },
        //    { SC_KEYCODE_KP_2,      AKEYCODE_DPAD_DOWN},
        //    { SC_KEYCODE_KP_3,      AKEYCODE_PAGE_DOWN},
        //    { SC_KEYCODE_KP_4,      AKEYCODE_DPAD_LEFT},
        //    { SC_KEYCODE_KP_6,      AKEYCODE_DPAD_RIGHT},
        //    { SC_KEYCODE_KP_7,      AKEYCODE_MOVE_HOME},
        //    { SC_KEYCODE_KP_8,      AKEYCODE_DPAD_UP},
        //    { SC_KEYCODE_KP_9,      AKEYCODE_PAGE_UP},
        //    { SC_KEYCODE_KP_PERIOD, AKEYCODE_FORWARD_DEL},
        //};




        AndroidKeyCode ResolveKey(KeyEventArgs e)
        {
            Key key = e.Key;
            if (IsShift(e))
            {
                switch (e.Key)
                {
                    case >= Key.D0 and <= Key.D9: return (AndroidKeyCode)((int)key + ((int)AndroidKeyCode.AKEYCODE_0 - (int)Key.D0));
                    case >= Key.A and <= Key.Z: return (AndroidKeyCode)((int)key + ((int)AndroidKeyCode.AKEYCODE_A - (int)Key.A));
                    case Key.Space: return AndroidKeyCode.AKEYCODE_SPACE;

                    case Key.OemPlus: return AndroidKeyCode.AKEYCODE_PLUS;
                    case Key.OemMinus: return AndroidKeyCode.AKEYCODE_SWITCH_CHARSET;

                    default: return AndroidKeyCode.AKEYCODE_UNKNOWN;
                }
            }
            else
            {
                switch (e.Key)
                {
                    case >= Key.D0 and <= Key.D9: return (AndroidKeyCode)((int)key + ((int)AndroidKeyCode.AKEYCODE_0 - (int)Key.D0));
                    case >= Key.A and <= Key.Z: return (AndroidKeyCode)((int)key + ((int)AndroidKeyCode.AKEYCODE_A - (int)Key.A));
                    case Key.Space: return AndroidKeyCode.AKEYCODE_SPACE;

                    case Key.Enter: return AndroidKeyCode.AKEYCODE_ENTER;
                    case Key.Escape: return AndroidKeyCode.AKEYCODE_ESCAPE;
                    case Key.Tab: return AndroidKeyCode.AKEYCODE_TAB;
                    case Key.Back: return AndroidKeyCode.AKEYCODE_DEL;

                    case Key.CapsLock: return AndroidKeyCode.AKEYCODE_CAPS_LOCK;
                    case Key.LeftShift: return AndroidKeyCode.AKEYCODE_SHIFT_LEFT;
                    case Key.RightShift: return AndroidKeyCode.AKEYCODE_SHIFT_RIGHT;
                    case Key.LeftAlt: return AndroidKeyCode.AKEYCODE_ALT_LEFT;
                    case Key.RightAlt: return AndroidKeyCode.AKEYCODE_ALT_RIGHT;
                    case Key.LeftCtrl: return AndroidKeyCode.AKEYCODE_CTRL_LEFT;
                    case Key.RightCtrl: return AndroidKeyCode.AKEYCODE_CTRL_RIGHT;

                    case Key.Left: return AndroidKeyCode.AKEYCODE_DPAD_LEFT;
                    case Key.Right: return AndroidKeyCode.AKEYCODE_DPAD_RIGHT;
                    case Key.Up: return AndroidKeyCode.AKEYCODE_DPAD_UP;
                    case Key.Down: return AndroidKeyCode.AKEYCODE_DPAD_DOWN;

                    case Key.Home: return AndroidKeyCode.AKEYCODE_MOVE_HOME;
                    case Key.End: return AndroidKeyCode.AKEYCODE_MOVE_END;
                    case Key.PageUp: return AndroidKeyCode.AKEYCODE_PAGE_UP;
                    case Key.PageDown: return AndroidKeyCode.AKEYCODE_PAGE_DOWN;

                    case Key.OemPlus: return AndroidKeyCode.AKEYCODE_NUMPAD_ADD;
                    case Key.OemMinus: return AndroidKeyCode.AKEYCODE_NUMPAD_SUBTRACT;
                    case Key.Divide: return AndroidKeyCode.AKEYCODE_NUMPAD_DIVIDE;
                    case Key.Multiply: return AndroidKeyCode.AKEYCODE_NUMPAD_MULTIPLY;
                    case Key.OemPeriod: return AndroidKeyCode.AKEYCODE_PERIOD;

                    default: return AndroidKeyCode.AKEYCODE_UNKNOWN;
                }
            }

        }
        bool IsShift(KeyEventArgs e) => e.KeyboardDevice.GetKeyStates(Key.LeftShift) == KeyStates.Down || e.KeyboardDevice.GetKeyStates(Key.RightShift) == KeyStates.Down;
        bool IsAlt(KeyEventArgs e) => e.KeyboardDevice.GetKeyStates(Key.LeftAlt) == KeyStates.Down || e.KeyboardDevice.GetKeyStates(Key.RightAlt) == KeyStates.Down;
        bool IsCtrl(KeyEventArgs e) => e.KeyboardDevice.GetKeyStates(Key.LeftCtrl) == KeyStates.Down || e.KeyboardDevice.GetKeyStates(Key.RightCtrl) == KeyStates.Down;
        bool IsNumlock(KeyEventArgs e) => false;
        bool IsCaptlock(KeyEventArgs e) => false;
        #endregion

    }
}
