using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.Scrcpy.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class ScrcpyException : Exception
    {
        internal ScrcpyException() { }
        internal ScrcpyException(string message) : base(message) { }
    }
}
