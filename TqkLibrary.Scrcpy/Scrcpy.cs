using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.Scrcpy.Control;
using System.Threading;

namespace TqkLibrary.Scrcpy
{
    internal delegate void NativeOnDisconnectDelegate();
    /// <summary>
    /// 
    /// </summary>
    public class Scrcpy : IDisposable
    {
        readonly object _lock = new object();
        private IntPtr _handle = IntPtr.Zero;

        /// <summary>
        /// 
        /// </summary>
        public string DeviceId { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceId"></param>
        public Scrcpy(string deviceId)
        {
            if(string.IsNullOrWhiteSpace(deviceId)) throw new ArgumentNullException(nameof(deviceId));
            this.DeviceId = deviceId;

            _handle = NativeWrapper.ScrcpyAlloc(deviceId);

            Control = new ScrcpyControl(this);
            Control.OnClipboardReceived += Control_OnClipboardReceived;


            this.NativeOnDisconnectDelegate = onDisconnect;
            IntPtr pointer = Marshal.GetFunctionPointerForDelegate(this.NativeOnDisconnectDelegate);
            NativeWrapper.RegisterDisconnectEvent(_handle, pointer);
        }
        /// <summary>
        /// 
        /// </summary>
        ~Scrcpy()
        {
            Dispose(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_handle != IntPtr.Zero)
            {
                lock (_lock)
                {
                    NativeWrapper.ScrcpyFree(_handle);
                    _handle = IntPtr.Zero;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Size ScreenSize
        {
            get
            {
                lock (_lock)
                {
                    CheckDispose();
                    return GetScreenSize();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IControl Control { get; }

        /// <summary>
        /// 
        /// </summary>
        public string LastClipboard { get; private set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>

        public event Action OnDisconnect;


        readonly NativeOnDisconnectDelegate NativeOnDisconnectDelegate;
        void onDisconnect()
        {
            if (OnDisconnect != null)
            {
                ThreadPool.QueueUserWorkItem((o) => OnDisconnect?.Invoke());
            }
        }


        private void CheckDispose()
        {
            if (_handle == IntPtr.Zero) throw new ObjectDisposedException(nameof(Scrcpy));
        }

        private void Control_OnClipboardReceived(IControl control, string data)
        {
            this.LastClipboard = data;
        }

        #region Function Control
        internal bool SendControl(ScrcpyControlMessage scrcpyControlMessage)
        {
            lock (_lock)
            {
                CheckDispose();
                byte[] command = scrcpyControlMessage.GetCommand();
                return NativeWrapper.ScrcpyControlCommand(_handle, command, command.Length);
            }
        }

        //init, dont lock
        internal bool RegisterClipboardEvent(NativeOnClipboardReceivedDelegate clipboardReceivedDelegate)
        {
            IntPtr pointer = Marshal.GetFunctionPointerForDelegate(clipboardReceivedDelegate);
            return NativeWrapper.RegisterClipboardEvent(_handle, pointer);
        }
        internal bool RegisterClipboardAcknowledgementEvent(NativeOnClipboardAcknowledgementDelegate clipboardAcknowledgementDelegate)
        {
            IntPtr pointer = Marshal.GetFunctionPointerForDelegate(clipboardAcknowledgementDelegate);
            return NativeWrapper.RegisterClipboardAcknowledgementEvent(_handle, pointer);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public bool Connect(ScrcpyConfig config = null)
        {
            lock (_lock)
            {
                CheckDispose();
                if (config == null) config = new ScrcpyConfig();
                string config_str = config.ToString();
                ScrcpyNativeConfig nativeConfig = config.NativeConfig();
                return NativeWrapper.ScrcpyConnect(_handle, config_str, ref nativeConfig);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        public void Stop()
        {
            lock (_lock)
            {
                CheckDispose();
                NativeWrapper.ScrcpyStop(_handle);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns><see cref="Bitmap"/></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public Bitmap GetScreenShot()
        {
            lock (_lock)
            {
                CheckDispose();
                Size size = GetScreenSize();
                Size fix_size = new Size(size.Width + size.Width % 16, size.Height);

                Bitmap bitmap = new Bitmap(fix_size.Width, fix_size.Height, PixelFormat.Format32bppArgb);
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, fix_size.Width, fix_size.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                //ARGB only
                NativeWrapper.ScrcpyGetScreenShot(
                    _handle,
                    bitmapData.Scan0,
                    fix_size.Width * fix_size.Height * 4,
                    size.Width,
                    size.Height,
                    fix_size.Width * 4);

                bitmap.UnlockBits(bitmapData);

                if (size.Width != fix_size.Width)
                {
                    Bitmap original_bitmap = bitmap.Clone(new Rectangle(0, 0, size.Width, size.Height), PixelFormat.Format32bppArgb);
                    bitmap.Dispose();
                    return original_bitmap;
                }
                else
                {
                    return bitmap;//blank
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ScrcpyUiView InitScrcpyUiView()
        {
            return new ScrcpyUiView(this);
        }

        internal bool D3DImageViewRender(IntPtr d3dView, IntPtr surface, bool isNewSurface)
        {
            lock (_lock)
            {
                return NativeWrapper.D3DImageViewRender(d3dView, this._handle, surface, isNewSurface);
            }
        }

        Size GetScreenSize()
        {
            int w = 0;
            int h = 0;
            NativeWrapper.ScrcpyGetScreenSize(_handle, ref w, ref h);
            return new Size(w, h);
        }

    }
}
