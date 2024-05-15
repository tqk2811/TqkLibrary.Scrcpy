using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TqkLibrary.Scrcpy.Enums;
using TqkLibrary.Scrcpy.Wpf.Interfaces;

namespace TqkLibrary.Scrcpy.Wpf.Handlers
{
    internal class ControlHandler
    {
        readonly IScrcpyUiControl _scrcpyUiControl;
        public ControlHandler(IScrcpyUiControl scrcpyUiControl)
        {
            _scrcpyUiControl = scrcpyUiControl ?? throw new ArgumentNullException(nameof(scrcpyUiControl));
            _RegisterEvent();
        }


        void _RegisterEvent()
        {
            _scrcpyUiControl.Image.MouseDown += _image_MouseDown;
            _scrcpyUiControl.Image.MouseUp += _image_MouseUp;
            _scrcpyUiControl.Image.MouseMove += _image_MouseMove;
            _scrcpyUiControl.Image.MouseEnter += _image_MouseEnter;
            _scrcpyUiControl.Image.MouseLeave += _image_MouseLeave;
            _scrcpyUiControl.Image.MouseWheel += _image_MouseWheel;
            _scrcpyUiControl.Image.KeyDown += _image_KeyDown;
            _scrcpyUiControl.Image.KeyUp += _image_KeyUp;
        }

        bool isMouseDown = false;
        private void _image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = _scrcpyUiControl.IsMouseHandler;
            Keyboard.Focus((IInputElement)sender);
            var p = _scrcpyUiControl.GetRemotePoint(e.GetPosition((IInputElement)sender));
            var control = _scrcpyUiControl.Control;
            if (_scrcpyUiControl.IsControl && control != null)
            {
                _scrcpyUiControl.ResolveMouseButton(p, e.ChangedButton, AndroidMotionEventAction.ACTION_DOWN, ref isMouseDown);
            }
        }
        private void _image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = _scrcpyUiControl.IsMouseHandler;
            var p = _scrcpyUiControl.GetRemotePoint(e.GetPosition((IInputElement)sender));
            var control = _scrcpyUiControl.Control;
            if (_scrcpyUiControl.IsControl && control != null)
            {
                _scrcpyUiControl.ResolveMouseButton(p, e.ChangedButton, AndroidMotionEventAction.ACTION_UP, ref isMouseDown);
            }
        }
        private void _image_MouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = _scrcpyUiControl.IsMouseHandler;
            var p = _scrcpyUiControl.GetRemotePoint(e.GetPosition((IInputElement)sender));
            var control = _scrcpyUiControl.Control;
            if (_scrcpyUiControl.IsControl && control != null)
            {
                if (isMouseDown)
                    control.InjectTouchEvent(
                        AndroidMotionEventAction.ACTION_MOVE,
                        _scrcpyUiControl.MousePointerId,
                        p,
                        1.0f,
                        e.HandleMouseButton(),
                        e.HandleMouseButton()
                        );
            }
        }
        private void _image_MouseEnter(object sender, MouseEventArgs e)
        {

        }
        private void _image_MouseLeave(object sender, MouseEventArgs e)
        {

        }
        private void _image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = _scrcpyUiControl.IsMouseHandler;
            var p = _scrcpyUiControl.GetRemotePoint(e.GetPosition((IInputElement)sender));
            var control = _scrcpyUiControl.Control;
            if (_scrcpyUiControl.IsControl && control != null)
            {
                control.InjectScrollEvent(p, e.Delta >= 0 ? 1 : -1);
            }
        }
        private async void _image_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = _scrcpyUiControl.IsKeyHandler;
            var control = _scrcpyUiControl.Control;
            if (_scrcpyUiControl.IsControl && control != null)
            {
                if (e.SystemKey == Key.None)
                {
                    AndroidKeyCode keyCode = await e.ResolveKeyAsync(control);
                    if (keyCode != AndroidKeyCode.AKEYCODE_UNKNOWN)
                    {
                        control.InjectKeycode(AndroidKeyEventAction.ACTION_DOWN, keyCode, 0, e.ResolveMetaKey());
#if DEBUG
                        Console.WriteLine($"ACTION_DOWN: {keyCode}");
#endif
                    }
                }
                else
                {

                }
            }
        }
        private async void _image_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = _scrcpyUiControl.IsKeyHandler;
            var control = _scrcpyUiControl.Control;
            if (_scrcpyUiControl.IsControl && control != null)
            {
                if (e.SystemKey == Key.None)
                {
                    AndroidKeyCode keyCode = await e.ResolveKeyAsync(control);
                    if (keyCode != AndroidKeyCode.AKEYCODE_UNKNOWN)
                    {
                        control.InjectKeycode(AndroidKeyEventAction.ACTION_UP, keyCode, 0, e.ResolveMetaKey());
#if DEBUG
                        Console.WriteLine($"ACTION_UP: {keyCode}");
#endif
                    }
                }
                else
                {

                }
            }
        }
    }
}
