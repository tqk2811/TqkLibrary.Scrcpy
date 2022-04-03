using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace TqkLibrary.Scrcpy
{
    /// <summary>
    /// 
    /// </summary>
    public static class IControlHelper
    {
        static readonly Random random = new Random();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="action"></param>
        /// <param name="pointerId"></param>
        /// <param name="point"></param>
        /// <param name="pressure"></param>
        /// <param name="buttons"></param>
        /// <returns></returns>
        public static bool InjectTouchEvent(this IControl control,
            AndroidMotionEventAction action, long pointerId, Point point, float pressure = 1f, AndroidMotionEventButton buttons = AndroidMotionEventButton.BUTTON_PRIMARY)
            => control.InjectTouchEvent(action, pointerId, new Rectangle(point, control.Scrcpy.ScreenSize), pressure, buttons);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="action"></param>
        /// <param name="pointerId"></param>
        /// <param name="point"></param>
        /// <param name="pressure"></param>
        /// <param name="buttons"></param>
        /// <returns></returns>
        public static Task<bool> InjectTouchEventAsync(this IControl control,
            AndroidMotionEventAction action, long pointerId, Point point, float pressure = 1f, AndroidMotionEventButton buttons = AndroidMotionEventButton.BUTTON_PRIMARY)
            => control.InjectTouchEventAsync(action, pointerId, new Rectangle(point, control.Scrcpy.ScreenSize), pressure, buttons);



        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="point"></param>
        /// <param name="vScroll"></param>
        /// <param name="hScroll"></param>
        /// <returns></returns>
        public static bool InjectScrollEvent(this IControl control, Point point, int vScroll, int hScroll = 0)
            => control.InjectScrollEvent(new Rectangle(point, control.Scrcpy.ScreenSize), vScroll, hScroll);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="point"></param>
        /// <param name="vScroll"></param>
        /// <param name="hScroll"></param>
        /// <returns></returns>
        public static Task<bool> InjectScrollEventAsync(this IControl control, Point point, int vScroll, int hScroll = 0)
            => control.InjectScrollEventAsync(new Rectangle(point, control.Scrcpy.ScreenSize), vScroll, hScroll);



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
            long pointerId = random.Next(int.MinValue, int.MaxValue);
            control.InjectTouchEvent(
              AndroidMotionEventAction.ACTION_DOWN,
              pointerId,
              new Rectangle(x, y, control.Scrcpy.ScreenSize.Width, control.Scrcpy.ScreenSize.Height),
              1f,
              AndroidMotionEventButton.BUTTON_PRIMARY);

            Task.Delay(releaseDelay, cancellationToken).Wait();

            control.InjectTouchEvent(
              AndroidMotionEventAction.ACTION_UP,
             pointerId,
             new Rectangle(x, y, control.Scrcpy.ScreenSize.Width, control.Scrcpy.ScreenSize.Height),
             1f,
             AndroidMotionEventButton.BUTTON_PRIMARY);
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
            long pointerId = random.Next(int.MinValue, int.MaxValue);
            await control.InjectTouchEventAsync(
                AndroidMotionEventAction.ACTION_DOWN,
                pointerId,
                new Rectangle(x, y, control.Scrcpy.ScreenSize.Width, control.Scrcpy.ScreenSize.Height),
                1f,
                AndroidMotionEventButton.BUTTON_PRIMARY);

            await Task.Delay(releaseDelay, cancellationToken);

            await control.InjectTouchEventAsync(
                AndroidMotionEventAction.ACTION_UP,
                pointerId,
                new Rectangle(x, y, control.Scrcpy.ScreenSize.Width, control.Scrcpy.ScreenSize.Height),
                1f,
                AndroidMotionEventButton.BUTTON_PRIMARY);
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
            => control.Tap(
                (int)(x * control.Scrcpy.ScreenSize.Width),
                (int)(y * control.Scrcpy.ScreenSize.Height),
                releaseDelay,
                cancellationToken);
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
            => control.TapAsync(
                (int)(x * control.Scrcpy.ScreenSize.Width),
                (int)(y * control.Scrcpy.ScreenSize.Height),
                releaseDelay,
                cancellationToken);




        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="androidKeyCode"></param>
        /// <param name="repeat"></param>
        /// <param name="releaseDelay"></param>
        /// <param name="cancellationToken"></param>
        public static void Key(this IControl control, AndroidKeyCode androidKeyCode, uint repeat = 1, int releaseDelay = 100, CancellationToken cancellationToken = default)
        {
            control.InjectKeycode(AndroidKeyEventAction.ACTION_DOWN, androidKeyCode, repeat, AndroidKeyEventMeta.META_NONE);
            Task.Delay(releaseDelay, cancellationToken).Wait();
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
        public static async Task KeyAsync(this IControl control, AndroidKeyCode androidKeyCode, uint repeat = 1, int releaseDelay = 100, CancellationToken cancellationToken = default)
        {
            await control.InjectKeycodeAsync(AndroidKeyEventAction.ACTION_DOWN, androidKeyCode, repeat, AndroidKeyEventMeta.META_NONE);
            await Task.Delay(releaseDelay, cancellationToken);
            await control.InjectKeycodeAsync(AndroidKeyEventAction.ACTION_UP, androidKeyCode, repeat, AndroidKeyEventMeta.META_NONE);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="vScroll"></param>
        /// <param name="hScroll"></param>
        public static bool Scroll(this IControl control, int x, int y, int vScroll, int hScroll = 0)
            => control.InjectScrollEvent(new Rectangle(x, y, control.Scrcpy.ScreenSize.Width, control.Scrcpy.ScreenSize.Height), vScroll, hScroll);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="vScroll"></param>
        /// <param name="hScroll"></param>
        /// <returns></returns>
        public static Task<bool> ScrollAsync(this IControl control, int x, int y, int vScroll, int hScroll = 0)
            => control.InjectScrollEventAsync(new Rectangle(x, y, control.Scrcpy.ScreenSize.Width, control.Scrcpy.ScreenSize.Height), vScroll, hScroll);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="point"></param>
        /// <param name="hScroll"></param>
        /// <param name="vScroll"></param>
        /// <returns></returns>
        public static bool Scroll(this IControl control, Point point, int hScroll, int vScroll)
            => control.Scroll(point.X, point.Y, hScroll, vScroll);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="point"></param>
        /// <param name="hScroll"></param>
        /// <param name="vScroll"></param>
        /// <returns></returns>
        public static Task<bool> ScrollAsync(this IControl control, Point point, int hScroll, int vScroll)
           => control.ScrollAsync(point.X, point.Y, hScroll, vScroll);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="hScroll"></param>
        /// <param name="vScroll"></param>
        public static bool ScrollPercent(this IControl control, double x, double y, int hScroll, int vScroll)
             => control.Scroll(
                 (int)(x * control.Scrcpy.ScreenSize.Width),
                 (int)(y * control.Scrcpy.ScreenSize.Height),
                 hScroll,
                 vScroll);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="hScroll"></param>
        /// <param name="vScroll"></param>
        /// <returns></returns>
        public static Task<bool> ScrollPercentAsync(this IControl control, double x, double y, int hScroll, int vScroll)
             => control.ScrollAsync(
                 (int)(x * control.Scrcpy.ScreenSize.Width),
                 (int)(y * control.Scrcpy.ScreenSize.Height),
                 hScroll,
                 vScroll);

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
            long pointerId = random.Next(int.MinValue, int.MaxValue);
            control.InjectTouchEvent(
              AndroidMotionEventAction.ACTION_DOWN,
              pointerId,
              new Rectangle(x1, y1, control.Scrcpy.ScreenSize.Width, control.Scrcpy.ScreenSize.Height));

            Task.Delay(delayStep, cancellationToken).Wait();

            int times = duration / delayStep;
            int x = (x2 - x1) / times;
            int y = (y2 - y1) / times;
            for (int i = 1; i < times; i++)
            {
                control.InjectTouchEvent(
                  AndroidMotionEventAction.ACTION_MOVE,
                  pointerId,
                  new Rectangle(x1 + x * i, y1 + y * i, control.Scrcpy.ScreenSize.Width, control.Scrcpy.ScreenSize.Height));
                Task.Delay(delayStep, cancellationToken).Wait();
            }

            control.InjectTouchEvent(
             AndroidMotionEventAction.ACTION_UP,
             pointerId,
             new Rectangle(x2, y2, control.Scrcpy.ScreenSize.Width, control.Scrcpy.ScreenSize.Height));
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
            long pointerId = random.Next(int.MinValue, int.MaxValue);
            await control.InjectTouchEventAsync(
                AndroidMotionEventAction.ACTION_DOWN,
                pointerId,
                new Rectangle(x1, y1, control.Scrcpy.ScreenSize.Width, control.Scrcpy.ScreenSize.Height));

            await Task.Delay(delayStep, cancellationToken);

            int times = duration / delayStep;
            int x = (x2 - x1) / times;
            int y = (y2 - y1) / times;
            for (int i = 1; i < times; i++)
            {
                await control.InjectTouchEventAsync(
                      AndroidMotionEventAction.ACTION_MOVE,
                      pointerId,
                      new Rectangle(x1 + x * i, y1 + y * i, control.Scrcpy.ScreenSize.Width, control.Scrcpy.ScreenSize.Height));
                await Task.Delay(delayStep, cancellationToken);
            }

            await control.InjectTouchEventAsync(
                 AndroidMotionEventAction.ACTION_UP,
                 pointerId,
                 new Rectangle(x2, y2, control.Scrcpy.ScreenSize.Width, control.Scrcpy.ScreenSize.Height));
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
          => control.Swipe(
              (int)(x1 * control.Scrcpy.ScreenSize.Width),
              (int)(y1 * control.Scrcpy.ScreenSize.Height),
              (int)(x2 * control.Scrcpy.ScreenSize.Width),
              (int)(y2 * control.Scrcpy.ScreenSize.Height),
              duration,
              delayStep,
              cancellationToken);
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
          => control.SwipeAsync(
              (int)(x1 * control.Scrcpy.ScreenSize.Width),
              (int)(y1 * control.Scrcpy.ScreenSize.Height),
              (int)(x2 * control.Scrcpy.ScreenSize.Width),
              (int)(y2 * control.Scrcpy.ScreenSize.Height),
              duration,
              delayStep,
              cancellationToken);

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
            long pointerId = random.Next(int.MinValue, int.MaxValue);
            control.InjectTouchEvent(
              AndroidMotionEventAction.ACTION_DOWN,
              pointerId,
              new Rectangle(x1, y1, control.Scrcpy.ScreenSize.Width, control.Scrcpy.ScreenSize.Height));
            Task.Delay(delayStep, cancellationToken).Wait();

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
                  new Rectangle(x1 + x * i, y1 + y * i, control.Scrcpy.ScreenSize.Width, control.Scrcpy.ScreenSize.Height));
                Task.Delay(delayStep, cancellationToken).Wait();
            }

            control.InjectTouchEvent(
             AndroidMotionEventAction.ACTION_UP,
             pointerId,
             new Rectangle(x2, y2, control.Scrcpy.ScreenSize.Width, control.Scrcpy.ScreenSize.Height));
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
            long pointerId = random.Next(int.MinValue, int.MaxValue);
            await control.InjectTouchEventAsync(
                AndroidMotionEventAction.ACTION_DOWN,
                pointerId,
                new Rectangle(x1, y1, control.Scrcpy.ScreenSize.Width, control.Scrcpy.ScreenSize.Height));
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
                await control.InjectTouchEventAsync(
                    AndroidMotionEventAction.ACTION_MOVE,
                    pointerId,
                    new Rectangle(x1 + x * i, y1 + y * i, control.Scrcpy.ScreenSize.Width, control.Scrcpy.ScreenSize.Height));
                await Task.Delay(delayStep, cancellationToken);
            }

            await control.InjectTouchEventAsync(
                AndroidMotionEventAction.ACTION_UP,
                pointerId,
                new Rectangle(x2, y2, control.Scrcpy.ScreenSize.Width, control.Scrcpy.ScreenSize.Height));
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
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<string> GetClipboardAsync(this IControl control, int timeout = 2000)
        {
            TaskCompletionSource<string> taskCompletionSource = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            OnDataReceived<string> dataDelegate = (text) => taskCompletionSource.TrySetResult(text);
            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
            try
            {
                using var register = cancellationTokenSource.Token.Register(() => taskCompletionSource.TrySetException(new TimeoutException($"GetClipboardAsync Timeout {timeout}")));
                control.OnClipboardReceived += dataDelegate;

                await taskCompletionSource.Task.ConfigureAwait(false);
                return taskCompletionSource.Task.Result;
            }
            finally
            {
                control.OnClipboardReceived -= dataDelegate;
            }
        }
    }
}
