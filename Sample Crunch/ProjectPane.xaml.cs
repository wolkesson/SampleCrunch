using GalaSoft.MvvmLight.Ioc;
using PluginFramework;
using Sample_Crunch.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sample_Crunch
{
    /// <summary>
    /// Interaction logic for ProjectPane.xaml
    /// </summary>
    public partial class ProjectPane : UserControl, IPanelView
    {
        public ProjectPane()
        {
            InitializeComponent();
            //AllowMultiSelection(this.treeView);
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.StackPanel tv = sender as System.Windows.Controls.StackPanel;
            SignalViewModel item = tv.DataContext as SignalViewModel;

            if (item != null && e.LeftButton == MouseButtonState.Pressed)
            {
                try
                {
                    DragDrop.DoDragDrop(this.treeView,
                                         new DataObject("Signal", item),
                                         System.Windows.DragDropEffects.Copy);

                }
                catch { }
            }
        }

        private void ExportSignalMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem tv = sender as System.Windows.Controls.MenuItem;
            SignalViewModel signal = tv.DataContext as SignalViewModel;
            if (signal != null)
            {

                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.DefaultExt = ".txt"; // Default file extension
                dlg.Filter = "Text files|*.txt"; // Filter files by extension
                dlg.FileName = signal.Title;
                dlg.AddExtension = true;

                // Show open file dialog box 
                Nullable<bool> result = dlg.ShowDialog();

                // Process open file dialog box results 
                if (result == true)
                {
                    Sample[] data = signal.GetData();

                    using (StreamWriter sw = File.CreateText(dlg.FileName))
                    {
                        CultureInfo invariantCulture = CultureInfo.InvariantCulture;
                        for (int i = 0; i < data.GetLength(0); i++)
                        {
                            sw.WriteLine(data[i].Time.ToString(invariantCulture) + "," + data[i].Value.ToString(invariantCulture));
                        }
                    }
                }
            }
        }

        private void ExportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem tv = sender as System.Windows.Controls.MenuItem;
            FileViewModel fileTopic = tv.DataContext as FileViewModel;
            if (fileTopic != null)
            {
                MessageBox.Show("Sorry this is not implemented! Export each signal separatly. ");
                //Sample[] data = signal.GetData();

                //using (StreamWriter sw = File.CreateText("C:\\text.txt"))
                //{
                //    for (int i = 0; i < data.GetLength(0); i++)
                //    {
                //        sw.WriteLine(data[i].Time + "," + data[i].Value);
                //    }
                //}
            }
        }

        private void MetaMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem tv = sender as System.Windows.Controls.MenuItem;
            FileViewModel fileTopic = tv.DataContext as FileViewModel;
            var results = new Dictionary<string, object>();

            if (fileTopic.LogFile != null)
            {
                string tmpStr = string.Empty;
                {
                    foreach (Type analyzerType in PluginFactory.FindAnalyzers(fileTopic.LogFile.GetType()))
                    {
                        IAnalyzer analyzer = PluginFactory.CreateAnalyzer(analyzerType);
                        foreach (var result in analyzer.Analyze(fileTopic.LogFile))
                            results.Add(result.Key, result.Value);
                    }
                }

                foreach (var item in results)
                {
                    tmpStr += item.Key + ": " + item.Value.ToString() + Environment.NewLine;
                }
                MessageBox.Show(tmpStr, Properties.Resources.computedDataCaption);
            }
        }

        private void treeView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                MainViewModel model = SimpleIoc.Default.GetInstance<MainViewModel>();

                foreach (string filename in files)
                {
                    try
                    {
                        model.Project.AddLogFile(filename);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }

        }

        private void treeView_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string filename in files)
                {
                    try
                    {
                        IParserFactory lfp = PluginFactory.FindLogFileParser(filename);
                        if (lfp != null)
                        {
                            e.Effects = DragDropEffects.Copy;
                            e.Handled = true;
                            return;
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }

            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        //List<TreeViewItem> selectedItems = new List<TreeViewItem>();
        //private void treeView_Selected(object sender, RoutedEventArgs e)
        //{
        //    if (CtrlPressed)
        //    {
        //        selectedItems.Add(e.OriginalSource as TreeViewItem);
        //    }
        //    else
        //    {
        //        selectedItems.Clear();
        //        selectedItems.Add(e.OriginalSource as TreeViewItem);
        //    }
        //}

        //bool CtrlPressed
        //{
        //    get
        //    {
        //        return System.Windows.Input.Keyboard.IsKeyDown(Key.LeftCtrl);
        //    }
        //}

        private static readonly PropertyInfo IsSelectionChangeActiveProperty = typeof(TreeView).GetProperty("IsSelectionChangeActive", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void AllowMultiSelection(TreeView treeView)
        {
            if (IsSelectionChangeActiveProperty == null)
                return;

            var selectedItems = new List<TreeViewItem>();

            treeView.SelectedItemChanged += (a, b) =>
            {
                var treeViewItem = treeView.SelectedItem as TreeViewItem;
                if (treeViewItem == null)
                    return;

                //allow multiple selection
                //when control key is pressed
                if (Keyboard.IsKeyDown(Key.LeftCtrl)
                || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    //suppress selection change notification
                    //select all selected items
                    //then restore selection change notifications
                    var isSelectionChangeActive
                    = IsSelectionChangeActiveProperty
                    .GetValue(treeView, null);

                    IsSelectionChangeActiveProperty
                    .SetValue(treeView, true, null);

                    selectedItems.ForEach(t => t.IsSelected = true);

                    IsSelectionChangeActiveProperty.SetValue(treeView,
                    isSelectionChangeActive, null);
                }
                else
                {
                    //unselect all selected items except the current one
                    selectedItems.ForEach(t => t.IsSelected = t == treeViewItem);
                    selectedItems.Clear();
                }

                if (selectedItems.Contains(treeViewItem) == false)
                {
                    selectedItems.Add(treeViewItem);
                }
                else
                {
                    //deselect if already selected
                    treeViewItem.IsSelected = false;
                    selectedItems.Remove(treeViewItem);
                }
            };

        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            MainViewModel model = SimpleIoc.Default.GetInstance<MainViewModel>();
            model.SelectedFile = treeView.SelectedItem as FileViewModel;
        }
    }
}
