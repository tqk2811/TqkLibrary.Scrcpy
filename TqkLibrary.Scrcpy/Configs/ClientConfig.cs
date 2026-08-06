using TqkLibrary.Scrcpy.Enums;

namespace TqkLibrary.Scrcpy.Configs
{
    /// <summary>
    /// Client-side (PC) decoding and rendering options.<br></br>
    /// Unlike <see cref="ScrcpyServerConfig"/> (arguments sent to the server on the device) and
    /// <see cref="ScrcpyDeployConfig"/> (how to reach the device), nothing here is sent to the
    /// device — these only change how the native library decodes and converts frames locally.
    /// </summary>
    public class ClientConfig
    {
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
        /// </br>Use <see cref="ScrcpyConfig.GetHwSupports"/> for get support list.
        /// </summary>
        public FFmpegAVHWDeviceType HwType { get; set; } = FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_NONE;

        /// <summary>
        /// Flush the D3D11 device after each UI draw so the rendered frame is submitted before the WPF
        /// surface queue presents it. Rendering uses a different D3D11 device than the surface queue's
        /// producer, so without this an isolated present (e.g. resizing the window while the device is
        /// idle) can show a black screen.<br></br>
        /// To use this feature, please set <see cref="IsUseD3D11ForUiRender"/> to true.<br></br>
        /// Default: true
        /// </summary>
        public bool IsForceUiGpuFlush { get; set; } = true;
    }
}
