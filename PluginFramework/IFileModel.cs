using System;
using System.Collections.ObjectModel;

namespace PluginFramework
{
    public interface IFileModel
    {
        string DataParserType { get; set; }
        string FileName { get; set; }
        ObservableCollection<IMarkerModel> Markers { get; }
        IMarkerModel SyncMark { get; set; }
        TimeSpan SyncOffset { get; set; }
        string Title { get; set; }
        ParserSettings Settings { get; }
    }
}