namespace TqkLibrary.Scrcpy.Enums
{
    /// <summary>
    /// https://github.com/Genymobile/scrcpy/blob/ce43fad645d4eb30f322dbeb50d5197601564931/server/src/main/java/com/genymobile/scrcpy/Device.java#L25
    /// https://android.googlesource.com/platform/frameworks/base.git/+/pie-release-2/core/java/android/view/SurfaceControl.java#305
    /// </summary>
    public enum ScrcpyScreenPowerMode : byte
    {
        /// <summary>
        /// Display power mode off: used while blanking the screen.
        /// </summary>
        POWER_MODE_OFF = 0,
        /// <summary>
        /// Display power mode doze: used while putting the screen into low power mode.
        /// </summary>
        POWER_MODE_DOZE = 1,
        /// <summary>
        /// Display power mode normal: used while unblanking the screen.
        /// </summary>
        POWER_MODE_NORMAL = 2,
        /// <summary>
        /// Display power mode doze: used while putting the screen into a suspended low power mode.
        /// </summary>
        POWER_MODE_DOZE_SUSPEND = 3,
        /// <summary>
        /// Display power mode on: used while putting the screen into a suspended full power mode.
        /// </summary>
        POWER_MODE_ON_SUSPEND = 4,

    }
}
