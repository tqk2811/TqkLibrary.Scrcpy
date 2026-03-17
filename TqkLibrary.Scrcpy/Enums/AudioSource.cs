namespace TqkLibrary.Scrcpy
{
    /// <summary>
    /// 
    /// </summary>
    public enum AudioSource
    {
        /// <summary>
        /// <see cref="Output"/> for video <see cref="VideoSource.Display"/>, <see cref="Mic"/> for video <see cref="VideoSource.Camera"/>
        /// </summary>
        Auto,
        /// <summary>
        /// 
        /// </summary>
        Output,
        /// <summary>
        ///
        /// </summary>
        Mic,
        /// <summary>
        /// Capture device audio playback without muting the device (Android 13+)
        /// </summary>
        Playback
    }
}
