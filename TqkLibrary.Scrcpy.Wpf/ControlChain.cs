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
        public ControlChain(IControl baseControl)
        {
            this.Add(baseControl ?? throw new ArgumentNullException(nameof(baseControl)));
        }
        public ControlChain(IControl baseControl, IEnumerable<IControl> controls)
        {
            if (controls == null) throw new ArgumentNullException(nameof(controls));
            this.Add(baseControl ?? throw new ArgumentNullException(nameof(baseControl)));
            foreach (var item in controls) this.Add(item);
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
            => this.First().GetClipboard(copyKey);//this.All(x => x.GetClipboard(copyKey));

        public bool InjectKeycode(AndroidKeyEventAction action, AndroidKeyCode keycode, uint repeat = 1, AndroidKeyEventMeta metaState = AndroidKeyEventMeta.META_NONE)
            => this.All(x => x.InjectKeycode(action, keycode, repeat, metaState));

        public bool InjectScrollEvent(Rectangle position, int vScroll, int hScroll = 0, AndroidMotionEventButton button = AndroidMotionEventButton.BUTTON_PRIMARY)
            => this.All(x => x.InjectScrollEvent(new Rectangle(position.Location, x.Scrcpy.ScreenSize), vScroll, hScroll, button));

        public bool InjectText(string text)
            => this.All(x => x.InjectText(text));

        public bool InjectTouchEvent(AndroidMotionEventAction action, long pointerId, Rectangle position, float pressure = 1, AndroidMotionEventButton buttons = AndroidMotionEventButton.BUTTON_PRIMARY)
            => this.All(x => x.InjectTouchEvent(action, pointerId, new Rectangle(position.Location, x.Scrcpy.ScreenSize), pressure, buttons));

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


    //internal static class ControlChainExtension
    //{
    //    internal static bool All<TIn>(this IEnumerable<TIn> list, Func<TIn, bool> func)
    //    {
    //        var result = true;
    //        foreach (var item in list)
    //        {
    //            if (result) result = func.Invoke(item);
    //            else func.Invoke(item);
    //        }
    //        return result;
    //    }
    //}
}
