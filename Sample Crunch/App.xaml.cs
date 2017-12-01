

namespace Sample_Crunch
{
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;
    using Squirrel;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const int MINIMUM_SPLASH_TIME = 1500; // Miliseconds   

        protected async override void OnStartup(StartupEventArgs e)
        {
            // Show splash screen
            SplashScreen splash = new SplashScreen();
            splash.ShowCloseButton = false;
            splash.Show();

            // Start a stop watch   
            Stopwatch timer = new Stopwatch();
            timer.Start();

            // Check for updates
            try
            {
                using (var mgr = new UpdateManager("C:\\Users\\henwo_000\\Documents\\GitHub\\SampleCrunch\\Releases"))
                {
                    await mgr.UpdateApp();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Automatic Update Failed");
            }

            // Load your windows but don't show it yet   
            base.OnStartup(e);
            MainWindow main = new MainWindow();
            timer.Stop();

            int remainingTimeToShowSplash = MINIMUM_SPLASH_TIME - (int)timer.ElapsedMilliseconds;
            if (remainingTimeToShowSplash > 0)
                Thread.Sleep(remainingTimeToShowSplash);

            splash.Close();
        }

    }
}
