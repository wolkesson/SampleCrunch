using OxyPlot;
using PluginFramework;
using Sample_Crunch.StandardPanels.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Sample_Crunch.StandardPanels
{
    /// <summary>
    /// Interaction logic for PlotWindow.xaml
    /// </summary>
    public partial class PlotWindow : UserControl, IPanelView
    {
        public PlotWindow()
        {
            //// Get call stack
            //StackTrace stackTrace = new StackTrace();

            //// Get calling method name
            //for (int i = 0; i < 10; i++)
            //{
            //    Console.WriteLine(stackTrace.GetFrame(i).GetMethod().Name);
            //}
            //instanceCount++;
            InitializeComponent();
        }

        //public static int instanceCount = 0;
        public PlotPaneViewModel PlotViewModel
        {
            get { return (PlotPaneViewModel)this.DataContext; }
        }
        
        private readonly List<OxyColor> colors = new List<OxyColor>
                                            {
                                                OxyColors.Green,
                                                OxyColors.IndianRed,
                                                OxyColors.Coral,
                                                OxyColors.Chartreuse,
                                                OxyColors.Azure,
                                                OxyColors.Blue,
                                                OxyColors.Black
                                            };

        private void Plot1_Drop(object sender, DragEventArgs e)
        {
            try
            {
                ISignalViewModel signal = e.Data.GetData("Signal") as ISignalViewModel;
                if (signal == null)
                {
                    return;
                }

                OxyPlot.Wpf.PlotView view = sender as OxyPlot.Wpf.PlotView;
                var pos = e.GetPosition(view);
                
                e.Effects = DragDropEffects.None;
                e.Handled = PlotViewModel.OnSignalDropped(view, new ScreenPoint(pos.X, pos.Y), signal);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        private void plot_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                e.Effects = e.Data.GetDataPresent("Signal") ? DragDropEffects.Copy : DragDropEffects.None;
            }
            catch (Exception)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void SaveImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.AddExtension = true;
            dlg.CheckPathExists = true;

            // Create filter extension filter string
            dlg.DefaultExt = ".png"; // Default file extension

            dlg.Filter = "Image | *.png"; // Filter files by extension

            // Show open file dialog box 
            Nullable<bool> result = dlg.ShowDialog();

            if (result.Value)
            {
                this.PlotViewModel.SaveToFile(dlg.FileName);
            }
        }
    }
}
