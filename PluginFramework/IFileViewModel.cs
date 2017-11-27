
namespace PluginFramework
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Media;

    public interface IFileViewModel : INotifyPropertyChanged
    {
        string Title { get; }
        ImageSource Icon { get; set; }
        ObservableCollection<ISignalViewModel> Signals { get; }
        ObservableCollection<IMarkerViewModel> Markers { get; }
        ICollectionView SignalsFiltered { get; }
        bool AlphabeticalSorted { get; set; }
        DateTime StartTime { get; }
        DateTime StopTime { get; }

        DateTime Origo { get; }
        TimeSpan SyncOffset { get; set; }
        IMarkerViewModel SyncMark { get; set; }
        bool IsMaster { get; set; }

        void AddMarker(IMarkerModel model);
    }
}