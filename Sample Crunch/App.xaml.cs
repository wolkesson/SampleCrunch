

namespace Sample_Crunch
{
    using System.Diagnostics;
    using System.Threading;
    using Squirrel;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using GalaSoft.MvvmLight.Ioc;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private const int MINIMUM_SPLASH_TIME = 2000; // Miliseconds   
        private const int MAXIMUM_SPLASH_TIME = 5000; // Miliseconds  

        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            // Show splash screen
            SplashScreen splash = new SplashScreen();
            splash.Show();
            splash.Topmost = true;

            // Minimum delay so that splash does to disappera too quickly
            var splashTask = Task.Delay(MINIMUM_SPLASH_TIME);

            // Note: This can not be done before splash loaded because types are registered in 
            // ViewModelLocator which is loaded by splash (for example)
            var upd = SimpleIoc.Default.GetInstance<ViewModel.UpdateViewModel>();

            // Check for updates async
            var updateTask = Task.Run(async () =>
            {
                await upd.CheckForUpdates(MAXIMUM_SPLASH_TIME);
            });

            // Load main window
            base.OnStartup(e);
            MainWindow main = new MainWindow();

            // Total delay will be between MINIMUM_SPLASH_TIME and MAXIMUM_SPLASH_TIME
            Task.WaitAll(updateTask, splashTask);

            // Autoclose Splash if no updates are available
            if (upd.CurrentState == ViewModel.UpdateViewModel.State.Failed ||
                    upd.CurrentState == ViewModel.UpdateViewModel.State.NoUpdateAvailable ||
                    upd.CurrentState == ViewModel.UpdateViewModel.State.Timedout)
            {
                splash.Close();
            }
        }
    }
}
