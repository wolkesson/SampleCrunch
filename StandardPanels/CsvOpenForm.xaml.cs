namespace Sample_Crunch
{
    using GalaSoft.MvvmLight;
    using System;
    using System.Windows;

    /// <summary>
    /// Interaction logic for MarkerForm.xaml
    /// </summary>
    public partial class CsvOpenForm : Window
    {
        public CsvOpenForm(CsvViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel; //Do this if you need access to the VM from inside your View. Or you could just use this.Datacontext to access the VM.
            this.DataContext = viewModel;
        }

        private readonly CsvViewModel viewModel;

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }

    public class CsvViewModel : ViewModelBase
    {
        private readonly string titleLine;
        public enum DelimiterType
        {
            Semicolon,
            Comma,
            Tab
        }

        public CsvViewModel(string titleLine)
        {
            this.titleLine = titleLine;
            this.SelectedDelimiter = DelimiterType.Semicolon;
            RaisePropertyChanged(nameof(SelectedDelimiter));
        }

        public static char GetDelimiter(DelimiterType delimiter)
        {
            switch (delimiter)
            {
                default:
                case DelimiterType.Semicolon:
                    return ';';
                case DelimiterType.Comma:
                    return ',';
                case DelimiterType.Tab:
                    return '\t';
            }
        }

        private DelimiterType delimiter = DelimiterType.Semicolon;
        public DelimiterType SelectedDelimiter
        {
            get
            {
                return this.delimiter;
            }
            set
            {
                this.delimiter = value;
                char[] splitchar = new char[1];
                splitchar[0] = GetDelimiter(this.delimiter);

                TimeVectors = titleLine.Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
                this.SelectedTimeVector = TimeVectors.Length > 0 ? 0: -1;
                
                RaisePropertyChanged(nameof(SelectedTimeVector));
                RaisePropertyChanged(nameof(TimeVectors));
                RaisePropertyChanged(nameof(CanClose));
            }
        }

        public string[] Delimiters { get { return Enum.GetNames(typeof(DelimiterType)); } }

        public string[] TimeVectors { get; private set; }
        private int selectedTimeVector = -1;
        public int SelectedTimeVector
        {
            get
            {
                return this.selectedTimeVector;
            }
            set
            {
                this.selectedTimeVector = value;

                RaisePropertyChanged(nameof(SelectedTimeVector));
                RaisePropertyChanged(nameof(CanClose));
            }
        }

        public bool CanClose { get
            {
                return SelectedTimeVector != -1;
            }
        }
    }
}
