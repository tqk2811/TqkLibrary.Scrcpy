using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.Scrcpy.Attributes;
using TqkLibrary.Scrcpy.Enums;
using TqkLibrary.Scrcpy.Interfaces;

namespace TqkLibrary.Scrcpy.Configs
{
    /// <summary>
    /// Camera capture configuration, used when <see cref="ScrcpyServerConfig.VideoSource"/> is
    /// <see cref="VideoSource.Camera"/>.
    /// </summary>
    public class CameraConfig : IConfig
    {
        /// <summary>
        /// Id of the camera to capture from (as reported in the device's camera list).<br></br>
        /// default: null
        /// </summary>
        [OptionName("camera_id")]
        public int CameraId { get; set; }
        /// <summary>
        /// Capture resolution; null lets the device choose a default size.<br></br>
        /// default: null
        /// </summary>
        [OptionName("camera_size")]
        public Size? CameraSize { get; set; }
        /// <summary>
        /// Select the camera by which way it faces (front / back / external).<br></br>
        /// default: <see cref="CameraFacing.Any"/>
        /// </summary>
        [OptionName("camera_facing")]
        public CameraFacing CameraFacing { get; set; } = CameraFacing.Any;
        /// <summary>
        /// Capture aspect ratio, e.g. "16:9", "4:3" or a float like "1.6"; null keeps the sensor default.<br></br>
        /// default: null
        /// </summary>
        [OptionName("camera_ar")]
        public string? CameraAr { get; set; } = null;
        /// <summary>
        /// Camera capture frame rate; 0 uses the device default.<br></br>
        /// default: 0
        /// </summary>
        [OptionName("camera_fps")]
        public int Camerafps { get; set; } = 0;
        /// <summary>
        /// Enable high-speed capture mode (higher fps, limited to specific resolutions/features).<br></br>
        /// default: false
        /// </summary>
        [OptionName("camera_high_speed")]
        public bool CameraHighSpeed { get; set; } = false;

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetArguments()
        {
            yield return this._GetArgument(x => x.CameraId);
            yield return this._GetArgument(x => x.CameraSize);
            yield return this._GetArgument(x => x.CameraFacing, x => x.ToString().ToLower());
            yield return this._GetArgument(x => x.CameraAr, string.IsNullOrWhiteSpace);
            yield return this._GetArgument(x => x.Camerafps, x => x > 0);
            yield return this._GetArgument(x => x.CameraHighSpeed, CameraHighSpeed);
        }
    }
}
