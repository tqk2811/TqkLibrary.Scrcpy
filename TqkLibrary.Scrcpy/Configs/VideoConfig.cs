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
    public class VideoConfig : IConfig
    {
        /// <summary>
        /// Index of screen android<br></br>
        /// Default: null
        /// </summary>
        [OptionName("display_id")]
        public int? DisplayId { get; set; }
        /// <summary>
        /// Default: <see cref="Orientations.Auto"/>
        /// </summary>
        [OptionName("lock_video_orientation")]
        public Orientations Orientation { get; set; } = Orientations.Auto;
        /// <summary>
        /// Default: 0 or null (unlimit)
        /// </summary>
        [OptionName("max_fps")]
        public int MaxFps { get; set; } = 0;
        /// <summary>
        /// Max bitrate of video stream<br></br>
        /// Default: 0 or null (ignore)
        /// </summary>
        [OptionName("video_bit_rate")]
        public int VideoBitrate { get; set; }
        /// <summary>
        /// VideoCodec<br></br>
        /// Default: null (ignore)<br></br>
        /// Support: h264, h265, av1, opus, aac, raw
        /// </summary>
        [OptionName("video_codec")]
        public string VideoCodec { get; set; }
        /// <summary>
        /// VideoCodecOption<br></br>
        /// Default: null (ignore)
        /// </summary>
        [OptionName("video_codec_options")]
        public string VideoCodecOption { get; set; }
        /// <summary>
        /// VideoEncoder<br></br>
        /// Default: null (ignore)
        /// </summary>
        [OptionName("video_encoder")]
        public string VideoEncoder { get; set; }
        /// <summary>
        /// Crop a region in base android screen<br></br>
        /// Default: null
        /// </summary>
        [OptionName("crop")]
        public Rectangle? Crop { get; set; } = null;
        //https://github.com/Genymobile/scrcpy/blob/21df2c240e544b1c1eba7775e1474c1c772be04b/server/src/main/java/com/genymobile/scrcpy/ScreenEncoder.java#L132
        /// <summary>
        /// Downsizing on error is only enabled if an encoding failure occurs before the first frame (downsizing later could be surprising)<br></br>
        /// default: true
        /// </summary>
        [OptionName("downsize_on_error")]
        public bool DownsizeOnError { get; set; } = true;
        /// <summary>
        /// print list Displays to adb shell output<br></br>
        /// default: false
        /// </summary>
        [OptionName("list_displays")]
        public bool ListDisplays { get; } = false;


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetArguments()
        {
            yield return this._GetArgument(x => x.DisplayId, x => x.HasValue);
            yield return this._GetArgument(x => x.Orientation, x => x != Orientations.Auto);
            yield return this._GetArgument(x => x.MaxFps, x => x > 0);
            yield return this._GetArgument(x => x.VideoBitrate, x => x > 0);
            yield return this._GetArgument(x => x.VideoCodec, string.IsNullOrWhiteSpace);
            yield return this._GetArgument(x => x.VideoCodecOption, string.IsNullOrWhiteSpace);
            yield return this._GetArgument(x => x.VideoEncoder, string.IsNullOrWhiteSpace);
            yield return this._GetArgument(x => x.Crop, x => x.HasValue);
            yield return this._GetArgument(x => x.DownsizeOnError, !DownsizeOnError);
            yield return this._GetArgument(x => x.ListDisplays, ListDisplays);
        }
    }
}
