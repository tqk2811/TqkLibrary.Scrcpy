using System;

namespace TqkLibrary.Scrcpy.Enums
{
    /// <summary>
    /// https://developer.android.com/reference/android/view/MotionEvent#BUTTON_PRIMARY
    /// </summary>
    [Flags]
    public enum AndroidMotionEventButton : int
    {
        /// <summary>
        /// 
        /// </summary>
        None = 0,
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
        /// <summary>
        /// 
        /// </summary>
        BUTTON_BACK = 8,
        /// <summary>
        /// 
        /// </summary>
        BUTTON_FORWARD = 16,
        /// <summary>
        /// 
        /// </summary>
        BUTTON_STYLUS_PRIMARY = 32,
        /// <summary>
        /// 
        /// </summary>
        BUTTON_STYLUS_SECONDARY = 64,
    }
}
