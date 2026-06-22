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
            stream.WriteHostToNetworkOrder(ScrcpyControlType.TYPE_INJECT_TEXT, (UInt32)utf8_text.Length, utf8_text);
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
            if (vScroll > 16.0f || vScroll < -16.0f) throw new InvalidRangeException($"{nameof(vScroll)} must be in range -16.0f <= {nameof(vScroll)} <= 16.0f");
            if (hScroll > 16.0f || hScroll < -16.0f) throw new InvalidRangeException($"{nameof(hScroll)} must be in range -16.0f <= {nameof(hScroll)} <= 16.0f");

            using MemoryStream stream = new MemoryStream();
            stream.WriteHostToNetworkOrder(
                ScrcpyControlType.TYPE_INJECT_SCROLL_EVENT,
                position,
                ToSignedFixedPoint16(Math.Max(-1.0f, Math.Min(1.0f, hScroll / 16.0f))),
                ToSignedFixedPoint16(Math.Max(-1.0f, Math.Min(1.0f, vScroll / 16.0f))),
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
                (UInt32)utf8_text.Length,
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

        internal static byte[] SetDisplayPower(bool on)
        {
            using MemoryStream memoryStream = new MemoryStream();
            memoryStream.WriteHostToNetworkOrder(ScrcpyControlType.TYPE_SET_DISPLAY_POWER);
            memoryStream.WriteByte((byte)(on ? 1 : 0));
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

        internal static byte[] UhdiCreate(UInt16 id, byte[] data, string? name = null, UInt16 vendorId = 0, UInt16 productId = 0)
        {
            byte[] nameBytes = name != null ? Encoding.UTF8.GetBytes(name) : Array.Empty<byte>();
            if (nameBytes.Length > 127) Array.Resize(ref nameBytes, 127);
            using MemoryStream memoryStream = new MemoryStream();
            memoryStream.WriteHostToNetworkOrder(ScrcpyControlType.TYPE_UHID_CREATE, id, vendorId, productId);
            memoryStream.WriteByte((byte)nameBytes.Length);
            memoryStream.Write(nameBytes, 0, nameBytes.Length);
            memoryStream.WriteHostToNetworkOrder((UInt16)data.Length, data);
            return memoryStream.ToArray();
        }

        internal static byte[] UhdiInput(UInt16 id, byte[] data)
        {
            using MemoryStream memoryStream = new MemoryStream();
            memoryStream.WriteHostToNetworkOrder(ScrcpyControlType.TYPE_UHID_INPUT, id, (UInt16)data.Length, data);
            return memoryStream.ToArray();
        }

        internal static byte[] UhidDestroy(UInt16 id)
        {
            using MemoryStream memoryStream = new MemoryStream();
            memoryStream.WriteHostToNetworkOrder(ScrcpyControlType.TYPE_UHID_DESTROY, id);
            return memoryStream.ToArray();
        }

        internal static byte[] OpenHardKeyboardSetting() => CreateEmpty(ScrcpyControlType.OPEN_HARD_KEYBOARD_SETTINGS);

        internal static byte[] StartApp(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            byte[] nameBytes = Encoding.UTF8.GetBytes(name);
            if (nameBytes.Length > 255) Array.Resize(ref nameBytes, 255);
            using MemoryStream memoryStream = new MemoryStream();
            memoryStream.WriteHostToNetworkOrder(ScrcpyControlType.TYPE_START_APP);
            memoryStream.WriteByte((byte)nameBytes.Length);
            memoryStream.Write(nameBytes, 0, nameBytes.Length);
            return memoryStream.ToArray();
        }

        internal static byte[] ResetVideo() => CreateEmpty(ScrcpyControlType.TYPE_RESET_VIDEO);



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
