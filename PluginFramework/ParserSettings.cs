namespace PluginFramework
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Collection of settings to use when parsing the log file
    /// </summary>
    [DataContract]
    public class ParserSettings
    {
        [CollectionDataContract(ItemName = "Item", KeyName = "Key", ValueName = "Value")]
        private class Settings : Dictionary<string, object> { }

        [DataMember]
        private Settings settings = new Settings();

        public ParserSettings()
        {

        }

        /// <summary>
        /// Write or updates setting in collection
        /// </summary>
        /// <remarks>Overwrites setting if it already exists</remarks>
        /// <param name="key">The key to store the setting under.</param>
        /// <param name="value">The value to store</param>
        /// <returns>True is setting was overwritten. False otherwise.</returns>
        public bool Write(string key, object value)
        {
            if (settings.ContainsKey(key))
            {
                settings[key] = value;
                return true;
            }
            else
            {
                settings.Add(key, value);
                return false;
            }
        }

        /// <summary>
        /// Reads setting from collection
        /// </summary>
        /// <param name="key">The key of the value to read.</param>
        /// <returns>The read value.</returns>
        public object Read(string key)
        {
            return settings[key];
        }
    }
}
