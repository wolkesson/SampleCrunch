
namespace Sample_Crunch.ViewModel
{
    using GalaSoft.MvvmLight;
    using PluginFramework;
    using Sample_Crunch.Model;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows.Data;
    using System.Windows.Media;

    [DebuggerDisplay("Title = {Title}, {SyncOffset}")]
    public class FileViewModel : ViewModelBase, IFileViewModel
    {
        private readonly IFileModel fileData;
        private readonly ObservableCollection<ISignalViewModel> signalViewModels = new ObservableCollection<ISignalViewModel>();
        private readonly ObservableCollection<IMarkerViewModel> markerViewModels = new ObservableCollection<IMarkerViewModel>();
        private readonly IProjectViewModel project;

        public FileViewModel(IProjectViewModel parentProject, IFileModel fileData)
        {
            this.project = parentProject;
            this.fileData = fileData;

            try
            {
                // Try to use specified factory
                var parserFactory = PluginFactory.FindParser(fileData.DataParserType);

                // If not available use any other able to use open that format
                if (parserFactory == null)
                {
                    parserFactory = PluginFactory.FindLogFileParser(fileData.FileName);
                }

                if (parserFactory == null) throw new InvalidOperationException("No parser for " + fileData.FileName + " available.");

                this.LogFile = parserFactory.Open(fileData.FileName, fileData.Settings);
            }
            catch (IOException ioe)
            {
                throw new IOException("Can no open file", ioe);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                throw;
            }

            this.Icon = ResourceHelper.SetIconFromBitmap(Properties.Resources.folder);
            signalViewModels = new ObservableCollection<ISignalViewModel>();

            foreach (Signal s in LogFile.Signals)
            {
                SignalViewModel subitem = new SignalViewModel(this, s);
                this.Signals.Add(subitem);
            }

            foreach (IMarkerModel m in fileData.Markers)
            {
                MarkerViewModel markerItem = new MarkerViewModel(m, false);
                this.Markers.Add(markerItem);
            }

            markerViewModels.CollectionChanged += MarkerViewModels_CollectionChanged;
        }

        private void MarkerViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (IMarkerViewModel item in e.NewItems)
                    {
                        //item.
                    }
                    break;
            }
            throw new NotImplementedException();
        }

        public bool alphabeticalSorted = false;
        public bool AlphabeticalSorted
        {
            get { return this.alphabeticalSorted; }
            set
            {
                if (value)
                {
                    this.SignalsFiltered.SortDescriptions.Add(new SortDescription(nameof(SignalViewModel.Title), ListSortDirection.Ascending));
                }
                else
                {
                    this.SignalsFiltered.SortDescriptions.Clear();
                }
                this.SignalsFiltered.Refresh();
            }
        }

        public ImageSource Icon { get; set; }

        public string Title { get { return this.fileData.Title; } }

        public ObservableCollection<IMarkerViewModel> Markers { get { return markerViewModels; } }

        public ObservableCollection<ISignalViewModel> Signals { get { return signalViewModels; } }
        
        public IParser LogFile { get; set; }

        public IParserFactory ParserFactory { get; set; }

        public ICollectionView SignalsFiltered
        {
            get
            {
                var source = CollectionViewSource.GetDefaultView(Signals);
                source.Filter = p => project.Filter((ISignalViewModel)p);
                return source;
            }
        }

        public DateTime StartTime { get { return Origo + SyncOffset; } }
        public DateTime StopTime { get { return StartTime.Add(LogFile.Length); } }

        public DateTime Origo { get { return LogFile.Origo; } }
        public IProjectViewModel ParentProject { get { return project; } }
        public TimeSpan SyncOffset {
            get { return fileData.SyncOffset; }
            set {
                fileData.SyncOffset = value;
                RaisePropertyChanged(nameof(StartTime));
                RaisePropertyChanged(nameof(StopTime));
                RaisePropertyChanged(nameof(SyncOffset));
            }
        }

        public bool IsMaster
        {
            get { return project.MasterFile == this; }
            set
            {
                if (value)
                {
                    project.MasterFile = this;
                }
                else if (project.MasterFile == this)
                {
                    project.MasterFile = null;
                }

                RaisePropertyChanged(nameof(IsMaster));
            }
        }

        public IEnumerable<IMarkerViewModel> SyncMarkers
        {
            get
            {
                var combined = project.Markers.Concat(Markers);
                return combined;
            }
        }
        
        public IMarkerViewModel SyncMark
        {
            get
            {
                if (fileData.SyncMark == null)
                {
                    return SyncMarkers.DefaultIfEmpty(null).First();
                }
                else
                {
                    return project.GetMarkerViewModel(fileData.SyncMark);
                }
            }
            set
            {
                MarkerViewModel mvm = (MarkerViewModel)value;
                fileData.SyncMark = mvm?.markerModel;
                RaisePropertyChanged(nameof(SyncMark));
            }
        }

        public override string ToString()
        {
            return this.Title;
        }

        public void AddMarker(IMarkerModel model)
        {
            this.fileData.Markers.Add(model);
        }
    }
}
