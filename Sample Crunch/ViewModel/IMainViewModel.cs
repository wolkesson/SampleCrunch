using System.Collections.ObjectModel;
using System.Windows.Input;
using PluginFramework;

namespace Sample_Crunch.ViewModel
{
    public interface IMainViewModel
    {
        IDialogServiceExt DialogService { get; }
        ICommand NewPluginCommand { get; }
        ICommand NewProjectCommand { get; }
        ICommand OpenProjectCommand { get; }
        ProjectViewModel Project { get; set; }
        ICommand SaveProjectCommand { get; }
        FileViewModel SelectedFile { get; set; }
        MarkerViewModel SelectedMarker { get; set; }
        string Title { get; set; }
        string Version { get; }
        ObservableCollection<IPanelFactory> Windows { get; }
    }
}