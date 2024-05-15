using System;
using System.Windows.Controls;
using System.Windows;
using TqkLibrary.Wpf.Interop.DirectX;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Input;
using TqkLibrary.Scrcpy.Enums;
using TqkLibrary.Scrcpy.Interfaces;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Drawing;
using TqkLibrary.Scrcpy.Wpf.Interfaces;
using TqkLibrary.Scrcpy.Wpf.Handlers;

namespace TqkLibrary.Scrcpy.Wpf
{
    public partial class ScrcpyControlExt : D3D11Image, IScrcpyUiControl
    {
        readonly System.Windows.Controls.Image _image;
        readonly ControlHandler _controlHandler;
        readonly RenderHandler _renderHandler;
        private ScrcpyControlExt(System.Windows.Controls.Image image)
        {
            this._image = image;
            this._controlHandler = new ControlHandler(this);
            this._renderHandler = new RenderHandler(image, this);
        }


        public ScrcpyUiView? ScrcpyUiView
        {
            get { return GetScrcpyUiView(_image); }
        }
        public IControl? Control
        {
            get { return GetControl(_image); }
        }
        public bool IsControl
        {
            get { return GetIsControl(_image); }
        }
        /// <summary>
        /// Default false
        /// </summary>
        public bool IsMouseHandler
        {
            get { return GetIsMouseHandler(_image); }
        }
        /// <summary>
        /// Defaut false
        /// </summary>
        public bool IsKeyHandler
        {
            get { return GetIsKeyHandler(_image); }
        }
        /// <summary>
        /// Default <see cref="ScrcpyMousePointerId.POINTER_ID_GENERIC_FINGER"/>
        /// </summary>
        public long MousePointerId
        {
            get { return GetMousePointerId(_image); }
        }

        public System.Windows.Controls.Image Image => _image;

        public System.Drawing.Size VideoSize => _renderHandler.VideoSize;

        public Rectangle DrawRect => _renderHandler.DrawRect;


        //public void ClearView()
        //{
        //    this.WindowOwner = IntPtr.Zero;
        //    this.OnRender = null;
        //    this.RequestRender();
        //}
        //public void RegisterView()
        //{
        //    Window window = Window.GetWindow(_image);
        //    if (window == null)
        //        return;//ignore design load
        //    WindowInteropHelper windowInteropHelper = new WindowInteropHelper(window);
        //    this.WindowOwner = windowInteropHelper.Handle;
        //    this.OnRender = DoRender;
        //    this.RequestRender();
        //}
        //void DoRender(IntPtr SurfacePointer, bool isNewSurface)
        //{
        //    ScrcpyUiView? view = ScrcpyUiView;
        //    bool isNewtargetView = false;
        //    view?.DoRender(SurfacePointer, isNewSurface, ref isNewtargetView);
        //    if (isNewtargetView) _renderHandler.TargetViewChanged();
        //}
    }
}