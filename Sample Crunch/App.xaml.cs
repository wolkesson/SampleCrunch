

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
        private const int MINIMUM_SPLASH_TIME = 1500; // Miliseconds   

        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            // Show splash screen
            SplashScreen splash = new SplashScreen();
            splash.Show();
            splash.Topmost = true;

            // Note: This can not be done before splash because types are registered in ViewModelLocator
            var upd = SimpleIoc.Default.GetInstance<ViewModel.UpdateViewModel>();

            // Check for updates async
            var updateTask = Task.Run(async () =>
            {
                await upd.CheckForUpdates(5000);
            });

            // Load your windows but don't show it yet   
            base.OnStartup(e);
            MainWindow main = new MainWindow();

            updateTask.Wait(2000);

            // Autoclose Splash if no update available
            if (upd.CurrentState == ViewModel.UpdateViewModel.State.Failed ||
                    upd.CurrentState == ViewModel.UpdateViewModel.State.NoUpdateAvailable ||
                    upd.CurrentState == ViewModel.UpdateViewModel.State.Timedout)
            {
                splash.Close();
            }
        }
    }
}
