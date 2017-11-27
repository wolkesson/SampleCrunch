using System.Windows;

namespace Sample_Crunch
{
    /// <summary>
    /// Interaction logic for PromptDialog.xaml
    /// </summary>
    public partial class PromptDialog : Window
    {
        public PromptDialog()
        {
            InitializeComponent();
        }

        public string ResponseText
        {
            get { return (string)GetValue(ResponseTextProperty); }
            set { SetValue(ResponseTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ResponseText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ResponseTextProperty =
            DependencyProperty.Register("ResponseText", typeof(string), typeof(PromptDialog), new PropertyMetadata(""));

        public string PromptMessage
        {
            get { return (string)GetValue(PromptMessageProperty); }
            set { SetValue(PromptMessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PromptMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PromptMessageProperty =
            DependencyProperty.Register("PromptMessage", typeof(string), typeof(PromptDialog), new PropertyMetadata("Enter text:"));
        
        private void btnOK_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
