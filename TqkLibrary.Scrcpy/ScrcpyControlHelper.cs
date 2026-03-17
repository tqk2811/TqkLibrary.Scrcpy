using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using TqkLibrary.Scrcpy.Configs;
using TqkLibrary.Scrcpy.Enums;
using TqkLibrary.Scrcpy.Interfaces;

namespace TqkLibrary.Scrcpy
{
    /// <summary>
    /// 
    /// </summary>
    public static class ScrcpyControlHelper
    {
#if NET5_0_OR_GREATER
        static int NextRandomInt() => Random.Shared.Next(int.MinValue, int.MaxValue);
#else
        static int NextRandomInt() => new Random().Next(int.MinValue, int.MaxValue);
#endif

        static Size GetValidatedScreenSize(this IControl control)
        {
            var size = control.Scrcpy.ScreenSize;
            if (size.IsEmpty)
                throw new InvalidOperationException(
                    "ScreenSize is empty. When IsVideo=false, call RefreshScreenSizeFromAdbAsync() before using touch/scroll controls.");
            return size;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="action"></param>
        /// <param name="pointerId"></param>
        /// <param name="point"></param>
        /// <param name="pressure"></param>
        /// <param name="buttons"></param>
        /// <param name="actionButton"></param>
        /// <returns></returns>
        public static bool InjectTouchEvent(this IControl control,
            AndroidMotionEventAction action, long pointerId, Point point, float pressure, AndroidMotionEventButton buttons, AndroidMotionEventButton actionButton)
            => control.InjectTouchEvent(action, pointerId, new Rectangle(point, control.GetValidatedScreenSize()), pressure, buttons, actionButton);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="point"></param>
        /// <param name="vScroll"></param>
        /// <param name="hScroll"></param>
        /// <param name="androidMotion"></param>
        /// <returns></returns>
        public static bool InjectScrollEvent(this IControl control, Point point, float vScroll, float hScroll = 0, AndroidMotionEventButton androidMotion = AndroidMotionEventButton.BUTTON_PRIMARY)
            => control.InjectScrollEvent(new Rectangle(point, control.GetValidatedScreenSize()), vScroll, hScroll, androidMotion);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="releaseDelay"></param>
        /// <param name="cancellationToken"></param>
        public static void Tap(this IControl control, int x, int y, int releaseDelay = 100, CancellationToken cancellationToken = default)
        {
            var screenSize = control.GetValidatedScreenSize();
            long pointerId = NextRandomInt();
            control.InjectTouchEvent(
              AndroidMotionEventAction.ACTION_DOWN,
              pointerId,
              new Rectangle(x, y, screenSize.Width, screenSize.Height),
              1f,
              AndroidMotionEventButton.BUTTON_PRIMARY,
              AndroidMotionEventButton.BUTTON_PRIMARY);

            cancellationToken.ThrowIfCancellationRequested();
            Thread.Sleep(releaseDelay);

            control.InjectTouchEvent(
              AndroidMotionEventAction.ACTION_UP,
             pointerId,
             new Rectangle(x, y, screenSize.Width, screenSize.Height),
             0f,
             AndroidMotionEventButton.BUTTON_PRIMARY,
             AndroidMotionEventButton.None);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="releaseDelay"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task TapAsync(this IControl control, int x, int y, int releaseDelay = 100, CancellationToken cancellationToken = default)
        {
            var screenSize = control.GetValidatedScreenSize();
            long pointerId = NextRandomInt();
            control.InjectTouchEvent(
                AndroidMotionEventAction.ACTION_DOWN,
                pointerId,
                new Rectangle(x, y, screenSize.Width, screenSize.Height),
                1f,
                AndroidMotionEventButton.BUTTON_PRIMARY,
                AndroidMotionEventButton.BUTTON_PRIMARY);

            await Task.Delay(releaseDelay, cancellationToken);

            control.InjectTouchEvent(
                AndroidMotionEventAction.ACTION_UP,
                pointerId,
                new Rectangle(x, y, screenSize.Width, screenSize.Height),
                0f,
                AndroidMotionEventButton.BUTTON_PRIMARY,
                AndroidMotionEventButton.None);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="point"></param>
        /// <param name="releaseDelay"></param>
        /// <param name="cancellationToken"></param>
        public static void Tap(this IControl control, Point point, int releaseDelay = 100, CancellationToken cancellationToken = default)
            => control.Tap(point.X, point.Y, releaseDelay, cancellationToken);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="point"></param>
        /// <param name="releaseDelay"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task TapAsync(this IControl control, Point point, int releaseDelay = 100, CancellationToken cancellationToken = default)
            => control.TapAsync(point.X, point.Y, releaseDelay, cancellationToken);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="releaseDelay"></param>
        /// <param name="cancellationToken"></param>
        public static void TapPercent(this IControl control, double x, double y, int releaseDelay = 100, CancellationToken cancellationToken = default)
        {
            var screenSize = control.GetValidatedScreenSize();
            control.Tap((int)(x * screenSize.Width), (int)(y * screenSize.Height), releaseDelay, cancellationToken);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="releaseDelay"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task TapPercentAsync(this IControl control, double x, double y, int releaseDelay = 100, CancellationToken cancellationToken = default)
        {
            var screenSize = control.GetValidatedScreenSize();
            return control.TapAsync((int)(x * screenSize.Width), (int)(y * screenSize.Height), releaseDelay, cancellationToken);
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="androidKeyCode"></param>
        /// <param name="repeat"></param>
        /// <param name="releaseDelay"></param>
        /// <param name="cancellationToken"></param>
        public static void Key(this IControl control, AndroidKeyCode androidKeyCode, uint repeat = 0, int releaseDelay = 100, CancellationToken cancellationToken = default)
        {
            control.InjectKeycode(AndroidKeyEventAction.ACTION_DOWN, androidKeyCode, repeat, AndroidKeyEventMeta.META_NONE);
            cancellationToken.ThrowIfCancellationRequested();
            Thread.Sleep(releaseDelay);
            control.InjectKeycode(AndroidKeyEventAction.ACTION_UP, androidKeyCode, repeat, AndroidKeyEventMeta.META_NONE);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="androidKeyCode"></param>
        /// <param name="repeat"></param>
        /// <param name="releaseDelay"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task KeyAsync(this IControl control, AndroidKeyCode androidKeyCode, uint repeat = 0, int releaseDelay = 100, CancellationToken cancellationToken = default)
        {
            control.InjectKeycode(AndroidKeyEventAction.ACTION_DOWN, androidKeyCode, repeat, AndroidKeyEventMeta.META_NONE);
            await Task.Delay(releaseDelay, cancellationToken);
            control.InjectKeycode(AndroidKeyEventAction.ACTION_UP, androidKeyCode, repeat, AndroidKeyEventMeta.META_NONE);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="vScroll"></param>
        /// <param name="hScroll"></param>
        /// <param name="androidMotion"></param>
        public static bool Scroll(this IControl control, int x, int y, float vScroll, float hScroll = 0, AndroidMotionEventButton androidMotion = AndroidMotionEventButton.BUTTON_PRIMARY)
        {
            var screenSize = control.GetValidatedScreenSize();
            return control.InjectScrollEvent(new Rectangle(x, y, screenSize.Width, screenSize.Height), vScroll, hScroll, androidMotion);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="point"></param>
        /// <param name="hScroll"></param>
        /// <param name="vScroll"></param>
        /// <returns></returns>
        public static bool Scroll(this IControl control, Point point, float hScroll, float vScroll)
            => control.Scroll(point.X, point.Y, hScroll, vScroll);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="hScroll"></param>
        /// <param name="vScroll"></param>
        public static bool ScrollPercent(this IControl control, double x, double y, float hScroll, float vScroll)
        {
            var screenSize = control.GetValidatedScreenSize();
            return control.Scroll((int)(x * screenSize.Width), (int)(y * screenSize.Height), hScroll, vScroll);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="duration"></param>
        /// <param name="delayStep"></param>
        /// <param name="cancellationToken"></param>
        public static void Swipe(this IControl control, int x1, int y1, int x2, int y2, int duration, int delayStep = 10, CancellationToken cancellationToken = default)
        {
            var screenSize = control.GetValidatedScreenSize();
            long pointerId = NextRandomInt();
            control.InjectTouchEvent(
              AndroidMotionEventAction.ACTION_DOWN,
              pointerId,
              new Rectangle(x1, y1, screenSize.Width, screenSize.Height),
              1f,
              AndroidMotionEventButton.BUTTON_PRIMARY,
              AndroidMotionEventButton.BUTTON_PRIMARY);

            cancellationToken.ThrowIfCancellationRequested();
            Thread.Sleep(delayStep);

            int times = duration / delayStep;
            if (times == 0) times = 1;
            int x = (x2 - x1) / times;
            int y = (y2 - y1) / times;
            for (int i = 1; i < times; i++)
            {
                control.InjectTouchEvent(
                  AndroidMotionEventAction.ACTION_MOVE,
                  pointerId,
                  new Rectangle(x1 + x * i, y1 + y * i, screenSize.Width, screenSize.Height),
                  1f,
                  AndroidMotionEventButton.BUTTON_PRIMARY,
                  AndroidMotionEventButton.None);
                cancellationToken.ThrowIfCancellationRequested();
                Thread.Sleep(delayStep);
            }

            control.InjectTouchEvent(
             AndroidMotionEventAction.ACTION_UP,
             pointerId,
             new Rectangle(x2, y2, screenSize.Width, screenSize.Height),
             0f,
             AndroidMotionEventButton.BUTTON_PRIMARY,
             AndroidMotionEventButton.None);
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="control"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="duration"></param>
        /// <param name="delayStep"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task SwipeAsync(this IControl control, int x1, int y1, int x2, int y2, int duration, int delayStep = 10, CancellationToken cancellationToken = default)
        {
            var screenSize = control.GetValidatedScreenSize();
            long pointerId = NextRandomInt();
            control.InjectTouchEvent(
                AndroidMotionEventAction.ACTION_DOWN,
                pointerId,
                new Rectangle(x1, y1, screenSize.Width, screenSize.Height),
                1f,
                AndroidMotionEventButton.BUTTON_PRIMARY,
                AndroidMotionEventButton.BUTTON_PRIMARY);

            await Task.Delay(delayStep, cancellationToken);

            int times = duration / delayStep;
            int x = (x2 - x1) / times;
            int y = (y2 - y1) / times;
            for (int i = 1; i < times; i++)
            {
                control.InjectTouchEvent(
                      AndroidMotionEventAction.ACTION_MOVE,
                      pointerId,
                      new Rectangle(x1 + x * i, y1 + y * i, screenSize.Width, screenSize.Height),
                      1f,
                      AndroidMotionEventButton.BUTTON_PRIMARY,
                      AndroidMotionEventButton.None);
                await Task.Delay(delayStep, cancellationToken);
            }

            control.InjectTouchEvent(
                 AndroidMotionEventAction.ACTION_UP,
                 pointerId,
                 new Rectangle(x2, y2, screenSize.Width, screenSize.Height),
                 0f,
                 AndroidMotionEventButton.BUTTON_PRIMARY,
                 AndroidMotionEventButton.None);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <param name="delayStep"></param>
        /// <param name="cancellationToken"></param>
        public static void Swipe(this IControl control, Point from, Point to, int duration, int delayStep = 10, CancellationToken cancellationToken = default)
            => control.Swipe(from.X, from.Y, to.X, to.Y, duration, delayStep, cancellationToken);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <param name="delayStep"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task SwipeAsync(this IControl control, Point from, Point to, int duration, int delayStep = 10, CancellationToken cancellationToken = default)
            => control.SwipeAsync(from.X, from.Y, to.X, to.Y, duration, delayStep, cancellationToken);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="duration"></param>
        /// <param name="delayStep"></param>
        /// <param name="cancellationToken"></param>
        public static void SwipePercent(this IControl control, double x1, double y1, double x2, double y2, int duration, int delayStep = 10, CancellationToken cancellationToken = default)
        {
            var screenSize = control.GetValidatedScreenSize();
            control.Swipe(
                (int)(x1 * screenSize.Width), (int)(y1 * screenSize.Height),
                (int)(x2 * screenSize.Width), (int)(y2 * screenSize.Height),
                duration, delayStep, cancellationToken);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="duration"></param>
        /// <param name="delayStep"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task SwipePercentAsync(this IControl control, double x1, double y1, double x2, double y2, int duration, int delayStep = 10, CancellationToken cancellationToken = default)
        {
            var screenSize = control.GetValidatedScreenSize();
            return control.SwipeAsync(
                (int)(x1 * screenSize.Width), (int)(y1 * screenSize.Height),
                (int)(x2 * screenSize.Width), (int)(y2 * screenSize.Height),
                duration, delayStep, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="pixelPerSec"></param>
        /// <param name="delayStep"></param>
        /// <param name="cancellationToken"></param>
        public static void SwipeSpeed(this IControl control, int x1, int y1, int x2, int y2, int pixelPerSec = 1000, int delayStep = 10, CancellationToken cancellationToken = default)
        {
            var screenSize = control.GetValidatedScreenSize();
            long pointerId = NextRandomInt();
            control.InjectTouchEvent(
                AndroidMotionEventAction.ACTION_DOWN,
                pointerId,
                new Rectangle(x1, y1, screenSize.Width, screenSize.Height),
                1f,
                AndroidMotionEventButton.BUTTON_PRIMARY,
                AndroidMotionEventButton.BUTTON_PRIMARY);
            cancellationToken.ThrowIfCancellationRequested();
            Thread.Sleep(delayStep);

            int x = x2 - x1;
            int y = y2 - y1;
            double range = Math.Pow((double)(x * x + y * y), 0.5);
            double duration = 1000 * range / pixelPerSec;

            int times = (int)(duration / delayStep);
            if (times == 0) times = 1;
            x /= times;
            y /= times;

            for (int i = 1; i <= times; i++)
            {
                control.InjectTouchEvent(
                    AndroidMotionEventAction.ACTION_MOVE,
                    pointerId,
                    new Rectangle(x1 + x * i, y1 + y * i, screenSize.Width, screenSize.Height),
                    1f,
                    AndroidMotionEventButton.BUTTON_PRIMARY,
                    AndroidMotionEventButton.None);
                cancellationToken.ThrowIfCancellationRequested();
                Thread.Sleep(delayStep);
            }

            control.InjectTouchEvent(
                AndroidMotionEventAction.ACTION_UP,
                pointerId,
                new Rectangle(x2, y2, screenSize.Width, screenSize.Height),
                0f,
                AndroidMotionEventButton.BUTTON_PRIMARY,
                AndroidMotionEventButton.None);
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="control"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="pixelPerSec"></param>
        /// <param name="delayStep"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task SwipeSpeedAsync(this IControl control, int x1, int y1, int x2, int y2, int pixelPerSec = 1000, int delayStep = 10, CancellationToken cancellationToken = default)
        {
            var screenSize = control.GetValidatedScreenSize();
            long pointerId = NextRandomInt();
            control.InjectTouchEvent(
                AndroidMotionEventAction.ACTION_DOWN,
                pointerId,
                new Rectangle(x1, y1, screenSize.Width, screenSize.Height),
                1f,
                AndroidMotionEventButton.BUTTON_PRIMARY,
                AndroidMotionEventButton.BUTTON_PRIMARY);
            await Task.Delay(delayStep, cancellationToken);

            int x = x2 - x1;
            int y = y2 - y1;
            double range = Math.Pow((double)(x * x + y * y), 0.5);
            double duration = 1000 * range / pixelPerSec;

            int times = (int)(duration / delayStep);
            if (times == 0) times = 1;
            x /= times;
            y /= times;

            for (int i = 1; i <= times; i++)
            {
                control.InjectTouchEvent(
                    AndroidMotionEventAction.ACTION_MOVE,
                    pointerId,
                    new Rectangle(x1 + x * i, y1 + y * i, screenSize.Width, screenSize.Height),
                    1f,
                    AndroidMotionEventButton.BUTTON_PRIMARY,
                    AndroidMotionEventButton.None);
                await Task.Delay(delayStep, cancellationToken);
            }

            control.InjectTouchEvent(
                AndroidMotionEventAction.ACTION_UP,
                pointerId,
                new Rectangle(x2, y2, screenSize.Width, screenSize.Height),
                0f,
                AndroidMotionEventButton.BUTTON_PRIMARY,
                AndroidMotionEventButton.None);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="pixelPerSec"></param>
        /// <param name="delayStep"></param>
        /// <param name="cancellationToken"></param>
        public static void SwipeSpeed(this IControl control, Point from, Point to, int pixelPerSec = 1000, int delayStep = 10, CancellationToken cancellationToken = default)
            => control.SwipeSpeed(from.X, from.Y, to.X, to.Y, pixelPerSec, delayStep, cancellationToken);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="pixelPerSec"></param>
        /// <param name="delayStep"></param>
        /// <param name="cancellationToken"></param>
        public static Task SwipeSpeedAsync(this IControl control, Point from, Point to, int pixelPerSec = 1000, int delayStep = 10, CancellationToken cancellationToken = default)
            => control.SwipeSpeedAsync(from.X, from.Y, to.X, to.Y, pixelPerSec, delayStep, cancellationToken);

        /// <summary>
        /// Work only when <see cref="ScrcpyServerConfig.ClipboardAutosync"/> is disable.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="copyKey"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<string> GetClipboardAsync(this IControl control, CopyKey copyKey = CopyKey.None, CancellationToken cancellationToken = default)
        {
            if (control.Scrcpy.IsClipboardAutoSync)
                return control.Scrcpy.LastClipboard;
            TaskCompletionSource<string> taskCompletionSource = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            OnDataReceived<string> dataDelegate = (control, text) => taskCompletionSource.TrySetResult(text);
            try
            {
                using var register = cancellationToken.Register(() => taskCompletionSource.TrySetCanceled());
                control.OnClipboardReceived += dataDelegate;
                if (control.GetClipboard(copyKey))
                {
                    return await taskCompletionSource.Task.ConfigureAwait(false);
                }
                return string.Empty;
            }
            finally
            {
                control.OnClipboardReceived -= dataDelegate;
            }
        }
    }
}
