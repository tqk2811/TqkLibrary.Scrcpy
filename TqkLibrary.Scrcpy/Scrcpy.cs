using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.Scrcpy.Control;

namespace TqkLibrary.Scrcpy
{
    /// <summary>
    /// 
    /// </summary>
    public class Scrcpy : IDisposable
    {
        internal IntPtr Handle { get; private set; } = IntPtr.Zero;

        /// <summary>
        /// 
        /// </summary>
        public Size ScreenSize
        {
            get
            {
                int w = 0;
                int h = 0;
                NativeWrapper.ScrcpyGetScreenSize(Handle, ref w, ref h);
                return new Size(w, h);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IControl Control { get; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceId"></param>
        public Scrcpy(string deviceId)
        {
            Handle = NativeWrapper.ScrcpyAlloc(deviceId);
            Control = new ScrcpyControl(this);
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
        /// <param name="config"></param>
        public void Connect(ScrcpyConfig config = null)
        {
            if (config == null) config = new ScrcpyConfig();
            string config_str = config.ToString();
            ScrcpyNativeConfig nativeConfig = config.NativeConfig();
            NativeWrapper.ScrcpyConnect(Handle, config_str, ref nativeConfig);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            NativeWrapper.ScrcpyStop(Handle);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Bitmap GetScreenShot()
        {
            Size size = ScreenSize;
            Bitmap bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, size.Width, size.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

            NativeWrapper.ScrcpyGetScreenShot(
                Handle,
                bitmapData.Scan0,
                size.Width * size.Height * 4,
                size.Width,
                size.Height,
                bitmapData.Stride);//ARGB only

            bitmap.UnlockBits(bitmapData);
            return bitmap;//blank
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
            if (Handle != IntPtr.Zero)
            {
                NativeWrapper.ScrcpyFree(Handle);
                Handle = IntPtr.Zero;
            }
        }
    }
}
