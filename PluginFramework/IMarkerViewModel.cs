namespace PluginFramework
{
    using GalaSoft.MvvmLight;
    using System;
    using System.ComponentModel;

    public interface IMarkerViewModel: INotifyPropertyChanged, ICleanup
    {
        bool Global { get; set; }
        DateTime Time { get; set; }
        string Title { get; set; }
    }
}