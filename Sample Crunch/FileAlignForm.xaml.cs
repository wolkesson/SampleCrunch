using Sample_Crunch.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace Sample_Crunch
{
    /// <summary>
    /// Interaction logic for MarkerForm.xaml
    /// </summary>
    public sealed partial class FileAlignForm : Window
    {
        private readonly ProjectViewModel viewModel;
        private bool blockReentrance = false;

        public FileAlignForm(ProjectViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            this.viewModel = viewModel;

            this.Loaded += FileAlignForm_Loaded;
            this.Closed += FileAlignForm_Closed;
        }

        private void FileAlignForm_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.AlignToMasterCommand.Execute(null); // Update first time
            foreach (var item in viewModel.Files)
            {
                item.PropertyChanged += File_PropertyChanged;
            }
        }

        private void FileAlignForm_Closed(object sender, EventArgs e)
        {
            foreach (var item in viewModel.Files)
            {
                item.PropertyChanged -= File_PropertyChanged;
            }
        }

        private void File_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!blockReentrance)
            {
                blockReentrance = true;
                viewModel.AlignToMasterCommand.Execute(null);
                blockReentrance = false;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Ugly hack to not automatically realign
            this.blockReentrance = true;

            foreach (var file in viewModel.Files)
            {
                file.SyncOffset = TimeSpan.Zero;
            }

            this.blockReentrance = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            // Command binding will also run!
        }
    }
}
