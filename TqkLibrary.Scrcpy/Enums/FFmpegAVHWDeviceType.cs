namespace TqkLibrary.Scrcpy
{
    public enum FFmpegAVHWDeviceType : int
    {
        AV_HWDEVICE_TYPE_NONE = 0,
        AV_HWDEVICE_TYPE_VDPAU = 1,
        AV_HWDEVICE_TYPE_CUDA = 2,
        AV_HWDEVICE_TYPE_VAAPI = 3,
        AV_HWDEVICE_TYPE_DXVA2 = 4,
        AV_HWDEVICE_TYPE_QSV = 5,
        AV_HWDEVICE_TYPE_VIDEOTOOLBOX = 6,
        AV_HWDEVICE_TYPE_D3D11VA = 7,
        AV_HWDEVICE_TYPE_DRM = 8,
        AV_HWDEVICE_TYPE_OPENCL = 9,
        AV_HWDEVICE_TYPE_MEDIACODEC = 10,
        AV_HWDEVICE_TYPE_VULKAN = 11
    }
}
