using System.Runtime.InteropServices;

namespace TqkLibrary.Scrcpy
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ScrcpyNativeConfig
    {
        [MarshalAs(UnmanagedType.U1)]
        public FFmpegAVHWDeviceType HwType;//uint8

        [MarshalAs(UnmanagedType.U1)]
        public bool ForceAdbForward;

        [MarshalAs(UnmanagedType.U1)]
        public bool IsControl;

        [MarshalAs(UnmanagedType.U1)]
        public bool IsUseD3D11Shader;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string ScrcpyServerPath;

        public int ConnectionTimeout;
    }
}
