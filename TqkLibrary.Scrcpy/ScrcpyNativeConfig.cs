using System.Runtime.InteropServices;

namespace TqkLibrary.Scrcpy
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ScrcpyNativeConfig
    {
        public FFmpegAVHWDeviceType HwType;//int
        public bool ForceAdbForward;
        public bool IsControl;
    }
}
