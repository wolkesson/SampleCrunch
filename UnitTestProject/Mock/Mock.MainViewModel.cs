using Sample_Crunch.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginFramework;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace UnitTestProject.Mock
{
    class MockMainViewModel : IMainViewModel
    {
        public IDialogServiceExt DialogService
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ICommand NewPluginCommand
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ICommand NewProjectCommand
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ICommand OpenProjectCommand
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ProjectViewModel Project
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public ICommand SaveProjectCommand
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public FileViewModel SelectedFile
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public MarkerViewModel SelectedMarker
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string Title
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string Version
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ObservableCollection<IPanelFactory> Windows
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
