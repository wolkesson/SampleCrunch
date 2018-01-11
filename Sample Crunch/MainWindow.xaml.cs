using PluginFramework;
using System;
using System.Deployment.Application;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.ApplicationInsights;
using System.Threading.Tasks;
using System.Text;

namespace Sample_Crunch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledceptionHandler);

                InitializeComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                // Log error (including InnerExceptions!)
                // Handle exception
            }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        [SuppressMessage("Await.Warning", "CS4014:Await.Warning")]
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Load plugins from plugin directory
                string pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");

                // Create plugin directory
                if (!Directory.Exists(pluginPath))
                {
                    Directory.CreateDirectory(pluginPath);
                }

                //  Move standard panels dll to plugin directory
                string standardPanelsTargetPath = Path.Combine(pluginPath, "StandardPanels.dll");
                if (!File.Exists(standardPanelsTargetPath))
                {
                    string standardPanelsSourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StandardPanels.dll");
                    File.Copy(standardPanelsSourcePath, standardPanelsTargetPath, true);
                }

                List<Exception> errors = PluginFactory.LoadPlugins(pluginPath);

                foreach (var ex in errors)
                {
                    await MainViewModel.DialogService.ShowError(ex.InnerException, ex.Message, "Continue", null);
                }
            }
            catch (Exception ex)
            {
                await MainViewModel.DialogService.ShowError(ex.Message, "Cold not load plugins", "Continue", null);
                Telemetry.TrackException(new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(ex));
            }

            // Add local factories which will not be found because they are not in dll's.
            PluginFactory.PanelFactories.Add(PluginFactory.CreatePanelFactory(typeof(Factory.MarkerPanelFactory)));
            PluginFactory.PanelFactories.Add(PluginFactory.CreatePanelFactory(typeof(Factory.ProjectPanelFactory)));

            MainViewModel.PropertyChanged += Main_PropertyChanged;

            foreach (var item in PluginFactory.PanelFactories)
            {
                // Add if it has no attribute set or if the attribute is set check the visible flag
                PanelPluginAttribute attr = item.GetType().GetCustomAttribute<PanelPluginAttribute>(false);
                if (attr == null || attr.Visible)
                {
                    MainViewModel.Windows.Add(item);
                }
            }

            MainViewModel.NewProjectCommand.Execute(null);

            if (AppDomain.CurrentDomain.SetupInformation.ActivationArguments != null && AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData != null && AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData.Length > 0)
            {
                string filename = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData[0].ToString();
                try
                {
                    MainViewModel.Project.AddLogFile(filename);
                }
                catch (FileLoadException)
                {
                    MessageBox.Show("There is no available log file reader that can read that file.");
                    return;
                }
            }

            // If this is first run show news
            try
            {
                if (ApplicationDeployment.CurrentDeployment.IsFirstRun)
                {
                    this.GettingStartedMenuItem_Click(null, new RoutedEventArgs());
                }

                Telemetry.Context.Session.IsFirst = ApplicationDeployment.CurrentDeployment.IsFirstRun;
                // Check if this was last run since update
                //else if (ApplicationDeployment.CurrentDeployment.)
            }
            catch (System.Deployment.Application.InvalidDeploymentException)
            {
                // We will end up here if application is not installed, but run locally. 
            }

            // Send telemetry async
            Task.Factory.StartNew(() => SendConfigurationTelemetry()); // No reason to await this
        }

        private void SendConfigurationTelemetry()
        {
            // Send Telemetry data to Application Insight. This is used to priorities the work forward.
            // Data sent in Custom data is put in a dDictionary<string,string> as follows
            // "Parsers"    Title1 (filetype1), Title2 (filetype2), ...
            // "Panels"     Title1 (classname1), Title2 (classname2), ... 
            // "Analyzers"  Title1, Title2, ...
            try
            {
                Dictionary<string, string> props = new Dictionary<string, string>();
                StringBuilder sb = new StringBuilder();

                // Log Parsers
                foreach (IParserFactory parser in PluginFactory.ParserFactories)
                {
                    ParserPluginAttribute attr = parser.GetType().GetCustomAttribute<ParserPluginAttribute>(false);
                    if (attr != null)
                    {
                        sb.AppendFormat("{0} ({1}), ", attr.Title, attr.FileType);
                    }
                }
                props.Add("Parsers", sb.ToString());
                sb.Clear();

                // Log Panels
                PluginFactory.PanelFactories.ForEach((str) => { sb.AppendFormat("{0} ({1}), ", str.Title, str.ToString()); });
                props.Add("Panels", sb.ToString());
                sb.Clear();

                // Log Analysers
                PluginFactory.Analyzers.ForEach((str) => { sb.AppendFormat("{0}, ", str.ToString()); });
                props.Add("Analyzers", sb.ToString());
                sb.Clear();

                // Send it
                Telemetry.TrackTrace("Plugins", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Information, props);
            }
            finally
            { }
        }

        private void UnhandledceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.ExceptionObject.ToString());

            // Send the exception telemetry:
            Telemetry.TrackException((Exception)e.ExceptionObject);

            // Close application
            if (e.IsTerminating) Environment.Exit(0);
        }

        private void Main_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Project")
            {
                ViewModel.MainViewModel main = SimpleIoc.Default.GetInstance<ViewModel.MainViewModel>();
                if (main.Project.ProjectModel.Layout == null) return;

                using (var stream = new StringReader(main.Project.ProjectModel.Layout))
                {
                    // Must create a new deseriaizer every call because otherwise data lingers inside
                    XmlLayoutSerializer dockingSerializer = new XmlLayoutSerializer(dockingManager);
                    dockingSerializer.Deserialize(stream);
                }
            }
        }

        void Documents_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Select newly added document
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                //this.documentPane.SelectedContentIndex = this.documentPane.Children.Count-1;
            }
        }

        private void Execute_ExitCommand(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void CanExecute_ExitCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var upd = SimpleIoc.Default.GetInstance<ViewModel.UpdateViewModel>();
            upd.CheckForUpdates(10000);

            SplashScreen splash = new SplashScreen();
            splash.Show();
        }

        public TelemetryClient Telemetry { get { return SimpleIoc.Default.GetInstance<TelemetryClient>(); } }

        public ViewModel.MainViewModel MainViewModel { get { return SimpleIoc.Default.GetInstance<ViewModel.MainViewModel>(); } }
        private void GettingStartedMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.DialogService.ShowWebPage("Getting Started", @"Resources/GettingStarted.html");
        }

        private void ChangeLogMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.DialogService.ShowWebPage("What's new", @"Resources/ChangeLog.html");
        }

        private void SaveProjectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MainViewModel main = SimpleIoc.Default.GetInstance<ViewModel.MainViewModel>();
            var serializer = new XmlLayoutSerializer(dockingManager);
            using (var stream = new StringWriter())
            {
                serializer.Serialize(stream);
                main.Project.ProjectModel.Layout = stream.ToString();
            }

            main.SaveProjectCommand.Execute(null);
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.ViewModelLocator.Cleanup();
            this.Close();
            App.Current.Shutdown();
        }
    }
}
