using System;
using System.Runtime.InteropServices;
using TqkLibrary.Scrcpy.Enums;

namespace TqkLibrary.Scrcpy.Configs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ScrcpyNativeConfig
    {
        [MarshalAs(UnmanagedType.U1)]
        public FFmpegAVHWDeviceType HwType;//uint8

        [MarshalAs(UnmanagedType.U1)]
        public bool IsControl;

        [MarshalAs(UnmanagedType.U1)]
        public bool IsUseD3D11ForUiRender;

        [MarshalAs(UnmanagedType.U1)]
        public bool IsUseD3D11ForConvert;

        [MarshalAs(UnmanagedType.U1)]
        public bool IsAudio;

        [MarshalAs(UnmanagedType.U1)]
        public bool IsVideo;

        public int ConnectionTimeout;

        public D3D11Filter Filter;

        public uint GpuThreadX;
        public uint GpuThreadY;
        [MarshalAs(UnmanagedType.Bool)]
        public bool IsForceUiGpuFlush;
    }
}
