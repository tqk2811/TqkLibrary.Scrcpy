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
        /// Capture orientation (scrcpy 3.0).<br></br>
        /// Combined with <see cref="CaptureOrientationLock"/> to produce the capture_orientation option.<br></br>
        /// Default: null (omit — server default)
        /// </summary>
        public CaptureOrientations? CaptureOrientation { get; set; }

        /// <summary>
        /// Lock mode for <see cref="CaptureOrientation"/>.<br></br>
        /// Default: <see cref="CaptureOrientationLock.Unlocked"/>
        /// </summary>
        public CaptureOrientationLock CaptureOrientationLock { get; set; } = CaptureOrientationLock.Unlocked;

        /// <summary>
        /// Default: 0 or null (unlimit)
        /// </summary>
        [OptionName("max_fps")]
        public float MaxFps { get; set; } = 0;

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
        public string? VideoCodec { get; set; }

        /// <summary>
        /// VideoCodecOption<br></br>
        /// Default: null (ignore)
        /// </summary>
        [OptionName("video_codec_options")]
        public string? VideoCodecOption { get; set; }

        /// <summary>
        /// VideoEncoder<br></br>
        /// Default: null (ignore)
        /// </summary>
        [OptionName("video_encoder")]
        public string? VideoEncoder { get; set; }

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
        /// Apply a rotation (in degrees, can be a float) to the video.<br></br>
        /// Default: null (omit)
        /// </summary>
        public float? Angle { get; set; }

        /// <summary>
        /// Round the video dimensions down to a multiple of this value (must be 1, 2, 4, 8 or 16).<br></br>
        /// Default: 1 (omit)<br></br>
        /// scrcpy 4.0 (--max-size alignment)
        /// </summary>
        [OptionName("min_size_alignment")]
        public int MinSizeAlignment { get; set; } = 1;

        /// <summary>
        /// Allow the virtual display resolution to change at runtime (flexible display).<br></br>
        /// Default: false (omit)<br></br>
        /// scrcpy 4.0 (--new-display flexible)
        /// </summary>
        [OptionName("flex_display")]
        public bool FlexDisplay { get; set; } = false;


        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetArguments()
        {
            yield return this._GetArgument(x => x.DisplayId, x => x.HasValue);
            yield return this._GetArgument(x => x.MaxFps, x => x > 0);
            yield return this._GetArgument(x => x.VideoBitrate, x => x > 0);
            yield return this._GetArgument(x => x.VideoCodec, string.IsNullOrWhiteSpace);
            yield return this._GetArgument(x => x.VideoCodecOption, string.IsNullOrWhiteSpace);
            yield return this._GetArgument(x => x.VideoEncoder, string.IsNullOrWhiteSpace);
            yield return this._GetArgument(x => x.Crop, x => x.HasValue);
            yield return this._GetArgument(x => x.DownsizeOnError, !DownsizeOnError);
            yield return this._GetArgument(x => x.MinSizeAlignment, x => x > 1);
            yield return this._GetArgument(x => x.FlexDisplay, FlexDisplay);

            // capture_orientation: [[@]<value>|@]
            if (CaptureOrientation.HasValue || CaptureOrientationLock == CaptureOrientationLock.LockedInitial)
            {
                var sb = new StringBuilder();
                if (CaptureOrientationLock == CaptureOrientationLock.LockedValue ||
                    CaptureOrientationLock == CaptureOrientationLock.LockedInitial)
                    sb.Append('@');
                if (CaptureOrientation.HasValue)
                    sb.Append(CaptureOrientationToString(CaptureOrientation.Value));
                yield return $"capture_orientation={sb}";
            }

            // angle
            if (Angle.HasValue)
                yield return $"angle={Angle.Value}";
        }

        static string CaptureOrientationToString(CaptureOrientations value) => value switch
        {
            CaptureOrientations.Orient0 => "0",
            CaptureOrientations.Orient90 => "90",
            CaptureOrientations.Orient180 => "180",
            CaptureOrientations.Orient270 => "270",
            CaptureOrientations.Flip0 => "flip0",
            CaptureOrientations.Flip90 => "flip90",
            CaptureOrientations.Flip180 => "flip180",
            CaptureOrientations.Flip270 => "flip270",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };
    }
}
