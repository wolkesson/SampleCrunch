namespace PluginFramework
{
    public interface IDialogServiceExt : GalaSoft.MvvmLight.Views.IDialogService
    {
        IRangeModel ShowRangeDialog(double from, double to);
        string ShowPromptDialog(string prompt, string defaultResponse);
    }
}
