using System.Drawing;

namespace TqkLibrary.Scrcpy
{
    /// <summary>
    /// 
    /// </summary>
    public class ScrcpyServerConfig
    {
        #region AndroidConfig
        /// <summary>
        /// Default: false
        /// </summary>
        public bool ShowTouches { get; set; } = false;
        /// <summary>
        /// Default: true
        /// </summary>
        public bool StayAwake { get; set; } = true;
        /// <summary>
        /// Turn off screen when scrcpy exit<br></br>
        /// default: false
        /// </summary>
        public bool PowerOffOnClose { get; set; } = false;
        /// <summary>
        /// Turn on screen when scrcpy start<br></br>
        /// default: true
        /// </summary>
        public bool PowerOn { get; set; } = true;
        #endregion



        #region ScrcpyConfig
        //Only use 31 bits to avoid issues with signed values on the Java-side
        /// <summary>
        /// scrcpy connection id<br></br>
        /// 31-bit non-negative value, or -1
        /// </summary>
        public int SCID { get; set; } = -1;
        /// <summary>
        /// Turn on for use <see cref="Scrcpy.Control"/><br>
        /// </br>Default: true
        /// </summary>
        public bool IsControl { get; set; } = true;
        /// <summary>
        /// 
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Info;
        /// <summary>
        /// 
        /// </summary>
        public bool ClipboardAutosync { get; set; } = false;
        /// <summary>
        /// 
        /// </summary>
        public bool Cleanup { get; set; } = false;
        #endregion



        #region VideoConfig
        /// <summary>
        /// Index of screen android<br></br>
        /// Default: null
        /// </summary>
        public int? DisplayId { get; set; }
        /// <summary>
        /// Default: <see cref="Orientations.Auto"/>
        /// </summary>
        public Orientations Orientation { get; set; } = Orientations.Auto;
        /// <summary>
        /// Default: 0 (unlimit)
        /// </summary>
        public int MaxFps { get; set; } = 0;
        /// <summary>
        /// Max bitrate of video stream<br></br>
        /// Default: 0 (ignore)
        /// </summary>
        public int VideoBitrate { get; set; }
        /// <summary>
        /// VideoCodec<br></br>
        /// Default: null (ignore)<br></br>
        /// Support: h264, h265, av1, opus, aac, raw
        /// </summary>
        public string VideoCodec { get; set; }
        /// <summary>
        /// VideoCodecOption<br></br>
        /// Default: null (ignore)
        /// </summary>
        public string VideoCodecOption { get; set; }
        /// <summary>
        /// VideoEncoder<br></br>
        /// Default: null (ignore)
        /// </summary>
        public string VideoEncoder { get; set; }
        /// <summary>
        /// Crop a region in base android screen<br></br>
        /// Default: null
        /// </summary>
        public Rectangle? Crop { get; set; } = null;
        //https://github.com/Genymobile/scrcpy/blob/21df2c240e544b1c1eba7775e1474c1c772be04b/server/src/main/java/com/genymobile/scrcpy/ScreenEncoder.java#L132
        /// <summary>
        /// Downsizing on error is only enabled if an encoding failure occurs before the first frame (downsizing later could be surprising)<br></br>
        /// default: true
        /// </summary>
        public bool DownsizeOnError { get; set; } = true;
        /// <summary>
        /// print list Displays to adb shell output<br></br>
        /// default: false
        /// </summary>
        public bool ListDisplays { get; } = false;
        /// <summary>
        /// default: <see cref="VideoSource.Display"/>
        /// </summary>
        public VideoSource VideoSource { get; set; } = VideoSource.Display;
        #endregion



        #region Camera Config
        /// <summary>
        /// default: null
        /// </summary>
        public string CameraId { get; set; } = null;
        /// <summary>
        /// default: null
        /// </summary>
        public Rectangle? CameraSize { get; set; }
        internal string CameraSize_string
        {
            get
            {
                if (CameraSize == null) return "-";
                else return $"{CameraSize.Value.Width}:{CameraSize.Value.Height}:{CameraSize.Value.X}:{CameraSize.Value.Y}";
            }
        }
        /// <summary>
        /// default: <see cref="CameraFacing.Any"/>
        /// </summary>
        public CameraFacing CameraFacing { get; set; } = CameraFacing.Any;
        /// <summary>
        /// default: null
        /// </summary>
        public string CameraAr { get; set; } = null;
        /// <summary>
        /// default: 0
        /// </summary>
        public int Camerafps { get; set; } = 0;
        /// <summary>
        /// default: false
        /// </summary>
        public bool CameraHighSpeed { get; set; } = false;
        #endregion



        #region AudioConfig
        /// <summary>
        /// true to Enable audio stream<br></br>
        /// Default: false
        /// </summary>
        public bool IsAudio { get; set; } = false;
        /// <summary>
        /// Max bitrate of audio stream<br></br>
        /// Default: 0 (ignore)
        /// </summary>
        public int AudioBitrate { get; set; }
        /// <summary>
        /// AudioCodec<br></br>
        /// Default: null (ignore)<br></br>
        /// Support: h264, h265, av1, opus, aac, raw
        /// </summary>
        public string AudioCodec { get; set; }
        /// <summary>
        /// AudioCodecOption<br></br>
        /// Default: null (ignore)
        /// </summary>
        public string AudioCodecOption { get; set; }
        /// <summary>
        /// AudioEncoder<br></br>
        /// Default: null (ignore)
        /// </summary>
        public string AudioEncoder { get; set; }
        /// <summary>
        /// default: <see cref="AudioSource.Auto"/>
        /// </summary>
        public AudioSource AudioSource { get; set; } = AudioSource.Auto;
        #endregion



        //unknow what is this for
        //https://github.com/Genymobile/scrcpy/blob/21df2c240e544b1c1eba7775e1474c1c772be04b/server/src/main/java/com/genymobile/scrcpy/ScreenInfo.java#L83
        /// <summary>
        /// Default: 0
        /// </summary>
        public int MaxSize { get; } = 0;
        /// <summary>
        /// print list Encoders support to adb shell output<br></br>
        /// default: false
        /// </summary>
        public bool ListEncoders { get; } = false;


        /// <summary>
        /// 2.0
        /// </summary>
        public string ScrcpyServerVersion { get; } = "2.0";



        /// <summary>
        /// if true: Use Adb Forward instead of Adb Reverse<br></br>
        /// Default: false
        /// </summary>
        internal bool TunnelForward { get; } = false;

        internal string Crop_string
        {
            get
            {
                if (Crop == null) return "-";
                else return $"{Crop.Value.Width}:{Crop.Value.Height}:{Crop.Value.X}:{Crop.Value.Y}";
            }
        }
    }
}
