using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.WpfUi;
using TqkLibrary.Scrcpy;
using TqkLibrary.Scrcpy.Interfaces;

namespace TestRenderWpf
{
    public class MainWindowVM : BaseViewModel
    {

        IControl _control;
        public IControl Control
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

        ScrcpyUiView _ScrcpyUiView;
        public ScrcpyUiView ScrcpyUiView
        {
            get { return _ScrcpyUiView; }
            set { _ScrcpyUiView = value; NotifyPropertyChange(); }
        }
    }

}
