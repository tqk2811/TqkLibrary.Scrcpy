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
    internal class ScrcpyControl : IControl
    {
        static readonly Random random = new Random();
        internal ScrcpyControl(Scrcpy scrcpy)
        {
            this.Scrcpy = scrcpy;

            this.NativeOnClipboardReceivedDelegate = NativeOnClipboardReceived;
            if (!this.Scrcpy.RegisterClipboardEvent(NativeOnClipboardReceivedDelegate))
                throw new InvalidOperationException();

            this.NativeOnClipboardAcknowledgementDelegate = NativeClipboardAcknowledgementReceived;
            if (!this.Scrcpy.RegisterClipboardAcknowledgementEvent(NativeOnClipboardAcknowledgementDelegate))
                throw new InvalidOperationException();
        }
        public Scrcpy Scrcpy { get; }

        bool SendControl(byte[] command) => this.Scrcpy.SendControl(command);

        #region BasicCommand
        public bool BackOrScreenOn(AndroidKeyEventAction KeyEventAction)
            => SendControl(ScrcpyControlHelper.BackOrScreenOn(KeyEventAction));
        public bool CollapsePanel()
            => SendControl(ScrcpyControlHelper.CollapsePanel());
        public bool ExpandNotificationPanel()
            => SendControl(ScrcpyControlHelper.ExpandNotificationPanel());
        public bool ExpandSettingsPanel()
            => SendControl(ScrcpyControlHelper.ExpandSettingsPanel());
        public bool GetClipboard(CopyKey copyKey)
            => SendControl(ScrcpyControlHelper.GetClipboard(copyKey));
        public bool InjectKeycode(
            AndroidKeyEventAction action,
            AndroidKeyCode keycode,
            uint repeat = 1,
            AndroidKeyEventMeta metaState = AndroidKeyEventMeta.META_NONE)
            => SendControl(ScrcpyControlHelper.InjectKeycode(action, keycode, repeat, metaState));
        public bool InjectScrollEvent(Rectangle position, float vScroll, float hScroll = 0, AndroidMotionEventButton button = AndroidMotionEventButton.BUTTON_PRIMARY)
            => SendControl(ScrcpyControlHelper.InjectScrollEvent(position, vScroll, hScroll, button));
        public bool InjectText(string text)
            => SendControl(ScrcpyControlHelper.InjectText(text));
        public bool InjectTouchEvent(AndroidMotionEventAction action, long pointerId, Rectangle position, float pressure, AndroidMotionEventButton buttons, AndroidMotionEventButton actionButton)
            => SendControl(ScrcpyControlHelper.InjectTouchEvent(action, pointerId, position, pressure, buttons, actionButton));
        public bool RotateDevice()
            => SendControl(ScrcpyControlHelper.RotateDevice());
        public bool SetClipboard(string text, bool paste)
            => SendControl(ScrcpyControlHelper.SetClipboard(text, paste, random.Next()));
        public bool SetClipboard(string text, bool paste, long sequence)
            => SendControl(ScrcpyControlHelper.SetClipboard(text, paste, sequence));
        public bool SetScreenPowerMode(ScrcpyScreenPowerMode powerMode)
            => SendControl(ScrcpyControlHelper.SetScreenPowerMode(powerMode));
        #endregion

        #region Event
        public event OnDataReceived<string>? OnClipboardReceived;
        public event OnDataReceived<long>? OnSetClipboardAcknowledgement;
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
