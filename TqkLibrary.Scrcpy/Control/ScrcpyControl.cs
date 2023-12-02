using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TqkLibrary.Scrcpy.Enums;
using TqkLibrary.Scrcpy.Interfaces;

namespace TqkLibrary.Scrcpy.Control
{
    internal delegate void NativeOnClipboardReceivedDelegate(IntPtr intPtr, int length);
    internal delegate void NativeOnClipboardAcknowledgementDelegate(long length);
    internal class ScrcpyControl : IControl
    {
        static readonly Random random = new Random();
        internal ScrcpyControl(Scrcpy scrcpy)
        {
            this.Scrcpy = scrcpy;

            this.NativeOnClipboardReceivedDelegate = NativeOnClipboardReceived;
            this.Scrcpy.RegisterClipboardEvent(NativeOnClipboardReceivedDelegate);

            this.NativeOnClipboardAcknowledgementDelegate = NativeClipboardAcknowledgementReceived;
            this.Scrcpy.RegisterClipboardAcknowledgementEvent(NativeOnClipboardAcknowledgementDelegate);
        }
        public Scrcpy Scrcpy { get; }

        bool SendControl(ScrcpyControlMessage scrcpyControlMessage) => this.Scrcpy.SendControl(scrcpyControlMessage);

        #region BasicCommand
        public bool BackOrScreenOn(AndroidKeyEventAction KeyEventAction)
            => SendControl(ScrcpyControlMessage.BackOrScreenOn(KeyEventAction));
        public bool CollapsePanel()
            => SendControl(ScrcpyControlMessage.CollapsePanel());
        public bool ExpandNotificationPanel()
            => SendControl(ScrcpyControlMessage.ExpandNotificationPanel());
        public bool ExpandSettingsPanel()
            => SendControl(ScrcpyControlMessage.ExpandSettingsPanel());
        public bool GetClipboard(CopyKey copyKey)
            => SendControl(ScrcpyControlMessage.CreateGetClipboard(copyKey));
        public bool InjectKeycode(
            AndroidKeyEventAction action,
            AndroidKeyCode keycode,
            uint repeat = 1,
            AndroidKeyEventMeta metaState = AndroidKeyEventMeta.META_NONE)
            => SendControl(ScrcpyControlMessage.CreateInjectKeycode(action, keycode, repeat, metaState));
        public bool InjectScrollEvent(Rectangle position, float vScroll, float hScroll = 0, AndroidMotionEventButton button = AndroidMotionEventButton.BUTTON_PRIMARY)
            => SendControl(ScrcpyControlMessage.CreateInjectScrollEvent(position, vScroll, hScroll, button));
        public bool InjectText(string text)
            => SendControl(ScrcpyControlMessage.CreateInjectText(text));
        public bool InjectTouchEvent(AndroidMotionEventAction action, long pointerId, Rectangle position, float pressure, AndroidMotionEventButton buttons, AndroidMotionEventButton actionButton)
            => SendControl(ScrcpyControlMessage.CreateInjectTouchEvent(action, pointerId, position, pressure, buttons, actionButton));
        public bool RotateDevice()
            => SendControl(ScrcpyControlMessage.RotateDevice());
        public bool SetClipboard(string text, bool paste)
            => SendControl(ScrcpyControlMessage.CreateSetClipboard(text, paste, random.Next()));
        public bool SetClipboard(string text, bool paste, long sequence)
            => SendControl(ScrcpyControlMessage.CreateSetClipboard(text, paste, sequence));
        public bool SetScreenPowerMode(ScrcpyScreenPowerMode powerMode)
            => SendControl(ScrcpyControlMessage.CreateSetScreenPowerMode(powerMode));
        #endregion

        #region Event
        public event OnDataReceived<string> OnClipboardReceived;
        public event OnDataReceived<long> OnSetClipboardAcknowledgement;
        #endregion

        #region Native Event
        //hold for gc not release this delegate
        readonly NativeOnClipboardReceivedDelegate NativeOnClipboardReceivedDelegate;
        readonly NativeOnClipboardAcknowledgementDelegate NativeOnClipboardAcknowledgementDelegate;
        void NativeOnClipboardReceived(IntPtr intPtr, int length)
        {
            if (length == 0)
            {
                ThreadPool.QueueUserWorkItem((o) =>//use thread pool for not hold native thread
                {
                    OnClipboardReceived?.Invoke(this, string.Empty);
                });
            }
            else
            {
                byte[] buffer = new byte[length];
                Marshal.Copy(intPtr, buffer, 0, length);
                ThreadPool.QueueUserWorkItem((o) =>//use thread pool for not hold native thread
                {
                    OnClipboardReceived?.Invoke(this, Encoding.UTF8.GetString(buffer));
                });
            }
        }
        void NativeClipboardAcknowledgementReceived(long sequence)
        {
            ThreadPool.QueueUserWorkItem((o) =>//use thread pool for not hold native thread
            {
                OnSetClipboardAcknowledgement?.Invoke(this, sequence);
            });
        }
        #endregion
    }
}
