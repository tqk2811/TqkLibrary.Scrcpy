using System.Collections.Generic;

namespace TqkLibrary.Scrcpy.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IConfig
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetArguments();
    }
}
