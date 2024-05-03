using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using TqkLibrary.Scrcpy.Enums;
using TqkLibrary.Scrcpy.Exceptions;

namespace TqkLibrary.Scrcpy.Control
{
    /// <summary>
    /// https://github.com/Genymobile/scrcpy/blob/master/server/src/main/java/com/genymobile/scrcpy/ControlMessage.java
    /// https://github.com/Genymobile/scrcpy/blob/master/app/src/control_msg.c
    /// </summary>
    internal static class ScrcpyControlHelper
    {
        internal static byte[] InjectKeycode(
                AndroidKeyEventAction action,
                AndroidKeyCode keycode,
                uint repeat = 1,
                AndroidKeyEventMeta metaState = AndroidKeyEventMeta.META_META_ON)
        {
            using MemoryStream stream = new MemoryStream();
            stream.WriteHostToNetworkOrder(ScrcpyControlType.TYPE_INJECT_KEYCODE, action, keycode, repeat, metaState);
            return stream.ToArray();
        }


        internal static byte[] InjectText(string text)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));

            byte[] utf8_text = Encoding.UTF8.GetBytes(text);
            using MemoryStream stream = new MemoryStream();
            stream.WriteHostToNetworkOrder(ScrcpyControlType.TYPE_INJECT_TEXT, utf8_text.Length, utf8_text);
            return stream.ToArray();
        }

        internal static byte[] InjectTouchEvent(
                AndroidMotionEventAction action,
                long pointerId,
                Rectangle position,
                float pressure,
                AndroidMotionEventButton buttons,
                AndroidMotionEventButton actionButton)
        {
            if (pressure != 1.0f && pressure != 0.0f) throw new InvalidRangeException($"{nameof(pressure)} must be 0.0f or 1.0f");

            using MemoryStream stream = new MemoryStream();
            stream.WriteHostToNetworkOrder(
                ScrcpyControlType.TYPE_INJECT_TOUCH_EVENT,
                action,
                pointerId,
                position,
                ToUnsignedFixedPoint16(pressure),
                actionButton,
                buttons
                );
            return stream.ToArray();
        }

        internal static byte[] InjectScrollEvent(
            Rectangle position,
            float vScroll,
            float hScroll,
            AndroidMotionEventButton button)
        {
            if (vScroll > 1.0f || vScroll < -1.0f) throw new InvalidRangeException($"{nameof(vScroll)} must be in range -1.0f <= {nameof(vScroll)} <= 1.0f");
            if (hScroll > 1.0f || hScroll < -1.0f) throw new InvalidRangeException($"{nameof(hScroll)} must be in range -1.0f <= {nameof(hScroll)} <= 1.0f");

            using MemoryStream stream = new MemoryStream();
            stream.WriteHostToNetworkOrder(
                ScrcpyControlType.TYPE_INJECT_SCROLL_EVENT,
                position,
                ToSignedFixedPoint16(hScroll),
                ToSignedFixedPoint16(vScroll),
                button
                );
            return stream.ToArray();
        }

        internal static byte[] SetClipboard(string text, bool paste, long sequence)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));

            byte[] utf8_text = Encoding.UTF8.GetBytes(text);
            using MemoryStream stream = new MemoryStream();
            stream.WriteHostToNetworkOrder(
                ScrcpyControlType.TYPE_SET_CLIPBOARD,
                sequence,
                paste,
                utf8_text.Length,
                utf8_text
                );
            return stream.ToArray();
        }
        internal static byte[] GetClipboard(CopyKey copyKey)
        {
            using MemoryStream stream = new MemoryStream();
            stream.WriteHostToNetworkOrder(
                ScrcpyControlType.TYPE_GET_CLIPBOARD,
                copyKey
                );
            return stream.ToArray();
        }

        internal static byte[] SetScreenPowerMode(ScrcpyScreenPowerMode powerMode)
        {
            using MemoryStream memoryStream = new MemoryStream();
            memoryStream.WriteHostToNetworkOrder(ScrcpyControlType.TYPE_SET_SCREEN_POWER_MODE, powerMode);
            return memoryStream.ToArray();
        }

        internal static byte[] BackOrScreenOn(AndroidKeyEventAction KeyEventAction)
        {
            using MemoryStream memoryStream = new MemoryStream();
            memoryStream.WriteHostToNetworkOrder(ScrcpyControlType.TYPE_BACK_OR_SCREEN_ON, KeyEventAction);
            return memoryStream.ToArray();
        }

        internal static byte[] ExpandNotificationPanel() => CreateEmpty(ScrcpyControlType.TYPE_EXPAND_NOTIFICATION_PANEL);

        internal static byte[] ExpandSettingsPanel() => CreateEmpty(ScrcpyControlType.TYPE_EXPAND_SETTINGS_PANEL);

        internal static byte[] CollapsePanel() => CreateEmpty(ScrcpyControlType.TYPE_COLLAPSE_PANELS);

        internal static byte[] RotateDevice() => CreateEmpty(ScrcpyControlType.TYPE_ROTATE_DEVICE);

        static byte[] CreateEmpty(ScrcpyControlType type)
        {
            using MemoryStream memoryStream = new MemoryStream();
            memoryStream.WriteHostToNetworkOrder(type);
            return memoryStream.ToArray();
        }



        /// <summary>
        /// Convert a float between 0 and 1 to an unsigned 16-bit fixed-point value
        /// </summary>
        static ushort ToUnsignedFixedPoint16(float f)
        {
            unchecked
            {
                uint u = (uint)(f * 65536.0f);// 0x1p16f; // 2^16
                if (u >= 0xffff)
                {
                    u = 0xffff;
                }
                return (ushort)u;
            }
        }


        /// <summary>
        /// Convert a float between -1 and 1 to a signed 16-bit fixed-point value
        /// </summary>
        static short ToSignedFixedPoint16(float f)
        {
            unchecked
            {
                int u = (int)(f * 32768.0F);// 0x1p15f; // 2^15
                if (u > 0x8000) throw new InvalidOperationException();
                if (u >= 0x7fff)
                {
                    u = 0x7fff;
                }
                return (short)u;
            }
        }
    }
}
