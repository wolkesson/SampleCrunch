using PluginFramework;
using System.ComponentModel;
using System.Windows;

namespace Sample_Crunch
{
    /// <summary>
    /// Interaction logic for MarkerForm.xaml
    /// </summary>
    public partial class RangeForm : Window
    {
        public RangeForm(RangeViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel; //Do this if you need access to the VM from inside your View. Or you could just use this.Datacontext to access the VM.
            this.DataContext = viewModel;
        }

        private readonly RangeViewModel viewModel;

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }

    public class RangeViewModel : INotifyPropertyChanged, IRangeModel
    {
        private bool auto = false;

        public RangeViewModel(double from, double to)
        {
            this.From = from;
            this.To = to;
        }

        public double From { get; set; }
        public double To { get; set; }

        public bool Auto
        {
            get { return this.auto; }
            set
            {
                this.auto = value;
                this.TriggerPropertyChanged("Auto");
                this.TriggerPropertyChanged("IsManualRange");
            }
        }

        public System.Windows.Data.IValueConverter ValueConverter { get; set; }

        public bool IsManualRange { get { return !this.Auto; } }

        public event PropertyChangedEventHandler PropertyChanged;

        private void TriggerPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
