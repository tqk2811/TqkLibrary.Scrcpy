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
    /// Top-level scrcpy client configuration: the server (device-side) config plus client-side
    /// decoding/rendering options and connection settings.
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
        public ScrcpyServerConfig? ServerConfig { get; set; } = new ScrcpyServerConfig();

        /// <summary>
        /// Where adb is, which scrcpy server jar to deploy and where it lives on the device, whether
        /// to push it again on every connect, and how long to wait for the device.<br></br>
        /// Also the single source of the adb path used for every device command while connecting
        /// (reverse tunnel, launching the server), so client and deploy can never disagree.<br></br>
        /// Pass this same instance to <see cref="Scrcpy.PushServer(ScrcpyDeployConfig?)"/> when pushing
        /// manually.
        /// </summary>
        public ScrcpyDeployConfig DeployConfig { get; set; } = new ScrcpyDeployConfig();

        /// <summary>
        /// Client-side decoding and rendering options. Nothing here is sent to the device.
        /// </summary>
        public ClientConfig ClientConfig { get; set; } = new ClientConfig();

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
            if (ServerConfig is null) ServerConfig = new ScrcpyServerConfig();
            if (DeployConfig is null) DeployConfig = new ScrcpyDeployConfig();
            if (ClientConfig is null) ClientConfig = new ClientConfig();
            bool isVideo = ServerConfig.IsVideo;
            bool isAudio = ServerConfig.AudioConfig?.IsAudio ?? false;
            bool isControl = ServerConfig.IsControl;
            if (!isVideo && !isAudio && !isControl)
                throw new InvalidOperationException("At least one stream (video, audio, control) must be enabled.");

            return new ScrcpyNativeConfig
            {
                HwType = ClientConfig.HwType,
                IsControl = isControl,
                IsUseD3D11ForUiRender = ClientConfig.IsUseD3D11ForUiRender,
                IsUseD3D11ForConvert = ClientConfig.IsUseD3D11ForConvert,
                IsAudio = isAudio,
                IsVideo = isVideo,
                ConnectionTimeout = DeployConfig.ConnectionTimeout,
                Filter = ClientConfig.Filter,
                IsForceUiGpuFlush = ClientConfig.IsForceUiGpuFlush,
            };
        }

    }
}
