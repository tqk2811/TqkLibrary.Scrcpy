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
using TqkLibrary.Scrcpy.Interfaces;

namespace TqkLibrary.Scrcpy.ListSupport
{
    /// <summary>
    /// 
    /// </summary>
    public class ScrcpyServerListSupport
    {
        static readonly Regex regex_video = new Regex("--video-codec=(\\S+) --video-encoder='(\\S+)'");
        static readonly Regex regex_audio = new Regex("--audio-codec=(\\S+) --audio-encoder='(\\S+)'");
        static readonly Regex regex_display = new Regex("--display-id=(\\d+) +\\((\\d+)x(\\d+)\\)");
        static readonly Regex regex_camera = new Regex("--camera-id=(\\d+) +\\((\\S+), (\\d+)x(\\d+), fps=\\[([0-9 ,]+)\\]\\)");
        static readonly Regex regex_camera_size = new Regex("- (\\d+)x(\\d+)");
        static readonly Regex regex_camera_fps = new Regex("\\(fps=\\[([0-9 ,]+)\\]\\)");
        internal static ScrcpyServerListSupport Parse(string data)
        {
            /*
[server] INFO: List of video encoders:
    --video-codec=h264 --video-encoder='OMX.qcom.video.encoder.avc'
    --video-codec=h264 --video-encoder='c2.android.avc.encoder'
    --video-codec=h264 --video-encoder='OMX.google.h264.encoder'
    --video-codec=h265 --video-encoder='OMX.qcom.video.encoder.hevc'
    --video-codec=h265 --video-encoder='OMX.qcom.video.encoder.hevc.cq'
    --video-codec=h265 --video-encoder='c2.android.hevc.encoder'
[server] INFO: List of audio encoders:
    --audio-codec=opus --audio-encoder='c2.android.opus.encoder'
    --audio-codec=aac --audio-encoder='c2.android.aac.encoder'
    --audio-codec=aac --audio-encoder='OMX.google.aac.encoder'
[server] INFO: List of displays:
    --display-id=0    (1080x2400)
[server] INFO: List of cameras:
    --video-source=camera --camera-id=0    (back, 4000x3000, fps=[15, 30])
        - 3840x2160
        - 3264x2448
        - 3200x2400
        - 2688x1512
        - 2608x1960
            .....
      High speed capture (--camera-high-speed):
        - 1280x720 (fps=[120, 240])
        - 720x480 (fps=[120, 240])
        - 640x480 (fps=[120, 240])
        - 1920x1080 (fps=[120, 240])
    --video-source=camera --camera-id=1    (front, 2304x1728, fps=[15, 30])
        - 2304x1728
        - 2160x1080
        - 1920x1440
        - 1920x1080
            ....
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
            CameraInfo cameraInfoCurrent = null;

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
