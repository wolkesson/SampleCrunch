using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.ApplicationInsights;
using PluginFramework;
using Sample_Crunch.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace Sample_Crunch.ViewModel
{
    public sealed class ProjectViewModel : ViewModelBase, IProjectViewModel
    {
        public ProjectViewModel(ProjectData projectModel)
        {
            this.ProjectModel = projectModel;


            foreach (PaneModel document in this.ProjectModel.Anchorables)
            {
                InternalAddAnchorable(document);
            }

            foreach (PaneModel document in this.ProjectModel.Documents)
            {
                InternalAddDocument(document);
            }

            foreach (FileModel file in this.ProjectModel.Files)
            {
                internalAddFile(file);
            }

            foreach (IMarkerModel marker in this.ProjectModel.Markers)
            {
                InternalAddMarker(marker);
            }

            this.ProjectModel.Documents.CollectionChanged += Documents_CollectionChanged;
            this.ProjectModel.Files.CollectionChanged += Files_CollectionChanged;
            this.ProjectModel.Markers.CollectionChanged += Markers_CollectionChanged;
            this.ProjectModel.Anchorables.CollectionChanged += Anchorables_CollectionChanged;
        }

        public List<ISignalViewModel> FindSignalByTag(string tag)
        {
            List<ISignalViewModel> signals = new List<ISignalViewModel>();

            foreach (var item in Files)
            {
                signals.AddRange(from s in item.Signals where s.Tag == tag select s);
            }

            return signals;
        }

        public IMainViewModel Main
        {
            get
            {
                return SimpleIoc.Default.GetInstance<IMainViewModel>();
            }
        }

        public TelemetryClient Telemetry
        {
            get
            {
                return SimpleIoc.Default.GetInstance<TelemetryClient>();
            }
        }

        internal IDialogServiceExt DialogService
        {
            get
            {
                return SimpleIoc.Default.GetInstance<IDialogServiceExt>();
            }
        }


        public ProjectData ProjectModel { get; private set; }

        private readonly ObservableCollection<IPanelViewModel> documents = new ObservableCollection<IPanelViewModel>();
        public ObservableCollection<IPanelViewModel> Documents { get { return documents; } }
        
        private readonly ObservableCollection<IPanelViewModel> anchorables = new ObservableCollection<IPanelViewModel>();
        public ObservableCollection<IPanelViewModel> Anchorables { get { return anchorables; } }

        private readonly ObservableCollection<IFileViewModel> files = new ObservableCollection<IFileViewModel>();
        public ObservableCollection<IFileViewModel> Files { get { return files; } }

        private readonly ObservableCollection<IMarkerViewModel> markers = new ObservableCollection<IMarkerViewModel>();
        public ObservableCollection<IMarkerViewModel> Markers { get { return markers; } }

        private object activeContent;
        public object ActiveContent
        {
            get { return activeContent; }
            set { activeContent = value; }
        }
        
        private ICommand removeFromProjectCommand;
        public ICommand RemoveFromProjectCommand
        {
            get
            {
                return removeFromProjectCommand ?? (removeFromProjectCommand = new RelayCommand<FileViewModel>(Execute_RemoveFromProjectCommand, p => p != null));
            }
        }

        private void Execute_RemoveFromProjectCommand(FileViewModel fileTopic)
        {
            if (fileTopic == null) return;
            string str = string.Format(Properties.Resources.removeFileFromProject, fileTopic.Title);
            var answer = DialogService.ShowMessage(str, "Warning", "Remove", "Cancel", null).Result;
            if (answer)
            {
                var fileModel = ProjectModel.Files.First(f => f.Title == fileTopic.Title); // ViewModel title is directly bound to Models Title so this is ok!
                ProjectModel.Files.Remove(fileModel);
            }
        }

        private ICommand addFileToProjectCommand;
        public ICommand AddFileToProjectCommand
        {
            get
            {
                return addFileToProjectCommand ?? (addFileToProjectCommand = new RelayCommand(Execute_AddFileToProjectCommand));
            }
        }

        private ICommand showFileAlignFormCommand;
        public ICommand ShowFileAlignFormCommand
        {
            get
            {
                return showFileAlignFormCommand ?? (showFileAlignFormCommand = new RelayCommand(Execute_ShowFileAlignFormCommand,()=>Files.Count>1));
            }
        }

        private void Execute_ShowFileAlignFormCommand()
        {
            var form = new FileAlignForm(this);

            // Create backup if user press cancel
            var backup = Files.Select(file => new
            {
                isMaster = file.IsMaster,
                offset = file.SyncOffset,
                mark = file.SyncMark
            }).ToList();

            if (form.ShowDialog().Value)
            {
                // OK was pressed. Does nothing since changes are made instantly
            }
            else
            {
                // Cancel was pressed. Revert to backup!
                for (int i = 0; i < backup.Count; i++)
                {
                    Files[i].IsMaster = backup[i].isMaster;
                    Files[i].SyncMark = backup[i].mark;
                    Files[i].SyncOffset = backup[i].offset;
                }
            }
        }

        private ICommand alignToMasterCommand;
        public ICommand AlignToMasterCommand
        {
            get
            {
                return alignToMasterCommand ?? (alignToMasterCommand = new RelayCommand(Execute_AlignToMasterCommand, () => MasterFile != null));
            }
        }

        private void Execute_AlignToMasterCommand()
        {
            var masterSync = MasterFile.SyncMark == null ? MasterFile.Origo : MasterFile.SyncMark.Time;
            foreach (var file in Files)
            {
                if (file.IsMaster)
                {
                    file.SyncOffset = TimeSpan.Zero;
                }
                else
                {
                    var d = file.SyncMark == null ? file.Origo : file.SyncMark.Time;
                    file.SyncOffset = masterSync - d;
                }
            }
        }

        private string searchText = string.Empty;
        public string SearchText
        {
            get { return this.searchText; }
            set
            {
                if (this.searchText != value)
                {
                    this.searchText = value;
                    foreach (var file in this.files)
                    {
                        file.SignalsFiltered.Refresh();
                    }
                    base.RaisePropertyChanged<string>(nameof(SearchText));
                }
            }
        }

        public bool Filter(ISignalViewModel signal)
        {
            return string.IsNullOrEmpty(SearchText) ? true : 
                (signal.Title.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 || signal.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private bool isAlphabeticalSorted = false;
        public bool AlphabeticalSorted
        {
            get { return this.isAlphabeticalSorted; }
            set
            {
                foreach (var file in Files)
                {
                    file.AlphabeticalSorted = value;
                }
                this.isAlphabeticalSorted = value;
            }
        }


        private DateTime startTime;
        public DateTime StartTime
        {
            get
            {
                return startTime;
            }
            set
            {
                if (value > stopTime) return;
                this.startTime = value;
                RaisePropertyChanged(nameof(StartTime));
            }
        }

        private DateTime stopTime;
        public DateTime StopTime
        {
            get
            {
                return stopTime;
            }
            set
            {
                if (value < startTime) return;
                this.stopTime = value;
                RaisePropertyChanged(nameof(StopTime));
            }
        }

        public DateTime MinimumTime { get; private set; }
        public DateTime MaximumTime { get; private set; }

        private IFileViewModel masterFile;
        public IFileViewModel MasterFile
        {
            get { return this.masterFile; }
            set
            {
                this.masterFile = value;
                RaisePropertyChanged(nameof(MasterFile));
            }
        }

        public event EventHandler FilesRealigned;

        private void Execute_AddFileToProjectCommand()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.Multiselect = true;

            // Create filter extension filter string
            dlg.DefaultExt = ".dat"; // Default file extension
            dlg.Filter = PluginFactory.FileFilterString; // Filter files by extension

            // Show open file dialog box 
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                try
                {
                    // All files
                    if (dlg.FilterIndex == PluginFactory.Parsers.Count+1)
                    {
                        foreach (var filename in dlg.FileNames)
                        {
                            this.AddLogFile(filename);
                        }
                    }
                    else
                    {
                        ILogFileParser lfp = PluginFactory.CreateLogFileParser(PluginFactory.Parsers[dlg.FilterIndex-1]);
                        foreach (var filename in dlg.FileNames)
                        {
                            this.AddLogFile(filename, lfp);
                        }
                    }

                }
                catch (Exception ex)
                {
                    DialogService.ShowError(ex, "Could not open file", null, null).Wait();
                    return;
                }
            }
        }

        public void AddLogFile(string filename, ILogFileParser parser)
        {
            FileModel file = new FileModel();
            file.FileName = filename;
            file.Title = System.IO.Path.GetFileNameWithoutExtension(file.FileName);

            int count = Files.Count(f => f.Title == file.Title);
            if (count>0) file.Title += $" ({count+1})";

            try
            {
                ParserSettings settings = file.Settings;
                // Show settings dialog and exit if canceled
                if (!parser.ShowSettingsDialog(filename, ref settings)) return;
                file.Settings = settings;
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

            file.DataParserType = parser.GetType().FullName;
            this.ProjectModel.Files.Add(file);
            this.Telemetry.TrackEvent("Add Logfile", new Dictionary<string, string> { { "Parser", parser.ToString() }, { "Filename", file.Title } });
        }

        public void AddLogFile(string filename)
        {
            ILogFileParser lfp = PluginFactory.FindLogFileParser(filename);
            if (lfp != null)
            {
                AddLogFile(filename, lfp);
            }
            else
            {
                throw new FileLoadException("No available log file parser can open the selected file.", filename);
            }
        }

        void Documents_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (IPanelModel document in e.NewItems)
                    {
                        InternalAddDocument(document);
                    }
                    break;
            }

            RaisePropertyChanged("Documents");
        }

        void Anchorables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (PaneModel pane in e.NewItems)
                    {
                        InternalAddAnchorable(pane);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (PaneModel pane in e.OldItems)
                    {
                        //var f = this.anchorables.First(p=>p.)
                        //this.anchorables.re.Remove(pane);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    this.Anchorables.Clear();
                    break;
            }

            RaisePropertyChanged("Anchorables");
        }

        private void InternalAddAnchorable(PaneModel pane)
        {
            IPanelFactory factory = PluginFactory.FindPanelFactory(pane);
            IPanelViewModel vm = factory.CreateViewModel(this, pane);
            this.Anchorables.Add(vm);
        }

        private void InternalAddDocument(IPanelModel document)
        {            
            IPanelFactory factory = PluginFactory.FindPanelFactory(document);
            IPanelViewModel vm = factory.CreateViewModel(this, document);

            this.Documents.Add(vm);
        }

        private void InternalAddMarker(IMarkerModel model)
        {
            IMarkerViewModel vm = new MarkerViewModel(model, true);
            this.Markers.Add(vm);
        }

        void Files_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (FileModel file in e.NewItems)
                    {
                        internalAddFile(file);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    this.files.RemoveAt(e.OldStartingIndex);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    break;
            }

            RaisePropertyChanged("Files");
        }

        private void internalAddFile(FileModel file)
        {
            var fileViewModel = new FileViewModel(this,file);
            fileViewModel.PropertyChanged += FileViewModel_PropertyChanged;
            Files.Add(fileViewModel);

            if (files.Count == 1)
            {
                MinimumTime = fileViewModel.StartTime;
                MaximumTime = fileViewModel.StopTime;
                StartTime = MinimumTime;
                StopTime = MaximumTime;
                fileViewModel.IsMaster = true;
            }
            else
            {
                MinimumTime = new DateTime(Math.Min(MinimumTime.Ticks, fileViewModel.StartTime.Ticks));
                MaximumTime = new DateTime(Math.Max(MaximumTime.Ticks, fileViewModel.StopTime.Ticks));
                StartTime = new DateTime(Math.Max(MinimumTime.Ticks, fileViewModel.StartTime.Ticks));
                StopTime = new DateTime(Math.Min(MaximumTime.Ticks, fileViewModel.StopTime.Ticks));
            }


            RaisePropertyChanged(nameof(MinimumTime));
            RaisePropertyChanged(nameof(MaximumTime));
            RaisePropertyChanged(nameof(StartTime));
            RaisePropertyChanged(nameof(StopTime));
        }

        private void FileViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FileViewModel.SyncOffset))
            {
                FilesRealigned?.Invoke(this, EventArgs.Empty);
            }
        }

        void Markers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (MarkerModel model in e.NewItems)
                    {
                        InternalAddMarker(model);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    this.markers.RemoveAt(e.OldStartingIndex);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    break;
            }
            
            RaisePropertyChanged("Markers");
        }
        interface IMarkerData
        {
            TimeSpan Time { get; set; }
            IFileViewModel File { get; set; }
        }
        private RelayCommand<DateTime> _AddMarkerCommand;
        public RelayCommand<DateTime> AddMarkerCommand
        {
            get
            {
                return _AddMarkerCommand ?? (_AddMarkerCommand = new RelayCommand<DateTime>(Execute_AddMarkerCommand, (dt) => { return true; }));
            }
        }

        private void Execute_AddMarkerCommand(DateTime time)
        {
            var model = new Model.MarkerModel() { Title = "New Marker" , Time = time };
            var mvm = new MarkerViewModel(model, true);
            var form = new MarkerForm(mvm);

            if (form.ShowDialog().Value)
            {
                if (mvm.Global)
                {
                    this.ProjectModel.Markers.Add(model);
                }
                else
                {
                    mvm.File.AddMarker(model);
                }
            }
        }

        private ICommand _RemoveMarkerCommand;
        public ICommand RemoveMarkerCommand
        {
            get
            {
                return _RemoveMarkerCommand ?? (_RemoveMarkerCommand = new RelayCommand<IMarkerViewModel>(
                    Execute_RemoveMarkerCommand, 
                    (marker) => { if (marker != null) { Console.Write(marker.Title); } return marker!=null; }
                    ));
            }
        }

        private void Execute_RemoveMarkerCommand(IMarkerViewModel marker)
        {
            MarkerViewModel vm = marker as MarkerViewModel;
            this.ProjectModel.Markers.Remove(vm?.markerModel);
        }

        public ISignalViewModel GetSignal(string path)
        {
            var parts = path.Split('\\');
            var file = from f in this.Files where f.Title == parts[0] select f.Signals;
            if (!file.Any()) return null;
            var signal = from s in file.First() where s.Signal.Name == parts[1] select s;
            return signal.First();
        }

        public IMarkerViewModel GetMarkerViewModel(IMarkerModel markerModel)
        {
            if (markerModel == null) return null;
            var markerVM = from m in Markers where ((MarkerViewModel) m)?.markerModel == markerModel select m;
            return markerVM.First();
        }

        //public IFileViewModel GetFileViewModel(IFileModel fileModel)
        //{
        //    if (fileModel == null) return null;
        //    var fileVM = from m in Files where m.FileData == fileModel select m;
        //    return fileVM.First();
        //}
    }
}