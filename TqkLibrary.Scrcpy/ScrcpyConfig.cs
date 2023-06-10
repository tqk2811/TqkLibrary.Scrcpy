using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.Scrcpy
{
    /// <summary>
    /// 
    /// </summary>
    public class ScrcpyConfig
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<FFmpegAVHWDeviceType> GetHwSupports()
        {
            FFmpegAVHWDeviceType type = FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
            do
            {
                type = (FFmpegAVHWDeviceType)NativeWrapper.FFmpegHWSupport((byte)type);
                yield return type;

            } while (type != FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_NONE);
        }

        /// <summary>
        /// Use Hardware Accelerator for decode image<br>
        /// </br>Default: <see cref="FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_NONE"/><br>
        /// </br>Use <see cref="ScrcpyConfig.GetHwSupports"/> for get support list.
        /// </summary>
        public FFmpegAVHWDeviceType HwType { get; set; } = FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_NONE;



        /// <summary>
        /// Config for scrcpy server
        /// </summary>
        public ScrcpyServerConfig ServerConfig { get; set; } = new ScrcpyServerConfig();

        /// <summary>
        /// Use directx 11 for convert image.<br>
        /// </br>Only work with <see cref="HwType"/> in mode <see cref="FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA"/> or <see cref="FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_NONE"/>
        /// </summary>
        public bool IsUseD3D11ForUiRender { get; set; } = false;

        /// <summary>
        /// To use this feature, please set <see cref="IsUseD3D11ForUiRender"/> to true, and <see cref="FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_NONE"/>
        /// </summary>
        public bool IsUseD3D11ForConvert { get; set; } = false;

        /// <summary>
        /// Default: 3000
        /// </summary>
        public int ConnectionTimeout { get; set; } = 3000;

        /// <summary>
        /// Default: adb.exe
        /// </summary>
        public string AdbPath { get; set; } = "adb.exe";

        /// <summary>
        /// Default: scrcpy-server.jar
        /// </summary>
        public string ScrcpyServerPath { get; set; } = "scrcpy-server.jar";



        /// <summary>
        /// Only work with <see cref="HwType"/> in mode <see cref="FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA"/><br></br>
        /// Default <see cref="D3D11Filter.D3D11_FILTER_MIN_MAG_LINEAR_MIP_POINT"/>
        /// </summary>
        public D3D11Filter Filter { get; set; } = D3D11Filter.D3D11_FILTER_MIN_MAG_LINEAR_MIP_POINT;

        internal bool TunnelForward { get; } = false;




        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (ServerConfig is null) ServerConfig = new ScrcpyServerConfig();

            List<string> args = new List<string>();
            args.Add(ServerConfig.ScrcpyServerVersion);

            //android
            if (ServerConfig.ShowTouches) args.Add($"show_touches=true");
            if (ServerConfig.StayAwake) args.Add($"stay_awake=true");
            if (ServerConfig.PowerOffOnClose) args.Add($"power_off_on_close=true");
            if (!ServerConfig.PowerOn) args.Add($"power_on=false");// By default, power_on is true

            //scrcpy
            if (!ServerConfig.IsControl) args.Add($"control=false"); // By default, control is true
            args.Add($"log_level={ServerConfig.LogLevel.ToString().ToLower()}");
            if (ServerConfig.SCID != -1) args.Add($"scid={(ServerConfig.SCID & 0x7FFFFFFF):X4}".ToLower());
            if (!ServerConfig.ClipboardAutosync) args.Add($"clipboard_autosync=false");// By default, clipboard_autosync is true
            if (!ServerConfig.Cleanup) args.Add($"cleanup=false");// By default, cleanup is true

            //video
            if (ServerConfig.DisplayId.HasValue) args.Add($"display_id={ServerConfig.DisplayId}");
            if (ServerConfig.Orientation != Orientations.Auto) args.Add($"lock_video_orientation={(int)ServerConfig.Orientation}");
            if (ServerConfig.MaxFps > 0) args.Add($"max_fps={ServerConfig.MaxFps}");
            if (ServerConfig.VideoBitrate > 0) args.Add($"video_bit_rate={ServerConfig.VideoBitrate}");
            if (!string.IsNullOrWhiteSpace(ServerConfig.VideoCodec)) args.Add($"video_codec={ServerConfig.VideoCodec}");
            if (!string.IsNullOrWhiteSpace(ServerConfig.VideoCodecOption)) args.Add($"video_codec_options={ServerConfig.VideoCodecOption}");
            if (!string.IsNullOrWhiteSpace(ServerConfig.VideoEncoder)) args.Add($"video_encoder={ServerConfig.VideoEncoder}");
            if (ServerConfig.Crop != null) args.Add($"crop={ServerConfig.Crop_string}");
            if (!ServerConfig.DownsizeOnError) args.Add($"downsize_on_error=false");// By default, downsize_on_error is true
            if (ServerConfig.ListDisplays) args.Add($"list_displays=true");

            //audio
            if (!ServerConfig.IsAudio) args.Add($"audio=false"); // By default, audio is true
            if (ServerConfig.IsAudio)
            {
                if (ServerConfig.AudioBitrate > 0) args.Add($"audio_bit_rate={ServerConfig.AudioBitrate}");
                if (!string.IsNullOrWhiteSpace(ServerConfig.AudioCodec)) args.Add($"audio_codec={ServerConfig.AudioCodec}");
                if (!string.IsNullOrWhiteSpace(ServerConfig.AudioCodecOption)) args.Add($"audio_codec_options={ServerConfig.AudioCodecOption}");
                if (!string.IsNullOrWhiteSpace(ServerConfig.AudioEncoder)) args.Add($"audio_encoder={ServerConfig.AudioEncoder}");
            }

            if (ServerConfig.TunnelForward) args.Add($"tunnel_forward=true");
            if (ServerConfig.MaxSize > 0) args.Add($"max_size={ServerConfig.MaxSize}");
            if (ServerConfig.ListEncoders) args.Add($"list_encoders=true");

            return string.Join(" ", args);
        }

        internal ScrcpyNativeConfig NativeConfig()
        {
            return new ScrcpyNativeConfig
            {
                HwType = this.HwType,
                ForceAdbForward = this.ServerConfig.TunnelForward,
                IsControl = this.ServerConfig.IsControl,
                IsUseD3D11ForUiRender = this.IsUseD3D11ForUiRender,
                IsAudio = this.ServerConfig.IsAudio,
                ScrcpyServerPath = this.ScrcpyServerPath,
                AdbPath = this.AdbPath,
                ConfigureArguments = this.ToString(),
                ConnectionTimeout = this.ConnectionTimeout,
                Filter = this.Filter,
                SCID = this.ServerConfig.SCID
            };
        }
    }
}
