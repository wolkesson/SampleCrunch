using PluginFramework;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GalaSoft.MvvmLight.Ioc;
using System.Threading.Tasks;
using System.Text;
using System.Globalization;

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
                await MainViewModel.DialogService.ShowError(ex.Message, "Could not load plugins", "Continue", null);
                AppTelemetry.ReportError("Loading", ex);
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
                bool firstrun = Properties.Settings.Default.FirstRun;
                if (firstrun)
                {
                    MainViewModel.ShowWebPageCommand.Execute(@"https://wolkesson.github.io/SampleCrunch/getting-started");
                    Properties.Settings.Default.AppTelemetry = (MessageBox.Show(
                        "Sample Crunch uses volentary telemetry to track usage and find bugs. Do you approve to send annonumous data?",
                        "Allow telemetry?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes);
                    Properties.Settings.Default.FirstRun = false;
                    Properties.Settings.Default.Save();
                }
            }
            catch (System.Deployment.Application.InvalidDeploymentException)
            {
                // We will end up here if application is not installed, but run locally. 
            }

            try
            {
                // Block App telemetry if user has disapproved it
                AppTelemetry.DoNotSend = !Properties.Settings.Default.AppTelemetry;
                if (string.IsNullOrEmpty(Properties.Settings.Default.AppTelemetryUID))
                {
                    AppTelemetry.RegisterUser(CultureInfo.InstalledUICulture.EnglishName, MainViewModel.Version);
                }
            }
            catch
            { }
        }

        private void UnhandledceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.ExceptionObject.ToString());

            // Send the exception telemetry:
            AppTelemetry.ReportError("Unhandled", (Exception)e.ExceptionObject);

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

        public ViewModel.MainViewModel MainViewModel { get { return SimpleIoc.Default.GetInstance<ViewModel.MainViewModel>(); } }

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
            try
            {
                TimeSpan runtime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime();
                AppTelemetry.ReportUsage((int)runtime.TotalMinutes, PluginFactory.ParserFactories.Count, PluginFactory.PanelFactories.Count, PluginFactory.Analyzers.Count);
            }
            catch (Exception)
            {
                // Catch all since we are closing
            }

            ViewModel.ViewModelLocator.Cleanup();
            this.Close();
            App.Current.Shutdown();
        }
    }
}
