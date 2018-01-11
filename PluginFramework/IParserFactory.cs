using System;

namespace PluginFramework
{
    /// <summary>
    /// Interface for providing a log file parser. 
    /// </summary>
    public interface IParserFactory
    {
        /// <summary>
        /// Returns true if the specified file can be opened.
        /// </summary>
        /// Allows for further checks that just the filename, for example reading a header.
        /// <param name="filename">the filename to check</param>
        /// <returns>True if file can be loaded. False if loading is not possible.</returns>
        bool CanOpen(string filename);

        /// <summary>
        /// Open the specified log file using the specified settings.
        /// </summary>
        /// <param name="filename">The filename to open</param>
        /// <param name="settings">The setting to use when loading. Setting should be collected from user with the ShowSettings method.</param>
        /// <returns>The loaded log file object.</returns>
        IParser Open(string filename, ParserSettings settings = null);

        /// <summary>
        /// Show file load settings dialog to user
        /// </summary>
        /// <param name="filename">The filename to open</param>
        /// <param name="settings">Key/value collection of settings. </param>
        /// <returns>True if file should be loaded. False if loading should be aborted.</returns>
        bool ShowSettingsDialog(string filename, ref ParserSettings settings);
    }

    /// <summary>
    /// Parser for a log format
    /// </summary>
    public interface IParser
    {
        /// <summary>
        /// List of signals provided by this parser
        /// </summary>
        SignalList Signals { get; }

        /// <summary>
        /// Read and return the provided signal
        /// </summary>
        /// <param name="signal">The signal to read</param>
        /// <returns>The sample data points of the signal</returns>
        Sample[] ReadSignal(Signal signal);

        /// <summary>
        /// The file origo time. 
        /// </summary>
        DateTime Origo { get; }

        /// <summary>
        /// The duration of the data in the file
        /// </summary>
        TimeSpan Length { get; }
    }

    public struct Sample
    {
        public DateTime Time;
        public double Value;
    }

    public struct Signal
    {
        public int UID;
        public string Name;
        public string FriendlyName;
        public string Unit;
        public string Description;
        public string Tag;
    }
}
