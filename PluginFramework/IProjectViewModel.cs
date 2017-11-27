namespace PluginFramework
{
    using GalaSoft.MvvmLight.CommandWpf;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Input;
    public interface IProjectViewModel: INotifyPropertyChanged
    {
        object ActiveContent { get; set; }
        ICommand AddFileToProjectCommand { get; }
        RelayCommand<DateTime> AddMarkerCommand { get; }
        ObservableCollection<IPanelViewModel> Anchorables { get; }
        ObservableCollection<IPanelViewModel> Documents { get; }
        ObservableCollection<IFileViewModel> Files { get; }
        //MainViewModel Main { get; }
        ObservableCollection<IMarkerViewModel> Markers { get; }
        DateTime MaximumTime { get; }
        DateTime MinimumTime { get; }

        bool AlphabeticalSorted { get; set; }
        //ProjectData ProjectModel { get; }
        ICommand RemoveFromProjectCommand { get; }
        DateTime StartTime { get; set; }
        DateTime StopTime { get; set; }
        IFileViewModel MasterFile { get; set; }
        event EventHandler FilesRealigned;
        
        void AddLogFile(string filename);
        void AddLogFile(string filename, ILogFileParser parser);
        List<ISignalViewModel> FindSignalByTag(string tag);
        ISignalViewModel GetSignal(string path);
        IMarkerViewModel GetMarkerViewModel(IMarkerModel markerModel);
        bool Filter(ISignalViewModel p);
    }
}