using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.Scrcpy;

namespace TqkLibrary.Scrcpy.Wpf
{
    public class ControlChain : ObservableCollection<IControl>, IControl
    {
        public ControlChain()
        {

        }


        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if ((e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Reset || 
                e.Action == NotifyCollectionChangedAction.Replace) &&

                e?.OldItems != null)
            {
                foreach (var item in e.OldItems.Cast<IControl>())
                {
                    item.OnClipboardReceived -= Item_OnClipboardReceived;
                    item.OnSetClipboardAcknowledgement -= Item_OnSetClipboardAcknowledgement;
                }
            }

            if ((e.Action == NotifyCollectionChangedAction.Add || 
                e.Action == NotifyCollectionChangedAction.Replace) && 
                
                e?.NewItems != null)
            {
                foreach (var item in e.NewItems.Cast<IControl>())
                {
                    item.OnClipboardReceived += Item_OnClipboardReceived;
                    item.OnSetClipboardAcknowledgement += Item_OnSetClipboardAcknowledgement;
                }
            }

            base.OnCollectionChanged(e);
        }

        private void Item_OnSetClipboardAcknowledgement(IControl control, long data) 
            => OnSetClipboardAcknowledgement?.Invoke(control, data);
        private void Item_OnClipboardReceived(IControl control, string data)
            => OnClipboardReceived?.Invoke(control, data);



        #region IControl
        public Scrcpy Scrcpy => this.FirstOrDefault()?.Scrcpy;

        public event OnDataReceived<string> OnClipboardReceived;
        public event OnDataReceived<long> OnSetClipboardAcknowledgement;

        public bool BackOrScreenOn(AndroidKeyEventAction KeyEventAction)
            => this.All(x => x.BackOrScreenOn(KeyEventAction));
        public bool CollapsePanel()
            => this.All(x => x.CollapsePanel());

        public bool ExpandNotificationPanel()
            => this.All(x => x.ExpandNotificationPanel());

        public bool ExpandSettingsPanel()
             => this.All(x => x.ExpandSettingsPanel());

        public bool GetClipboard(CopyKey copyKey = CopyKey.None)
            => this.All(x => x.GetClipboard(copyKey));

        public bool InjectKeycode(AndroidKeyEventAction action, AndroidKeyCode keycode, uint repeat = 1, AndroidKeyEventMeta metaState = AndroidKeyEventMeta.META_NONE)
            => this.All(x => x.InjectKeycode(action, keycode, repeat, metaState));

        public bool InjectScrollEvent(Rectangle position, int vScroll, int hScroll = 0, AndroidMotionEventButton button = AndroidMotionEventButton.BUTTON_PRIMARY)
            => this.All(x => x.InjectScrollEvent(position, vScroll, hScroll, button));

        public bool InjectText(string text)
            => this.All(x => x.InjectText(text));

        public bool InjectTouchEvent(AndroidMotionEventAction action, long pointerId, Rectangle position, float pressure = 1, AndroidMotionEventButton buttons = AndroidMotionEventButton.BUTTON_PRIMARY)
            => this.All(x => x.InjectTouchEvent(action, pointerId, position, pressure, buttons));

        public bool RotateDevice()
            => this.All(x => x.RotateDevice());

        public bool SetClipboard(string text, bool paste)
            => this.All(x => x.SetClipboard(text, paste));

        public bool SetClipboard(string text, bool paste, long sequence)
            => this.All(x => x.SetClipboard(text, paste, sequence));

        public bool SetScreenPowerMode(ScrcpyScreenPowerMode powerMode)
            => this.All(x => x.SetScreenPowerMode(powerMode));

        #endregion
    }
}
