using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.Scrcpy.Attributes;
using TqkLibrary.Scrcpy.Interfaces;

namespace TqkLibrary.Scrcpy.Configs
{
    /// <summary>
    /// 
    /// </summary>
    public class AndroidConfig : IConfig
    {
        /// <summary>
        /// Default: false
        /// </summary>
        [OptionName("show_touches")]
        public bool ShowTouches { get; set; } = false;
        /// <summary>
        /// Default: true
        /// </summary>
        [OptionName("stay_awake")]
        public bool StayAwake { get; set; } = true;
        /// <summary>
        /// Turn off screen when scrcpy exit<br></br>
        /// default: false
        /// </summary>
        [OptionName("power_off_on_close")]
        public bool PowerOffOnClose { get; set; } = false;
        /// <summary>
        /// Turn on screen when scrcpy start<br></br>
        /// default: true
        /// </summary>
        [OptionName("power_on")]
        public bool PowerOn { get; set; } = true;


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetArguments()
        {
            yield return this._GetArgument(x => x.ShowTouches, ShowTouches);
            yield return this._GetArgument(x => x.StayAwake, StayAwake);
            yield return this._GetArgument(x => x.PowerOffOnClose, PowerOffOnClose);
            yield return this._GetArgument(x => x.PowerOn, !PowerOn);
        }
    }
}
