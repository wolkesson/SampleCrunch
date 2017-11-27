namespace Sample_Crunch.ViewModel
{
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Ioc;
    using PluginFramework;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class MarkerViewModel : ViewModelBase, IMarkerViewModel
    {
        internal readonly IMarkerModel markerModel;

        public MainViewModel Main
        {
            get
            {
                return SimpleIoc.Default.GetInstance<MainViewModel>();
            }
        }

        public MarkerViewModel(IMarkerModel model, bool global)
        {
            this.markerModel = model;
            this.global = global;

            if (!global)
            {
                // Return file view model according to data or first file
                var m = from f in Main.Project.Files where f.Markers.Contains(this) select f;
                var match = m.FirstOrDefault();
                if (match == null && Main.Project.Files.Count > 0)
                {
                    file = Main.Project.Files[0];
                }
                else
                {
                    file = match;
                }
            }

            //if (this.Global)
            //{
            //    markerModel.ReferenceFile = null;
            //}
            //else
            //{
            //    markerModel.ReferenceFile = ((FileViewModel)value).FileData;
            //}
        }
        
        public string Title
        {
            get { return markerModel.Title; }
            set
            {
                markerModel.Title = value;
                RaisePropertyChanged<string>(nameof(Title));
            }
        }
        
        public DateTime Time
        {
            get { return markerModel.Time; }
            set
            {
                markerModel.Time = value;
                RaisePropertyChanged<TimeSpan>(nameof(Time));
            }
        }
        private bool global = false;
        public bool Global
        {
            get { return this.global; }
            set
            {
                this.global = value;
                RaisePropertyChanged<bool>(nameof(Global));
                RaisePropertyChanged<bool>(nameof(FileScope));
            }
        }

        public bool FileScope
        {
            get { return !Global; }
            set { Global = !value; }
        }

        public bool HasFiles
        {
            get { return References.Count() > 0; }
        }

        public IEnumerable<IFileViewModel> References
        {
            get
            {
                return Main.Project.Files;
              }
        }

        private IFileViewModel file;
        public IFileViewModel File
        {
            get
            {
                return file;
            }
            set
            {
                this.file = value;
                RaisePropertyChanged<IFileViewModel>(nameof(File));
            }
        }
    }
}
