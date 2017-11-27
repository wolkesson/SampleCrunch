using System;

namespace PluginFramework
{
    public interface IMarkerModel
    {
        bool Global { get; set; }
        DateTime Time { get; set; }
        string Title { get; set; }
    }
}