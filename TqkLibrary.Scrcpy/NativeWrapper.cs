using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using TqkLibrary.Scrcpy.Configs;

namespace TqkLibrary.Scrcpy
{
    internal static class NativeWrapper
    {
#if DEBUG
        static NativeWrapper()
        {
            string path = Path.Combine(
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                Environment.Is64BitProcess ? "x64" : "x86");

            bool r = SetDllDirectory(path);
            if (!r)
                throw new InvalidOperationException("Can't set Kernel32.SetDllDirectory");
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool SetDllDirectory(string PathName);
#endif

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern byte FFmpegHWSupport(byte bHWSupport);

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool ClearKey();

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool AddKey(byte[] key, int sizeInByte);

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ScrcpyAlloc(string deviceId);

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ScrcpyFree(IntPtr scrcpy);

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool ScrcpyConnect(IntPtr scrcpy, ref ScrcpyNativeConfig nativeConfig);

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ScrcpyStop(IntPtr scrcpy);
        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool IsHaveScrcpyInstance(IntPtr scrcpy);

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool ScrcpyGetScreenSize(IntPtr scrcpy, ref int w, ref int h);
        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool ScrcpyGetDeviceName(IntPtr scrcpy, [In][Out][MarshalAs(UnmanagedType.LPArray)] byte[] buffer, int sizeInByte);

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool ScrcpyControlCommand(IntPtr scrcpy, [In][MarshalAs(UnmanagedType.LPArray)] byte[] command, int sizeInByte);

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool ScrcpyGetScreenShot(IntPtr scrcpy, IntPtr buffer, int sizeInByte, int w, int h, int lineSize);






        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool RegisterClipboardEvent(IntPtr scrcpy, IntPtr delegateHandle);

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool RegisterClipboardAcknowledgementEvent(IntPtr scrcpy, IntPtr delegateHandle);

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool RegisterDisconnectEvent(IntPtr scrcpy, IntPtr delegateHandle);





        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr D3DImageViewAlloc();

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void D3DImageViewFree(IntPtr d3dView);

        [DllImport("TqkLibrary.ScrcpyNative.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool D3DImageViewRender(IntPtr d3dView, IntPtr scrcpy, IntPtr surface, bool isNewSurface, ref bool isNewtargetView);
    }
}
