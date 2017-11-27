using GalaSoft.MvvmLight.Ioc;
using PluginFramework;
using Sample_Crunch.ViewModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sample_Crunch
{
    /// <summary>
    /// Interaction logic for MarkersPane.xaml
    /// </summary>
    public partial class MarkersPane : UserControl, IPanelView
    {
        public MarkersPane()
        {
            InitializeComponent();
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var list = e.Source as ListBox;
            if (list == null) return;

            var mvm = list.SelectedItem as MarkerViewModel;
            if (mvm == null) return;
            var form = new MarkerForm(mvm);

            if (form.ShowDialog().Value)
            {

            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainViewModel model = SimpleIoc.Default.GetInstance<MainViewModel>();
            if (e.AddedItems.Count > 0)
            {
                model.SelectedMarker = e.AddedItems[0] as MarkerViewModel;
            }
            else
            {
                model.SelectedMarker = null;
            }
        }
    }
}
