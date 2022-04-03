using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.Scrcpy.Control
{
    internal class ScrcpyControl : IControl
    {
        internal ScrcpyControl(Scrcpy scrcpy)
        {
            this.Scrcpy = scrcpy;
        }
        public Scrcpy Scrcpy { get; }

        public event OnDataReceived<string> OnClipboardReceived;


        bool SendControl(ScrcpyControlMessage scrcpyControlMessage)
        {
            byte[] command = scrcpyControlMessage.GetCommand();
            return NativeWrapper.ScrcpyControl(Scrcpy.Handle, command, command.Length);
        }


        #region BasicCommand
        public bool BackOrScreenOn(AndroidKeyEventAction KeyEventAction)
        {
            throw new NotImplementedException();
        }

        public Task<bool> BackOrScreenOnAsync(AndroidKeyEventAction KeyEventAction)
        {
            throw new NotImplementedException();
        }

        public bool CollapsePanel()
        {
            throw new NotImplementedException();
        }

        public Task<bool> CollapsePanelAsync()
        {
            throw new NotImplementedException();
        }

        public bool ExpandNotificationPanel()
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExpandNotificationPanelAsync()
        {
            throw new NotImplementedException();
        }

        public bool ExpandSettingsPanel()
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExpandSettingsPanelAsync()
        {
            throw new NotImplementedException();
        }

        public bool GetClipboard()
        {
            throw new NotImplementedException();
        }

        public Task<bool> GetClipboardAsync()
        {
            throw new NotImplementedException();
        }

        public bool InjectKeycode(AndroidKeyEventAction action, AndroidKeyCode keycode, uint repeat = 1, AndroidKeyEventMeta metaState = AndroidKeyEventMeta.META_NONE)
        {
            throw new NotImplementedException();
        }

        public Task<bool> InjectKeycodeAsync(AndroidKeyEventAction action, AndroidKeyCode keycode, uint repeat = 1, AndroidKeyEventMeta metaState = AndroidKeyEventMeta.META_NONE)
        {
            throw new NotImplementedException();
        }

        public bool InjectScrollEvent(Rectangle position, int vScroll, int hScroll = 0)
        {
            throw new NotImplementedException();
        }

        public Task<bool> InjectScrollEventAsync(Rectangle position, int vScroll, int hScroll = 0)
        {
            throw new NotImplementedException();
        }

        public bool InjectText(string text)
        {
            throw new NotImplementedException();
        }

        public Task<bool> InjectTextAsync(string text)
        {
            throw new NotImplementedException();
        }

        public bool InjectTouchEvent(AndroidMotionEventAction action, long pointerId, Rectangle position, float pressure = 1, AndroidMotionEventButton buttons = AndroidMotionEventButton.BUTTON_PRIMARY)
        {
            throw new NotImplementedException();
        }

        public Task<bool> InjectTouchEventAsync(AndroidMotionEventAction action, long pointerId, Rectangle position, float pressure = 1, AndroidMotionEventButton buttons = AndroidMotionEventButton.BUTTON_PRIMARY)
        {
            throw new NotImplementedException();
        }

        public bool RotateDevice()
        {
            throw new NotImplementedException();
        }

        public Task<bool> RotateDeviceAsync()
        {
            throw new NotImplementedException();
        }

        public bool SetClipboard(string text, bool paste)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetClipboardAsync(string text, bool paste)
        {
            throw new NotImplementedException();
        }

        public bool SetScreenPowerMode(ScrcpyScreenPowerMode powerMode)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetScreenPowerModeAsync(ScrcpyScreenPowerMode powerMode)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
