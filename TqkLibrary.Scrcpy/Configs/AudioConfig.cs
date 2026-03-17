using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.Scrcpy.Attributes;
using TqkLibrary.Scrcpy.Enums;
using TqkLibrary.Scrcpy.Interfaces;

namespace TqkLibrary.Scrcpy.Configs
{
    /// <summary>
    /// 
    /// </summary>
    public class AudioConfig : IConfig
    {
        /// <summary>
        /// true to Enable audio stream<br></br>
        /// Default: false
        /// </summary>
        [OptionName("audio")]
        public bool IsAudio { get; set; } = false;
        /// <summary>
        /// Max bitrate of audio stream<br></br>
        /// Default: 0 (ignore)
        /// </summary>
        [OptionName("audio_bit_rate")]
        public int AudioBitrate { get; set; }
        /// <summary>
        /// AudioCodec<br></br>
        /// Default: null (ignore)<br></br>
        /// Support: h264, h265, av1, opus, aac, raw
        /// </summary>
        [OptionName("audio_codec")]
        public string? AudioCodec { get; set; }
        /// <summary>
        /// AudioCodecOption<br></br>
        /// Default: null (ignore)
        /// </summary>
        [OptionName("audio_codec_options")]
        public string? AudioCodecOption { get; set; }
        /// <summary>
        /// AudioEncoder<br></br>
        /// Default: null (ignore)
        /// </summary>
        [OptionName("audio_encoder")]
        public string? AudioEncoder { get; set; }
        /// <summary>
        /// Audio source<br></br>
        /// Default: null (auto)<br></br>
        /// Use <see cref="AudioSource.Playback"/> to capture device audio playback without muting it (Android 13+)
        /// </summary>
        [OptionName("audio_source")]
        public AudioSource? AudioSource { get; set; }
        /// <summary>
        /// Mirror audio to both scrcpy and the device speaker simultaneously<br></br>
        /// Default: false
        /// </summary>
        [OptionName("audio_dup")]
        public bool AudioDup { get; set; } = false;



        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetArguments()
        {
            yield return this._GetArgument(x => x.IsAudio);
            yield return this._GetArgument(x => x.AudioBitrate, x => IsAudio && x > 0);
            yield return this._GetArgument(x => x.AudioCodec, x => IsAudio && string.IsNullOrWhiteSpace(x));
            yield return this._GetArgument(x => x.AudioCodecOption, x => IsAudio && string.IsNullOrWhiteSpace(x));
            yield return this._GetArgument(x => x.AudioEncoder, x => IsAudio && string.IsNullOrWhiteSpace(x));
            yield return this._GetArgument(x => x.AudioSource, x => IsAudio && x.HasValue && x.Value != AudioSource.Auto, x => x!.Value.ToString().ToLower());
            yield return this._GetArgument(x => x.AudioDup, IsAudio && AudioDup);
        }
    }
}
