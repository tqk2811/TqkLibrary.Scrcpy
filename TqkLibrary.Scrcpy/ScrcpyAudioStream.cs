using System;
using System.IO;
using TqkLibrary.Scrcpy.Enums;

namespace TqkLibrary.Scrcpy
{
    /// <summary>
    /// A read-only <see cref="Stream"/> that delivers decoded audio from a scrcpy session,
    /// resampled to the requested format, sample rate, and channel count.
    /// </summary>
    public class ScrcpyAudioStream : Stream
    {
        private readonly Scrcpy _scrcpy;
        private readonly byte[] _internalBuffer;
        private int _bufferOffset;
        private int _bufferCount;
        private long _lastPts;

        /// <summary>Output sample format.</summary>
        public AVSampleFormat Format { get; }

        /// <summary>Output sample rate in Hz.</summary>
        public int SampleRate { get; }

        /// <summary>Number of output channels.</summary>
        public int Channels { get; }

        /// <summary>Bytes per single sample (one channel).</summary>
        public int SampleSizeBytes { get; }

        internal ScrcpyAudioStream(Scrcpy scrcpy, AVSampleFormat format, int sampleRate, int channels)
        {
            _scrcpy = scrcpy ?? throw new ArgumentNullException(nameof(scrcpy));
            Format = format;
            SampleRate = sampleRate;
            Channels = channels;
            SampleSizeBytes = GetBytesPerSample(format);
            // Buffer large enough for ~1 second of audio
            _internalBuffer = new byte[sampleRate * channels * SampleSizeBytes];
        }

        /// <inheritdoc/>
        public override bool CanRead => true;
        /// <inheritdoc/>
        public override bool CanSeek => false;
        /// <inheritdoc/>
        public override bool CanWrite => false;
        /// <inheritdoc/>
        public override long Length => throw new NotSupportedException();
        /// <inheritdoc/>
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
        /// <inheritdoc/>
        public override void Flush() { }
        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void SetLength(long value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        /// <summary>
        /// Reads decoded audio bytes. Blocks until data is available or the connection is lost.
        /// Returns 0 when the scrcpy session is disconnected.
        /// </summary>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer is null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException();

            while (true)
            {
                if (_bufferCount > 0)
                {
                    int toCopy = Math.Min(count, _bufferCount);
                    Array.Copy(_internalBuffer, _bufferOffset, buffer, offset, toCopy);
                    _bufferOffset += toCopy;
                    _bufferCount -= toCopy;
                    return toCopy;
                }

                if (!_scrcpy.IsConnected)
                    return 0;

                int bytesWritten = 0;
                long newPts = NativeWrapper.ScrcpyReadAudioRaw(
                    _scrcpy.Handle,
                    _internalBuffer,
                    _internalBuffer.Length,
                    Channels,
                    SampleRate,
                    (int)Format,
                    _lastPts,
                    100,
                    ref bytesWritten);

                if (newPts >= 0 && bytesWritten > 0)
                {
                    _lastPts = newPts;
                    _bufferOffset = 0;
                    _bufferCount = bytesWritten;
                }
            }
        }

        private static int GetBytesPerSample(AVSampleFormat fmt)
        {
            switch (fmt)
            {
                case AVSampleFormat.U8:
                case AVSampleFormat.U8P:
                    return 1;
                case AVSampleFormat.S16:
                case AVSampleFormat.S16P:
                    return 2;
                case AVSampleFormat.S32:
                case AVSampleFormat.S32P:
                case AVSampleFormat.FLT:
                case AVSampleFormat.FLTP:
                    return 4;
                case AVSampleFormat.DBL:
                case AVSampleFormat.DBLP:
                case AVSampleFormat.S64:
                case AVSampleFormat.S64P:
                    return 8;
                default:
                    return 2;
            }
        }
    }
}
