namespace TqkLibrary.Scrcpy.Enums
{
    /// <summary>
    /// Display IME policy (scrcpy 3.2, --display-ime-policy).
    /// </summary>
    public enum DisplayImePolicy
    {
        /// <summary>
        /// Show the IME on the captured display (local).
        /// </summary>
        Local,
        /// <summary>
        /// Show the IME on the fallback (default) display.
        /// </summary>
        Fallback,
        /// <summary>
        /// Hide the IME.
        /// </summary>
        Hide
    }
}
