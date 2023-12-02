using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.Scrcpy.Control;
using TqkLibrary.Scrcpy.Enums;

namespace TqkLibrary.Scrcpy.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="data"></param>
    public delegate void OnDataReceived<T>(IControl control, T data);
    /// <summary>
    /// 
    /// </summary>
    public interface IControl
    {
        /// <summary>
        /// 
        /// </summary>
        Scrcpy Scrcpy { get; }

        /// <summary>
        /// 
        /// </summary>
        event OnDataReceived<string> OnClipboardReceived;
        /// <summary>
        /// 
        /// </summary>
        event OnDataReceived<long> OnSetClipboardAcknowledgement;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="keycode"></param>
        /// <param name="repeat"></param>
        /// <param name="metaState"></param>
        /// <returns></returns>
        bool InjectKeycode(
                                    AndroidKeyEventAction action,
                                    AndroidKeyCode keycode,
                                    uint repeat,
                                    AndroidKeyEventMeta metaState);

        /// <summary>
        /// ASCII only
        /// </summary>
        /// <param name="text">No unicode</param>
        /// <returns></returns>
        bool InjectText(string text);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="pointerId">
        /// <see cref="ScrcpyMousePointerId.POINTER_ID_MOUSE"/> or <see cref="ScrcpyMousePointerId.POINTER_ID_VIRTUAL_MOUSE"/> for use Mouse<br>
        /// </br>Otherwise use finger
        /// </param>
        /// <param name="position"></param>
        /// <param name="pressure">when <see cref="AndroidMotionEventAction.ACTION_DOWN"/> or <see cref="AndroidMotionEventAction.ACTION_MOVE"/> pressure = 1f, else pressure = 0f</param>
        /// <param name="buttons">ButtonChange</param>
        /// <param name="actionButton">All button current state<br></br>
        /// Example: buttons <see cref="AndroidMotionEventButton.BUTTON_PRIMARY"/> down then actionButton is <see cref="AndroidMotionEventButton.BUTTON_PRIMARY"/><br></br>
        /// buttons <see cref="AndroidMotionEventButton.BUTTON_PRIMARY"/> up then actionButton is <see cref="AndroidMotionEventButton.None"/></param>
        /// <returns></returns>
        bool InjectTouchEvent(
                                    AndroidMotionEventAction action,
                                    long pointerId,
                                    Rectangle position,
                                    float pressure,
                                    AndroidMotionEventButton buttons,
                                    AndroidMotionEventButton actionButton);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="vScroll"></param>
        /// <param name="hScroll"></param>
        /// <param name="button"></param>
        /// <returns></returns>
        bool InjectScrollEvent(
                                    Rectangle position,
                                    float vScroll,
                                    float hScroll,
                                    AndroidMotionEventButton button);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="paste"></param>
        /// <returns></returns>
        bool SetClipboard(string text, bool paste);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="paste"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        bool SetClipboard(string text, bool paste, long sequence);

        /// <summary>
        /// Fire on event <see cref="OnClipboardReceived"/>
        /// </summary>
        bool GetClipboard(CopyKey copyKey);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="powerMode"></param>
        /// <returns></returns>
        bool SetScreenPowerMode(ScrcpyScreenPowerMode powerMode);

        /// <summary>
        /// Action for Back button in android
        /// </summary>
        /// <param name="KeyEventAction"></param>
        /// <returns></returns>
        bool BackOrScreenOn(AndroidKeyEventAction KeyEventAction);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool ExpandNotificationPanel();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool ExpandSettingsPanel();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool CollapsePanel();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool RotateDevice();
    }
}
