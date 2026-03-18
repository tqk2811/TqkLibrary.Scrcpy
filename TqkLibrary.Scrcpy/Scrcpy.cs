using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TqkLibrary.Scrcpy.Control;
using System.Threading;
using TqkLibrary.Scrcpy.Interfaces;
using TqkLibrary.Scrcpy.Configs;
using TqkLibrary.Scrcpy.ListSupport;
using TqkLibrary.Scrcpy.Helpers;
using TqkLibrary.Scrcpy.Enums;
using System.IO;

namespace TqkLibrary.Scrcpy
{
    /// <summary>
    /// 
    /// </summary>
    public class Scrcpy : IDisposable
    {
        private readonly CountdownEvent countdownEvent = new CountdownEvent(1);
        private IntPtr _handle = IntPtr.Zero;
        internal IntPtr Handle { get { return _handle; } }

        /// <summary>
        /// 
        /// </summary>
        public string DeviceId { get; }

        /// <summary>
        /// 
        /// </summary>
        public string DeviceName => _deviceName;

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
        public bool IsClipboardAutoSync { get; private set; } = false;
        /// <summary>
        /// 
        /// </summary>

        public event Action<ScrcpyDisconnectSource>? OnDisconnect;



        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceId"></param>
        public Scrcpy(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId)) throw new ArgumentNullException(nameof(deviceId));
            this.DeviceId = deviceId;

            _handle = NativeWrapper.ScrcpyAlloc();

            Control = new ScrcpyControl(this);
            Control.OnClipboardReceived += Control_OnClipboardReceived;

            this.NativeOnDisconnectDelegate = onDisconnect;
            if (!this.RegisterDisconnectEvent(this.NativeOnDisconnectDelegate))
                throw new InvalidOperationException();

            this._uhdiOutputDelegate = UhdiOutputCallback;
            if (!this.RegisterUhdiOutputEvent(this._uhdiOutputDelegate))
                throw new InvalidOperationException();

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

        private void TerminationHandler(object? sender, EventArgs e)
        {
            //fix crash on forgot dispose
            //when native disconnect, it will callback to NativeOnDisconnectDelegate. But domain was unload -> crash
            this.Dispose();
        }

        int _isDisposed = 0;
        private void Dispose(bool disposing)
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0) return;

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
        /// <param name="config"></param>
        public bool Connect(ScrcpyConfig? config = null)
        {
            bool result = false;
            if (countdownEvent.TryAddCount())
            {
                if (config == null) config = new ScrcpyConfig();
                _adbPath = config.AdbPath;
                _physicalScreenSizeCache = null;
                ScrcpyNativeConfig nativeConfig = config.NativeConfig();
                result = ConnectInternal(config, ref nativeConfig);
                this.IsClipboardAutoSync = config.ServerConfig?.ClipboardAutosync ?? false;
                countdownEvent.Signal();
            }
            return result;
        }

        private bool ConnectInternal(ScrcpyConfig config, ref ScrcpyNativeConfig nativeConfig)
        {
            string scidPrefix = "localabstract:scrcpy";
            int scid = config.ServerConfig?.SCID ?? -1;
            if (scid != -1)
                scidPrefix += $"_{(scid & 0x7FFFFFFF):x}";

            bool isVideo = nativeConfig.IsVideo;
            bool isAudio = nativeConfig.IsAudio;
            bool isControl = nativeConfig.IsControl;
            int backlog = (isVideo ? 1 : 0) + (isAudio ? 1 : 0) + (isControl ? 1 : 0);

            // Create TCP listener on a free port chosen by the OS
            var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
            listener.Start(backlog);
            int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;

            // adb setup
            RunAdbSync(config.AdbPath, $"-s {DeviceId} reverse --remove {scidPrefix}");
            if (RunAdbSync(config.AdbPath, $"-s {DeviceId} push \"{config.ScrcpyServerPath}\" /sdcard/scrcpy-server-tqk.jar") != 0)
                return false;
            if (RunAdbSync(config.AdbPath, $"-s {DeviceId} reverse {scidPrefix} tcp:{port}") != 0)
                return false;

            // Start scrcpy server process
            Process? serverProcess = StartAdbProcess(config.AdbPath,
                $"-s {DeviceId} shell CLASSPATH=/sdcard/scrcpy-server-tqk.jar app_process / com.genymobile.scrcpy.Server {config}");
            if (serverProcess is null)
                return false;

            // Accept connections in order: video → audio → control
            listener.Server.ReceiveTimeout = nativeConfig.ConnectionTimeout;
            System.Net.Sockets.Socket? videoSock = null, audioSock = null, controlSock = null;
            System.Net.Sockets.Socket? firstSock = null;
            try
            {
                if (isVideo) { videoSock = listener.AcceptSocket(); firstSock = videoSock; }
                if (isAudio) { audioSock = listener.AcceptSocket(); firstSock ??= audioSock; }
                if (isControl) { controlSock = listener.AcceptSocket(); firstSock ??= controlSock; }
            }
            catch
            {
                videoSock?.Close(); audioSock?.Close(); controlSock?.Close();
                try { serverProcess.Kill(); } catch { } serverProcess.Dispose();
                return false;
            }

            // Read 64-byte device name from first socket
            try
            {
                byte[] nameBytes = new byte[64];
                int total = 0;
                while (total < 64)
                {
                    int r = firstSock!.Receive(nameBytes, total, 64 - total, System.Net.Sockets.SocketFlags.None);
                    if (r <= 0) throw new Exception("Connection closed while reading device name");
                    total += r;
                }
                int len = Array.IndexOf(nameBytes, (byte)0);
                _deviceName = Encoding.ASCII.GetString(nameBytes, 0, len < 0 ? nameBytes.Length : len);
            }
            catch
            {
                videoSock?.Close(); audioSock?.Close(); controlSock?.Close();
                try { serverProcess.Kill(); } catch { } serverProcess.Dispose();
                return false;
            }

            IntPtr videoHandle = GetSocketHandle(videoSock);
            IntPtr audioHandle = GetSocketHandle(audioSock);
            IntPtr controlHandle = GetSocketHandle(controlSock);

            bool connected = NativeWrapper.ScrcpyConnect(_handle, ref nativeConfig, videoHandle, audioHandle, controlHandle);
            if (connected)
            {
                _serverProcess = serverProcess;
                // C# owns the sockets; closed in Stop() after ScrcpyStop shuts them down
                _activeSockets = new[] { videoSock, audioSock, controlSock };
            }
            else
            {
                videoSock?.Close(); audioSock?.Close(); controlSock?.Close();
                try { serverProcess.Kill(); } catch { } serverProcess.Dispose();
            }
            return connected;
        }

        private static IntPtr GetSocketHandle(System.Net.Sockets.Socket? socket)
        {
            if (socket is null) return new IntPtr(-1); // INVALID_SOCKET
            socket.ReceiveTimeout = 0;
            return socket.Handle; // C# retains ownership; C++ reads only
        }

        private static int RunAdbSync(string adbPath, string arguments)
        {
            try
            {
                using var p = new Process();
                p.StartInfo = new ProcessStartInfo(adbPath, arguments)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                p.Start();
                p.WaitForExit();
                return p.ExitCode;
            }
            catch { return -1; }
        }

        private static Process? StartAdbProcess(string adbPath, string arguments)
        {
            try
            {
                var p = new Process();
                p.StartInfo = new ProcessStartInfo(adbPath, arguments)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                p.Start();
                return p;
            }
            catch { return null; }
        }

        private string _adbPath = "adb.exe";
        private string _deviceName = string.Empty;
        private Process? _serverProcess;
        private System.Net.Sockets.Socket?[]? _activeSockets;
        private readonly object _physicalScreenSizeLock = new();
        private Size? _physicalScreenSizeCache;

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        public void Stop()
        {
            if (countdownEvent.SafeTryAddCount())
            {
                NativeWrapper.ScrcpyStop(_handle); // calls shutdown() on all sockets → threads exit
                var sockets = Interlocked.Exchange(ref _activeSockets, null);
                if (sockets != null)
                    foreach (var s in sockets) s?.Close();
                var proc = Interlocked.Exchange(ref _serverProcess, null);
                if (proc != null)
                {
                    try { proc.Kill(); } catch { }
                    proc.Dispose();
                }
                countdownEvent.Signal();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="swsFlag"></param>
        /// <returns><see cref="Bitmap"/></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public Bitmap? GetScreenShot(SwsFlag swsFlag = SwsFlag.SWS_FAST_BILINEAR)
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
                        fix_size.Width * 4,
                        swsFlag
                        );

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

        /// <summary>
        /// Returns a read-only <see cref="ScrcpyAudioStream"/> that delivers decoded audio
        /// resampled to the specified format, sample rate, and channel count.
        /// </summary>
        /// <param name="format">Output sample format (default: <see cref="AVSampleFormat.S16"/>).</param>
        /// <param name="sampleRate">Output sample rate in Hz (default: 48000).</param>
        /// <param name="channels">Number of output channels (default: 2).</param>
        public ScrcpyAudioStream GetAudioStream(
            AVSampleFormat format = AVSampleFormat.S16,
            int sampleRate = 48000,
            int channels = 2)
        {
            return new ScrcpyAudioStream(this, format, sampleRate, channels);
        }


        readonly NativeOnDisconnectDelegate NativeOnDisconnectDelegate;
        void onDisconnect(ScrcpyDisconnectSource source)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                Stop();
                OnDisconnect?.Invoke(source);
            });
        }

        private void Control_OnClipboardReceived(IControl control, string data)
        {
            this.LastClipboard = data;
        }


        #region Function Control
        internal bool SendControl(byte[] command)
        {
            bool result = false;

            if (countdownEvent.TryAddCount())
            {
                result = NativeWrapper.ScrcpyControlCommand(_handle, command, command.Length);
                countdownEvent.Signal();
            }

            return result;
        }
        #endregion


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
            if (w == -1 || h == -1)
                return _physicalScreenSizeCache ?? Size.Empty;
            return new Size(w, h);
        }

        /// <summary>
        /// Force re-query screen size from ADB and update the cache.<br/>
        /// Only use this when <c>IsVideo = false</c> (control-only mode),
        /// for example after a screen rotation event.
        /// </summary>
        /// <returns>The updated screen size, or <see cref="Size.Empty"/> if the query failed.</returns>
        public async Task<Size> RefreshScreenSizeFromAdbAsync(CancellationToken cancellationToken = default)
        {
            _physicalScreenSizeCache = await QueryScreenSizeViaAdbAsync(cancellationToken).ConfigureAwait(false);
            return _physicalScreenSizeCache ?? Size.Empty;
        }

        async Task<Size?> QueryScreenSizeViaAdbAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var p = new Process();
                p.StartInfo = new ProcessStartInfo(_adbPath, $"-s {DeviceId} shell wm size")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                p.Start();
                string output = await p.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
                await Task.Run(() => p.WaitForExit(5000), cancellationToken).ConfigureAwait(false);
                // Parse "Physical size: WxH" — last match wins (handles Override size)
                var matches = Regex.Matches(output, @"(\d+)x(\d+)");
                if (matches.Count > 0)
                {
                    var m = matches[matches.Count - 1];
                    return new Size(int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value));
                }
            }
            catch { }
            return null;
        }



        readonly NativeUhdiOutputDelegate _uhdiOutputDelegate;
        void UhdiOutputCallback(UInt16 id, UInt16 size, IntPtr buff)
        {
            byte[] clone_buff = new byte[size];
            Marshal.Copy(buff, clone_buff, 0, size);
            ThreadPool.QueueUserWorkItem((o) =>
            {

            }, null);
        }

    }
}
