using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.Scrcpy
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    public delegate void OnDataReceived<T>(T data);
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
        /// <param name="action"></param>
        /// <param name="keycode"></param>
        /// <param name="repeat"></param>
        /// <param name="metaState"></param>
        /// <returns></returns>
        bool InjectKeycode(
                            AndroidKeyEventAction action,
                            AndroidKeyCode keycode,
                            uint repeat = 1,
                            AndroidKeyEventMeta metaState = AndroidKeyEventMeta.META_NONE);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="keycode"></param>
        /// <param name="repeat"></param>
        /// <param name="metaState"></param>
        /// <returns></returns>
        Task<bool> InjectKeycodeAsync(
                            AndroidKeyEventAction action,
                            AndroidKeyCode keycode,
                            uint repeat = 1,
                            AndroidKeyEventMeta metaState = AndroidKeyEventMeta.META_NONE);




        /// <summary>
        /// 
        /// </summary>
        /// <param name="text">No unicode</param>
        /// <returns></returns>
        bool InjectText(string text);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text">No unicode</param>
        /// <returns></returns>
        Task<bool> InjectTextAsync(string text);




        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="pointerId"></param>
        /// <param name="position"></param>
        /// <param name="pressure"></param>
        /// <param name="buttons"></param>
        /// <returns></returns>
        bool InjectTouchEvent(
                                      AndroidMotionEventAction action,
                                      long pointerId,
                                      Rectangle position,
                                      float pressure = 1f,
                                      AndroidMotionEventButton buttons = AndroidMotionEventButton.BUTTON_PRIMARY);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="pointerId"></param>
        /// <param name="position"></param>
        /// <param name="pressure"></param>
        /// <param name="buttons"></param>
        /// <returns></returns>
        Task<bool> InjectTouchEventAsync(
                                      AndroidMotionEventAction action,
                                      long pointerId,
                                      Rectangle position,
                                      float pressure = 1f,
                                      AndroidMotionEventButton buttons = AndroidMotionEventButton.BUTTON_PRIMARY);




        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="vScroll"></param>
        /// <param name="hScroll"></param>
        /// <returns></returns>
        bool InjectScrollEvent(Rectangle position, int vScroll, int hScroll = 0);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="vScroll"></param>
        /// <param name="hScroll"></param>
        /// <returns></returns>
        Task<bool> InjectScrollEventAsync(Rectangle position, int vScroll, int hScroll = 0);




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
        /// <returns></returns>
        Task<bool> SetClipboardAsync(string text, bool paste);



        /// <summary>
        /// Fire on event <see cref="OnClipboardReceived"/>
        /// </summary>
        bool GetClipboard();
        /// <summary>
        /// Fire on event <see cref="OnClipboardReceived"/>
        /// </summary>
        /// <returns></returns>
        Task<bool> GetClipboardAsync();



        /// <summary>
        /// 
        /// </summary>
        /// <param name="powerMode"></param>
        /// <returns></returns>
        bool SetScreenPowerMode(ScrcpyScreenPowerMode powerMode);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="powerMode"></param>
        /// <returns></returns>
        Task<bool> SetScreenPowerModeAsync(ScrcpyScreenPowerMode powerMode);



        /// <summary>
        /// Action for Back button in android
        /// </summary>
        /// <param name="KeyEventAction"></param>
        /// <returns></returns>
        bool BackOrScreenOn(AndroidKeyEventAction KeyEventAction);
        /// <summary>
        /// Action for Back button in android
        /// </summary>
        /// <param name="KeyEventAction"></param>
        /// <returns></returns>
        Task<bool> BackOrScreenOnAsync(AndroidKeyEventAction KeyEventAction);




        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool ExpandNotificationPanel();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<bool> ExpandNotificationPanelAsync();




        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool ExpandSettingsPanel();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<bool> ExpandSettingsPanelAsync();




        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool CollapsePanel();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<bool> CollapsePanelAsync();




        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool RotateDevice();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<bool> RotateDeviceAsync();
    }
}
