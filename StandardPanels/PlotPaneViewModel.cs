using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using PluginFramework;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace Sample_Crunch.StandardPanels.ViewModel
{
    public abstract class PlotPaneViewModel : PanelViewModel<TimePlotModel>
    {
        private TimePlotModel paneModel;

        protected ScreenPoint selectedPosition;
        public ScreenPoint SelectedPosition
        {
            get
            {
                return selectedPosition;
            }
            set
            {
                selectedPosition = value;
                RaisePropertyChanged<IFileViewModel>("SelectedSeries");
            }
        }

        protected PlotModel plotModel;
        public PlotModel PlotModel
        {
            get { return this.plotModel; }
            set { this.plotModel = value; RaisePropertyChanged("PlotModel"); }
        }

        public PlotPaneViewModel(IProjectViewModel project, IPanelFactory factory, TimePlotModel paneModel)
            : base(factory, paneModel)
        {
            // The project in main.Project and the project actually loading the panel is not the same here when loading project from file. 
            this.projectVM = project;
            this.paneModel = paneModel;
            PlotModel = new PlotModel();
            SetUpModel();

            this.PlotModel.Title = paneModel.Title;

            project.Files.CollectionChanged += Files_CollectionChanged;
            project.Markers.CollectionChanged += Markers_CollectionChanged;
        }

        protected virtual void Files_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            
        }

        //Func<double, bool> FilterByTime()
        //{
        //    return 
        //}


        void Markers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (IMarkerViewModel item in e.NewItems)
                    {
                        AddAnnotation(item);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (var model in e.OldItems)
                    {
                        var annotationVm = (from annotation in this.PlotModel.Annotations where annotation.Tag == model select annotation).First();
                        PlotModel.Annotations.Remove(annotationVm);
                    }
                    this.PlotModel.InvalidatePlot(false);
                    break;
                default:
                    break;
            }
        }

        public virtual void AddAnnotation(IMarkerViewModel mvm)
        {
            mvm.PropertyChanged += (a, b) =>
            {
                foreach (LineAnnotation item in this.PlotModel.Annotations)
                {
                    if (item.Tag == a)
                    {
                        item.X = OxyPlot.Axes.DateTimeAxis.ToDouble(mvm.Time);
                        item.Text = mvm.Title;
                        this.PlotModel.InvalidatePlot(false);
                    }
                }
            };

            var vline = new LineAnnotation();
            vline.Type = LineAnnotationType.Vertical;
            vline.X = OxyPlot.Axes.DateTimeAxis.ToDouble(mvm.Time);
            vline.Text = mvm.Title;
            vline.Tag = mvm;
            this.PlotModel.Annotations.Add(vline);
            this.PlotModel.InvalidatePlot(false);
        }

        protected abstract void SetUpModel();
                
        public virtual bool OnSignalDropped(IPlotView view, ScreenPoint point, ISignalViewModel signal)
        { return true; }

        private ICommand removeSignalCommand;
        public ICommand RemoveSignalCommand
        {
            get
            {
                return removeSignalCommand ?? (removeSignalCommand = new RelayCommand<OxyPlot.Series.Series>(RemoveSignal, (series) => { return series != null; }));
            }
        }

        protected virtual void RemoveSignal(OxyPlot.Series.Series series)
        {
            if (series == null)
            {
                return;
            }

            PlotSignalModel signal = (PlotSignalModel)series.Tag;

            this.paneModel.Signals.Remove(signal);

            PlotModel.Series.Remove(series);

            OxyPlot.Series.LineSeries lax = series as OxyPlot.Series.LineSeries;
            if (lax != null)
            {
                PlotModel.Axes.Remove(lax.YAxis);
            }

            if (SelectedSeries == series)
            {
                SelectedSeries = null;
            }

            PlotModel.InvalidatePlot(false);
        }

        IProjectViewModel projectVM;

        public IProjectViewModel ProjectViewModel
        {
            get { return projectVM; }
        }

        protected OxyPlot.Series.Series selectedSeries;
        public OxyPlot.Series.Series SelectedSeries
        {
            get
            {
                return selectedSeries;
            }
            set
            {
                selectedSeries = value;
                RaisePropertyChanged<IFileViewModel>("SelectedSeries");
            }
        }

        

        protected virtual void PlotModel_MouseDown(object sender, OxyMouseDownEventArgs e)
        {
            IDialogServiceExt svc = SimpleIoc.Default.GetInstance<IDialogServiceExt>();
            selectedPosition = e.Position;
            if (e.ChangedButton == OxyMouseButton.Left && e.ClickCount > 1 && PlotModel.TitleArea.Contains(e.Position))
            {
                string response = svc.ShowPromptDialog("Enter new title:", Title);
                if (response != string.Empty)
                {
                    Title = response;
                    plotModel.Title = response;
                }

                e.Handled = true;
            }
        }

        public void SaveToFile(string filename)
        {
            using (var stream = File.Create(filename))
            {
                var pngExporter = new OxyPlot.Wpf.PngExporter();
                pngExporter.Export(this.PlotModel, stream);
            }
        }

        protected void ShowRangeDialog(Axis axis)
        {
            IDialogServiceExt svc = SimpleIoc.Default.GetInstance<IDialogServiceExt>();
            IRangeModel rangeVM = svc.ShowRangeDialog(axis.ActualMinimum, axis.ActualMaximum);

            if (rangeVM != null)
            {
                axis.Reset();
                if (rangeVM.Auto)
                {
                    axis.Minimum = double.NaN;
                    axis.Maximum = double.NaN;
                }
                else
                {
                    axis.Minimum = rangeVM.From;
                    axis.Maximum = rangeVM.To;
                }

                this.plotModel.InvalidatePlot(false);
            }
        }
    }
}
