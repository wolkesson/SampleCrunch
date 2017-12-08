namespace Sample_Crunch
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for PluginManager.xaml
    /// </summary>
    public partial class PluginManager : Window
    {
        public PluginManager()
        {
            InitializeComponent();
            this.DataContext = new ViewModel.PluginManagerViewModel();
        }
    }
}
