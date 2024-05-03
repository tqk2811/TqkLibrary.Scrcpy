using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.Scrcpy;
using TqkLibrary.Scrcpy.Enums;
using TqkLibrary.Scrcpy.Interfaces;

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

        protected override void InsertItem(int index, IControl item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, IControl item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));
            base.SetItem(index, item);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e?.NewItems != null)
            {
                foreach (var item in e.NewItems.Cast<IControl>())
                {
                    item.OnClipboardReceived += Item_OnClipboardReceived;
                    item.OnSetClipboardAcknowledgement += Item_OnSetClipboardAcknowledgement;
                }
            }
            if (e?.OldItems != null)
            {
                foreach (var item in e.OldItems.Cast<IControl>())
                {
                    item.OnClipboardReceived -= Item_OnClipboardReceived;
                    item.OnSetClipboardAcknowledgement -= Item_OnSetClipboardAcknowledgement;
                }
            }
            base.OnCollectionChanged(e!);
        }

        private void Item_OnSetClipboardAcknowledgement(IControl control, long data)
            => OnSetClipboardAcknowledgement?.Invoke(control, data);
        private void Item_OnClipboardReceived(IControl control, string data)
            => OnClipboardReceived?.Invoke(control, data);



        #region IControl
#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
        public Scrcpy? Scrcpy => this.FirstOrDefault()?.Scrcpy;
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).

        public event OnDataReceived<string>? OnClipboardReceived;
        public event OnDataReceived<long>? OnSetClipboardAcknowledgement;

        public bool BackOrScreenOn(AndroidKeyEventAction KeyEventAction)
            => this.ForEach(x => x.BackOrScreenOn(KeyEventAction));
        public bool CollapsePanel()
            => this.ForEach(x => x.CollapsePanel());

        public bool ExpandNotificationPanel()
            => this.ForEach(x => x.ExpandNotificationPanel());

        public bool ExpandSettingsPanel()
             => this.ForEach(x => x.ExpandSettingsPanel());

        public bool GetClipboard(CopyKey copyKey = CopyKey.None)
             => this.ForEach(x => x.GetClipboard(copyKey));

        public bool InjectKeycode(AndroidKeyEventAction action, AndroidKeyCode keycode, uint repeat = 0, AndroidKeyEventMeta metaState = AndroidKeyEventMeta.META_NONE)
            => this.ForEach(x => x.InjectKeycode(action, keycode, repeat, metaState));

        public bool InjectScrollEvent(Rectangle position, float vScroll, float hScroll = 0, AndroidMotionEventButton button = AndroidMotionEventButton.BUTTON_PRIMARY)
            => this.ForEach(x => x.InjectScrollEvent(new Rectangle(position.Location, x.Scrcpy.ScreenSize), vScroll, hScroll, button));

        public bool InjectText(string text)
            => this.ForEach(x => x.InjectText(text));

        public bool InjectTouchEvent(
            AndroidMotionEventAction action,
            long pointerId,
            Rectangle position,
            float pressure = 1,
            AndroidMotionEventButton buttons = AndroidMotionEventButton.BUTTON_PRIMARY,
            AndroidMotionEventButton actionButton = AndroidMotionEventButton.BUTTON_PRIMARY)
            => this.ForEach(x => x.InjectTouchEvent(action, pointerId, new Rectangle(position.Location, x.Scrcpy.ScreenSize), pressure, buttons, actionButton));

        public bool RotateDevice()
            => this.ForEach(x => x.RotateDevice());

        public bool SetClipboard(string text, bool paste)
            => this.ForEach(x => x.SetClipboard(text, paste));

        public bool SetClipboard(string text, bool paste, long sequence)
            => this.ForEach(x => x.SetClipboard(text, paste, sequence));

        public bool SetScreenPowerMode(ScrcpyScreenPowerMode powerMode)
            => this.ForEach(x => x.SetScreenPowerMode(powerMode));

        public bool OpenHardKeyboardSetting()
            => this.ForEach(x => x.OpenHardKeyboardSetting());
        #endregion
    }


    internal static class ControlChainExtension
    {
        internal static bool ForEach<TIn>(this IEnumerable<TIn> list, Func<TIn, bool> func)
        {
            var result = true;
            foreach (var item in list)
            {
                if (result) result = func.Invoke(item);
                else func.Invoke(item);
            }
            return result;
        }
    }
}
