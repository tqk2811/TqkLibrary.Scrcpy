using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.Scrcpy.Attributes
{
    /// <summary>
    /// 
    /// </summary>
    public class OptionNameAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public OptionNameAttribute(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; }
    }
}
