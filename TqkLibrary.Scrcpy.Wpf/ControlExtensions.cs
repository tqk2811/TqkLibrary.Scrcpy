using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TqkLibrary.Scrcpy.Enums;
using TqkLibrary.Scrcpy.Interfaces;
using TqkLibrary.Scrcpy.Wpf.Interfaces;

namespace TqkLibrary.Scrcpy.Wpf
{
    internal static class ControlExtensions
    {
        public static System.Drawing.Point GetRemotePoint(this IScrcpyUiControl scrcpyUiControl, System.Windows.Point point)
        {
            double x = point.X / scrcpyUiControl.DrawRect.Width;// (point.X - drawRect.X) / drawRect.Width;
            double y = point.Y / scrcpyUiControl.DrawRect.Height;// (point.Y - drawRect.Y) / drawRect.Height;
            double real_x = x * scrcpyUiControl.VideoSize.Width;
            double real_y = y * scrcpyUiControl.VideoSize.Height;
            var outpoint = new System.Drawing.Point((int)real_x, (int)real_y);
#if DEBUG
            //Debug.WriteLine($"drawRect: {scrcpyUiControl.DrawRect}, videoSize: {scrcpyUiControl.VideoSize}, inPoint: {point}, outPoint: {outpoint}");
#endif
            return outpoint;
        }


        #region Mouse
        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlHelper"></param>
        /// <param name="point"></param>
        /// <param name="mouseButton"></param>
        /// <param name="action"></param>
        /// <returns>Is mouse down</returns>
        public static void ResolveMouseButton(
            this IScrcpyUiControl controlHelper,
            System.Drawing.Point point,
            MouseButton mouseButton,
            AndroidMotionEventAction action,
            ref bool isDown
            )
        {
            if (action == AndroidMotionEventAction.ACTION_DOWN || action == AndroidMotionEventAction.ACTION_UP)
            {
                IControl? control = controlHelper.Control;
                if (control is null)
                    return;
                switch (mouseButton)
                {
                    case MouseButton.Left:
                        isDown = action == AndroidMotionEventAction.ACTION_DOWN;
                        if (isDown) controlHelper.Image.CaptureMouse();
                        else controlHelper.Image.ReleaseMouseCapture();
                        control.InjectTouchEvent(
                            action,
                            controlHelper.MousePointerId,
                            point,
                            action == AndroidMotionEventAction.ACTION_DOWN ? 1.0f : 0.0f,
                            AndroidMotionEventButton.BUTTON_PRIMARY,
                            action == AndroidMotionEventAction.ACTION_DOWN ? AndroidMotionEventButton.BUTTON_PRIMARY : AndroidMotionEventButton.None);
                        break;

                    case MouseButton.Right:
                        control.BackOrScreenOn(
                            action == AndroidMotionEventAction.ACTION_DOWN ? AndroidKeyEventAction.ACTION_DOWN : AndroidKeyEventAction.ACTION_UP);
                        break;

                    case MouseButton.Middle:
                        control.InjectKeycode(
                            action == AndroidMotionEventAction.ACTION_DOWN ? AndroidKeyEventAction.ACTION_DOWN : AndroidKeyEventAction.ACTION_UP,
                            AndroidKeyCode.AKEYCODE_HOME,
                            0,
                            AndroidKeyEventMeta.META_NONE);
                        break;

                    case MouseButton.XButton1:
                        control.InjectKeycode(
                            action == AndroidMotionEventAction.ACTION_DOWN ? AndroidKeyEventAction.ACTION_DOWN : AndroidKeyEventAction.ACTION_UP,
                            AndroidKeyCode.AKEYCODE_APP_SWITCH,
                            0,
                            AndroidKeyEventMeta.META_NONE);
                        break;

                    case MouseButton.XButton2:
                        if (action == AndroidMotionEventAction.ACTION_DOWN)
                        {
                            control.ExpandNotificationPanel();
                        }
                        break;
                }
            }
        }
        public static AndroidMotionEventButton HandleMouseButton(this MouseEventArgs e)
        {
            AndroidMotionEventButton button = AndroidMotionEventButton.None;
            if (e.LeftButton == MouseButtonState.Pressed) button |= AndroidMotionEventButton.BUTTON_PRIMARY;
            if (e.RightButton == MouseButtonState.Pressed) button |= AndroidMotionEventButton.BUTTON_SECONDARY;
            if (e.MiddleButton == MouseButtonState.Pressed) button |= AndroidMotionEventButton.BUTTON_TERTIARY;
            if (e.XButton1 == MouseButtonState.Pressed) button |= AndroidMotionEventButton.BUTTON_BACK;
            if (e.XButton2 == MouseButtonState.Pressed) button |= AndroidMotionEventButton.BUTTON_FORWARD;
            return button;
        }
        #endregion



        #region Key

        static readonly IReadOnlyDictionary<Key, AndroidKeyCode> special_keys = new Dictionary<Key, AndroidKeyCode>() {
            // Navigation keys and ENTER.
            // Used in all modes.
            { Key.Enter, AndroidKeyCode.AKEYCODE_ENTER },
            { Key.Escape, AndroidKeyCode.AKEYCODE_ESCAPE },
            { Key.Back, AndroidKeyCode.AKEYCODE_DEL },
            { Key.Tab, AndroidKeyCode.AKEYCODE_TAB },
            { Key.PageUp, AndroidKeyCode.AKEYCODE_PAGE_UP },
            { Key.Delete, AndroidKeyCode.AKEYCODE_FORWARD_DEL },
            { Key.Home, AndroidKeyCode.AKEYCODE_MOVE_HOME },
            { Key.End, AndroidKeyCode.AKEYCODE_MOVE_END },
            { Key.PageDown, AndroidKeyCode.AKEYCODE_PAGE_DOWN },
            { Key.Right, AndroidKeyCode.AKEYCODE_DPAD_RIGHT },
            { Key.Left, AndroidKeyCode.AKEYCODE_DPAD_LEFT },
            { Key.Down, AndroidKeyCode.AKEYCODE_DPAD_DOWN },
            { Key.Up, AndroidKeyCode.AKEYCODE_DPAD_UP },
            { Key.LeftCtrl, AndroidKeyCode.AKEYCODE_CTRL_LEFT },
            { Key.RightCtrl, AndroidKeyCode.AKEYCODE_CTRL_RIGHT },
            { Key.LeftShift, AndroidKeyCode.AKEYCODE_SHIFT_LEFT },
            { Key.RightShift, AndroidKeyCode.AKEYCODE_SHIFT_RIGHT },
        };

        static readonly IReadOnlyDictionary<Key, AndroidKeyCode> alphaspace_keys = new Dictionary<Key, AndroidKeyCode>()
        {
            { Key.A, AndroidKeyCode.AKEYCODE_A },
            { Key.B, AndroidKeyCode.AKEYCODE_B },
            { Key.C, AndroidKeyCode.AKEYCODE_C },
            { Key.D, AndroidKeyCode.AKEYCODE_D },
            { Key.E, AndroidKeyCode.AKEYCODE_E },
            { Key.F, AndroidKeyCode.AKEYCODE_F },
            { Key.G, AndroidKeyCode.AKEYCODE_G },
            { Key.H, AndroidKeyCode.AKEYCODE_H },
            { Key.I, AndroidKeyCode.AKEYCODE_I },
            { Key.J, AndroidKeyCode.AKEYCODE_J },
            { Key.K, AndroidKeyCode.AKEYCODE_K },
            { Key.L, AndroidKeyCode.AKEYCODE_L },
            { Key.M, AndroidKeyCode.AKEYCODE_M },
            { Key.N, AndroidKeyCode.AKEYCODE_N },
            { Key.O, AndroidKeyCode.AKEYCODE_O },
            { Key.P, AndroidKeyCode.AKEYCODE_P },
            { Key.Q, AndroidKeyCode.AKEYCODE_Q },
            { Key.R, AndroidKeyCode.AKEYCODE_R },
            { Key.S, AndroidKeyCode.AKEYCODE_S },
            { Key.T, AndroidKeyCode.AKEYCODE_T },
            { Key.U, AndroidKeyCode.AKEYCODE_U },
            { Key.V, AndroidKeyCode.AKEYCODE_V },
            { Key.W, AndroidKeyCode.AKEYCODE_W },
            { Key.X, AndroidKeyCode.AKEYCODE_X },
            { Key.Y, AndroidKeyCode.AKEYCODE_Y },
            { Key.Z, AndroidKeyCode.AKEYCODE_Z },
            { Key.Space, AndroidKeyCode.AKEYCODE_SPACE },

        };

        static readonly IReadOnlyDictionary<Key, AndroidKeyCode> numbers_punct_keys = new Dictionary<Key, AndroidKeyCode>()
        {
            //{SC_KEYCODE_HASH,           AndroidKeyCode.AKEYCODE_POUND},
            //{SC_KEYCODE_PERCENT,        AndroidKeyCode.AKEYCODE_PERIOD},            
            {Key.Multiply,              AndroidKeyCode.AKEYCODE_STAR},
            {Key.Divide,                AndroidKeyCode.AKEYCODE_NUMPAD_DIVIDE},
            {Key.Subtract,              AndroidKeyCode.AKEYCODE_MINUS},
            {Key.Add,                   AndroidKeyCode.AKEYCODE_NUMPAD_ADD},

            {Key.Decimal,               AndroidKeyCode.AKEYCODE_NUMPAD_DOT},

            {Key.OemPeriod,             AndroidKeyCode.AKEYCODE_PERIOD},
            {Key.OemQuotes,             AndroidKeyCode.AKEYCODE_APOSTROPHE},
            {Key.OemPlus,               AndroidKeyCode.AKEYCODE_EQUALS},
            {Key.OemComma,              AndroidKeyCode.AKEYCODE_COMMA},
            {Key.OemQuestion,           AndroidKeyCode.AKEYCODE_SLASH},
            {Key.OemMinus,              AndroidKeyCode.AKEYCODE_MINUS},
            //{Key.OemPlus,               AndroidKeyCode.AKEYCODE_EQUALS},
            {Key.OemOpenBrackets,       AndroidKeyCode.AKEYCODE_LEFT_BRACKET},

            {Key.D0,                    AndroidKeyCode.AKEYCODE_0},
            {Key.D1,                    AndroidKeyCode.AKEYCODE_1},
            {Key.D2,                    AndroidKeyCode.AKEYCODE_2},
            {Key.D3,                    AndroidKeyCode.AKEYCODE_3},
            {Key.D4,                    AndroidKeyCode.AKEYCODE_4},
            {Key.D5,                    AndroidKeyCode.AKEYCODE_5},
            {Key.D6,                    AndroidKeyCode.AKEYCODE_6},
            {Key.D7,                    AndroidKeyCode.AKEYCODE_7},
            {Key.D8,                    AndroidKeyCode.AKEYCODE_8},
            {Key.D9,                    AndroidKeyCode.AKEYCODE_9},

            {Key.Oem1,                  AndroidKeyCode.AKEYCODE_SEMICOLON},
            {Key.Oem3,                  AndroidKeyCode.AKEYCODE_GRAVE},
            {Key.Oem5,                  AndroidKeyCode.AKEYCODE_BACKSLASH},
            {Key.Oem6,                  AndroidKeyCode.AKEYCODE_RIGHT_BRACKET},

            //{Key.D2 + shift,             AndroidKeyCode.AKEYCODE_AT},

            {Key.NumPad1,               AndroidKeyCode.AKEYCODE_NUMPAD_1},
            {Key.NumPad2,               AndroidKeyCode.AKEYCODE_NUMPAD_2},
            {Key.NumPad3,               AndroidKeyCode.AKEYCODE_NUMPAD_3},
            {Key.NumPad4,               AndroidKeyCode.AKEYCODE_NUMPAD_4},
            {Key.NumPad5,               AndroidKeyCode.AKEYCODE_NUMPAD_5},
            {Key.NumPad6,               AndroidKeyCode.AKEYCODE_NUMPAD_6},
            {Key.NumPad7,               AndroidKeyCode.AKEYCODE_NUMPAD_7},
            {Key.NumPad8,               AndroidKeyCode.AKEYCODE_NUMPAD_8},
            {Key.NumPad9,               AndroidKeyCode.AKEYCODE_NUMPAD_9},
            {Key.NumPad0,               AndroidKeyCode.AKEYCODE_NUMPAD_0},
            //{Key.Multiply,              AndroidKeyCode.AKEYCODE_NUMPAD_MULTIPLY},
            //{Key.Subtract,            AndroidKeyCode.AKEYCODE_NUMPAD_SUBTRACT},
            //{SC_KEYCODE_KP_EQUALS,      AndroidKeyCode.AKEYCODE_NUMPAD_EQUALS},
            //{Key.D9,   AndroidKeyCode.AKEYCODE_NUMPAD_LEFT_PAREN},
            //{Key.D0,  AndroidKeyCode.AKEYCODE_NUMPAD_RIGHT_PAREN},
        };

        public static async Task<AndroidKeyCode> ResolveKeyAsync(this KeyEventArgs e, IControl control)
        {
            Key key = e.Key;
#if DEBUG
            Debug.WriteLine($"Key: {key} is {e.KeyStates}");
#endif
            switch (key)
            {
                case Key.LeftCtrl:
                case Key.RightCtrl:
                case Key.LeftAlt:
                case Key.RightAlt:
                case Key.LeftShift:
                case Key.RightShift:
                    return AndroidKeyCode.AKEYCODE_UNKNOWN;//skip system key, it using in AndroidKeyEventMeta
            }

            if (control != null)
            {
                if (IsKeyDown(e, Key.C) && IsCtrl(e))
                {
                    string text = await control.GetClipboardAsync(CopyKey.Copy);
                    if (!string.IsNullOrEmpty(text)) Clipboard.SetText(text);
                    return AndroidKeyCode.AKEYCODE_UNKNOWN;
                }
                if (IsKeyDown(e, Key.V) && IsCtrl(e))
                {
                    string text = Clipboard.GetText();
                    if (!string.IsNullOrEmpty(text)) control.SetClipboard(text, true);
                    return AndroidKeyCode.AKEYCODE_UNKNOWN;
                }
                if (IsKeyDown(e, Key.X) && IsCtrl(e))
                {
                    string text = await control.GetClipboardAsync(CopyKey.Cut);
                    if (!string.IsNullOrEmpty(text)) Clipboard.SetText(text);
                    return AndroidKeyCode.AKEYCODE_UNKNOWN;
                }
            }

            if (special_keys.ContainsKey(key))
                return special_keys[key];

            if (numbers_punct_keys.ContainsKey(key))
                return numbers_punct_keys[key];

            if (/*!IsCtrl(e) && !IsAlt(e) &&*/ alphaspace_keys.ContainsKey(key))
                return alphaspace_keys[key];

            return AndroidKeyCode.AKEYCODE_UNKNOWN;
        }
        public static AndroidKeyEventMeta ResolveMetaKey(this KeyEventArgs e)
        {
            AndroidKeyEventMeta result = AndroidKeyEventMeta.META_NONE;

            if (e.KeyboardDevice.GetKeyStates(Key.LeftShift).HasFlag(KeyStates.Down)) result |= AndroidKeyEventMeta.META_SHIFT_LEFT_ON;
            if (e.KeyboardDevice.GetKeyStates(Key.RightShift).HasFlag(KeyStates.Down)) result |= AndroidKeyEventMeta.META_SHIFT_RIGHT_ON;

            if (e.KeyboardDevice.GetKeyStates(Key.LeftAlt).HasFlag(KeyStates.Down)) result |= AndroidKeyEventMeta.META_ALT_LEFT_ON;
            if (e.KeyboardDevice.GetKeyStates(Key.RightAlt).HasFlag(KeyStates.Down)) result |= AndroidKeyEventMeta.META_ALT_RIGHT_ON;

            if (e.KeyboardDevice.GetKeyStates(Key.LeftCtrl).HasFlag(KeyStates.Down)) result |= AndroidKeyEventMeta.META_CTRL_LEFT_ON;
            if (e.KeyboardDevice.GetKeyStates(Key.RightCtrl).HasFlag(KeyStates.Down)) result |= AndroidKeyEventMeta.META_CTRL_RIGHT_ON;

            if (IsNumlock(e)) result |= AndroidKeyEventMeta.META_NUM_LOCK_ON;
            if (IsCaptlock(e)) result |= AndroidKeyEventMeta.META_CAPS_LOCK_ON;
#if DEBUG
            Debug.WriteLine($"MetaKey: {result}");
#endif
            return result;
        }
        public static bool IsShift(this KeyEventArgs e) => e.KeyboardDevice.GetKeyStates(Key.LeftShift).HasFlag(KeyStates.Down) ||
                                        e.KeyboardDevice.GetKeyStates(Key.RightShift).HasFlag(KeyStates.Down);
        public static bool IsAlt(this KeyEventArgs e) => e.KeyboardDevice.GetKeyStates(Key.LeftAlt).HasFlag(KeyStates.Down) ||
                                        e.KeyboardDevice.GetKeyStates(Key.RightAlt).HasFlag(KeyStates.Down);
        public static bool IsCtrl(this KeyEventArgs e) => e.KeyboardDevice.GetKeyStates(Key.LeftCtrl).HasFlag(KeyStates.Down) ||
                                        e.KeyboardDevice.GetKeyStates(Key.RightCtrl).HasFlag(KeyStates.Down);
        public static bool IsNumlock(this KeyEventArgs e) => e.KeyboardDevice.GetKeyStates(Key.NumLock).HasFlag(KeyStates.Toggled);
        public static bool IsCaptlock(this KeyEventArgs e) => e.KeyboardDevice.GetKeyStates(Key.CapsLock).HasFlag(KeyStates.Toggled);

        public static bool IsKeyDown(this KeyEventArgs e, Key key) => e.KeyboardDevice.GetKeyStates(key).HasFlag(KeyStates.Down);

        #endregion
    }
}
