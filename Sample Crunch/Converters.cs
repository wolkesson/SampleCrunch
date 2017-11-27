using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Sample_Crunch
{
    public class TimeToPositionConverter : System.Windows.Data.IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime time = (DateTime)values[0];
            DateTime minimum = (DateTime)values[1];
            DateTime maximum = (DateTime)values[2];
            double width = (double)values[3];

            double timeRange = maximum.Ticks - minimum.Ticks;
            var positionRatio = (time.Ticks - minimum.Ticks) / timeRange;
            return width * positionRatio;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            object[] dts = new object[4];
            dts[0] = DateTime.Now;
            return dts;
            //double timeRange = maximum.Ticks - minimum.Ticks;
            //var positionRatio = (time.Ticks - minimum.Ticks) / timeRange;
            //return width * positionRatio;

            //DateTime time = (DateTime)values[0];
            //DateTime minimum = (DateTime)values[1];
            //DateTime maximum = (DateTime)values[2];
            //double width = (double)values[3];


            //throw new NotImplementedException();
        }
    }

    public class TimeToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime startTime = (DateTime)values[0];
            DateTime stopTime = (DateTime)values[1];
            DateTime minimum = (DateTime)values[2];
            DateTime maximum = (DateTime)values[3];
            double width = (double)values[4];
            
            double fullRange = maximum.Ticks - minimum.Ticks;
            double timeRange = stopTime.Ticks - startTime.Ticks;
            var rangeRatio = timeRange / fullRange;
            return width * rangeRatio;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PercentToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //int x = (int) value;
            //if (this.PlotModel.Series.Count == 0) return 0;
            //OxyPlot.Series.LineSeries ls = (OxyPlot.Series.LineSeries)this.PlotModel.Series[0];
            //return ls.MinX + x * (ls.MaxX - ls.MinX);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //double x = (double)value;
            //if (this.PlotModel.Series.Count == 0) return 0;
            //OxyPlot.Series.LineSeries ls = (OxyPlot.Series.LineSeries)this.PlotModel.Series[0];
            //return (x- ls.MinX) /(ls.MaxX - ls.MinX);
            return value;
        }
    }

    public class TimeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime dt = (DateTime)value;
            return dt.ToString("yyyy-MM-dd HH:mm:ss.ffff");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime dt;
            if (DateTime.TryParse(value.ToString(), out dt))
            {
                return dt;
            }
            return DateTime.MinValue;
        }
    }

    public class BooleanConverter<T> : IValueConverter
    {
        public BooleanConverter(T trueValue, T falseValue)
        {
            True = trueValue;
            False = falseValue;
        }

        public T True { get; set; }
        public T False { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && ((bool)value) ? True : False;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T && EqualityComparer<T>.Default.Equals((T)value, True);
        }
    }

    public sealed class BooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public BooleanToVisibilityConverter() : base(Visibility.Visible, Visibility.Collapsed)
        { }
    }

    [ContentProperty("Items")]
    public class EnumToObjectConverter : IValueConverter
    {
        public ResourceDictionary Items { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string key = Enum.GetName(value.GetType(), value);
            return Items[key];
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("This converter only works for one way binding");
        }
    }
}
