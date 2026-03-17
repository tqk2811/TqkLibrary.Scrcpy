using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TqkLibrary.Scrcpy.Exceptions;

namespace TqkLibrary.Scrcpy
{
    /// <summary>
    /// 
    /// </summary>
    public class ScrcpyUiView : IDisposable
    {
        private readonly CountdownEvent countdownEvent = new CountdownEvent(1);

        IntPtr d3dView;

        /// <summary>
        /// 
        /// </summary>
        public Scrcpy Scrcpy { get; }

        /// <summary>
        /// 
        /// </summary>
        internal ScrcpyUiView(Scrcpy scrcpy)
        {
            this.Scrcpy = scrcpy ?? throw new ArgumentNullException(nameof(scrcpy));
            d3dView = NativeWrapper.D3DImageViewAlloc();
            if (d3dView == IntPtr.Zero)
                throw new ScrcpyException("D3DImageViewAlloc failed.");
        }
        /// <summary>
        /// 
        /// </summary>
        ~ScrcpyUiView()
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

        void Dispose(bool disposing)
        {
            countdownEvent.Signal();
            countdownEvent.Wait();
            if (d3dView != IntPtr.Zero)
            {
                NativeWrapper.D3DImageViewFree(d3dView);
                d3dView = IntPtr.Zero;
            }
            countdownEvent.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="isNewSurface"></param>
        /// <param name="isNewtargetView"></param>
        /// <returns></returns>
        public bool DoRender(IntPtr surface, bool isNewSurface, ref bool isNewtargetView)
        {
            bool result = false;
            if(countdownEvent.TryAddCount())
            {
                result = Scrcpy.D3DImageViewRender(d3dView, surface, isNewSurface, ref isNewtargetView);
                countdownEvent.Signal();
            }
            return result;
        }
    }
}
