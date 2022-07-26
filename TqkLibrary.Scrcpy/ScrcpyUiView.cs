using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.Scrcpy
{
    /// <summary>
    /// 
    /// </summary>
    public class ScrcpyUiView : IDisposable
    {
        readonly IntPtr d3dView;

        /// <summary>
        /// 
        /// </summary>
        public Scrcpy Scrcpy { get; }


        bool isDisposed = false;
        /// <summary>
        /// 
        /// </summary>
        internal ScrcpyUiView(Scrcpy scrcpy)
        {
            this.Scrcpy = scrcpy ?? throw new ArgumentNullException(nameof(scrcpy));
            d3dView = NativeWrapper.D3DImageViewAlloc();
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
            if (isDisposed) return;
            NativeWrapper.D3DImageViewFree(d3dView);
            isDisposed = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="isNewSurface"></param>
        /// <returns></returns>
        public bool DoRender(IntPtr surface, bool isNewSurface)
        {
            return Scrcpy.D3DImageViewRender(d3dView, surface, isNewSurface);
        }
    }
}
