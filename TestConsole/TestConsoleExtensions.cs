using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.Scrcpy;
using TqkLibrary.Scrcpy.Configs;
using TqkLibrary.Scrcpy.Enums;

namespace TestConsole
{
    static class TestConsoleExtensions
    {
        public static ScrcpyConfig GenControlOnlyConfigure()
        {
            return new ScrcpyConfig()
            {
                HwType = FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_NONE,
                IsUseD3D11ForConvert = false,
                IsUseD3D11ForUiRender = false,
                ConnectionTimeout = 10000,
                ServerConfig = new ScrcpyServerConfig()
                {
                    IsControl = true,
                    IsVideo = false,
                    AndroidConfig = new()
                    {
                        ShowTouches = true,
                        StayAwake = true,
                        PowerOn = true,
                    },
                    Cleanup = false,
                    ClipboardAutosync = true,
                    VideoConfig = null,
                    AudioConfig = null,
                    CameraConfig = null,
                    VideoSource = VideoSource.Display,
                    SCID = new Random(DateTime.Now.Millisecond).Next()
                },
            };
        }
        public static ScrcpyConfig GenVideoOnlyConfigure()
        {
            return new ScrcpyConfig()
            {
                HwType = FFmpegAVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA,
                IsUseD3D11ForConvert = true,
                IsUseD3D11ForUiRender = true,
                ConnectionTimeout = 10000,
                ServerConfig = new ScrcpyServerConfig()
                {
                    IsControl = false,
                    IsVideo = true,
                    VideoSource = VideoSource.Display,
                    AndroidConfig = new()
                    {
                        ShowTouches = true,
                        StayAwake = true,
                        PowerOn = true,
                    },
                    Cleanup = false,
                    ClipboardAutosync = false,
                    VideoConfig = new VideoConfig()
                    {
                        MaxFps = 24,
                        Orientation = Orientations.Natural,
                    },
                    SCID = new Random(DateTime.Now.Millisecond).Next()
                },
            };
        }

        public static ScrcpyConfig EnableControl(this ScrcpyConfig scrcpyConfig)
        {
            scrcpyConfig.ServerConfig.IsControl = true;
            return scrcpyConfig;
        }


    }
}
