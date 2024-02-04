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
using TqkLibrary.Scrcpy.Interfaces;
using TqkLibrary.Scrcpy.Configs;
using TqkLibrary.Scrcpy.ListSupport;
using TqkLibrary.Scrcpy.Helpers;

namespace TqkLibrary.Scrcpy
{
    internal delegate void NativeOnDisconnectDelegate();
    /// <summary>
    /// 
    /// </summary>
    public class Scrcpy : IDisposable
    {
        private readonly CountdownEvent countdownEvent = new CountdownEvent(1);



        private IntPtr _handle = IntPtr.Zero;
        /// <summary>
        /// 
        /// </summary>
        public string DeviceId { get; }

        /// <summary>
        /// 
        /// </summary>
        public string DeviceName
        {
            get { return GetDeviceName(); }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsConnected
        {
            get
            {
                bool result = false;
                if (countdownEvent.TryAddCount())
                {
                    result = NativeWrapper.IsHaveScrcpyInstance(_handle);
                    countdownEvent.Signal();
                }
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceId"></param>
        public Scrcpy(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId)) throw new ArgumentNullException(nameof(deviceId));
            this.DeviceId = deviceId;

            _handle = NativeWrapper.ScrcpyAlloc(deviceId);

            Control = new ScrcpyControl(this);
            Control.OnClipboardReceived += Control_OnClipboardReceived;

            this.NativeOnDisconnectDelegate = onDisconnect;
            IntPtr pointer = Marshal.GetFunctionPointerForDelegate(this.NativeOnDisconnectDelegate);
            NativeWrapper.RegisterDisconnectEvent(_handle, pointer);

            if (AppDomain.CurrentDomain.IsDefaultAppDomain())
                AppDomain.CurrentDomain.ProcessExit += TerminationHandler;
            else
                AppDomain.CurrentDomain.DomainUnload += TerminationHandler;//DomainUnload not fire on DefaultAppDomain
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

        private void TerminationHandler(object sender, EventArgs e)
        {
            //fix crash on forgot dispose
            //when native disconnect, it will callback to NativeOnDisconnectDelegate. But domain was unload -> crash
            this.Dispose();
        }

        bool isDisposed = false;
        private void Dispose(bool disposing)
        {
            if (!isDisposed) return;
            isDisposed = true;

            if (AppDomain.CurrentDomain.IsDefaultAppDomain())
                AppDomain.CurrentDomain.ProcessExit -= TerminationHandler;
            else
                AppDomain.CurrentDomain.DomainUnload -= TerminationHandler;

            Stop();
            countdownEvent.Signal();
            countdownEvent.Wait();//TryAddCount will false if wait success
            if (_handle != IntPtr.Zero)
            {
                NativeWrapper.ScrcpyFree(_handle);
                _handle = IntPtr.Zero;
            }
            countdownEvent.Dispose();//TryAddCount will throw ObjectDisposeException
        }

        /// <summary>
        /// 
        /// </summary>
        public Size ScreenSize
        {
            get
            {
                return GetScreenSize();
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
            ThreadPool.QueueUserWorkItem(o =>
            {
                Stop();
                OnDisconnect?.Invoke();
            });
        }

        private void Control_OnClipboardReceived(IControl control, string data)
        {
            this.LastClipboard = data;
        }

        #region Function Control
        internal bool SendControl(ScrcpyControlMessage scrcpyControlMessage)
        {
            bool result = false;

            if (countdownEvent.TryAddCount())
            {
                byte[] command = scrcpyControlMessage.GetCommand();
                result = NativeWrapper.ScrcpyControlCommand(_handle, command, command.Length);
                countdownEvent.Signal();
            }

            return result;
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
            bool result = false;
            if (countdownEvent.TryAddCount())
            {
                if (config == null) config = new ScrcpyConfig();
                ScrcpyNativeConfig nativeConfig = config.NativeConfig();
                result = NativeWrapper.ScrcpyConnect(_handle, ref nativeConfig);
                countdownEvent.Signal();
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        public void Stop()
        {
            if (countdownEvent.SafeTryAddCount())
            {
                NativeWrapper.ScrcpyStop(_handle);
                countdownEvent.Signal();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns><see cref="Bitmap"/></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public Bitmap GetScreenShot()
        {
            if (countdownEvent.TryAddCount())
            {
                try
                {
                    Size size = GetScreenSize();
                    if (size.Width <= 0 || size.Height <= 0) return null;

                    int width = size.Width % 16 == 0 ? size.Width : size.Width + 16 - (size.Width % 16);
                    Size fix_size = new Size(width, size.Height);

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
                finally
                {
                    countdownEvent.Signal();
                }
            }
            return null;
        }

        /// <summary>
        /// Work only when enable <see cref="ScrcpyConfig.IsUseD3D11ForUiRender"/>
        /// </summary>
        /// <returns></returns>
        public ScrcpyUiView InitScrcpyUiView()
        {
            return new ScrcpyUiView(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listSupportQuery"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ScrcpyServerListSupport> ListSupportAsync(
            ListSupportQuery listSupportQuery,
            CancellationToken cancellationToken = default)
        {
            if (listSupportQuery is null) throw new ArgumentNullException(nameof(listSupportQuery));

            await AdbHelper.PushServerAsync(listSupportQuery.AdbPath, DeviceId, listSupportQuery.ScrcpyPath, cancellationToken);

            string q = string.Join(" ", listSupportQuery.GetArguments().Where(x => !string.IsNullOrWhiteSpace(x)));
            var result = await AdbHelper.RunServerWithAdbAsync(
                listSupportQuery.AdbPath,
                DeviceId,
                $"shell CLASSPATH=/sdcard/scrcpy-server-tqk.jar app_process / com.genymobile.scrcpy.Server {q}",
                cancellationToken
                );

            if (!string.IsNullOrWhiteSpace(result.StdErr))
                throw new Exception(result.StdErr);

            return ScrcpyServerListSupport.Parse(result.StdOut);
        }

        internal bool D3DImageViewRender(IntPtr d3dView, IntPtr surface, bool isNewSurface, ref bool isNewtargetView)
        {
            bool result = false;
            if (countdownEvent.SafeTryAddCount())//must safe because this is call from ui thread
            {
                result = NativeWrapper.D3DImageViewRender(d3dView, this._handle, surface, isNewSurface, ref isNewtargetView);
                countdownEvent.Signal();
            }
            return result;
        }

        Size GetScreenSize()
        {
            int w = 0;
            int h = 0;
            if (countdownEvent.TryAddCount())
            {
                NativeWrapper.ScrcpyGetScreenSize(_handle, ref w, ref h);
                countdownEvent.Signal();
            }
            return new Size(w, h);
        }

        string GetDeviceName()
        {
            byte[] buffer = new byte[64];
            if (countdownEvent.TryAddCount())
            {
                NativeWrapper.ScrcpyGetDeviceName(_handle, buffer, 64);
                countdownEvent.Signal();
            }
            return Encoding.ASCII.GetString(buffer);
        }
    }
}
