using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.Scrcpy.Interfaces;

namespace TqkLibrary.Scrcpy.Configs
{
    /// <summary>
    /// 
    /// </summary>
    public class CameraConfig : IConfig
    {
        /// <summary>
        /// default: null
        /// </summary>
        public string CameraId { get; set; } = null;
        /// <summary>
        /// default: null
        /// </summary>
        public Rectangle? CameraSize { get; set; }
        /// <summary>
        /// default: <see cref="CameraFacing.Any"/>
        /// </summary>
        public CameraFacing CameraFacing { get; set; } = CameraFacing.Any;
        /// <summary>
        /// default: null
        /// </summary>
        public string CameraAr { get; set; } = null;
        /// <summary>
        /// default: 0
        /// </summary>
        public int Camerafps { get; set; } = 0;
        /// <summary>
        /// default: false
        /// </summary>
        public bool CameraHighSpeed { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetArguments()
        {
            yield return this._GetArgument(x => x.CameraId, string.IsNullOrWhiteSpace);
            yield return this._GetArgument(x => x.CameraSize, x => !string.IsNullOrWhiteSpace(CameraId));
            yield return this._GetArgument(x => x.CameraFacing, x => x.ToString().ToLower());
            yield return this._GetArgument(x => x.CameraAr, string.IsNullOrWhiteSpace);
            yield return this._GetArgument(x => x.Camerafps, x => x > 0);
            yield return this._GetArgument(x => x.CameraHighSpeed, CameraHighSpeed);
        }
    }
}
