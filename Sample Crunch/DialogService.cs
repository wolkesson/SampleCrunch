using PluginFramework;
using System;
using System.Threading.Tasks;

namespace Sample_Crunch
{
    internal class DialogService : GalaSoft.MvvmLight.Views.IDialogService, IDialogServiceExt
    {
        public Task ShowError(Exception error, string title, string buttonText, Action afterHideCallback)
        {
            return ShowMessageBox(error.ToString(), title);
        }

        public Task ShowError(string message, string title, string buttonText, Action afterHideCallback)
        {
            return ShowMessageBox(message, title);
        }

        public Task<bool> ShowMessage(string message, string title, string buttonConfirmText, string buttonCancelText, Action<bool> afterHideCallback)
        {
            return Task<bool>.Factory.StartNew(() =>
            {
                return System.Windows.MessageBox.Show(message, title,
                    System.Windows.MessageBoxButton.OKCancel,
                    System.Windows.MessageBoxImage.Question,
                    System.Windows.MessageBoxResult.Cancel) == System.Windows.MessageBoxResult.OK;
            });
        }

        public Task ShowMessage(string message, string title, string buttonText, Action afterHideCallback)
        {
            throw new NotImplementedException();
        }

        public Task ShowMessage(string message, string title)
        {
            return ShowMessageBox(message, title);
        }

        public Task ShowMessageBox(string message, string title)
        {
            return Task.Factory.StartNew(() => { System.Windows.MessageBox.Show(message, title); });
        }

        public void ShowWebPage(string title, string address)
        {
            WebWindow w = new WebWindow();
            w.Title = title;
            w.Address = address;
            w.Show();
        }

        public IRangeModel ShowRangeDialog(double from, double to)
        {
            RangeViewModel rangeVM = new RangeViewModel(from, to);
            rangeVM.Auto = double.IsNaN(from) || double.IsNaN(to);

            var dlg = new RangeForm(rangeVM);
            if (dlg.ShowDialog() == true)
            {
                return rangeVM;
            }
            else
            {
                return null;
            }
        }

        public string ShowPromptDialog(string prompt, string defaultResponse="")
        {
            PromptDialog dlg = new PromptDialog();
            dlg.PromptMessage = prompt;
            dlg.ResponseText = defaultResponse;
            if (dlg.ShowDialog() == true)
            {
                return dlg.ResponseText;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
