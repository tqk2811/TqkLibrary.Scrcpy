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
using TqkLibrary.Scrcpy.Control;
using TqkLibrary.Scrcpy.Enums;
using TqkLibrary.Scrcpy.Interfaces;

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

        public static readonly DependencyProperty IsMouseHandlerProperty = DependencyProperty.Register(
            nameof(IsMouseHandler),
            typeof(bool),
            typeof(ScrcpyControl),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty IsKeyHandlerProperty = DependencyProperty.Register(
            nameof(IsKeyHandler),
            typeof(bool),
            typeof(ScrcpyControl),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty MousePointerIdProperty = DependencyProperty.Register(
            nameof(MousePointerId),
            typeof(long),
            typeof(ScrcpyControl),
            new FrameworkPropertyMetadata(ScrcpyMousePointerId.POINTER_ID_GENERIC_FINGER, FrameworkPropertyMetadataOptions.None));




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
        /// <summary>
        /// Default false
        /// </summary>
        public bool IsMouseHandler
        {
            get { return (bool)GetValue(IsMouseHandlerProperty); }
            set { SetValue(IsMouseHandlerProperty, value); }
        }
        /// <summary>
        /// Defaut false
        /// </summary>
        public bool IsKeyHandler
        {
            get { return (bool)GetValue(IsKeyHandlerProperty); }
            set { SetValue(IsKeyHandlerProperty, value); }
        }
        /// <summary>
        /// Default <see cref="ScrcpyMousePointerId.POINTER_ID_GENERIC_FINGER"/>
        /// </summary>
        public long MousePointerId
        {
            get { return (long)GetValue(MousePointerIdProperty); }
            set { SetValue(MousePointerIdProperty, value); }
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
            if (window == null) return;//ignore design load

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
            view?.DoRender(surface, isNewSurface, ref isNewtargetView);
            if (isNewtargetView)
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
        bool isdown = false;
        private void img_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = IsMouseHandler;
            Keyboard.Focus(sender as Image);
            var p = GetRemotePoint(e.GetPosition((IInputElement)sender));
            var control = Control;
            if (IsControl && control != null)
            {
                ResolveMouseButton(control, p, e.ChangedButton, AndroidMotionEventAction.ACTION_DOWN);
            }
        }

        private void img_MouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = IsMouseHandler;
            var p = GetRemotePoint(e.GetPosition((IInputElement)sender));
            var control = Control;
            if (IsControl && control != null)
            {
                ResolveMouseButton(control, p, e.ChangedButton, AndroidMotionEventAction.ACTION_UP);
            }
        }

        private void img_MouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = IsMouseHandler;
            var p = GetRemotePoint(e.GetPosition((IInputElement)sender));
            var control = Control;
            if (IsControl && control != null)
            {
                if (isdown)
                    control.InjectTouchEvent(
                        AndroidMotionEventAction.ACTION_MOVE,
                        MousePointerId,
                        p,
                        1.0f,
                        HandleMouseButton(e),
                        HandleMouseButton(e));
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
            e.Handled = IsMouseHandler;
            var p = GetRemotePoint(e.GetPosition((IInputElement)sender));
            var control = Control;
            if (IsControl && control != null)
            {
                control.InjectScrollEvent(p, e.Delta >= 0 ? 1 : -1);
            }
        }

        private async void img_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = IsKeyHandler;
            var control = Control;
            if (IsControl && control != null)
            {
                if (e.SystemKey == Key.None)
                {
                    AndroidKeyCode keyCode = await ResolveKey(e);
                    if (keyCode != AndroidKeyCode.AKEYCODE_UNKNOWN)
                    {
                        control.InjectKeycode(AndroidKeyEventAction.ACTION_DOWN, keyCode, 0, ResolveMetaKey(e));
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

        private async void img_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = IsKeyHandler;
            var control = Control;
            if (IsControl && control != null)
            {
                if (e.SystemKey == Key.None)
                {
                    AndroidKeyCode keyCode = await ResolveKey(e);
                    if (keyCode != AndroidKeyCode.AKEYCODE_UNKNOWN)
                    {
                        control.InjectKeycode(AndroidKeyEventAction.ACTION_UP, keyCode, 0, ResolveMetaKey(e));
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

        void ResolveMouseButton(IControl control, System.Drawing.Point point, MouseButton mouseButton, AndroidMotionEventAction action)
        {
            if (action == AndroidMotionEventAction.ACTION_DOWN || action == AndroidMotionEventAction.ACTION_UP)
            {
                switch (mouseButton)
                {
                    case MouseButton.Left:
                        isdown = action == AndroidMotionEventAction.ACTION_DOWN;
                        if (isdown) img.CaptureMouse();
                        else img.ReleaseMouseCapture();
                        control.InjectTouchEvent(
                            action,
                            MousePointerId,
                            point,
                            action == AndroidMotionEventAction.ACTION_DOWN ? 1.0f : 0.0f,
                            AndroidMotionEventButton.BUTTON_PRIMARY,
                            action == AndroidMotionEventAction.ACTION_DOWN ? AndroidMotionEventButton.BUTTON_PRIMARY : AndroidMotionEventButton.None);
                        break;

                    case MouseButton.Right:
                        control.BackOrScreenOn(
                            action == AndroidMotionEventAction.ACTION_DOWN ? AndroidKeyEventAction.ACTION_DOWN : AndroidKeyEventAction.ACTION_UP);
                        break;

                    case MouseButton.Middle:
                        control.InjectKeycode(
                            action == AndroidMotionEventAction.ACTION_DOWN ? AndroidKeyEventAction.ACTION_DOWN : AndroidKeyEventAction.ACTION_UP,
                            AndroidKeyCode.AKEYCODE_HOME,
                            0,
                            AndroidKeyEventMeta.META_NONE);
                        break;

                    case MouseButton.XButton1:
                        control.InjectKeycode(
                            action == AndroidMotionEventAction.ACTION_DOWN ? AndroidKeyEventAction.ACTION_DOWN : AndroidKeyEventAction.ACTION_UP,
                            AndroidKeyCode.AKEYCODE_APP_SWITCH,
                            0,
                            AndroidKeyEventMeta.META_NONE);
                        break;

                    case MouseButton.XButton2:
                        if (action == AndroidMotionEventAction.ACTION_DOWN)
                        {
                            control.ExpandNotificationPanel();
                        }
                        break;
                }
            }
        }
        AndroidMotionEventButton HandleMouseButton(MouseEventArgs e)
        {
            AndroidMotionEventButton button = AndroidMotionEventButton.None;
            if (e.LeftButton == MouseButtonState.Pressed) button |= AndroidMotionEventButton.BUTTON_PRIMARY;
            if (e.RightButton == MouseButtonState.Pressed) button |= AndroidMotionEventButton.BUTTON_SECONDARY;
            if (e.MiddleButton == MouseButtonState.Pressed) button |= AndroidMotionEventButton.BUTTON_TERTIARY;
            if (e.XButton1 == MouseButtonState.Pressed) button |= AndroidMotionEventButton.BUTTON_BACK;
            if (e.XButton2 == MouseButtonState.Pressed) button |= AndroidMotionEventButton.BUTTON_FORWARD;
            return button;
        }










        #region Key

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

        static readonly Dictionary<Key, AndroidKeyCode> alphaspace_keys = new Dictionary<Key, AndroidKeyCode>()
        {
            { Key.A, AndroidKeyCode.AKEYCODE_A },
            { Key.B, AndroidKeyCode.AKEYCODE_B },
            { Key.C, AndroidKeyCode.AKEYCODE_C },
            { Key.D, AndroidKeyCode.AKEYCODE_D },
            { Key.E, AndroidKeyCode.AKEYCODE_E },
            { Key.F, AndroidKeyCode.AKEYCODE_F },
            { Key.G, AndroidKeyCode.AKEYCODE_G },
            { Key.H, AndroidKeyCode.AKEYCODE_H },
            { Key.I, AndroidKeyCode.AKEYCODE_I },
            { Key.J, AndroidKeyCode.AKEYCODE_J },
            { Key.K, AndroidKeyCode.AKEYCODE_K },
            { Key.L, AndroidKeyCode.AKEYCODE_L },
            { Key.M, AndroidKeyCode.AKEYCODE_M },
            { Key.N, AndroidKeyCode.AKEYCODE_N },
            { Key.O, AndroidKeyCode.AKEYCODE_O },
            { Key.P, AndroidKeyCode.AKEYCODE_P },
            { Key.Q, AndroidKeyCode.AKEYCODE_Q },
            { Key.R, AndroidKeyCode.AKEYCODE_R },
            { Key.S, AndroidKeyCode.AKEYCODE_S },
            { Key.T, AndroidKeyCode.AKEYCODE_T },
            { Key.U, AndroidKeyCode.AKEYCODE_U },
            { Key.V, AndroidKeyCode.AKEYCODE_V },
            { Key.W, AndroidKeyCode.AKEYCODE_W },
            { Key.X, AndroidKeyCode.AKEYCODE_X },
            { Key.Y, AndroidKeyCode.AKEYCODE_Y },
            { Key.Z, AndroidKeyCode.AKEYCODE_Z },
            { Key.Space, AndroidKeyCode.AKEYCODE_SPACE },

        };

        static readonly Dictionary<Key, AndroidKeyCode> numbers_punct_keys = new Dictionary<Key, AndroidKeyCode>()
        {
            //{SC_KEYCODE_HASH,           AndroidKeyCode.AKEYCODE_POUND},
            //{SC_KEYCODE_PERCENT,        AndroidKeyCode.AKEYCODE_PERIOD},            
            {Key.Multiply,              AndroidKeyCode.AKEYCODE_STAR},
            {Key.Divide,                AndroidKeyCode.AKEYCODE_NUMPAD_DIVIDE},
            {Key.Subtract,              AndroidKeyCode.AKEYCODE_MINUS},
            {Key.Add,                   AndroidKeyCode.AKEYCODE_NUMPAD_ADD},

            {Key.Decimal,               AndroidKeyCode.AKEYCODE_NUMPAD_DOT},

            {Key.OemPeriod,             AndroidKeyCode.AKEYCODE_PERIOD},
            {Key.OemQuotes,             AndroidKeyCode.AKEYCODE_APOSTROPHE},
            {Key.OemPlus,               AndroidKeyCode.AKEYCODE_EQUALS},
            {Key.OemComma,              AndroidKeyCode.AKEYCODE_COMMA},
            {Key.OemQuestion,           AndroidKeyCode.AKEYCODE_SLASH},
            {Key.OemMinus,              AndroidKeyCode.AKEYCODE_MINUS},
            //{Key.OemPlus,               AndroidKeyCode.AKEYCODE_EQUALS},
            {Key.OemOpenBrackets,       AndroidKeyCode.AKEYCODE_LEFT_BRACKET},

            {Key.D0,                    AndroidKeyCode.AKEYCODE_0},
            {Key.D1,                    AndroidKeyCode.AKEYCODE_1},
            {Key.D2,                    AndroidKeyCode.AKEYCODE_2},
            {Key.D3,                    AndroidKeyCode.AKEYCODE_3},
            {Key.D4,                    AndroidKeyCode.AKEYCODE_4},
            {Key.D5,                    AndroidKeyCode.AKEYCODE_5},
            {Key.D6,                    AndroidKeyCode.AKEYCODE_6},
            {Key.D7,                    AndroidKeyCode.AKEYCODE_7},
            {Key.D8,                    AndroidKeyCode.AKEYCODE_8},
            {Key.D9,                    AndroidKeyCode.AKEYCODE_9},

            {Key.Oem1,                  AndroidKeyCode.AKEYCODE_SEMICOLON},
            {Key.Oem3,                  AndroidKeyCode.AKEYCODE_GRAVE},
            {Key.Oem5,                  AndroidKeyCode.AKEYCODE_BACKSLASH},
            {Key.Oem6,                  AndroidKeyCode.AKEYCODE_RIGHT_BRACKET},

            //{Key.D2 + shift,             AndroidKeyCode.AKEYCODE_AT},

            {Key.NumPad1,               AndroidKeyCode.AKEYCODE_NUMPAD_1},
            {Key.NumPad2,               AndroidKeyCode.AKEYCODE_NUMPAD_2},
            {Key.NumPad3,               AndroidKeyCode.AKEYCODE_NUMPAD_3},
            {Key.NumPad4,               AndroidKeyCode.AKEYCODE_NUMPAD_4},
            {Key.NumPad5,               AndroidKeyCode.AKEYCODE_NUMPAD_5},
            {Key.NumPad6,               AndroidKeyCode.AKEYCODE_NUMPAD_6},
            {Key.NumPad7,               AndroidKeyCode.AKEYCODE_NUMPAD_7},
            {Key.NumPad8,               AndroidKeyCode.AKEYCODE_NUMPAD_8},
            {Key.NumPad9,               AndroidKeyCode.AKEYCODE_NUMPAD_9},
            {Key.NumPad0,               AndroidKeyCode.AKEYCODE_NUMPAD_0},
            //{Key.Multiply,              AndroidKeyCode.AKEYCODE_NUMPAD_MULTIPLY},
            //{Key.Subtract,            AndroidKeyCode.AKEYCODE_NUMPAD_SUBTRACT},
            //{SC_KEYCODE_KP_EQUALS,      AndroidKeyCode.AKEYCODE_NUMPAD_EQUALS},
            //{Key.D9,   AndroidKeyCode.AKEYCODE_NUMPAD_LEFT_PAREN},
            //{Key.D0,  AndroidKeyCode.AKEYCODE_NUMPAD_RIGHT_PAREN},
        };

        async Task<AndroidKeyCode> ResolveKey(KeyEventArgs e)
        {
            Key key = e.Key;
#if DEBUG
            Debug.WriteLine($"Key: {key} is {e.KeyStates}");
#endif
            switch (key)
            {
                case Key.LeftCtrl:
                case Key.RightCtrl:
                case Key.LeftAlt:
                case Key.RightAlt:
                case Key.LeftShift:
                case Key.RightShift:
                    return AndroidKeyCode.AKEYCODE_UNKNOWN;//skip system key, it using in AndroidKeyEventMeta
            }
            if (IsKeyDown(e, Key.C) && IsCtrl(e))
            {
                string text = await Control.GetClipboardAsync(CopyKey.Copy);
                if (!string.IsNullOrEmpty(text)) Clipboard.SetText(text);
                return AndroidKeyCode.AKEYCODE_UNKNOWN;
            }
            if (IsKeyDown(e, Key.V) && IsCtrl(e))
            {
                string text = Clipboard.GetText();
                if (!string.IsNullOrEmpty(text)) Control.SetClipboard(text, true);
                return AndroidKeyCode.AKEYCODE_UNKNOWN;
            }
            if (IsKeyDown(e, Key.X) && IsCtrl(e))
            {
                string text = await Control.GetClipboardAsync(CopyKey.Cut);
                if (!string.IsNullOrEmpty(text)) Clipboard.SetText(text);
                return AndroidKeyCode.AKEYCODE_UNKNOWN;
            }

            if (special_keys.ContainsKey(key))
                return special_keys[key];

            if (numbers_punct_keys.ContainsKey(key))
                return numbers_punct_keys[key];

            if (/*!IsCtrl(e) && !IsAlt(e) &&*/ alphaspace_keys.ContainsKey(key))
                return alphaspace_keys[key];

            return AndroidKeyCode.AKEYCODE_UNKNOWN;
        }

        AndroidKeyEventMeta ResolveMetaKey(KeyEventArgs e)
        {
            AndroidKeyEventMeta result = AndroidKeyEventMeta.META_NONE;

            if (e.KeyboardDevice.GetKeyStates(Key.LeftShift).HasFlag(KeyStates.Down)) result |= AndroidKeyEventMeta.META_SHIFT_LEFT_ON;
            if (e.KeyboardDevice.GetKeyStates(Key.RightShift).HasFlag(KeyStates.Down)) result |= AndroidKeyEventMeta.META_SHIFT_RIGHT_ON;

            if (e.KeyboardDevice.GetKeyStates(Key.LeftAlt).HasFlag(KeyStates.Down)) result |= AndroidKeyEventMeta.META_ALT_LEFT_ON;
            if (e.KeyboardDevice.GetKeyStates(Key.RightAlt).HasFlag(KeyStates.Down)) result |= AndroidKeyEventMeta.META_ALT_RIGHT_ON;

            if (e.KeyboardDevice.GetKeyStates(Key.LeftCtrl).HasFlag(KeyStates.Down)) result |= AndroidKeyEventMeta.META_CTRL_LEFT_ON;
            if (e.KeyboardDevice.GetKeyStates(Key.RightCtrl).HasFlag(KeyStates.Down)) result |= AndroidKeyEventMeta.META_CTRL_RIGHT_ON;

            if (IsNumlock(e)) result |= AndroidKeyEventMeta.META_NUM_LOCK_ON;
            if (IsCaptlock(e)) result |= AndroidKeyEventMeta.META_CAPS_LOCK_ON;
#if DEBUG
            Debug.WriteLine($"MetaKey: {result}");
#endif
            return result;
        }
        bool IsShift(KeyEventArgs e) => e.KeyboardDevice.GetKeyStates(Key.LeftShift).HasFlag(KeyStates.Down) ||
                                        e.KeyboardDevice.GetKeyStates(Key.RightShift).HasFlag(KeyStates.Down);
        bool IsAlt(KeyEventArgs e) => e.KeyboardDevice.GetKeyStates(Key.LeftAlt).HasFlag(KeyStates.Down) ||
                                        e.KeyboardDevice.GetKeyStates(Key.RightAlt).HasFlag(KeyStates.Down);
        bool IsCtrl(KeyEventArgs e) => e.KeyboardDevice.GetKeyStates(Key.LeftCtrl).HasFlag(KeyStates.Down) ||
                                        e.KeyboardDevice.GetKeyStates(Key.RightCtrl).HasFlag(KeyStates.Down);
        bool IsNumlock(KeyEventArgs e) => e.KeyboardDevice.GetKeyStates(Key.NumLock).HasFlag(KeyStates.Toggled);
        bool IsCaptlock(KeyEventArgs e) => e.KeyboardDevice.GetKeyStates(Key.CapsLock).HasFlag(KeyStates.Toggled);

        bool IsKeyDown(KeyEventArgs e, Key key) => e.KeyboardDevice.GetKeyStates(key).HasFlag(KeyStates.Down);

        #endregion



        #endregion
    }
}
