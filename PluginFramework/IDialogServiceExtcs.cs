namespace PluginFramework
{
    public interface IDialogServiceExt : GalaSoft.MvvmLight.Views.IDialogService
    {
        void ShowWebPage(string title, string address);
        IRangeModel ShowRangeDialog(double from, double to);
        string ShowPromptDialog(string prompt, string defaultResponse);
    }
}
