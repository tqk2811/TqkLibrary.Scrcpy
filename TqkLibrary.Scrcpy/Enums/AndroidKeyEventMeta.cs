using System;

namespace TqkLibrary.Scrcpy.Enums
{
    /// <summary>
    /// https://developer.android.com/reference/android/view/KeyEvent#META_ALT_LEFT_ON
    /// </summary>
    [Flags]
    public enum AndroidKeyEventMeta : uint
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        META_NONE = 0,
        META_SYM_ON = 1 << 2,//4
        META_FUNCTION_ON = 1 << 3,//8
        META_CAPS_LOCK_ON = 1 << 20,//1048576
        META_NUM_LOCK_ON = 1 << 21,//2097152
        META_SCROLL_LOCK_ON = 1 << 22,//4194304

        META_ALT_ON = 1 << 1,//2
        META_ALT_LEFT_ON = 1 << 4,//16
        META_ALT_RIGHT_ON = 1 << 5,//32
        META_ALT_MASK = META_ALT_ON | META_ALT_LEFT_ON | META_ALT_RIGHT_ON,//50

        META_SHIFT_ON = 1 << 0,//1
        META_SHIFT_LEFT_ON = 1 << 6,//64
        META_SHIFT_RIGHT_ON = 1 << 7,//128
        META_SHIFT_MASK = META_SHIFT_ON | META_SHIFT_LEFT_ON | META_SHIFT_RIGHT_ON,//193

        META_CTRL_ON = 1 << 12,//4096
        META_CTRL_LEFT_ON = 1 << 13,//8192
        META_CTRL_RIGHT_ON = 1 << 14,//16384
        META_CTRL_MASK = META_CTRL_ON | META_CTRL_LEFT_ON | META_CTRL_RIGHT_ON,// 28672

        META_META_ON = 1 << 16,//65536
        META_META_LEFT_ON = 1 << 17,//131072
        META_META_RIGHT_ON = 1 << 18,//262144
        META_META_MASK = META_META_ON | META_META_LEFT_ON | META_META_RIGHT_ON,// 458752
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
