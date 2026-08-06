using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.Scrcpy.Configs
{
    /// <summary>
    /// Everything needed to reach the device and deploy the scrcpy server jar onto it: where adb is,
    /// which local jar to send, and where it lands on the device.<br></br>
    /// Held by <see cref="ScrcpyConfig.DeployConfig"/> and usable on its own with
    /// <see cref="Scrcpy.PushServer(ScrcpyDeployConfig?)"/> — deploying is a one-off step, so it needs
    /// none of the video/audio/control options a mirroring session carries.
    /// </summary>
    public class ScrcpyDeployConfig
    {
        /// <summary>
        /// Path to the adb executable used for every device command: pushing the jar, setting up the
        /// reverse tunnel and launching the server.<br></br>
        /// Default: adb.exe
        /// </summary>
        public string AdbPath { get; set; } = "adb.exe";

        /// <summary>
        /// Path to the local scrcpy server jar that is pushed to the device.<br></br>
        /// Default: scrcpy-server.jar
        /// </summary>
        public string ScrcpyServerPath { get; set; } = "scrcpy-server.jar";

        /// <summary>
        /// Path on the device the jar is pushed to, and the path
        /// <see cref="Scrcpy.Connect(ScrcpyConfig?)"/> launches the server from. <c>{ver}</c> is replaced
        /// with the server version bundled with this package, so jars of different versions never
        /// overwrite each other.<br></br>
        /// Default: /sdcard/scrcpy-server-tqk-{ver}.jar
        /// </summary>
        public string ScrcpyServerAndroidPath { get; set; } = Constant.ScrcpyServerAndroidPath;

        /// <summary>
        /// Push <see cref="ScrcpyServerPath"/> to the device on every connect.<br></br>
        /// The jar stays on the device between connections, so pushing it again each time only costs
        /// time. Set to false to skip the push and reconnect faster — the caller is then responsible
        /// for the jar already being on the device at <see cref="ScrcpyServerAndroidPath"/> (call
        /// <see cref="Scrcpy.PushServer(ScrcpyDeployConfig?)"/> once, e.g. on the first connect),
        /// otherwise the server fails to start and the connect fails.<br></br>
        /// Default: true
        /// </summary>
        public bool ForcePush { get; set; } = true;

        /// <summary>
        /// Time in milliseconds to wait for the device to connect before giving up.<br></br>
        /// Default: 3000
        /// </summary>
        public int ConnectionTimeout { get; set; } = 3000;

        /// <summary>
        /// <see cref="ScrcpyServerAndroidPath"/> with <c>{ver}</c> resolved — the real path on the device.
        /// </summary>
        /// <returns></returns>
        public string GetResolvedAndroidPath()
        {
            return ScrcpyServerAndroidPath.Replace("{ver}", Constant.ScrcpyServerVersion);
        }
    }
}
