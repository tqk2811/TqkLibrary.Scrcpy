using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.WpfUi;
using TqkLibrary.Scrcpy;
using TqkLibrary.Scrcpy.Interfaces;
using System.Windows;
using TqkLibrary.Scrcpy.Enums;

namespace TestRenderWpf
{
    public class MainWindowVM : BaseViewModel
    {
        public MainWindowVM()
        {
            InjectTextCommand = new BaseCommand(_InjectTextCommand);


            AndroidKeyEventActions = Enum.GetValues<AndroidKeyEventAction>();
            BackOrScreenOnCommand = new BaseCommand(_BackOrScreenOnCommand);

            ExpandNotificationPanelCommand = new BaseCommand(_ExpandNotificationPanelCommand);
            ExpandSettingsPanelCommand = new BaseCommand(_ExpandSettingsPanelCommand);
            CollapsePanelCommand = new BaseCommand(_CollapsePanelCommand);
            RotateDeviceCommand = new BaseCommand(_RotateDeviceCommand);
            OpenHardKeyboardSettingCommand = new BaseCommand(_OpenHardKeyboardSettingCommand);
            ResetVideoCommand = new BaseCommand(() => Control?.ResetVideo());
        }

        IControl? _control;
        public IControl? Control
        {
            get { return _control; }
            set { _control = value; NotifyPropertyChange(); }
        }
        bool _isControl = true;
        public bool IsControl
        {
            get { return _isControl; }
            set { _isControl = value; NotifyPropertyChange(); }
        }

        ScrcpyUiView? _ScrcpyUiView;
        public ScrcpyUiView? ScrcpyUiView
        {
            get { return _ScrcpyUiView; }
            set { _ScrcpyUiView = value; NotifyPropertyChange(); }
        }



        public BaseCommand InjectTextCommand { get; }
        void _InjectTextCommand()
        {
            string text = Clipboard.GetText();
            if (string.IsNullOrWhiteSpace(text)) text = "InjectText";
            Control?.InjectText(text);
        }


        bool _DisplayPower = true;
        public bool DisplayPower
        {
            get { return _DisplayPower; }
            set
            {
                if (Control != null)
                {
                    _DisplayPower = value;
                    Control.SetDisplayPower(value);
                }
                else
                {
                    _DisplayPower = true;
                }
                NotifyPropertyChange();
            }
        }


        public IEnumerable<AndroidKeyEventAction> AndroidKeyEventActions { get; }

        AndroidKeyEventAction _AndroidKeyEventActionSelected = AndroidKeyEventAction.ACTION_DOWN;
        public AndroidKeyEventAction AndroidKeyEventActionSelected
        {
            get { return _AndroidKeyEventActionSelected; }
            set { _AndroidKeyEventActionSelected = value; NotifyPropertyChange(); }
        }
        public BaseCommand BackOrScreenOnCommand { get; }
        void _BackOrScreenOnCommand()
        {
            Control?.BackOrScreenOn(AndroidKeyEventActionSelected);
        }


        public BaseCommand ExpandNotificationPanelCommand { get; }
        void _ExpandNotificationPanelCommand()
        {
            Control?.ExpandNotificationPanel();
        }


        public BaseCommand ExpandSettingsPanelCommand { get; }
        void _ExpandSettingsPanelCommand()
        {
            Control?.ExpandSettingsPanel();
        }


        public BaseCommand CollapsePanelCommand { get; }
        void _CollapsePanelCommand()
        {
            Control?.CollapsePanel();
        }


        public BaseCommand RotateDeviceCommand { get; }
        void _RotateDeviceCommand()
        {
            Control?.RotateDevice();
        }


        public BaseCommand OpenHardKeyboardSettingCommand { get; }
        void _OpenHardKeyboardSettingCommand()
        {
            Control?.OpenHardKeyboardSetting();
        }

        public BaseCommand ResetVideoCommand { get; }
    }
}
