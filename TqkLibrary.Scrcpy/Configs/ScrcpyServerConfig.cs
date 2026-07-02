using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TqkLibrary.Scrcpy.Attributes;
using TqkLibrary.Scrcpy.Enums;
using TqkLibrary.Scrcpy.Interfaces;

namespace TqkLibrary.Scrcpy.Configs
{
    /// <summary>
    /// scrcpy server (device-side) configuration: which streams to enable plus their video/audio/camera
    /// options and how the session behaves.
    /// </summary>
    public class ScrcpyServerConfig : IConfig
    {
        /// <summary>
        /// Device-side behaviour options (show touches, stay awake, power).
        /// </summary>
        public AndroidConfig? AndroidConfig { get; set; } = new AndroidConfig();

        /// <summary>
        /// Display video stream options (used when the video source is the device display).
        /// </summary>
        public VideoConfig? VideoConfig { get; set; } = new VideoConfig();

        /// <summary>
        /// Audio stream options.
        /// </summary>
        public AudioConfig? AudioConfig { get; set; } = new AudioConfig();

        /// <summary>
        /// Camera capture options (used when the video source is a camera).
        /// </summary>
        public CameraConfig? CameraConfig { get; set; } = new CameraConfig();

        /// <summary>
        /// Enable video stream<br></br>
        /// Default: true
        /// </summary>
        [OptionName("video")]
        public bool IsVideo { get; set; } = true;

        /// <summary>
        /// Source of the video stream: the device display or a camera.<br></br>
        /// Default: Display
        /// </summary>
        [OptionName("video_source")]
        public VideoSource VideoSource { get; set; } = VideoSource.Display;

        /// <summary>
        /// Turn on for use <see cref="Scrcpy.Control"/><br>
        /// </br>Default: true
        /// </summary>
        [OptionName("control")]
        public bool IsControl { get; set; } = true;

        /// <summary>
        /// Verbosity of the scrcpy server log.<br></br>
        /// Default: Info
        /// </summary>
        [OptionName("log_level")]
        public LogLevel LogLevel { get; set; } = LogLevel.Info;

        //Only use 31 bits to avoid issues with signed values on the Java-side
        /// <summary>
        /// scrcpy connection id<br></br>
        /// 31-bit non-negative value, or -1
        /// </summary>
        [OptionName("scid")]
        public int SCID { get; set; } = -1;

        /// <summary>
        /// Automatically keep the device and computer clipboards in sync.<br></br>
        /// Default: false
        /// </summary>
        [OptionName("clipboard_autosync")]
        public bool ClipboardAutosync { get; set; } = false;

        /// <summary>
        /// Run the server's cleanup step on exit to restore device settings changed during the session
        /// (show touches, stay awake, ...).<br></br>
        /// Default: false
        /// </summary>
        [OptionName("cleanup")]
        public bool Cleanup { get; set; } = false;

        /// <summary>
        /// if true: Use Adb Forward instead of Adb Reverse<br></br>
        /// Default: false
        /// </summary>
        [OptionName("tunnel_forward")]
        internal bool TunnelForward { get; } = false;

        //https://github.com/Genymobile/scrcpy/blob/21df2c240e544b1c1eba7775e1474c1c772be04b/server/src/main/java/com/genymobile/scrcpy/ScreenInfo.java#L83
        /// <summary>
        /// Limits the longer side of the captured video to this many pixels, preserving aspect ratio
        /// (the shorter side scales down proportionally, rounded to a multiple of 8). Maps to scrcpy's
        /// <c>--max-size</c>. Lowering it cuts client-side decode and GPU memory load — most useful when
        /// mirroring many devices at once — at the cost of a softer image.<br></br>
        /// Default: 0 (no limit — capture at the device's native resolution).
        /// </summary>
        [OptionName("max_size")]
        public int MaxSize { get; set; } = 0;


        /// <summary>
        /// Protocol/version string sent to the server; must match the deployed scrcpy-server.jar.
        /// </summary>
        public string ScrcpyServerVersion { get; } = Constant.ScrcpyServerVersion;

        /// <summary>
        /// Path on the device where the scrcpy server jar is pushed and executed.
        /// </summary>
        public string ScrcpyServerAndroidPath { get; set; } = Constant.ScrcpyServerAndroidPath;

        IEnumerable<string> _GetArguments()
        {
            yield return ScrcpyServerVersion;
            yield return this._GetArgument(x => x.IsVideo, !IsVideo);
            yield return this._GetArgument(x => x.IsControl, !IsControl);
            yield return this._GetArgument(x => x.SCID, x => x != -1, x => $"{SCID & 0x7FFFFFFF:X4}".ToLower());
            yield return this._GetArgument(x => x.ClipboardAutosync, !ClipboardAutosync);
            yield return this._GetArgument(x => x.Cleanup, !Cleanup);
            yield return this._GetArgument(x => x.TunnelForward, TunnelForward);
            yield return this._GetArgument(x => x.MaxSize, x => x > 0);
            if (IsVideo) yield return this._GetArgument(x => x.VideoSource, x => x != VideoSource.Display, x => x.ToString().ToLower());
        }
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetArguments()
        {
            if (AndroidConfig is null) AndroidConfig = new AndroidConfig();
            if (AudioConfig is null) AudioConfig = new AudioConfig();

            IEnumerable<string> result = _GetArguments()
                .Concat(AndroidConfig.GetArguments())
                .Concat(AudioConfig.GetArguments());

            if (IsVideo)
            {
                switch (VideoSource)
                {
                    case VideoSource.Camera:
                        if (CameraConfig is null) CameraConfig = new CameraConfig();
                        result = result.Concat(CameraConfig.GetArguments());
                        break;

                    case VideoSource.Display:
                        if (VideoConfig is null) VideoConfig = new VideoConfig();
                        result = result.Concat(VideoConfig.GetArguments());
                        break;

                    default:
                        throw new System.NotSupportedException(VideoSource.ToString());
                }
            }
            return result.Where(x => !string.IsNullOrWhiteSpace(x));
        }
    }
}
