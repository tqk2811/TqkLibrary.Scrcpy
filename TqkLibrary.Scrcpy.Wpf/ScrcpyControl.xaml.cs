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
using TqkLibrary.Scrcpy.Wpf.Handlers;
using TqkLibrary.Scrcpy.Wpf.Interfaces;

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
    public partial class ScrcpyControl : UserControl, IScrcpyUiControl
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




        public ScrcpyUiView? ScrcpyUiView
        {
            get { return (ScrcpyUiView)GetValue(ScrcpyUiViewProperty); }
            set { SetValue(ScrcpyUiViewProperty, value); }
        }
        public IControl? Control
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

        /// <summary>
        /// 
        /// </summary>
        public event OnUiChanged<System.Drawing.Size>? RenderSizeChanged;
        /// <summary>
        /// 
        /// </summary>
        public event OnUiChanged<System.Drawing.Size>? VideoSizeChanged;

        public System.Drawing.Size VideoSize { get { return _renderHandler.VideoSize; } }
        public System.Drawing.Rectangle DrawRect { get { return _renderHandler.DrawRect; } }
        public System.Drawing.Size RenderVideoSize { get { return _renderHandler.DrawRect.Size; } }
        Image IScrcpyUiControl.Image => this.img;


        readonly ControlHandler _controlHandler;
        readonly RenderHandler _renderHandler;
        public ScrcpyControl()
        {
            InitializeComponent();
            _controlHandler = new ControlHandler(this);
            _renderHandler = new RenderHandler(img, this);
        }
    }
}
