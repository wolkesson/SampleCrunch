/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:Sample_Crunch"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight.Ioc;
using Microsoft.ApplicationInsights;
using PluginFramework;
using System;

namespace Sample_Crunch.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        static ViewModelLocator()
        {
            // Other registrations...

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}
            if (!SimpleIoc.Default.IsRegistered<IDialogServiceExt>())
            {
                SimpleIoc.Default.Register<IDialogServiceExt, DialogService>();
            }
            SimpleIoc.Default.Register<PluginManagerViewModel>();
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register(() => SetupTelemetry(), true);
        }

        public static MainViewModel Main
        {
            get
            {
                return SimpleIoc.Default.GetInstance<MainViewModel>();
            }
        }

        public static PluginManagerViewModel PluginManager
        {
            get
            {
                return SimpleIoc.Default.GetInstance<PluginManagerViewModel>();
            }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
            Main.Cleanup();
            CloseTelemetry();
        }

        private static TelemetryClient SetupTelemetry()
        {
            //Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.Active.InstrumentationKey = "1afc4688-781b-4a76-8760-2b20d70db562";

            TelemetryClient telemetry = new TelemetryClient();

            // Alternative to setting ikey in config file:
            telemetry.InstrumentationKey = "1afc4688-781b-4a76-8760-2b20d70db562";

            // Set session data:
            telemetry.Context.User.Id = Environment.UserName;
            telemetry.Context.Session.Id = Guid.NewGuid().ToString();
            telemetry.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
            telemetry.Context.Component.Version = Main.Version;
            return telemetry;
        }

        private static void CloseTelemetry()
        {
            TelemetryClient telem = SimpleIoc.Default.GetInstance<TelemetryClient>();
            SimpleIoc.Default.Unregister<TelemetryClient>();

            TimeSpan runtime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime();
            telem?.TrackMetric("Runtime", runtime.TotalHours);
            telem?.TrackMetric("Processor Time", System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime.TotalHours);
            telem?.Flush(); // only for desktop apps

            // Allow time for flushing:
            //System.Threading.Thread.Sleep(1000);
        }
    }
}