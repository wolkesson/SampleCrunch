using GalaSoft.MvvmLight;
using PluginFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Sample_Crunch.ViewModel
{
    public class SignalViewModel : ViewModelBase, ISignalViewModel
    {
        Signal signal;
        SolidColorBrush foreground = Brushes.Black;
        IParser logFile;

        public SignalViewModel(FileViewModel parentFileModel, Signal signal)
        {
            this.signal = signal;
            this.ParentFile = parentFileModel;
            logFile = parentFileModel.LogFile;
            //this.Origo = parentFileTopic.FileData.LogFile.Origo;

            this.Foreground = Brushes.Black;
            this.Icon = ResourceHelper.SetIconFromBitmap(Properties.Resources.signal);
        }


        public string Title { get { return (string.IsNullOrEmpty(signal.FriendlyName) ? signal.Name : signal.FriendlyName); } }

        public string Name { get { return signal.Name; } }

        public bool Expanded { get; set; }

        public ImageSource Icon { get; set; }

        public SolidColorBrush Foreground
        {
            get { return foreground; }
            set
            {
                this.foreground = value;
            }
        }

        public IFileViewModel ParentFile { get; private set; }
        
        //public DateTime Origo { get; set; }

        public string Unit { get { return signal.Unit; } }

        public string Description { get { return signal.Description; } }

        public string Tag { get { return signal.Tag; } }

        public Sample[] GetData()
        {
            var samples = this.logFile.ReadSignal(this.signal);
            if (ParentFile.SyncOffset!=TimeSpan.Zero)
            {
                for (int i = 0; i < samples.LongLength; i++)
                {
                    samples[i].Time += ParentFile.SyncOffset;
                }
            }
            return samples;
        }

        public Signal Signal { get { return signal; } }

        public string GetPath()
        {
            return this.ParentFile.Title + '\\' + Signal.Name;
        }
    }
}
