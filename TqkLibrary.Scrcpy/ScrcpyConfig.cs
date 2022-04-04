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
        //public int WaitConnectionTimeout { get; set; } = 5000;

        /// <summary>
        /// Default: 1MB
        /// </summary>
        public int PacketBufferLength { get; set; } = 1024 * 1024;

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
        /// Default: true
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
        /// Bitrate of video stream
        /// </summary>
        public int Bitrate { get; set; } = 8000000;

        /// <summary>
        /// Index of screen android
        /// Default: 0
        /// </summary>
        public int DisplayId { get; set; } = 0;

        /// <summary>
        /// Crop screen
        /// Default: null
        /// </summary>
        public Rectangle? Crop { get; set; } = null;

        /// <summary>
        /// 
        /// </summary>
        public FFmpegAVHWDeviceType HwType { get; set; } = FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_NONE;

        /// <summary>
        /// 
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Info;

        /// <summary>
        /// 
        /// </summary>
        public bool PowerOffOnClose { get; set; } = false;

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
                Orientation = this.Orientation,
                HwType = this.HwType,
                PacketBufferLength = this.PacketBufferLength,
                ForceAdbForward = this.ForceAdbForward,
                IsControl = this.IsControl
            };
        }
    }
}
