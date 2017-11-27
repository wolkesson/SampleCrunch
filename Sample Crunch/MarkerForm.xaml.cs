namespace Sample_Crunch
{
    using Sample_Crunch.ViewModel;
    using System.Windows;

    /// <summary>
    /// Interaction logic for MarkerForm.xaml
    /// </summary>
    public partial class MarkerForm : Window
    {
        public MarkerForm(MarkerViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel; //Do this if you need access to the VM from inside your View. Or you could just use this.Datacontext to access the VM.
            this.DataContext = viewModel;
        }

        private readonly MarkerViewModel viewModel;

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
