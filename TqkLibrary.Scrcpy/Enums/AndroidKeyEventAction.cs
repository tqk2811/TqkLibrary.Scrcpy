namespace TqkLibrary.Scrcpy
{
    /// <summary>
    /// https://developer.android.com/reference/android/view/KeyEvent#ACTION_DOWN
    /// </summary>
    public enum AndroidKeyEventAction : byte
    {
        ACTION_DOWN = 0,
        ACTION_UP = 1,
        /// <summary>
        /// API level < 29
        /// </summary>
        ACTION_MULTIPLE = 2,
    }
}
