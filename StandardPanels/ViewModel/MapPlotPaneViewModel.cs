using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using PluginFramework;
using System;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace Sample_Crunch.StandardPanels.ViewModel
{
    public class MapPlotPaneViewModel : XYPlotPaneViewModel
    {
        public MapPlotPaneViewModel(IProjectViewModel project, IPanelFactory factory, TimePlotModel paneModel)
            : base(project, factory, paneModel)
        {
            
        }

        protected override void SetUpModel()
        {
            PlotModel.IsLegendVisible = false;

            // Add the tile map annotation
            PlotModel.Annotations.Add(
                     new TileMapAnnotation
                     {
                         Url = "http://tile.openstreetmap.org/{Z}/{X}/{Y}.png",
                         CopyrightNotice = "OpenStreetMap",
                         MinZoomLevel = 5,
                         MaxZoomLevel = 19,
                         Selectable = false,
                         Layer = AnnotationLayer.BelowAxes,
                         MaxNumberOfDownloads = 8
                     });

            xAxis = new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Title = "Lon",
                Key = "X"
            };

            xAxis.AxisChanged += XAxis_AxisChanged;
            PlotModel.Axes.Add(xAxis);

            yAxis = new LinearAxis()
            {
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Title = "Lat",
                Key = "Y"
            };

            PlotModel.Axes.Add(yAxis);

            PlotModel.MouseDown += PlotModel_MouseDown;
            Files_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void Files_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (xSignal== null)
            {
                var signal = ProjectViewModel.FindSignalByTag("Longitude");
                if (signal.Count > 0)
                {
                    xSignal = signal[0];
                    internalAddSignal(signal[0],xAxis);
                }
            }

            if (ySignal == null)
            {
                var signal = ProjectViewModel.FindSignalByTag("Latitude");
                if (signal.Count > 0)
                {
                    ySignal = signal[0];
                    internalAddSignal(signal[0], yAxis);
                }
            }
        }

        protected override PlotSignalModel internalAddSignal(ISignalViewModel signal, Axis lastDroppedAxis)
        {
            PlotSignalModel ps = new PlotSignalModel() { Path = signal.GetPath(), Axis = lastDroppedAxis.Key };
            var ax = PlotModel.GetAxisOrDefault(ps.Axis, xAxis);
            ax.Title = signal.Title;

            if (lastDroppedAxis == xAxis)
            {
                xSignal = signal;
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
                Title = xSignal.Title + "/" + ySignal.Title,
                TrackerFormatString = "{1}: \t{2}\n{3}: \t{4}"
            };

            // Find pixel size here
            scatterSerie.BinSize = 4;

            List<DataPoint> dataPoints = new List<DataPoint>(xdata.GetLength(0));
            double xInRad = (xSignal.Unit == "rad") ? 180.0 / Math.PI : 1;
            double yInRad = (ySignal.Unit == "rad") ? 180.0 / Math.PI : 1;

            for (int i = 0; i < xdata.GetLength(0); i++)
            {
                XYPlotPaneViewModel.DataPoint dp = new XYPlotPaneViewModel.DataPoint();
                dp.X = xdata[i].Value * xInRad;
                dp.Y = ydata[i].Value * yInRad;
                dp.T = xdata[i].Time;
               
                dp.ScatterPoint = new OxyPlot.Series.ScatterPoint(dp.X, dp.Y);
                dataPoints.Add(dp);
                //scatterSerie.Points.Add(new OxyPlot.Series.ScatterPoint(xdata[i].Value, ydata[i].Value,double.NaN, double.NaN, xdata[i].Time));
            }

            scatterSerie.ItemsSource = dataPoints;
            scatterSerie.Mapping = Filter;
            scatterSerie.XAxisKey = "X";
            scatterSerie.YAxisKey = "Y";

            PlotModel.Series.Add(scatterSerie);

            //double a, b;
            //LeastSquaresFit(scatterSerie.Points, out a, out b);
            //PlotModel.Annotations.Add(new LineAnnotation { Slope = a, Intercept = b, Text = "Least squares fit" });
            PlotModel.InvalidatePlot(true);
            return ps;
        }

        private void XAxis_AxisChanged(object sender, AxisChangedEventArgs e)
        {
            // Make sure to keep aspect ratio
            var xrange = Deg2Rad(xAxis.ActualMaximum - xAxis.ActualMinimum);
            var yrange = Deg2Rad(yAxis.ActualMaximum - yAxis.ActualMinimum);

            var xmid = Deg2Rad(xAxis.ActualMaximum + xAxis.ActualMinimum) / 2;
            var ymid = Deg2Rad(yAxis.ActualMaximum + yAxis.ActualMinimum) / 2;
            var nrange = xrange * Math.Cos(ymid);
            yAxis.Minimum = Rad2Deg(ymid - nrange/2);
            yAxis.Maximum = Rad2Deg(ymid + nrange/2);
            //throw new NotImplementedException();
        }

        public static double Deg2Rad(double deg)
        {
            return deg * Math.PI / 180;
        }

        public static double Rad2Deg(double rad)
        {
            return rad * 180 / Math.PI;
        }

        ISignalViewModel xSignal;
        ISignalViewModel ySignal;
                        
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
    }
}
