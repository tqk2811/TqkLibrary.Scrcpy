using System.Drawing;
using TqkLibrary.Scrcpy.Enums;

namespace TqkLibrary.Scrcpy.ListSupport
{
    /// <summary>
    /// 
    /// </summary>
    public class CameraInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public int CameraId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public CameraFacing CameraFacing { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsHighSpeed { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Size Size { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int FpsMax { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int FpsMin { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{CameraFacing}, Size: {Size}, IsHighSpeed: {IsHighSpeed}, Fps=[{FpsMin}-{FpsMax}]";
        }
    }
}
