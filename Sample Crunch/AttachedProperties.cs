using System;
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

        //cssStyleSheet is the path to the .css file
        public static void ApplyCascadingStyleSheet(WebBrowser browser, string cssStyleSheet)
        {
            // TODO: apply custom styling
            //IHTMLDocument2 htmlDoc = browser.Document as IHTMLDocument2;
            //// The first parameter is the url, the second is the index of the added style sheet.
            //if (htmlDoc != null)
                //htmlDoc.createStyleSheet("", 0);
            //htmlDoc.createElement("script");
            //    htmlDoc.createStyleSheet(cssStyleSheet, 0);
        }

        public static void BindableSourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            WebBrowser browser = o as WebBrowser;
            if (browser != null)
            {
                string str = e.NewValue.ToString();
                Uri u = new Uri(str);

                if (u.IsAbsoluteUri)
                {
                    browser.Navigate(u);
                    ApplyCascadingStyleSheet(browser, "");
                }
                else
                {
                    StreamResourceInfo info = ResourceHelper.GetResourceStreamInfo(str);
                    if (info != null)
                    {
                        browser.NavigateToStream(info.Stream);
                    }
                }
            }
        }
    }
}
