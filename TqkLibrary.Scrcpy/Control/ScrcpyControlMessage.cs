using System;
using System.Drawing;
using System.Linq;
using System.Text;
using TqkLibrary.Scrcpy.Enums;
using TqkLibrary.Scrcpy.Exceptions;

namespace TqkLibrary.Scrcpy.Control
{
    /// <summary>
    /// https://github.com/Genymobile/scrcpy/blob/master/server/src/main/java/com/genymobile/scrcpy/ControlMessage.java
    /// https://github.com/Genymobile/scrcpy/blob/master/app/src/control_msg.c
    /// </summary>
    internal sealed class ScrcpyControlMessage
    {
        private ScrcpyControlType ControlType { get; set; }
        private string Text { get; set; }
        private AndroidKeyEventMeta MetaState { get; set; } // KeyEvent.META_*
        private AndroidMotionEventAction MotionEventAction { get; set; }//MotionEvent.ACTION_
        private AndroidKeyEventAction KeyEventAction { get; set; }//KeyEvent.ACTION_*
        private AndroidKeyCode Keycode { get; set; } // KeyEvent.KEYCODE_*
        private AndroidMotionEventButton ActionButton { get; set; } // MotionEvent.BUTTON_*
        private AndroidMotionEventButton Buttons { get; set; } // MotionEvent.BUTTON_*
        private long PointerId { get; set; }
        private float Pressure { get; set; }
        private Rectangle Position { get; set; }
        private float HScroll { get; set; }
        private float VScroll { get; set; }
        private bool Paste { get; set; }
        private uint Repeat { get; set; }
        private long Sequence { get; set; }
        private ScrcpyScreenPowerMode PowerMode { get; set; }
        private CopyKey CopyKey { get; set; }

        private ScrcpyControlMessage() { }

        internal static ScrcpyControlMessage CreateInjectKeycode(
                AndroidKeyEventAction action,
                AndroidKeyCode keycode,
                uint repeat = 1,
                AndroidKeyEventMeta metaState = AndroidKeyEventMeta.META_META_ON)
          => new ScrcpyControlMessage
          {
              ControlType = ScrcpyControlType.TYPE_INJECT_KEYCODE,
              KeyEventAction = action,
              Keycode = keycode,
              Repeat = repeat,
              MetaState = metaState
          };

        internal static ScrcpyControlMessage CreateInjectText(string text)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));
            return new ScrcpyControlMessage
            {
                ControlType = ScrcpyControlType.TYPE_INJECT_TEXT,
                Text = text
            };
        }

        internal static ScrcpyControlMessage CreateInjectTouchEvent(
                AndroidMotionEventAction action,
                long pointerId,
                Rectangle position,
                float pressure,
                AndroidMotionEventButton buttons,
                AndroidMotionEventButton actionButton)
        {
            if (pressure != 1.0f && pressure != 0.0f) throw new InvalidRangeException($"{nameof(pressure)} must be 0.0f or 1.0f");
            return new ScrcpyControlMessage
            {
                ControlType = ScrcpyControlType.TYPE_INJECT_TOUCH_EVENT,
                MotionEventAction = action,
                PointerId = pointerId,
                Pressure = pressure,
                Position = position,
                Buttons = buttons,
                ActionButton = actionButton,
            };
        }

        internal static ScrcpyControlMessage CreateInjectScrollEvent(
            Rectangle position,
            float vScroll,
            float hScroll,
            AndroidMotionEventButton button)
        {
            if (vScroll > 1.0f || vScroll < -1.0f) throw new InvalidRangeException($"{nameof(vScroll)} must be in range -1.0f <= {nameof(vScroll)} <= 1.0f");
            if (hScroll > 1.0f || hScroll < -1.0f) throw new InvalidRangeException($"{nameof(hScroll)} must be in range -1.0f <= {nameof(hScroll)} <= 1.0f");
            return new ScrcpyControlMessage
            {
                ControlType = ScrcpyControlType.TYPE_INJECT_SCROLL_EVENT,
                Position = position,
                HScroll = hScroll,
                VScroll = vScroll,
                Buttons = button
            };
        }

        internal static ScrcpyControlMessage CreateSetClipboard(string text, bool paste, long sequence)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));
            return new ScrcpyControlMessage
            {
                ControlType = ScrcpyControlType.TYPE_SET_CLIPBOARD,
                Sequence = sequence,
                Text = text,
                Paste = paste
            };
        }
        internal static ScrcpyControlMessage CreateGetClipboard(CopyKey copyKey)
        {
            return new ScrcpyControlMessage
            {
                ControlType = ScrcpyControlType.TYPE_GET_CLIPBOARD,
                CopyKey = copyKey
            };
        }

        internal static ScrcpyControlMessage CreateSetScreenPowerMode(ScrcpyScreenPowerMode powerMode)
          => new ScrcpyControlMessage
          {
              ControlType = ScrcpyControlType.TYPE_SET_SCREEN_POWER_MODE,
              PowerMode = powerMode
          };

        internal static ScrcpyControlMessage BackOrScreenOn(AndroidKeyEventAction KeyEventAction)
          => new ScrcpyControlMessage
          {
              ControlType = ScrcpyControlType.TYPE_BACK_OR_SCREEN_ON,
              KeyEventAction = KeyEventAction
          };

        internal static ScrcpyControlMessage ExpandNotificationPanel() => CreateEmpty(ScrcpyControlType.TYPE_EXPAND_NOTIFICATION_PANEL);

        internal static ScrcpyControlMessage ExpandSettingsPanel() => CreateEmpty(ScrcpyControlType.TYPE_EXPAND_SETTINGS_PANEL);

        internal static ScrcpyControlMessage CollapsePanel() => CreateEmpty(ScrcpyControlType.TYPE_COLLAPSE_PANELS);

        internal static ScrcpyControlMessage RotateDevice() => CreateEmpty(ScrcpyControlType.TYPE_ROTATE_DEVICE);

        static ScrcpyControlMessage CreateEmpty(ScrcpyControlType type)
          => new ScrcpyControlMessage
          {
              ControlType = type
          };

        /// <summary>
        /// Convert a float between 0 and 1 to an unsigned 16-bit fixed-point value
        /// </summary>
        ushort ToUnsignedFixedPoint16(float f)
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
        short ToSignedFixedPoint16(float f)
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


        /// <summary>
        /// https://github.com/Genymobile/scrcpy/blob/master/server/src/main/java/com/genymobile/scrcpy/ControlMessageReader.java
        /// https://github.com/Genymobile/scrcpy/blob/master/app/src/control_msg.c
        /// </summary>
        /// <returns></returns>
        internal byte[] GetCommand()
        {
            byte[] buffer = null;
            switch (ControlType)
            {
                case ScrcpyControlType.TYPE_INJECT_KEYCODE:
                    {
                        buffer = new byte[14];
                        buffer[0] = (byte)ControlType;
                        buffer[1] = (byte)KeyEventAction;
                        Array.Copy(BitConverter.GetBytes((uint)Keycode).Reverse().ToArray(), 0, buffer, 2, 4);//2-5
                        Array.Copy(BitConverter.GetBytes(Repeat).Reverse().ToArray(), 0, buffer, 6, 4);//6-9
                        Array.Copy(BitConverter.GetBytes((uint)MetaState).Reverse().ToArray(), 0, buffer, 10, 4);//10-13
                    }
                    break;

                case ScrcpyControlType.TYPE_INJECT_TEXT:
                    {
                        byte[] utf8_text = Encoding.UTF8.GetBytes(Text);
                        buffer = new byte[5 + utf8_text.Length];
                        buffer[0] = (byte)ControlType;
                        Array.Copy(BitConverter.GetBytes(utf8_text.Length).Reverse().ToArray(), 0, buffer, 1, 4);//1-4
                        Array.Copy(utf8_text, 0, buffer, 5, utf8_text.Length);//5-....
                    }
                    break;

                case ScrcpyControlType.TYPE_INJECT_TOUCH_EVENT:
                    {
                        buffer = new byte[32];
                        buffer[0] = (byte)ControlType;
                        buffer[1] = (byte)MotionEventAction;
                        Array.Copy(BitConverter.GetBytes((ulong)PointerId).Reverse().ToArray(), 0, buffer, 2, 8);//2-9
                        Array.Copy(BitConverter.GetBytes(Position.X).Reverse().ToArray(), 0, buffer, 10, 4);//10-13
                        Array.Copy(BitConverter.GetBytes(Position.Y).Reverse().ToArray(), 0, buffer, 14, 4);//14-17
                        Array.Copy(BitConverter.GetBytes((UInt16)Position.Width).Reverse().ToArray(), 0, buffer, 18, 2);//18-19
                        Array.Copy(BitConverter.GetBytes((UInt16)Position.Height).Reverse().ToArray(), 0, buffer, 20, 2);//20-21
                        Array.Copy(BitConverter.GetBytes(ToUnsignedFixedPoint16(Pressure)).Reverse().ToArray(), 0, buffer, 22, 2);//22-23
                        Array.Copy(BitConverter.GetBytes((int)ActionButton).Reverse().ToArray(), 0, buffer, 24, 4);//24-27
                        Array.Copy(BitConverter.GetBytes((int)Buttons).Reverse().ToArray(), 0, buffer, 28, 4);//28-31
                    }
                    break;

                case ScrcpyControlType.TYPE_INJECT_SCROLL_EVENT:
                    {
                        buffer = new byte[21];
                        buffer[0] = (byte)ControlType;//0
                        Array.Copy(BitConverter.GetBytes(Position.X).Reverse().ToArray(), 0, buffer, 1, 4);//1-4
                        Array.Copy(BitConverter.GetBytes(Position.Y).Reverse().ToArray(), 0, buffer, 5, 4);//5-8
                        Array.Copy(BitConverter.GetBytes((UInt16)Position.Width).Reverse().ToArray(), 0, buffer, 9, 2);//9-10
                        Array.Copy(BitConverter.GetBytes((UInt16)Position.Height).Reverse().ToArray(), 0, buffer, 11, 2);//11-12
                        Array.Copy(BitConverter.GetBytes(ToSignedFixedPoint16(HScroll)).Reverse().ToArray(), 0, buffer, 13, 2);//13-14
                        Array.Copy(BitConverter.GetBytes(ToSignedFixedPoint16(VScroll)).Reverse().ToArray(), 0, buffer, 15, 2);//15-16
                        Array.Copy(BitConverter.GetBytes((int)Buttons).Reverse().ToArray(), 0, buffer, 17, 4);//17-20
                    }
                    break;

                case ScrcpyControlType.TYPE_SET_CLIPBOARD:
                    {
                        byte[] utf8_text = Encoding.UTF8.GetBytes(Text);
                        buffer = new byte[1 + 8 + 1 + 4 + utf8_text.Length];

                        buffer[0] = (byte)ControlType;//0
                        Array.Copy(BitConverter.GetBytes(Sequence).Reverse().ToArray(), 0, buffer, 1, 8);//1-8
                        Array.Copy(BitConverter.GetBytes(Paste), 0, buffer, 9, 1);//9-9
                        Array.Copy(BitConverter.GetBytes(utf8_text.Length).Reverse().ToArray(), 0, buffer, 10, 4);//10-13
                        Array.Copy(utf8_text, 0, buffer, 14, utf8_text.Length);//14-....
                    }
                    break;

                case ScrcpyControlType.TYPE_SET_SCREEN_POWER_MODE:
                    {
                        buffer = new byte[2];
                        buffer[0] = (byte)ControlType;
                        buffer[1] = (byte)PowerMode;
                    }
                    break;

                case ScrcpyControlType.TYPE_BACK_OR_SCREEN_ON:
                    {
                        buffer = new byte[2];
                        buffer[0] = (byte)ControlType;
                        buffer[1] = (byte)KeyEventAction; // action for the BACK key
                                                          // screen may only be turned on on ACTION_DOWN
                        break;
                    }
                case ScrcpyControlType.TYPE_GET_CLIPBOARD:
                    {
                        buffer = new byte[2];
                        buffer[0] = (byte)ControlType;
                        buffer[1] = (byte)CopyKey;
                        break;
                    }
                case ScrcpyControlType.TYPE_EXPAND_NOTIFICATION_PANEL:
                case ScrcpyControlType.TYPE_EXPAND_SETTINGS_PANEL:
                case ScrcpyControlType.TYPE_COLLAPSE_PANELS:
                case ScrcpyControlType.TYPE_ROTATE_DEVICE:
                    {
                        buffer = new byte[1];
                        buffer[0] = (byte)ControlType;
                    }
                    break;

                default: throw new NotSupportedException(ControlType.ToString());
            }
            return buffer;
        }
    }
}
