using OxyPlot;
using OxyPlot.Axes;
using System;
using System.ComponentModel;
using OxyPlot.Series;
using PluginFramework;
using System.Collections.Specialized;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using OxyPlot.Annotations;

namespace Sample_Crunch.StandardPanels.ViewModel
{
    public class TimePlotViewModel:PlotPaneViewModel
    {
        public TimePlotViewModel(IProjectViewModel project, IPanelFactory factory, TimePlotModel paneModel)
            : base(project, factory, paneModel)
        {
            project.PropertyChanged += ProjectVM_PropertyChanged;
            project.FilesRealigned += Project_FilesRealigned;
        }

    private void Project_FilesRealigned(object sender, EventArgs e)
        {
            foreach (OxyPlot.Series.LineSeries serie in PlotModel.Series)
            {
                PlotSignalModel plotSignal = (PlotSignalModel) serie.Tag;

                var signalModel = ProjectViewModel.GetSignal(plotSignal.Path);
                if (signalModel != null)
                {
                    PluginFramework.Sample[] data = signalModel.GetData();
                    serie.Points.Clear();
                    serie.Points.Capacity = data.GetLength(0);
                    for (int i = 0; i < data.GetLength(0); i++)
                    {
                        serie.Points.Add(OxyPlot.Axes.DateTimeAxis.CreateDataPoint(data[i].Time, data[i].Value));
                    }
                }
            }

            plotModel.InvalidatePlot(true);
        }

        private DateTimeAxis dateAxis;
        private AxisPosition axisPosition = AxisPosition.Left;

        protected override void SetUpModel()
        {
            PlotModel.LegendTitle = "Legend";
            PlotModel.LegendOrientation = LegendOrientation.Horizontal;
            PlotModel.LegendPlacement = LegendPlacement.Outside;
            PlotModel.LegendPosition = LegendPosition.TopRight;
            PlotModel.LegendBackground = OxyColor.FromAColor(200, OxyColors.White);
            PlotModel.LegendBorder = OxyColors.Black;

            this.dateAxis = new DateTimeAxis()
            {
                Position = AxisPosition.Bottom,
                Title = "Time",
                IntervalType = OxyPlot.Axes.DateTimeIntervalType.Seconds,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                IntervalLength = 80
            };

            PlotModel.Axes.Add(this.dateAxis);

            try
            {
                var startTime = DateTimeAxis.ToDouble(ProjectViewModel.StartTime);
                var stopTime = DateTimeAxis.ToDouble(ProjectViewModel.StopTime);
                this.dateAxis.Zoom(startTime, stopTime);
                this.dateAxis.AbsoluteMinimum = startTime;
                this.dateAxis.AbsoluteMaximum = stopTime + (stopTime == startTime?0.01:0); // Hack to avoid Minimum equals Maximum
                PlotModel.InvalidatePlot(false);
            }
            catch (Exception)
            {

                //throw;
            }

            PlotModel.MouseDown += PlotModel_MouseDown;
        }

        private void ProjectVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProjectViewModel.StartTime))
            {
                if (ProjectViewModel.StopTime < ProjectViewModel.StartTime) return;
                try
                {
                    var startTime = DateTimeAxis.ToDouble(ProjectViewModel.StartTime);
                    this.dateAxis.Zoom(startTime, dateAxis.ActualMaximum);
                    this.dateAxis.AbsoluteMinimum = startTime;
                    PlotModel.InvalidatePlot(false);
                }
                catch (Exception)
                {

                    //throw;
                }
            }
            else if (e.PropertyName == nameof(ProjectViewModel.StopTime))
            {
                if (ProjectViewModel.StopTime < ProjectViewModel.StartTime) return;
                try
                {
                    var stopTime = DateTimeAxis.ToDouble(ProjectViewModel.StopTime);
                    this.dateAxis.Zoom(dateAxis.ActualMinimum, stopTime);
                    this.dateAxis.AbsoluteMaximum = stopTime;
                    PlotModel.InvalidatePlot(false);
                }
                catch (Exception)
                {

                    //throw;
                }
            }
        }
        
        protected PlotSignalModel internalAddSignal(ISignalViewModel signal)
        {

            PluginFramework.Sample[] data = signal.GetData();
            var lineSerie = new OxyPlot.Series.LineSeries
            {
                StrokeThickness = 1,
                CanTrackerInterpolatePoints = false,
                Title = signal.Title,
                Smooth = false,
                Decimator = Decimator.Decimate,
                TrackerFormatString = "{1}: \t{2:HH:mm:ss}\n{3}: \t{4}"
            };

            lineSerie.Points.Capacity = data.GetLength(0);
            for (int i = 0; i < data.GetLength(0); i++)
            {
                lineSerie.Points.Add(OxyPlot.Axes.DateTimeAxis.CreateDataPoint(data[i].Time, data[i].Value));
            }
            
            // Create axis
            var valueAxis = new OxyPlot.Axes.LinearAxis() { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, Title = signal.Title };
            valueAxis.Key = signal.Title;
            valueAxis.Position = axisPosition;
            axisPosition = (axisPosition == AxisPosition.Left ? AxisPosition.Right : AxisPosition.Left);
            valueAxis.TextColor = lineSerie.MarkerStroke;
            valueAxis.PositionTier = PlotModel.Axes.Count;
            valueAxis.Unit = signal.Unit;
            PlotModel.Axes.Add(valueAxis);

            lineSerie.YAxisKey = signal.Title;
            PlotSignalModel ps = new PlotSignalModel() { Path = signal.GetPath() };
            lineSerie.Tag = ps;
            //lineSerie.MouseDown += lineSerie_MouseDown;
            PlotModel.Series.Add(lineSerie);
            PlotModel.InvalidatePlot(true);
            return ps;
        }

        protected override void RemoveSignal(Series series)
        {
            axisPosition = ((LineSeries)series).YAxis.Position;
            base.RemoveSignal(series);
        }
        
        private ICommand addMarkerCommand;
        public ICommand AddMarkerCommand
        {
            get
            {
                return addMarkerCommand ?? (addMarkerCommand = new RelayCommand(Execute_AddMarkerCommand));
            }
        }

        protected virtual void Execute_AddMarkerCommand()
        {
            var pos = dateAxis.InverseTransform(this.SelectedPosition.X);

            DateTime dt = OxyPlot.Axes.DateTimeAxis.ToDateTime(pos);
                    this.ProjectViewModel.AddMarkerCommand.Execute(dt);
        }

        protected override void PlotModel_MouseDown(object sender, OxyMouseDownEventArgs e)
        {
            base.PlotModel_MouseDown(sender, e);

            Axis xAxis, yAxis;
            PlotModel.GetAxesFromPoint(e.Position, out xAxis, out yAxis);

            if (xAxis != null && yAxis != null)
            {
                SelectedSeries = null;
            }
            else if (xAxis == null && yAxis != null)
            {
                // A vertical axis was clicked
                if (e.ClickCount == 1)
                {
                    // Set selected series here!
                    foreach (OxyPlot.Series.LineSeries series in plotModel.Series)
                    {
                        if (series.YAxis.Key == yAxis.Key)
                        {
                            SelectedSeries = series;
                            return;
                        }
                    }
                }
                else if (e.ChangedButton == OxyMouseButton.Left && e.ClickCount > 1)
                {
                    ShowRangeDialog(yAxis);
                }
            }
            else if (xAxis != null && yAxis == null && e.ChangedButton == OxyMouseButton.Left && e.ClickCount > 1 && PlotModel.Series.Count > 0)
            {
                ShowRangeDialog(xAxis);
            }
        }

        public override bool OnSignalDropped(IPlotView view, ScreenPoint point, ISignalViewModel signal)
        {
            base.OnSignalDropped(view, point, signal);
            var s = internalAddSignal(signal);
            model.Signals.Add(s);
            return true;
        }

        protected override void Files_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.Files_CollectionChanged(sender, e);
            UpdateAllSignals();
        }

        private void UpdateAllSignals()
        {
            foreach (var item in model.Signals)
            {
                // Create a list of unresolved signals and only search for them here. 
                var signalModel = ProjectViewModel.GetSignal(item.Path);
                if (signalModel != null)
                {
                    internalAddSignal(signalModel);
                }
            }
        }
    }
}
