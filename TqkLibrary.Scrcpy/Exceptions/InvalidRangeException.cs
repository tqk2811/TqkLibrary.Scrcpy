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
    public class InvalidRangeException : ScrcpyException
    {
        /// <summary>
        /// 
        /// </summary>
        public InvalidRangeException() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public InvalidRangeException(string message) : base(message) { }
    }
}
