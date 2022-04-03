using System;

namespace TqkLibrary.Scrcpy
{
    /// <summary>
    /// https://developer.android.com/reference/android/view/MotionEvent#BUTTON_PRIMARY
    /// </summary>
    [Flags]
    public enum AndroidMotionEventButton : int
    {
        /// <summary>
        /// left mouse button
        /// </summary>
        BUTTON_PRIMARY = 1,
        /// <summary>
        /// right mouse button
        /// </summary>
        BUTTON_SECONDARY = 2,
        /// <summary>
        /// middle mouse button
        /// </summary>
        BUTTON_TERTIARY = 4,
        BUTTON_BACK = 8,
        BUTTON_FORWARD = 16,
        BUTTON_STYLUS_PRIMARY = 32,
        BUTTON_STYLUS_SECONDARY = 64,
    }
}
