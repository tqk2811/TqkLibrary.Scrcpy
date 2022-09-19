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
        /// Default: false
        /// </summary>
        public bool ShowTouches { get; set; } = false;

        /// <summary>
        /// Default: true
        /// </summary>
        public bool StayAwake { get; set; } = true;

        /// <summary>
        /// Default: Orientation.Auto
        /// </summary>
        public Orientations Orientation { get; set; } = Orientations.Auto;

        /// <summary>
        /// Bật để sử dụng <see cref="Scrcpy.Control"/><br>
        /// </br>Default: true
        /// </summary>
        public bool IsControl { get; set; } = true;

        /// <summary>
        /// Default: 0 (unlimit)
        /// </summary>
        public int MaxFps { get; set; } = 0;

        /// <summary>
        /// Default: 0
        /// </summary>
        public int MaxSize { get; set; } = 0;

        /// <summary>
        /// Max bitrate of video stream
        /// </summary>
        public int Bitrate { get; set; } = 8000000;

        /// <summary>
        /// Index of screen android
        /// Default: 0
        /// </summary>
        public int DisplayId { get; set; } = 0;

        /// <summary>
        /// Cắt một vùng màn hình
        /// Mặc định: null
        /// </summary>
        public Rectangle? Crop { get; set; } = null;

        /// <summary>
        /// Dùng phần cứng đặc biệt để giải mã hình ảnh (Ví dụ: VGA/GPU)<br>
        /// </br>Mặc định: <see cref="FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_NONE"/><br>
        /// </br>Sử dụng <see cref="ScrcpyConfig.GetHwSupports"/> để lấy danh sách hỗ trợ
        /// </summary>
        public FFmpegAVHWDeviceType HwType { get; set; } = FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_NONE;

        /// <summary>
        /// 
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Info;

        /// <summary>
        /// default: false
        /// </summary>
        public bool PowerOffOnClose { get; set; } = false;

        /// <summary>
        /// Dùng directX 11 shader để giải mã ảnh.<br>
        /// </br>Note: Chỉ hoạt động với <see cref="HwType"/> ở mode <see cref="FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA"/>
        /// </summary>
        public bool IsUseD3D11Shader { get; set; } = false;


        /// <summary>
        /// Default: 3000
        /// </summary>
        public int ConnectionTimeout { get; set; } = 3000;

        /// <summary>
        /// 
        /// </summary>
        public string ScrcpyServerPath { get; set; } = "scrcpy-server-v1.24.jar";

        /// <summary>
        /// if true: Use Adb Forward instead of Adb Reverse
        /// Default: false
        /// </summary>
        internal bool ForceAdbForward { get; } = false;

        /// <summary>
        /// 
        /// </summary>
        public bool ClipboardAutosync { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        public bool DownsizeOnError { get; set; } = true;

        /// <summary>
        /// 
        /// </summary>
        public bool Cleanup { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        public bool PowerOn { get; set; } = true;

        /// <summary>
        /// Default
        /// </summary>
        public D3D11Filter Filter { get; set; } = D3D11Filter.D3D11_FILTER_MIN_MAG_LINEAR_MIP_POINT;

        internal bool TunnelForward { get; } = false;

        internal string Crop_string
        {
            get
            {
                if (Crop == null) return "-";
                else return $"{Crop.Value.Width}:{Crop.Value.Height}:{Crop.Value.X}:{Crop.Value.Y}";
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            List<string> args = new List<string>();
            args.Add($"log_level={LogLevel.ToString().ToLower()}");
            args.Add($"bit_rate={Bitrate}");
            args.Add($"display_id={DisplayId}");

            if (MaxSize > 0) args.Add($"max_size={MaxSize}");
            if (MaxFps > 0) args.Add($"max_fps={MaxFps}");
            if (Crop != null) args.Add($"crop={Crop_string}");
            if (Orientation != Orientations.Auto) args.Add($"lock_video_orientation={(int)Orientation}");

            if (TunnelForward) args.Add($"tunnel_forward=true");
            if (ShowTouches) args.Add($"show_touches=true");
            if (StayAwake) args.Add($"stay_awake=true");
            if (PowerOffOnClose) args.Add($"power_off_on_close=true");
            //codec_options
            //encoder_name

            if (!IsControl) args.Add($"control=false"); // By default, control is true
            if (!ClipboardAutosync) args.Add($"clipboard_autosync=false");// By default, clipboard_autosync is true
            if (!DownsizeOnError) args.Add($"downsize_on_error=false");// By default, downsize_on_error is true
            if (!Cleanup) args.Add($"cleanup=false");// By default, cleanup is true
            if (!PowerOn) args.Add($"power_on=false");// By default, power_on is true

            return string.Join(" ", args);
        }

        internal ScrcpyNativeConfig NativeConfig()
        {
            return new ScrcpyNativeConfig
            {
                HwType = this.HwType,
                ForceAdbForward = this.ForceAdbForward,
                IsControl = this.IsControl,
                IsUseD3D11Shader = this.IsUseD3D11Shader,
                ScrcpyServerPath = this.ScrcpyServerPath,
                ConnectionTimeout = this.ConnectionTimeout,
                Filter = this.Filter,
            };
        }
    }
}
