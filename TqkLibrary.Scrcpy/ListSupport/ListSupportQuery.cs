using System.Collections.Generic;
using TqkLibrary.Scrcpy.Attributes;
using TqkLibrary.Scrcpy.Interfaces;

namespace TqkLibrary.Scrcpy.ListSupport
{
    /// <summary>
    /// 
    /// </summary>
    public class ListSupportQuery : IConfig
    {
        /// <summary>
        /// 
        /// </summary>
        public string AdbPath { get; set; } = "adb.exe";
        /// <summary>
        /// 
        /// </summary>
        public string ScrcpyPath { get; set; } = "scrcpy-server.jar";


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
        /// print the list of installed apps to adb shell output (scrcpy 3.0+)<br></br>
        /// default: false
        /// </summary>
        [OptionName("list_apps")]
        public bool ListApps { get; set; } = false;




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
            yield return this._GetArgument(x => x.ListApps, ListApps);
        }
    }
}
