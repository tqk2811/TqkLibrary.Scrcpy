namespace TqkLibrary.Scrcpy.ListSupport
{
    /// <summary>
    /// An installed application reported by scrcpy <c>--list-apps</c> (scrcpy 3.0+).
    /// </summary>
    public class AppInfo
    {
        /// <summary>
        /// Display name (label) of the app.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Package name (e.g. <c>com.android.settings</c>).
        /// </summary>
        public string PackageName { get; set; } = string.Empty;
        /// <summary>
        /// true for a system app ('*' in the scrcpy output), false for a user app ('-').
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{(IsSystem ? "*" : "-")} {Name} ({PackageName})";
        }
    }
}
