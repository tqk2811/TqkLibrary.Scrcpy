using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TqkLibrary.Scrcpy.Attributes;
using TqkLibrary.Scrcpy.Enums;
using TqkLibrary.Scrcpy.Interfaces;

namespace TqkLibrary.Scrcpy.ListSupport
{
    /// <summary>
    /// 
    /// </summary>
    public class ScrcpyServerListSupport
    {
        // Tolerant of scrcpy 2.x and 3.0+/4.0 --list-* output:
        //  - encoder name: quoted ('name', scrcpy 2.x) OR unquoted with trailing " (hw)/(sw) [vendor]" (scrcpy 3.0+, commit acff5b00)
        //  - camera fps set: "[15, 30]" (<=3.3.4) OR "{15, 30}" (4.0, commit af355804); optional trailing ", zoom-range=[...]" (4.0)
        static readonly Regex regex_video = new Regex("--video-codec=(\\S+) --video-encoder='?([^'\\s]+)'?");
        static readonly Regex regex_audio = new Regex("--audio-codec=(\\S+) --audio-encoder='?([^'\\s]+)'?");
        static readonly Regex regex_display = new Regex("--display-id=(\\d+) +\\((\\d+)x(\\d+)\\)");
        static readonly Regex regex_camera = new Regex("--camera-id=(\\d+) +\\((\\S+), (\\d+)x(\\d+), fps=[\\[{]([0-9 ,]+)[\\]}]");
        static readonly Regex regex_camera_size = new Regex("- (\\d+)x(\\d+)");
        static readonly Regex regex_camera_fps = new Regex("\\(fps=[\\[{]([0-9 ,]+)[\\]}]\\)");
        internal static ScrcpyServerListSupport Parse(string data)
        {
            /*
[server] INFO: Device: [Xiaomi] Redmi Redmi Note 9S (Android 12)
[server] INFO: List of video encoders:
    --video-codec=h264 --video-encoder=OMX.qcom.video.encoder.avc     (hw) [vendor]
    --video-codec=h264 --video-encoder=c2.android.avc.encoder         (sw)
    --video-codec=h264 --video-encoder=OMX.google.h264.encoder        (sw) (alias for c2.android.avc.encoder)
    --video-codec=h265 --video-encoder=OMX.qcom.video.encoder.hevc    (hw) [vendor]
    --video-codec=h265 --video-encoder=OMX.qcom.video.encoder.hevc.cq (hw) [vendor]
    --video-codec=h265 --video-encoder=c2.android.hevc.encoder        (sw)
[server] INFO: List of audio encoders:
    --audio-codec=opus --audio-encoder=c2.android.opus.encoder        (sw)
    --audio-codec=aac --audio-encoder=c2.android.aac.encoder          (sw)
    --audio-codec=aac --audio-encoder=OMX.google.aac.encoder          (sw) (alias for c2.android.aac.encoder)
    --audio-codec=flac --audio-encoder=c2.android.flac.encoder        (sw)
    --audio-codec=flac --audio-encoder=OMX.google.flac.encoder        (sw) (alias for c2.android.flac.encoder)
[server] INFO: List of displays:
    --display-id=0    (1080x2400)
[server] INFO: List of cameras:
    --camera-id=0    (back, 4000x3000, fps=[15, 30])
        - 3840x2160
        - 3264x2448
        - 3200x2400
        - 2688x1512
        - 2608x1960
        - 2592x1944
        - 2592x1940
        - 2400x1080
        - 2340x1080
        - 2304x1728
        - 2160x1080
        - 1920x1440
        - 1920x1080
        - 1600x1200
        - 1560x720
        - 1440x1080
        - 1440x720
        - 1280x960
        - 1280x768
        - 1280x720
        - 1024x738
        - 1024x768
        - 800x600
        - 800x480
        - 720x480
        - 640x480
        - 640x360
        - 352x288
        - 320x240
        - 176x144
      High speed capture (--camera-high-speed):
        - 1280x720 (fps=[120, 240])
        - 720x480 (fps=[120, 240])
        - 640x480 (fps=[120, 240])
        - 1920x1080 (fps=[120, 240])
    --camera-id=1    (front, 2304x1728, fps=[15, 30])
        - 2304x1728
        - 2160x1080
        - 1920x1440
        - 1920x1080
        - 1600x1200
        - 1560x720
        - 1440x1080
        - 1440x720
        - 1280x960
        - 1280x768
        - 1280x720
        - 1024x738
        - 1024x768
        - 800x600
        - 800x480
        - 720x480
        - 640x480
        - 640x360
        - 352x288
        - 320x240
        - 176x144
      High speed capture (--camera-high-speed):
        - 1280x720 (fps=[120])
        - 720x480 (fps=[120])
        - 640x480 (fps=[120])

             */

            var datas = data.Split('\r', '\n')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToArray();
            ScrcpyServerListSupport result = new ScrcpyServerListSupport();
            string l_current = string.Empty;
            bool isCameraHighSpeed = false;
            CameraInfo? cameraInfoCurrent = null;

            foreach (var d in datas)
            {
                if (d.StartsWith("[server] INFO: List of"))
                {
                    l_current = d;
                }

                Match match = regex_video.Match(d);
                if (match.Success)
                {
                    string codec = match.Groups[1].Value;
                    string encoder = match.Groups[2].Value;
                    result.Videos.Add(new CodecInfo()
                    {
                        Codec = codec,
                        Encoder = encoder,
                    });
                    continue;
                }

                match = regex_audio.Match(d);
                if (match.Success)
                {
                    string codec = match.Groups[1].Value;
                    string encoder = match.Groups[2].Value;
                    result.Audios.Add(new CodecInfo()
                    {
                        Codec = codec,
                        Encoder = encoder,
                    });
                    continue;
                }

                match = regex_display.Match(d);
                if (match.Success)
                {
                    string display = match.Groups[1].Value;
                    string w = match.Groups[2].Value;
                    string h = match.Groups[3].Value;
                    result.Displays.Add(new DisplayInfo()
                    {
                        Display = display,
                        Size = new Size(int.Parse(w), int.Parse(h)),
                    });
                    continue;
                }

                if (l_current.Contains("cameras:"))
                {
                    match = regex_camera.Match(d);
                    if (match.Success)
                    {
                        string id = match.Groups[1].Value;
                        string facing = match.Groups[2].Value;
                        string w = match.Groups[3].Value;
                        string h = match.Groups[4].Value;
                        string[] fps = match.Groups[5].Value.Split(',').Select(x => x.Trim()).ToArray();
                        isCameraHighSpeed = false;//reset
                        cameraInfoCurrent = new CameraInfo()
                        {
                            CameraId = int.Parse(id),
                            CameraFacing = (CameraFacing)Enum.Parse(typeof(CameraFacing), facing, true),
                            FpsMin = int.Parse(fps.First()),
                            FpsMax = int.Parse(fps.Last())
                        };
                    }
                    else
                    {
                        if (d.Contains("--camera-high-speed"))
                        {
                            isCameraHighSpeed = true;
                        }
                        else
                        {
                            match = regex_camera_size.Match(d);
                            if (match.Success)
                            {
                                string w = match.Groups[1].Value;
                                string h = match.Groups[2].Value;
                                if (cameraInfoCurrent is null)
                                    throw new InvalidOperationException();
                                CameraInfo cameraInfo = new CameraInfo()
                                {
                                    CameraId = cameraInfoCurrent.CameraId,
                                    CameraFacing = cameraInfoCurrent.CameraFacing,
                                    FpsMin = cameraInfoCurrent.FpsMin,
                                    FpsMax = cameraInfoCurrent.FpsMax,
                                    Size = new Size(int.Parse(w), int.Parse(h)),
                                    IsHighSpeed = isCameraHighSpeed,
                                };

                                match = regex_camera_fps.Match(d);
                                if (match.Success)
                                {
                                    string[] fps = match.Groups[1].Value.Split(',').Select(x => x.Trim()).ToArray();
                                    cameraInfo.FpsMin = int.Parse(fps.First());
                                    cameraInfo.FpsMax = int.Parse(fps.Last());
                                }
                                result.CameraInfos.Add(cameraInfo);
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        public List<CodecInfo> Videos { get; set; } = new List<CodecInfo>();
        /// <summary>
        /// 
        /// </summary>
        public List<CodecInfo> Audios { get; set; } = new List<CodecInfo>();
        /// <summary>
        /// 
        /// </summary>
        public List<DisplayInfo> Displays { get; set; } = new List<DisplayInfo>();
        /// <summary>
        /// 
        /// </summary>
        public List<CameraInfo> CameraInfos { get; set; } = new List<CameraInfo>();
    }
}
