namespace TqkLibrary.Scrcpy.Enums
{
    /// <summary>
    /// https://github.com/Genymobile/scrcpy/tree/master/server/src/main/java/com/genymobile/scrcpy/ControlMessage.java#L8
    /// </summary>
    internal enum ScrcpyControlType : byte
    {
        TYPE_INJECT_KEYCODE = 0,
        TYPE_INJECT_TEXT = 1,
        TYPE_INJECT_TOUCH_EVENT = 2,
        TYPE_INJECT_SCROLL_EVENT = 3,
        TYPE_BACK_OR_SCREEN_ON = 4,
        TYPE_EXPAND_NOTIFICATION_PANEL = 5,
        TYPE_EXPAND_SETTINGS_PANEL = 6,
        TYPE_COLLAPSE_PANELS = 7,
        TYPE_GET_CLIPBOARD = 8,
        TYPE_SET_CLIPBOARD = 9,
        TYPE_SET_SCREEN_POWER_MODE = 10,
        TYPE_ROTATE_DEVICE = 11
    }
}
