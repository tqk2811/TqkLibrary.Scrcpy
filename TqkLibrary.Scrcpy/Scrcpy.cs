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

        internal IntPtr Handle { get; private set; } = IntPtr.Zero;


        


        
        /// <summary>
        /// 
        /// </summary>
        public Size ScreenSize
        {
            get
            {
                if (Handle == IntPtr.Zero) throw new ObjectDisposedException(nameof(Scrcpy));
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
        /// <param name="config"></param>
        public bool Connect(ScrcpyConfig config = null)
        {
            if (config == null) config = new ScrcpyConfig();
            string config_str = config.ToString();
            ScrcpyNativeConfig nativeConfig = config.NativeConfig();
            return NativeWrapper.ScrcpyConnect(Handle, config_str, ref nativeConfig);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        public void Stop()
        {
            if (Handle == IntPtr.Zero) throw new ObjectDisposedException(nameof(Scrcpy));
            NativeWrapper.ScrcpyStop(Handle);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns><see cref="Bitmap"/></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public Bitmap GetScreenShot()
        {
            if (Handle == IntPtr.Zero) throw new ObjectDisposedException(nameof(Scrcpy));
            
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



    }
}
