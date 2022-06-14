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
        public string ScrcpyServerPath { get; set; } = "scrcpy-server1.19.jar";


        /// <summary>
        /// if true: Use Adb Forward instead of Adb Reverse
        /// Default: false
        /// </summary>
        internal bool ForceAdbForward { get; } = false;


        internal string Version { get; } = "1.19";
        internal bool FrameMeta { get; } = true;// always send frame meta (packet boundaries + timestamp)

        internal bool TunnelForward { get; } = false;
        internal string CodecOptions { get; } = "-";
        internal string EncoderName { get; } = "-";
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
            return $"{Version} {LogLevel.ToString().ToLower()} {MaxSize} {Bitrate} {MaxFps} {(int)Orientation} {TunnelForward} {Crop_string} {FrameMeta} {IsControl} {DisplayId} {ShowTouches} {StayAwake} {CodecOptions} {EncoderName} {PowerOffOnClose}";
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
                ConnectionTimeout = ConnectionTimeout,
            };
        }
    }
}
