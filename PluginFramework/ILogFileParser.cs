namespace PluginFramework
{
    public interface ILogFileParser
    {
        bool CanOpen(string filename);

        /// <summary>
        /// Open the specified log file using the specified settings.
        /// </summary>
        /// <param name="filename">The filename to open</param>
        /// <param name="settings">The setting to use when loading. Setting should be collected from user with the ShowSettings method.</param>
        /// <returns>The loaded log file object.</returns>
        ILogFile Open(string filename, ParserSettings settings = null);

        /// <summary>
        /// Show file load settings dialog to user
        /// </summary>
        /// <param name="filename">The filename to open</param>
        /// <param name="settings">Key/value collection of settings. </param>
        /// <returns>True if file should be loaded. False if loading should be aborted.</returns>
        bool ShowSettingsDialog(string filename, ref ParserSettings settings);
    }
}
