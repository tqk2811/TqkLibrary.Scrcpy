using System.Collections.Generic;
using TqkLibrary.Scrcpy.Attributes;
using TqkLibrary.Scrcpy.Configs;
using TqkLibrary.Scrcpy.Interfaces;

namespace TqkLibrary.Scrcpy.ListSupport
{
    /// <summary>
    ///
    /// </summary>
    public class ListSupportQuery : IConfig
    {
        /// <summary>
        /// Where adb is, which scrcpy server jar to deploy and where it lives on the device.<br></br>
        /// Pass the same instance as <see cref="ScrcpyConfig.DeployConfig"/> so the jar this query runs
        /// is the very one <see cref="Scrcpy.Connect(ScrcpyConfig?)"/> launches, instead of a second
        /// copy left behind at another path on the device.
        /// </summary>
        public ScrcpyDeployConfig DeployConfig { get; set; } = new ScrcpyDeployConfig();


        /// <summary>
        /// print list Encoders support to adb shell output<br></br>
        /// default: false
        /// </summary>
        [OptionName("list_encoders")]
        public bool ListEncoders { get; set; } = false;

        /// <summary>
        /// print list Displays to adb shell output<br></br>
        /// default: false
        /// </summary>
        [OptionName("list_displays")]
        public bool ListDisplays { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        [OptionName("list_cameras")]
        public bool ListCameras { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        [OptionName("list_camera_sizes")]
        public bool ListCameraSizes { get; set; } = false;




        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetArguments()
        {
            yield return Constant.ScrcpyServerVersion;
            yield return this._GetArgument(x => x.ListEncoders, ListEncoders);
            yield return this._GetArgument(x => x.ListDisplays, ListDisplays);
            yield return this._GetArgument(x => x.ListCameras, ListCameras);
            yield return this._GetArgument(x => x.ListCameraSizes, ListCameraSizes);
        }
    }
}
