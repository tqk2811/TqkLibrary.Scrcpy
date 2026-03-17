namespace TqkLibrary.Scrcpy.Enums
{
    /// <summary>
    /// Audio sample formats, matching FFmpeg AVSampleFormat values.
    /// </summary>
    public enum AVSampleFormat : int
    {
        /// <summary>unsigned 8 bits</summary>
        U8 = 0,
        /// <summary>signed 16 bits (PCM_S16LE)</summary>
        S16 = 1,
        /// <summary>signed 32 bits</summary>
        S32 = 2,
        /// <summary>float (32-bit IEEE 754)</summary>
        FLT = 3,
        /// <summary>double (64-bit IEEE 754)</summary>
        DBL = 4,
        /// <summary>unsigned 8 bits, planar</summary>
        U8P = 5,
        /// <summary>signed 16 bits, planar</summary>
        S16P = 6,
        /// <summary>signed 32 bits, planar</summary>
        S32P = 7,
        /// <summary>float, planar</summary>
        FLTP = 8,
        /// <summary>double, planar</summary>
        DBLP = 9,
        /// <summary>signed 64 bits</summary>
        S64 = 10,
        /// <summary>signed 64 bits, planar</summary>
        S64P = 11,
    }
}
