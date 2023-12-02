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
        public AndroidConfig AndroidConfig { get; set; } = new AndroidConfig();

        /// <summary>
        /// 
        /// </summary>
        public VideoConfig VideoConfig { get; set; } = new VideoConfig();

        /// <summary>
        /// 
        /// </summary>
        public AudioConfig AudioConfig { get; set; } = new AudioConfig();

        /// <summary>
        /// 
        /// </summary>
        public CameraConfig CameraConfig { get; set; } = new CameraConfig();



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
        /// 2.0
        /// </summary>
        public string ScrcpyServerVersion { get; } = Constant.ScrcpyServerVersion;

        IEnumerable<string> _GetArguments()
        {
            yield return ScrcpyServerVersion;
            yield return this._GetArgument(x => x.IsControl, !IsControl);
            yield return this._GetArgument(x => x.SCID, x => x != -1, x => $"{SCID & 0x7FFFFFFF:X4}".ToLower());
            yield return this._GetArgument(x => x.ClipboardAutosync, !ClipboardAutosync);
            yield return this._GetArgument(x => x.Cleanup, !Cleanup);
            yield return this._GetArgument(x => x.TunnelForward, TunnelForward);
            yield return this._GetArgument(x => x.MaxSize, x => x > 0);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetArguments()
        {
            if (AndroidConfig is null) AndroidConfig = new AndroidConfig();
            if (VideoConfig is null) VideoConfig = new VideoConfig();
            if (AudioConfig is null) AudioConfig = new AudioConfig();
            return _GetArguments()
                .Concat(AndroidConfig.GetArguments())
                .Concat(VideoConfig.GetArguments())
                .Concat(AudioConfig.GetArguments())
                .Concat(CameraConfig.GetArguments())
                .Where(x => !string.IsNullOrWhiteSpace(x));
        }
    }
}
