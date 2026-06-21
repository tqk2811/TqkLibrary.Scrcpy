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
    /// 
    /// </summary>
    public class CameraConfig : IConfig
    {
        /// <summary>
        /// default: null
        /// </summary>
        [OptionName("camera_id")]
        public int CameraId { get; set; }
        /// <summary>
        /// default: null
        /// </summary>
        [OptionName("camera_size")]
        public Size? CameraSize { get; set; }
        /// <summary>
        /// default: <see cref="CameraFacing.Any"/>
        /// </summary>
        [OptionName("camera_facing")]
        public CameraFacing CameraFacing { get; set; } = CameraFacing.Any;
        /// <summary>
        /// default: null
        /// </summary>
        [OptionName("camera_ar")]
        public string? CameraAr { get; set; } = null;
        /// <summary>
        /// default: 0
        /// </summary>
        [OptionName("camera_fps")]
        public int Camerafps { get; set; } = 0;
        /// <summary>
        /// default: false
        /// </summary>
        [OptionName("camera_high_speed")]
        public bool CameraHighSpeed { get; set; } = false;

        /// <summary>
        /// Set the camera zoom (a multiplicative factor, e.g. 2.0 for 2x).<br></br>
        /// default: 1 (omit)<br></br>
        /// scrcpy 4.0 (--camera-zoom)
        /// </summary>
        public float CameraZoom { get; set; } = 1;

        /// <summary>
        /// Turn the camera torch (flashlight) on.<br></br>
        /// default: false<br></br>
        /// scrcpy 4.0 (--camera-torch)
        /// </summary>
        [OptionName("camera_torch")]
        public bool CameraTorch { get; set; } = false;

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
            yield return this._GetArgument(x => x.CameraTorch, CameraTorch);
            // camera_zoom is a float (not supported by _GetArgument); emit manually when non-default.
            if (CameraZoom != 1)
                yield return $"camera_zoom={CameraZoom.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
        }
    }
}
