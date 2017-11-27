using GalaSoft.MvvmLight.CommandWpf;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using PluginFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using System.Linq;
using System.Collections.Specialized;

namespace Sample_Crunch.StandardPanels.ViewModel
{
    public class XYPlotPaneViewModel : PlotPaneViewModel
    {
        public XYPlotPaneViewModel(IProjectViewModel project, IPanelFactory factory, TimePlotModel paneModel)
            : base(project, factory, paneModel)
        {
            project.PropertyChanged += ProjectVM_PropertyChanged;
        }

        public override void AddAnnotation(IMarkerViewModel mvm)
        {
            var annotation = new PointAnnotation();
            annotation.Text = mvm.Title;
            annotation.Tag = mvm;
            InterpolatePosition(mvm, ref annotation);

            mvm.PropertyChanged += (a, b) =>
            {
                var annotations = (from anno in this.PlotModel.Annotations where anno.Tag == a select anno).ToList();

                for (int i = 0; i < annotations.Count; i++)
                {
                    annotation.Text = mvm.Title;
                    InterpolatePosition(mvm, ref annotation);
                }

                this.PlotModel.InvalidatePlot(false);
            };

            this.PlotModel.Annotations.Add(annotation);

            this.PlotModel.InvalidatePlot(false);
        }

        private void InterpolatePosition(IMarkerViewModel mvm, ref PointAnnotation annotation)
        {
            foreach (OxyPlot.Series.ScatterSeries series in PlotModel.Series)
            {
                List<DataPoint> seriesData = series.ItemsSource as List<DataPoint>;
                for (int i = 0; i < seriesData.Count; i++)
                {
                    if (seriesData[i].T >= mvm.Time)
                    {
                        var lastIndex = i > 0 ? i - 1 : i;
                        var nextData = seriesData[i];
                        var lastData = seriesData[lastIndex];
                        var t = mvm.Time.Ticks;
                        var t_0 = lastData.T.Ticks;
                        var t_1 = nextData.T.Ticks;
                        double frac = (t - t_0) / (double)(t_1 - t_0);

                        // Do backward linear interpolation
                        // d = d_0 + (d_1-d_0)\frac{t - t_0}{t_1-t_0} 
                        annotation.Y = lastData.Y + (nextData.Y - lastData.Y) * frac;
                        annotation.X = lastData.X + (nextData.X - lastData.X) * frac;
                        break;
                    }
                }
            }
        }

        protected LinearAxis xAxis;
        protected LinearAxis yAxis;

        protected override void SetUpModel()
        {
            PlotModel.IsLegendVisible = false;
            //PlotModel.LegendTitle = "Legend";
            //PlotModel.LegendOrientation = LegendOrientation.Horizontal;
            //PlotModel.LegendPlacement = LegendPlacement.Outside;
            //PlotModel.LegendPosition = LegendPosition.TopRight;
            //PlotModel.LegendBackground = OxyColor.FromAColor(200, OxyColors.White);
            //PlotModel.LegendBorder = OxyColors.Black;

            xAxis = new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Title = "Drop signal here",
                Key = "X"
            };
            PlotModel.Axes.Add(xAxis);

            yAxis = new LinearAxis()
            {
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Title = "Drop signal here",
                Key = "Y"
            };
            PlotModel.Axes.Add(yAxis);

            PlotModel.MouseDown += PlotModel_MouseDown;
        }
        
        public override bool OnSignalDropped(IPlotView view, ScreenPoint point, ISignalViewModel signal)
        {
            base.OnSignalDropped(view, point, signal);

            Axis xAxis, yAxis;
            PlotModel.GetAxesFromPoint(point, out xAxis, out yAxis);

            if (xAxis != null && yAxis != null)
            {
                // Do nothing
            }
            else if (xAxis == null && yAxis != null)
            {
                // Dropped on y axis
                internalAddSignal(signal, yAxis);
            }
            else if (xAxis != null && yAxis == null)
            {
                // Dropped on x axis
                internalAddSignal(signal, xAxis);
            }

            return true;
        }
        ISignalViewModel xSignal, ySignal;

        internal struct DataPoint
        {
            public double X { get; set; }
            public double Y { get; set; }
            public DateTime T { get; set; }
            public OxyPlot.Series.ScatterPoint ScatterPoint { get; set; }
        }

        protected virtual PlotSignalModel internalAddSignal(ISignalViewModel signal, Axis lastDroppedAxis)
        {
            PlotSignalModel ps = new PlotSignalModel() { Path = signal.GetPath(), Axis = lastDroppedAxis.Key };
            var ax = PlotModel.GetAxisOrDefault(ps.Axis, xAxis);
            ax.Title = signal.Title;
            
            if (lastDroppedAxis == xAxis)
            {
                xSignal = signal;
                //var ySignal = (from PlotSignalModel s in model.Signals where s.Axis == "Y" select s).First();
            }
            else if (lastDroppedAxis == yAxis)
            {
                ySignal = signal;
            }

            if (xSignal == null || ySignal == null)
            {
                PlotModel.InvalidatePlot(true);
                return ps;
            }

            var xdata = xSignal.GetData();
            var ydata = ySignal.GetData();

            var scatterSerie = new OxyPlot.Series.ScatterSeries()
            {
                MarkerType = OxyPlot.MarkerType.Cross,
                MarkerSize = 2,
                MarkerStroke = OxyColors.Blue,
                //Title = xSignal.Title + "/" + ySignal.Title,
                TrackerFormatString = "{1}: \t{2}\n{3}: \t{4}"
            };

            // Find pixel size here
            scatterSerie.BinSize = 4;
            
            List<DataPoint> dataPoints = new List<DataPoint>(xdata.GetLength(0));

            for (int i = 0; i < xdata.GetLength(0); i++)
            {
                DataPoint dp = new DataPoint();
                dp.X = xdata[i].Value;
                dp.Y = ydata[i].Value;
                dp.T = xdata[i].Time;
                dp.ScatterPoint = new OxyPlot.Series.ScatterPoint(xdata[i].Value, ydata[i].Value);
                dataPoints.Add(dp);
                //scatterSerie.Points.Add(new OxyPlot.Series.ScatterPoint(xdata[i].Value, ydata[i].Value,double.NaN, double.NaN, xdata[i].Time));
            }
            scatterSerie.ItemsSource = dataPoints;
            scatterSerie.Mapping =  Filter;
            scatterSerie.XAxisKey = "X";
            scatterSerie.YAxisKey = "Y";

            PlotModel.Series.Add(scatterSerie);

            //double a, b;
            //LeastSquaresFit(scatterSerie.Points, out a, out b);
            //PlotModel.Annotations.Add(new LineAnnotation { Slope = a, Intercept = b, Text = "Least squares fit" });
            PlotModel.InvalidatePlot(true);
            return ps;
        }

        OxyPlot.Series.ScatterPoint invalidPoint = new OxyPlot.Series.ScatterPoint(double.NaN, double.NaN);
        protected OxyPlot.Series.ScatterPoint Filter(object item)
        {
            DataPoint dp = (DataPoint)item;
            if (dp.T < ProjectViewModel.StartTime || dp.T > ProjectViewModel.StopTime)
            {
                return invalidPoint;
            }
            else
            {
                return dp.ScatterPoint;
            }
            
        }

        protected override void Files_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.Files_CollectionChanged(sender, e);
            foreach (var item in model.Signals)
            {
                var signalModel = ProjectViewModel.GetSignal(item.Path);
                if (signalModel != null)
                {
                    var axis = plotModel.GetAxisOrDefault(item.Axis, xAxis);
                    internalAddSignal(signalModel, axis);
                }
            }
        }

        /// <summary>
        /// Calculates the Least squares fit of a list of DataPoints.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="a">The slope.</param>
        /// <param name="b">The intercept.</param>
        public static void LeastSquaresFit(IReadOnlyList<OxyPlot.Series.ScatterPoint> points, out double a, out double b)
        {
            // http://en.wikipedia.org/wiki/Least_squares
            // http://mathworld.wolfram.com/LeastSquaresFitting.html
            // http://web.cecs.pdx.edu/~gerry/nmm/course/slides/ch09Slides4up.pdf

            double Sx = 0;
            double Sy = 0;
            double Sxy = 0;
            double Sxx = 0;
            int m = 0;
            foreach (var p in points)
            {
                Sx += p.X;
                Sy += p.Y;
                Sxy += p.X * p.Y;
                Sxx += p.X * p.X;
                m++;
            }
            double d = Sx * Sx - m * Sxx;
            a = 1 / d * (Sx * Sy - m * Sxy);
            b = 1 / d * (Sx * Sxy - Sxx * Sy);
        }
        
        protected override void RemoveSignal(OxyPlot.Series.Series series)
        {
            if (series == null)
            {
                return;
            }

            PlotSignalModel signal = (PlotSignalModel)series.Tag;

            //this.paneModel.Signals.Remove(signal);

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

        protected override void PlotModel_MouseDown(object sender, OxyMouseDownEventArgs e)
        {
            base.PlotModel_MouseDown(sender, e);
            if (e.Handled) return;

            if (e.ClickCount == 1)
            {
                OxyPlot.Series.Series ser = PlotModel.GetSeriesFromPoint(e.Position, 20);
                SelectedSeries = ser;
            }

            Axis xAxis, yAxis;
            PlotModel.GetAxesFromPoint(e.Position, out xAxis, out yAxis);

            if (xAxis != null && yAxis != null)
            {
                SelectedSeries = null;
            }
            else if (xAxis == null && yAxis != null && e.ChangedButton == OxyMouseButton.Left && e.ClickCount > 1 && PlotModel.Series.Count > 0)
            {
                ShowRangeDialog(yAxis);
            }
            else if (xAxis != null && yAxis == null && e.ChangedButton == OxyMouseButton.Left && e.ClickCount > 1 && PlotModel.Series.Count > 0)
            {
                ShowRangeDialog(xAxis);
            }
        }

        private void ProjectVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProjectViewModel.StartTime))
            {
                PlotModel.InvalidatePlot(true);
            }
            else if (e.PropertyName == nameof(ProjectViewModel.StopTime))
            {
                PlotModel.InvalidatePlot(true);
            }
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
            var pos = PlotModel.GetSeriesFromPoint(this.SelectedPosition, 20);
            var point = pos.GetNearestPoint(this.SelectedPosition, false);
            var dp = (XYPlotPaneViewModel.DataPoint)point.Item;
            this.ProjectViewModel.AddMarkerCommand.Execute(dp.T);
        }
    }
}
