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
        /// <summary>
        /// 
        /// </summary>
        internal ScrcpyUiView(Scrcpy scrcpy, IntPtr viewHandle)
        {

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
        }

        void Dispose(bool disposing)
        {

        }


        public bool DoRender(IntPtr surface, bool isNewSurface)
        {
            return false;
        }
    }
}
