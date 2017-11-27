using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Resources;

namespace Sample_Crunch
{
        public static class WebBrowserUtility
        {
            public static readonly DependencyProperty BindableSourceProperty =
                DependencyProperty.RegisterAttached("BindableSource", typeof(string), typeof(WebBrowserUtility), new UIPropertyMetadata(null, BindableSourcePropertyChanged));

            public static string GetBindableSource(DependencyObject obj)
            {
                return (string)obj.GetValue(BindableSourceProperty);
            }

            public static void SetBindableSource(DependencyObject obj, string value)
            {
                obj.SetValue(BindableSourceProperty, value);
            }

            public static void BindableSourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            {
                WebBrowser browser = o as WebBrowser;
                if (browser != null)
                {
                    string str = e.NewValue.ToString();
                    StreamResourceInfo info = ResourceHelper.GetResourceStreamInfo(str); //ResourceHelper.GetResourceStreamInfo(@"Resources/GettingStarted.html");
                    if (info != null)
                    {
                        browser.NavigateToStream(info.Stream);
                    }
                }
            }

        }
}
