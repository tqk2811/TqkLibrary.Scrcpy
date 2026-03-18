using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using TqkLibrary.Scrcpy.Configs;
using TqkLibrary.Scrcpy.Enums;

namespace TqkLibrary.Scrcpy
{
    internal delegate void NativeOnDisconnectDelegate();
    internal delegate void NativeOnClipboardReceivedDelegate(IntPtr intPtr, int length);
    internal delegate void NativeOnClipboardAcknowledgementDelegate(long length);
    internal delegate void NativeUhdiOutputDelegate(UInt16 id, UInt16 size, IntPtr buff);
    internal static class NativeWrapper
    {
#if DEBUG
#if NETFRAMEWORK
        static NativeWrapper()
        {
            string path = Path.Combine(
                Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!,
                "runtimes",
                Environment.Is64BitProcess ? "win-x64" : "win-x86",
                "native"
                );

            bool r = SetDllDirectory(path);
            if (!r)
                throw new InvalidOperationException("Can't set Kernel32.SetDllDirectory");
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        internal static extern bool SetDllDirectory(string PathName);
#endif
#endif

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern byte FFmpegHWSupport(byte bHWSupport);

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ScrcpyAlloc();

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ScrcpyFree(IntPtr scrcpy);

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool ScrcpyConnect(IntPtr scrcpy, ref ScrcpyNativeConfig nativeConfig, IntPtr videoSock, IntPtr audioSock, IntPtr controlSock);

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ScrcpyStop(IntPtr scrcpy);
        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool IsHaveScrcpyInstance(IntPtr scrcpy);

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool ScrcpyGetScreenSize(IntPtr scrcpy, ref int w, ref int h);
[DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool ScrcpyControlCommand(IntPtr scrcpy, [In][MarshalAs(UnmanagedType.LPArray)] byte[] command, int sizeInByte);

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool ScrcpyGetScreenShot(IntPtr scrcpy, IntPtr buffer, int sizeInByte, int w, int h, int lineSize, SwsFlag swsFlag = SwsFlag.SWS_FAST_BILINEAR);

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long ScrcpyReadAudioRaw(
            IntPtr scrcpy,
            [In][Out][MarshalAs(UnmanagedType.LPArray)] byte[] buffer,
            int bufferSize,
            int outNbChannels,
            int outSampleRate,
            int outSampleFmt,
            long last_pts,
            UInt32 waitFrameTime,
            ref int outBytesWritten);


        #region Callback

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool RegisterClipboardEvent(IntPtr scrcpy, IntPtr nativeOnClipboardReceivedDelegate);
        internal static bool RegisterClipboardEvent(this Scrcpy scrcpy, NativeOnClipboardReceivedDelegate onClipboardReceivedDelegate)
        {
            IntPtr pointer = Marshal.GetFunctionPointerForDelegate(onClipboardReceivedDelegate);
            return RegisterClipboardEvent(scrcpy.Handle, pointer);
        }

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool RegisterClipboardAcknowledgementEvent(IntPtr scrcpy, IntPtr clipboardAcknowledgementDelegate);
        internal static bool RegisterClipboardAcknowledgementEvent(this Scrcpy scrcpy, NativeOnClipboardAcknowledgementDelegate clipboardAcknowledgementDelegate)
        {
            IntPtr pointer = Marshal.GetFunctionPointerForDelegate(clipboardAcknowledgementDelegate);
            return RegisterClipboardAcknowledgementEvent(scrcpy.Handle, pointer);
        }

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool RegisterDisconnectEvent(IntPtr scrcpy, IntPtr delegateHandle);
        internal static bool RegisterDisconnectEvent(this Scrcpy scrcpy, NativeOnDisconnectDelegate onDisconnectDelegate)
        {
            IntPtr pointer = Marshal.GetFunctionPointerForDelegate(onDisconnectDelegate);
            return RegisterDisconnectEvent(scrcpy.Handle, pointer);
        }

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool RegisterUhdiOutputEvent(IntPtr scrcpy, IntPtr uhdiOutputDelegate);
        internal static bool RegisterUhdiOutputEvent(this Scrcpy scrcpy, NativeUhdiOutputDelegate uhdiOutputDelegate)
        {
            IntPtr pointer = Marshal.GetFunctionPointerForDelegate(uhdiOutputDelegate);
            return RegisterUhdiOutputEvent(scrcpy.Handle, pointer);
        }
        #endregion




        #region D3D

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr D3DImageViewAlloc();

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void D3DImageViewFree(IntPtr d3dView);

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool D3DImageViewRender(IntPtr d3dView, IntPtr scrcpy, IntPtr surface, bool isNewSurface, ref bool isNewtargetView);

        #endregion





    }
}
