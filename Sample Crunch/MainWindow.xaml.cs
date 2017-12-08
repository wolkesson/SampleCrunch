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
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Load plugins from programs folder and program data folder
                string pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
                List<Exception> errors = PluginFactory.LoadPlugins(pluginPath);

                foreach (var ex in errors)
                {
                    await MainViewModel.DialogService.ShowError(ex.InnerException, ex.Message, "Continue", null);
                }
            }
            catch (Exception ex)
            {
                await MainViewModel.DialogService.ShowError(ex.Message, "Cold not load plugins", "Continue", null);
            }

            try
            {
                // Log Parsers
                Dictionary<string, string> parsers = new Dictionary<string, string>();
                foreach (Type parserType in PluginFactory.Parsers)
                {
                    ParserPluginAttribute attr = parserType.GetCustomAttribute<ParserPluginAttribute>(false);
                    if (attr != null)
                    {
                        parsers.AddUnique(attr.Title, attr.FileType);
                    }
                }
                Telemetry.TrackTrace("Plugins: Parsers", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Information, parsers);

                // Log Panels
                Dictionary<string, string> panels = new Dictionary<string, string>();
                PluginFactory.PanelFactorys.ForEach((str) => { panels.AddUnique(str.ToString(), str.Title); });
                Telemetry.TrackTrace("Plugins: Panels", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Information, panels);

                // Log Analysers
                Dictionary<string, string> analysers = new Dictionary<string, string>();
                PluginFactory.Analyzers.ForEach((str) => { analysers.AddUnique(str.ToString(), ""); });
                Telemetry.TrackTrace("Plugins: Analyzers", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Information, analysers);
            }
            finally
            { }

            // Add local factories which will not be found because they are not in dll's.
            PluginFactory.PanelFactorys.Add(PluginFactory.CreatePanelFactory(typeof(Factory.MarkerPanelFactory)));
            PluginFactory.PanelFactorys.Add(PluginFactory.CreatePanelFactory(typeof(Factory.ProjectPanelFactory)));
            
            MainViewModel.PropertyChanged += Main_PropertyChanged;


            foreach (var item in PluginFactory.PanelFactorys)
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
            SplashScreen splash = new SplashScreen();
            splash.ShowCloseButton = true;
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
            this.Close();

            ViewModel.ViewModelLocator.Cleanup();
        }
    }
}
