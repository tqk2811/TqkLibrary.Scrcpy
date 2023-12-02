namespace TqkLibrary.Scrcpy.Enums
{
    /// <summary>
    /// https://developer.android.com/reference/android/view/MotionEvent#ACTION_BUTTON_PRESS
    /// </summary>
    public enum AndroidMotionEventAction : byte
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        ACTION_DOWN = 0,
        ACTION_UP = 1,
        ACTION_MOVE = 2,
        ACTION_CANCEL = 3,
        ACTION_OUTSIDE = 4,
        ACTION_POINTER_DOWN = 5,
        ACTION_POINTER_UP = 6,
        ACTION_HOVER_MOVE = 7,
        ACTION_SCROLL = 8,
        ACTION_POINTER_INDEX_SHIFT = 8,
        ACTION_HOVER_ENTER = 9,
        ACTION_HOVER_EXIT = 10,
        ACTION_BUTTON_PRESS = 11,
        ACTION_BUTTON_RELEASE = 12,
        ACTION_MASK = 255,
        //ACTION_POINTER_INDEX_MASK = 65280,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
