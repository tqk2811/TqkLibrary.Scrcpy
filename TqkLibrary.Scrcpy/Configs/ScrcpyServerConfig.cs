using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TqkLibrary.Scrcpy.Attributes;
using TqkLibrary.Scrcpy.Enums;
using TqkLibrary.Scrcpy.Interfaces;

namespace TqkLibrary.Scrcpy.Configs
{
    /// <summary>
    /// 
    /// </summary>
    public class ScrcpyServerConfig : IConfig
    {
        /// <summary>
        /// 
        /// </summary>
        public AndroidConfig? AndroidConfig { get; set; } = new AndroidConfig();

        /// <summary>
        /// 
        /// </summary>
        public VideoConfig? VideoConfig { get; set; } = new VideoConfig();

        /// <summary>
        /// 
        /// </summary>
        public AudioConfig? AudioConfig { get; set; } = new AudioConfig();

        /// <summary>
        /// 
        /// </summary>
        public CameraConfig? CameraConfig { get; set; } = new CameraConfig();

        /// <summary>
        /// Enable video stream<br></br>
        /// Default: true
        /// </summary>
        [OptionName("video")]
        public bool IsVideo { get; set; } = true;

        /// <summary>
        ///
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
        /// 
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
        /// 
        /// </summary>
        [OptionName("clipboard_autosync")]
        public bool ClipboardAutosync { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        [OptionName("cleanup")]
        public bool Cleanup { get; set; } = false;

        /// <summary>
        /// if true: Use Adb Forward instead of Adb Reverse<br></br>
        /// Default: false
        /// </summary>
        [OptionName("tunnel_forward")]
        internal bool TunnelForward { get; } = false;

        //unknow what is this for
        //https://github.com/Genymobile/scrcpy/blob/21df2c240e544b1c1eba7775e1474c1c772be04b/server/src/main/java/com/genymobile/scrcpy/ScreenInfo.java#L83
        /// <summary>
        /// Default: 0
        /// </summary>
        [OptionName("max_size")]
        public int MaxSize { get; } = 0;

        /// <summary>
        /// Turn the screen off after this delay (in milliseconds). -1 means no timeout (default).<br></br>
        /// scrcpy 3.0 (--screen-off-timeout)
        /// </summary>
        [OptionName("screen_off_timeout")]
        public int ScreenOffTimeout { get; set; } = -1;

        /// <summary>
        /// Create a new virtual display with the specified resolution and optional DPI.<br></br>
        /// Format: [&lt;width&gt;x&lt;height&gt;][/&lt;dpi&gt;] e.g. "1920x1080/320" or "" for device default.<br></br>
        /// Default: null (omit — use physical display)<br></br>
        /// scrcpy 3.0 (--new-display)
        /// </summary>
        [OptionName("new_display")]
        public string? NewDisplay { get; set; }

        /// <summary>
        /// Show system decorations on the virtual display.<br></br>
        /// Set to false to pass vd_system_decorations=false (--no-vd-system-decorations).<br></br>
        /// Default: true (omit)<br></br>
        /// scrcpy 3.0
        /// </summary>
        public bool VdSystemDecorations { get; set; } = true;

        /// <summary>
        /// Destroy the content (move tasks to the main display) when the virtual display is closed.<br></br>
        /// Set to false to pass vd_destroy_content=false (--no-vd-destroy-content).<br></br>
        /// Default: true (omit)<br></br>
        /// scrcpy 3.1
        /// </summary>
        public bool VdDestroyContent { get; set; } = true;

        /// <summary>
        /// Set the policy for the IME (input method, e.g. soft keyboard) on the captured display.<br></br>
        /// Default: null (omit — server default)<br></br>
        /// scrcpy 3.2 (--display-ime-policy)
        /// </summary>
        [OptionName("display_ime_policy")]
        public DisplayImePolicy? DisplayImePolicy { get; set; }

        /// <summary>
        /// Keep the device "active" (prevent the captured display from going idle/dimming) while scrcpy is running.<br></br>
        /// Default: false (omit)<br></br>
        /// scrcpy 4.0 (--keep-active)
        /// </summary>
        [OptionName("keep_active")]
        public bool KeepActive { get; set; } = false;


        /// <summary>
        ///
        /// </summary>
        public string ScrcpyServerVersion { get; } = Constant.ScrcpyServerVersion;

        /// <summary>
        /// 
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
            yield return this._GetArgument(x => x.ScreenOffTimeout, x => x != -1);
            yield return this._GetArgument(x => x.NewDisplay, x => !string.IsNullOrWhiteSpace(x));
            if (!VdSystemDecorations)
                yield return "vd_system_decorations=false";
            if (!VdDestroyContent)
                yield return "vd_destroy_content=false";
            if (DisplayImePolicy.HasValue)
                yield return $"display_ime_policy={DisplayImePolicy.Value.ToString().ToLower()}";
            yield return this._GetArgument(x => x.KeepActive, KeepActive);
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
