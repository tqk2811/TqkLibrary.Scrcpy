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
    /// Device-side behaviour options applied while scrcpy is running (touches, wake, power).
    /// </summary>
    public class AndroidConfig : IConfig
    {
        /// <summary>
        /// Show physical touches on the device screen (the Android "Show taps" developer option) while
        /// mirroring.<br></br>
        /// Default: false
        /// </summary>
        [OptionName("show_touches")]
        public bool ShowTouches { get; set; } = false;
        /// <summary>
        /// Keep the device awake (prevent the screen from turning off) while mirroring; the original
        /// setting is restored when scrcpy exits.<br></br>
        /// Default: true
        /// </summary>
        [OptionName("stay_awake")]
        public bool StayAwake { get; set; } = true;
        /// <summary>
        /// Turn off the device screen when scrcpy exits.<br></br>
        /// default: false
        /// </summary>
        [OptionName("power_off_on_close")]
        public bool PowerOffOnClose { get; set; } = false;
        /// <summary>
        /// Turn on the device screen when scrcpy starts.<br></br>
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
