namespace TqkLibrary.Scrcpy
{
    internal struct ScrcpyNativeConfig
    {
        public Orientations Orientation;//int
        public FFmpegAVHWDeviceType HwType;//int
        public int PacketBufferLength;
        public bool ForceAdbForward;
    }
}
