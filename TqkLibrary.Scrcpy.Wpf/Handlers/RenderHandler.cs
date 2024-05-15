using System;
using System.Windows.Controls;
using System.Windows;
using TqkLibrary.Wpf.Interop.DirectX;
using System.Windows.Media;
using System.Drawing;
using TqkLibrary.Scrcpy.Wpf.Interfaces;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Interop;
using Microsoft.Win32;

namespace TqkLibrary.Scrcpy.Wpf
{
    internal delegate void OnSizeChanged(RenderHandler renderHandler, System.Drawing.Size size);
    class RenderHandler
    {
        public IScrcpyControlBinding ScrcpyControlBinding { get; }
        public System.Windows.Controls.Image Image { get; }
        public D3D11Image? D3D11Image { get { return Image.Source as D3D11Image; } }
        public FrameworkElement? Parent { get { return Image.Parent as FrameworkElement; } }



        public System.Drawing.Size VideoSize { get; private set; }
        public System.Drawing.Rectangle DrawRect { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public event OnSizeChanged? RenderSizeChanged;
        /// <summary>
        /// 
        /// </summary>
        public event OnSizeChanged? VideoSizeChanged;

        public RenderHandler(System.Windows.Controls.Image image, IScrcpyControlBinding scrcpyControlBinding)
        {
            this.Image = image ?? throw new ArgumentNullException(nameof(image));
            this.ScrcpyControlBinding = scrcpyControlBinding ?? throw new ArgumentNullException(nameof(scrcpyControlBinding));
            this.Image.Loaded += Image_Loaded;
            this.Image.Unloaded += Image_Unloaded;
            Image_Loaded(null,null);
        }
        public void TargetViewChanged()
        {
            Parent_SizeChanged(_hold_Parent, null);
        }
        public void ClearView()
        {
            D3D11Image? d3D11Image = D3D11Image;
            if (d3D11Image is not null)
            {
                d3D11Image.WindowOwner = IntPtr.Zero;
                d3D11Image.OnRender = null;
                d3D11Image.RequestRender();
            }
        }
        public void RegisterView()
        {
            D3D11Image? d3D11Image = D3D11Image;
            if (d3D11Image is not null)
            {
                Window window = Window.GetWindow(Image);
                if (window == null)
                    return;//ignore design load
                           WindowInteropHelper windowInteropHelper = new WindowInteropHelper(window);
                d3D11Image.WindowOwner = windowInteropHelper.Handle;
                d3D11Image.OnRender = DoRender;
                d3D11Image.RequestRender();
            }
        }
        private void Image_Loaded(object? sender, RoutedEventArgs? e)
        {
            RegisterView();
            RegisterParent();
        }
        private void Image_Unloaded(object sender, RoutedEventArgs e)
        {
            ClearView();
            UnregisterParent();
        }

        FrameworkElement? _hold_Parent = null;
        void RegisterParent()
        {
            _hold_Parent = Parent;
            if (_hold_Parent is not null)
            {
                _hold_Parent.SizeChanged += Parent_SizeChanged;
                Parent_SizeChanged(_hold_Parent, null);
            }
        }
        void UnregisterParent()
        {
            if (_hold_Parent is not null)
            {
                _hold_Parent.SizeChanged -= Parent_SizeChanged;
                _hold_Parent = null;
                Parent_SizeChanged(_hold_Parent, null);
            }
        }

        bool lastVisible;
        double rateVideoAndDraw;
        TimeSpan lastRender;
        private void Parent_SizeChanged(object? sender, SizeChangedEventArgs? e)
        {
            if (sender is FrameworkElement host)
            {
                double base_surfWidth = host.ActualWidth < 0 ? 0 : Math.Ceiling(host.ActualWidth);
                double base_surfHeight = host.ActualHeight < 0 ? 0 : Math.Ceiling(host.ActualHeight);

                var videoSize = ScrcpyControlBinding.ScrcpyUiView?.Scrcpy?.ScreenSize;

                double surfWidth = base_surfWidth;
                double surfHeight = base_surfHeight;
                if (videoSize.HasValue && videoSize.Value.Width != 0 && videoSize.Value.Height != 0)
                {
                    this.VideoSize = videoSize.Value;
                    rateVideoAndDraw = Math.Min(surfWidth / videoSize.Value.Width, surfHeight / videoSize.Value.Height);
                    surfWidth = videoSize.Value.Width * rateVideoAndDraw;
                    surfHeight = videoSize.Value.Height * rateVideoAndDraw;
                }
                DrawRect = new System.Drawing.Rectangle(
                    (int)((base_surfWidth - surfWidth) / 2),
                    (int)((base_surfHeight - surfHeight) / 2),
                    (int)surfWidth,
                    (int)surfHeight);
                // Notify the D3D11Image of the pixel size desired for the DirectX rendering.
                // The D3DRendering component will determine the size of the new surface it is given, at that point.
                D3D11Image?.SetPixelSize(DrawRect.Width, DrawRect.Height);
                RenderSizeChanged?.Invoke(this, DrawRect.Size);


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
            else
            {
                if (lastVisible)
                {
                    CompositionTarget.Rendering -= CompositionTarget_Rendering;
                    lastVisible = false;
                }
            }
        }

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            RenderingEventArgs? args = e as RenderingEventArgs;

            // It's possible for Rendering to call back twice in the same frame 
            // so only render when we haven't already rendered in this frame.
            if (args is not null && this.lastRender != args.RenderingTime)
            {
                var videoSize = ScrcpyControlBinding.ScrcpyUiView?.Scrcpy.ScreenSize;
                if (videoSize != this.VideoSize)
                {
                    if (videoSize.HasValue) VideoSizeChanged?.Invoke(this, videoSize.Value);
                    Parent_SizeChanged(Parent, null);
                }
                D3D11Image?.RequestRender();
                this.lastRender = args.RenderingTime;
            }
        }

        void DoRender(IntPtr SurfacePointer, bool isNewSurface)
        {
            ScrcpyUiView? view = ScrcpyControlBinding.ScrcpyUiView;
            bool isNewtargetView = false;
            view?.DoRender(SurfacePointer, isNewSurface, ref isNewtargetView);
            if (isNewtargetView) TargetViewChanged();
        }
    }
}
