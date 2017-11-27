using PluginFramework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sample_Crunch.Controls
{
    /// <summary>
    /// Interaction logic for Timeline.xaml
    /// </summary>
    public partial class Timeline : UserControl
    {
        public Timeline()
        {
            InitializeComponent();
            StartTime = Minimum;
            StopTime = Maximum;
        }

        public DateTime Minimum
        {
            get { return (DateTime)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(DateTime), typeof(Timeline),
                new FrameworkPropertyMetadata(DateTime.MinValue, FrameworkPropertyMetadataOptions.AffectsRender, UpdateLimits));

        public DateTime Maximum
        {
            get { return (DateTime)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(DateTime), typeof(Timeline),
                new FrameworkPropertyMetadata(DateTime.MaxValue, FrameworkPropertyMetadataOptions.AffectsRender, UpdateLimits));


        public DateTime StartTime
        {
            get { return (DateTime)GetValue(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartTimeProperty =
            DependencyProperty.Register("StartTime", typeof(DateTime), typeof(Timeline),
                new FrameworkPropertyMetadata(DateTime.MinValue, FrameworkPropertyMetadataOptions.AffectsRender, UpdateRange));


        public DateTime StopTime
        {
            get { return (DateTime)GetValue(StopTimeProperty); }
            set { SetValue(StopTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StopTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StopTimeProperty =
            DependencyProperty.Register("StopTime", typeof(DateTime), typeof(Timeline),
                new FrameworkPropertyMetadata(DateTime.MaxValue, FrameworkPropertyMetadataOptions.AffectsRender, UpdateRange));

        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register("Size", typeof(Size), typeof(Timeline),
                new FrameworkPropertyMetadata(Size.Empty, FrameworkPropertyMetadataOptions.AffectsRender, UpdateRange));

        private static void UpdateRange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Timeline timeline = d as Timeline;
            if (timeline == null) return;

            if (e.Property == StartTimeProperty)
            {
                DateTime startTime = (DateTime)e.NewValue;
                if (startTime < timeline.Minimum || startTime > timeline.Maximum) return;

                double startX = timeline.TimeToPosition(startTime);
                if (!double.IsInfinity(startX))
                {
                    Canvas.SetLeft(timeline.startMarker, startX);
                    Canvas.SetLeft(timeline.rectangle, startX);
                }

                double offset = Canvas.GetRight(timeline.stopMarker);
                timeline.rectangle.Width = (timeline.ActualWidth - offset - startX).Clamp(0, timeline.ActualWidth);
            }
            else if (e.Property == StopTimeProperty)
            {
                DateTime stopTime = (DateTime)e.NewValue;
                if (stopTime < timeline.Minimum || stopTime > timeline.Maximum) return;
                double offset = Math.Min(timeline.ActualWidth, timeline.TimeToPosition(stopTime)); // Use min to limit to full width
                Canvas.SetRight(timeline.stopMarker, timeline.ActualWidth - offset);
                timeline.rectangle.Width = offset - Canvas.GetLeft(timeline.startMarker);
            }
        }

        private void UpdateRange2(DateTime startTime, DateTime stopTime)
        {
            if (startTime < this.Minimum || startTime > this.Maximum) return;

            double startX = this.TimeToPosition(startTime).Clamp(0, this.ActualWidth);
            double stopX = this.TimeToPosition(stopTime).Clamp(0, this.ActualWidth);
            if (!double.IsInfinity(startX))
            {
                Canvas.SetLeft(this.startMarker, startX);
                Canvas.SetLeft(this.rectangle, startX);
            }
            if (!double.IsInfinity(stopX))
            {
                this.rectangle.Width = Math.Max(0, stopX - startX);
                Canvas.SetLeft(this.stopMarker, stopX - this.stopMarker.ActualWidth);
            }
        }

        private static void UpdateLimits(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Timeline timeline = d as Timeline;
            if (timeline == null) return;

            if (timeline.StartTime < timeline.Minimum)
            {
                timeline.StartTime = timeline.Minimum;
            }

            if (timeline.StopTime > timeline.Maximum)
            {
                timeline.StopTime = timeline.Maximum;
            }
            timeline.UpdateIntervals(timeline.ActualWidth);
        }

        public double ActualMinorStep { get; private set; }
        public double ActualMajorStep { get; private set; }
        public string ActualStringFormat { get; private set; }
        public string StringFormat { get; private set; }

        private DateTimeIntervalType actualIntervalType = DateTimeIntervalType.Hours;
        //private DateTimeIntervalType actualMinorIntervalType = DateTimeIntervalType.Auto;

        private DateTime PositionToTime(double position)
        {
            var timeRange = Maximum - Minimum;
            var positionRatio = position / this.ActualWidth;
            return new DateTime(Minimum.Ticks + (long)(timeRange.Ticks * positionRatio));
        }

        private double TimeToPosition(DateTime time)
        {
            double timeRange = Maximum.Ticks - Minimum.Ticks;
            var positionRatio = (time.Ticks - Minimum.Ticks) / timeRange;
            return this.ActualWidth * positionRatio;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            // Move these to normal labels
            Typeface courierTypeface = new Typeface(this.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            FormattedText ftMinimum = new FormattedText(Minimum.ToShortTimeString(), System.Globalization.CultureInfo.CurrentCulture, this.FlowDirection, courierTypeface, 14.0, this.Foreground);
            FormattedText ftMaximum = new FormattedText(Maximum.ToShortTimeString(), System.Globalization.CultureInfo.CurrentCulture, this.FlowDirection, courierTypeface, 14.0, this.Foreground);

            drawingContext.DrawText(ftMinimum, new Point(0, this.ActualHeight - ftMinimum.Height));
            drawingContext.DrawText(ftMaximum, new Point(this.ActualWidth - ftMaximum.Width, this.ActualHeight - ftMaximum.Height));

            if (this.Minimum == this.Maximum) return;
            Pen p = new Pen(this.Foreground, 1);

            //IList<double> minorTickValues = this.CreateDateTimeTickValues(               this.Minimum, this.Maximum, this.ActualMinorStep, this.actualMinorIntervalType);
            IList<DateTime> majorTickValues = this.CreateDateTimeTickValues(this.Minimum, this.Maximum, this.ActualMajorStep, this.actualIntervalType);
            IList<DateTime> majorLabelValues = majorTickValues;

            foreach (var item in majorTickValues)
            {
                var x = this.TimeToPosition(item);
                drawingContext.DrawLine(p, new Point(x, 0), new Point(x, 5));
            }

            UpdateRange2(this.StartTime, this.StopTime);
        }

        public static DateTimeIntervalType AutoSelectInterval(double width, long range)
        {
            var ticksPerPixel = range / width;
            double minDistance = 120 * ticksPerPixel;
            double maxDistance = 120 * ticksPerPixel;

            DateTimeIntervalType interval = DateTimeIntervalType.Milliseconds;

            // Autoselect a good major interval type (Days, Hours, Seconds etc)
            if (minDistance > TimeSpan.TicksPerMillisecond)
            {
                interval = DateTimeIntervalType.Milliseconds;
            }
            if (minDistance > 100*TimeSpan.TicksPerMillisecond)
            {
                interval = DateTimeIntervalType.Seconds;
            }
            if (minDistance > TimeSpan.TicksPerSecond)
            {
                interval = DateTimeIntervalType.Seconds;
            }
            if (minDistance > TimeSpan.TicksPerMinute)
            {
                interval = DateTimeIntervalType.Minutes;
            }
            if (minDistance > TimeSpan.TicksPerHour)
            {
                interval = DateTimeIntervalType.Hours;
            }
            if (minDistance > TimeSpan.TicksPerDay)
            {
                interval = DateTimeIntervalType.Days;
            }

            return interval;
        }

        /// <summary> 
        /// Updates the intervals. 
        /// </summary> 
        /// <param name="plotArea">The plot area.</param> 
        internal void UpdateIntervals(double width)
        {
            // Calculate number of ticks per pixel
            var range = this.Maximum.Ticks - this.Minimum.Ticks;
            //var ticksPerPixel = range / width;
            if (range <= 0 || width <= 0) return;
            this.actualIntervalType = AutoSelectInterval(width, range);

            this.ActualMajorStep = 1;

            switch (this.actualIntervalType)
            {
                case DateTimeIntervalType.Years:
                    this.ActualMinorStep = 31;
                    //this.actualMinorIntervalType = DateTimeIntervalType.Years;
                    if (this.StringFormat == null)
                    {
                        this.ActualStringFormat = "yyyy";
                    }
                    break;
                case DateTimeIntervalType.Months:
                    //this.actualMinorIntervalType = DateTimeIntervalType.Months;
                    if (this.StringFormat == null)
                    {
                        this.ActualStringFormat = "yyyy-MM-dd";
                    }
                    break;
                case DateTimeIntervalType.Weeks:
                    //this.actualMinorIntervalType = DateTimeIntervalType.Days;
                    this.ActualMajorStep = 7;
                    this.ActualMinorStep = 1;
                    if (this.StringFormat == null)
                    {
                        this.ActualStringFormat = "yyyy/ww";
                    }
                    break;
                case DateTimeIntervalType.Days:
                    this.ActualMinorStep = this.ActualMajorStep;
                    if (this.StringFormat == null)
                    {
                        this.ActualStringFormat = "yyyy-MM-dd";
                    }
                    break;
                case DateTimeIntervalType.Hours:
                    this.ActualMinorStep = this.ActualMajorStep;
                    if (this.StringFormat == null)
                    {
                        this.ActualStringFormat = "HH:mm";
                    }
                    break;
                case DateTimeIntervalType.Minutes:
                    this.ActualMinorStep = this.ActualMajorStep;
                    if (this.StringFormat == null)
                    {
                        this.ActualStringFormat = "HH:mm";
                    }
                    break;
                case DateTimeIntervalType.Seconds:
                    this.ActualMinorStep = this.ActualMajorStep;
                    if (this.StringFormat == null)
                    {
                        this.ActualStringFormat = "HH:mm:ss";
                    }
                    break;
                case DateTimeIntervalType.Manual:
                    break;
                case DateTimeIntervalType.Auto:

                    break;
            }
        }


        /// <summary> 
        /// Creates the date tick values. 
        /// </summary> 
        /// <param name="min">The min.</param> 
        /// <param name="max">The max.</param> 
        /// <param name="step">The step.</param> 
        /// <param name="intervalType">Type of the interval.</param> 
        /// <returns>Date tick values.</returns> 
        private IList<DateTime> CreateDateTimeTickValues(DateTime start, DateTime max, double step, DateTimeIntervalType intervalType)
        {
            var values = new Collection<DateTime>();
            if (start == DateTime.MinValue || max == DateTime.MaxValue || start >= max)
            {
                // Invalid start time 
                return values;
            }


            switch (intervalType)
            {
                case DateTimeIntervalType.Minutes:
                    // make sure the first tick is at the 1st minute of a hour 
                    start = new DateTime(start.Year, start.Month, start.Day, start.Hour, 0, 0);
                    break;
                case DateTimeIntervalType.Hours:
                    // make sure the first tick is at the 1st hour of a day 
                    start = new DateTime(start.Year, start.Month, start.Day, 0, 0, 0);
                    break;
                case DateTimeIntervalType.Weeks:
                    // make sure the first tick is at the 1st day of a week 
                    start = start.AddDays(-(int)start.DayOfWeek);
                    break;
                case DateTimeIntervalType.Months:
                    // make sure the first tick is at the 1st of a month 
                    start = new DateTime(start.Year, start.Month, 1);
                    break;
                case DateTimeIntervalType.Years:
                    // make sure the first tick is at Jan 1st 
                    start = new DateTime(start.Year, 1, 1);
                    break;
            }


            // Adds a tick to the end time to make sure the end DateTime is included. 
            var end = max.AddTicks(1);
            if (end.Ticks == 0)
            {
                // Invalid end time 
                return values;
            }


            var current = start;
            double eps = step * 1e-3;
            var minDateTime = start;//.AddDays(-eps);
            var maxDateTime = max;//.AddDays(+eps);


            if (minDateTime.Ticks == 0 || maxDateTime.Ticks == 0)
            {
                // Invalid min/max time 
                return values;
            }


            while (current < end)
            {
                if (current > minDateTime && current < maxDateTime)
                {
                    values.Add(current);
                }


                try
                {
                    switch (intervalType)
                    {
                        case DateTimeIntervalType.Months:
                            current = current.AddMonths((int)Math.Ceiling(step));
                            break;
                        case DateTimeIntervalType.Years:
                            current = current.AddYears((int)Math.Ceiling(step));
                            break;
                        case DateTimeIntervalType.Days:
                            current = current.AddDays((int)Math.Ceiling(step));
                            break;
                        case DateTimeIntervalType.Hours:
                            current = current.AddHours((int)Math.Ceiling(step));
                            break;
                        case DateTimeIntervalType.Minutes:
                            current = current.AddMinutes((int)Math.Ceiling(step));
                            break;
                        case DateTimeIntervalType.Seconds:
                            current = current.AddSeconds((int)Math.Ceiling(step));
                            break;
                        case DateTimeIntervalType.Milliseconds:
                            current = current.AddMilliseconds((int)Math.Ceiling(step));
                            break;
                        default:
                            current = current.AddHours(step);
                            break;
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    // AddMonths/AddYears/AddDays can throw an exception 
                    // We could test this by comparing to MaxDayValue/MinDayValue, but it is easier to catch the exception... 
                    break;
                }
            }


            return values;
        }

        private void canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateIntervals(this.ActualWidth);
        }

        private void startThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double startX = Math.Max(0, Canvas.GetLeft(startMarker) + e.HorizontalChange);
            if (!double.IsNaN(startX)) this.StartTime = PositionToTime(startX);
        }

        private void stopMarker_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double stopX = Math.Max(0, Canvas.GetLeft(stopMarker) + e.HorizontalChange);
            if (!double.IsNaN(stopX)) this.StopTime = PositionToTime(stopX);
        }
    }
}
