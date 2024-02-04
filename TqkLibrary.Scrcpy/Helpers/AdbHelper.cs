using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TqkLibrary.Scrcpy.Helpers
{
    internal static class AdbHelper
    {
        public static async Task PushServerAsync(
            string adbPath,
            string deviceId,
            string scrcpyPath,
            CancellationToken cancellationToken = default)
        {
            using Process process = CreateProcess(adbPath, $"-s {deviceId} push {scrcpyPath} /sdcard/scrcpy-server-tqk.jar");
            string stdout = await process.StandardOutput.ReadToEndAsync();
            string stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync(cancellationToken);
        }


        public static async Task<ProcessStd> RunServerWithAdbAsync(
            string adbPath,
            string deviceId,
            string argument,
            CancellationToken cancellationToken = default)
        {
            using Process process = CreateProcess(adbPath, $"-s {deviceId} {argument}");
            string stdout = await process.StandardOutput.ReadToEndAsync();
            string stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync(cancellationToken);
            return new ProcessStd(stdout, stderr);
        }




        static Process CreateProcess(string file, string query)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = file;
            processStartInfo.UseShellExecute = false;
            processStartInfo.Arguments = query;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            return Process.Start(processStartInfo);
        }

#if !NET5_0_OR_GREATER
        static async Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default)
        {
            process.EnableRaisingEvents = true;
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            process.Exited += (object sender, EventArgs e) => tcs.TrySetResult(null);
            using var register = cancellationToken.Register(() => tcs.TrySetCanceled());
            if (process.HasExited) return;
            await tcs.Task;
        }
#endif
        public class ProcessStd
        {
            public ProcessStd(string stdOut, string stdErr)
            {
                this.StdOut = stdOut;
                this.StdErr = stdErr;
            }
            public string StdOut { get; set; }
            public string StdErr { get; set; }
        }
    }
}
