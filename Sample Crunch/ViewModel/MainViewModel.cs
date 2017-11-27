using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using PluginFramework;
using System;
using System.Collections.ObjectModel;
using System.Deployment.Application;
using System.Windows.Input;

namespace Sample_Crunch.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public sealed class MainViewModel : ViewModelBase, IMainViewModel
    {
        public string Title
        {
            get;
            set;
        }

        public string Version
        {
            get
            {
                string version = null;
                try
                {
                    //// get deployment version
                    version = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
                }
                catch (InvalidDeploymentException)
                {
                    //// you cannot read publish version when app isn't installed 
                    //// (e.g. during debug)
                    version = "not installed";
                }

                return version;
            }
        }
        
        private ProjectViewModel project;

        public ProjectViewModel Project
        {
            get { return project; }
            set
            {
                this.project = value;
                this.RaisePropertyChanged("Project");
            }
        }

        public IDialogServiceExt DialogService
        {
            get
            {
                return SimpleIoc.Default.GetInstance<IDialogServiceExt>();
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                this.Title = "Sample Crunch Design";
                this.Project = new ProjectViewModel(new ProjectData());
                // Code runs in Blend --> create design time data.
            }
            else
            {
                // Code runs "for real"
                try
                {
                    string exename = System.Reflection.Assembly.GetEntryAssembly().Location;
                    this.Title = System.IO.Path.GetFileNameWithoutExtension(exename);
                }
                catch (InvalidDeploymentException)
                {
                    this.Title = "Sample Crunch";
                }
            }
        }

        private ICommand showPluginManagerFormCommand;
        public ICommand ShowPluginManagerFormCommand
        {
            get
            {
                return showPluginManagerFormCommand ?? (showPluginManagerFormCommand = new RelayCommand(Execute_ShowPluginManagerFormCommand));
            }
        }

        private void Execute_ShowPluginManagerFormCommand()
        {
            var form = new PluginManager();
            form.Show();
        }

        #region Project Commands
        private ICommand newProjectCommand;
        public ICommand NewProjectCommand
        {
            get
            {
                return newProjectCommand ?? (newProjectCommand = new RelayCommand(Execute_NewProjectCommand));
            }
        }

        private void Execute_NewProjectCommand()
        {
            var newProject = new ProjectViewModel(new ProjectData());
            newProject.ProjectModel.Name = "New Project";
            
            IPanelModel docProject = new Factory.ProjectPanelFactory().CreateModel();
            newProject.ProjectModel.Anchorables.Add(docProject);

            IPanelModel docMarkers = new Factory.MarkerPanelFactory().CreateModel();
            newProject.ProjectModel.Anchorables.Add(docMarkers);

            newProject.ProjectModel.Layout = $"<?xml version = '1.0' encoding = 'utf-16' ?>"+
            "<LayoutRoot xmlns:xsi = 'http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd = 'http://www.w3.org/2001/XMLSchema'> "+
                "<RootPanel Orientation = 'Horizontal'> "+
                    "<LayoutPanel Orientation = 'Horizontal'> "+
                        "<LayoutDocumentPaneGroup Orientation = 'Horizontal'> "+
                            "<LayoutDocumentPane /> "+
                        "</LayoutDocumentPaneGroup> "+
                    "</LayoutPanel> "+
                    "<LayoutAnchorablePane DockWidth = '200'> "+
                       // "<LayoutAnchorable AutoHideMinWidth = '100' AutoHideMinHeight = '100' Title = 'Project' ContentId = '{docProj.ContentID.ToString()}' IsSelected = 'True' /> " +
                       // "<LayoutAnchorable AutoHideMinWidth = '100' AutoHideMinHeight = '100' Title = 'Markers' ContentId = '{doc.ContentID.ToString()}' /> "+
                    "</LayoutAnchorablePane> "+
                "</RootPanel> "+
                "<TopSide /> "+
                "<RightSide /> "+
                "<LeftSide /> "+
                "<BottomSide /> "+
                "<FloatingWindows /> "+
                "<Hidden /> "+
            "</LayoutRoot>";

            this.Project = newProject;
        }

        private ICommand openProjectCommand;
        public ICommand OpenProjectCommand
        {
            get
            {
                return openProjectCommand ?? (openProjectCommand = new RelayCommand(Execute_OpenProjectCommand));
            }
        }

        private void Execute_OpenProjectCommand()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.CheckFileExists = true;

            // Create filte extension filter string
            dlg.DefaultExt = ".scp"; // Default file extension

            dlg.Filter = "Sample Crunch Project | *.scp"; // Filter files by extension

            // Show open file dialog box 
            Nullable<bool> result = dlg.ShowDialog();

            if (string.IsNullOrEmpty(dlg.FileName)) return;

            try
            {
                Project.ProjectModel.Anchorables.Clear();
                Project = new ProjectViewModel(ProjectData.Open(dlg.FileName));
                //Project.RaisePropertyChanged(nameof(Project.Files));
                //Project.RaisePropertyChanged(nameof(Project.Markers));
            }
            catch (InvalidOperationException ex)
            {
                DialogService.ShowError(ex, "Could not open project", null, null).Wait();
            }
        }

        private ICommand saveProjectCommand;
        public ICommand SaveProjectCommand
        {
            get
            {
                return saveProjectCommand ?? (saveProjectCommand = new RelayCommand(Execute_SaveProjectCommand));
            }
        }

        private void Execute_SaveProjectCommand()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.AddExtension = true;
            dlg.CheckPathExists = true;

            // Create filte extension filter string
            dlg.DefaultExt = ".scp"; // Default file extension

            dlg.Filter = "Sample Crunch Project | *.scp"; // Filter files by extension

            // Show open file dialog box 
            Nullable<bool> result = dlg.ShowDialog();

            this.Project.ProjectModel.Save(dlg.FileName);
        }
        #endregion

        FileViewModel selectedFile;
        public FileViewModel SelectedFile
        {
            get
            {
                return selectedFile;
            }
            set
            {
                selectedFile = value;
                RaisePropertyChanged<FileViewModel>(nameof(SelectedFile));
            }
        }

        MarkerViewModel selectedMarker;
        public MarkerViewModel SelectedMarker
        {
            get
            {
                return selectedMarker;
            }
            set
            {
                selectedMarker = value;
                RaisePropertyChanged<MarkerViewModel>(nameof(SelectedMarker));
            }
        }

        private readonly ObservableCollection<IPanelFactory> _windows = new ObservableCollection<IPanelFactory>();

        public ObservableCollection<IPanelFactory> Windows
        {
            get { return _windows; }
        }

        private ICommand newPluginCommand;
        public ICommand NewPluginCommand
        {
            get
            {
                return newPluginCommand ?? (newPluginCommand = new RelayCommand<IPanelFactory>(Execute_NewPluginCommand));
            }
        }

        private void Execute_NewPluginCommand(IPanelFactory factory)
        {
            IPanelModel model =  factory.CreateModel();
            if (string.IsNullOrEmpty( model.Title) || string.IsNullOrEmpty(model.FactoryReference))
            {
                throw new InvalidOperationException("Invalid plugin plugin title or factory reference. Contact plugin author!");
            }
            this.Project.ProjectModel.Documents.Add(model);
        }
    }
}