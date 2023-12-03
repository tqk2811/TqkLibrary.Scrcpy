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
        /// Only work with <see cref="HwType"/> in mode <see cref="FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA"/><br></br>
        /// Default <see cref="D3D11Filter.D3D11_FILTER_MIN_MAG_LINEAR_MIP_POINT"/>
        /// </summary>
        public D3D11Filter Filter { get; set; } = D3D11Filter.D3D11_FILTER_MIN_MAG_LINEAR_MIP_POINT;

        /// <summary>
        /// Use Hardware Accelerator for decode image<br>
        /// </br>Default: <see cref="FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_NONE"/><br>
        /// </br>Use <see cref="GetHwSupports"/> for get support list.
        /// </summary>
        public FFmpegAVHWDeviceType HwType { get; set; } = FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_NONE;

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
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (ServerConfig is null) ServerConfig = new ScrcpyServerConfig();
            return string.Join(" ", ServerConfig.GetArguments());
        }

        internal ScrcpyNativeConfig NativeConfig()
        {
            var ConfigureArguments = ToString();
            return new ScrcpyNativeConfig
            {
                HwType = HwType,
                ForceAdbForward = ServerConfig.TunnelForward,
                IsControl = ServerConfig.IsControl,
                IsUseD3D11ForUiRender = IsUseD3D11ForUiRender,
                IsAudio = ServerConfig.AudioConfig.IsAudio,
                ScrcpyServerPath = ScrcpyServerPath,
                AdbPath = AdbPath,
                ConfigureArguments = ConfigureArguments,
                ConnectionTimeout = ConnectionTimeout,
                Filter = Filter,
                SCID = ServerConfig.SCID
            };
        }

    }
}
