using System;
using System.Windows.Media;

namespace PluginFramework
{
    public interface ISignalViewModel
    {
        string Description { get; }
        bool Expanded { get; set; }
        SolidColorBrush Foreground { get; set; }
        ImageSource Icon { get; set; }
        string Name { get; }
        //DateTime Origo { get; set; }
        Signal Signal { get; }
        string Tag { get; }
        string Title { get; }
        string Unit { get; }
        IFileViewModel ParentFile { get; }

        Sample[] GetData();
        string GetPath();
    }
}