

namespace Sample_Crunch
{
    using System.Diagnostics;
    using System.Threading;
    using Squirrel;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;

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
            splash.ShowCloseButton = false;
            splash.Show();

            // Start a stop watch   
            Stopwatch timer = new Stopwatch();
            timer.Start();

            // Check for updates
            var updateTask = Task.Run(async () =>
            {
                await UpdateApp();
            });

            // Load your windows but don't show it yet   
            base.OnStartup(e);
            MainWindow main = new MainWindow();
            timer.Stop();

            int remainingTimeToShowSplash = MINIMUM_SPLASH_TIME - (int)timer.ElapsedMilliseconds;
            if (remainingTimeToShowSplash > 0)
                Thread.Sleep(remainingTimeToShowSplash);

            splash.Close();
        }


        public static async Task UpdateApp()
        {
            try
            {
                using (var mgr = await UpdateManager.GitHubUpdateManager("https://github.com/wolkesson/SampleCrunch", null, null, null, true))
                {
                    var updates = await mgr.CheckForUpdate();
                    var lastVersion = updates?.ReleasesToApply?.OrderBy(x => x.Version).LastOrDefault();
                    if (lastVersion == null)
                    {
                        System.Windows.Forms.MessageBox.Show("No Updates are available at this time.");
                        return;
                    }

                    if (System.Windows.Forms.MessageBox.Show($"An update to version {lastVersion.Version} is available. Do you want to update?",
                            "Update available", MessageBoxButtons.OKCancel) != DialogResult.OK)
                    {
                        return;
                    }

                    await mgr.DownloadReleases(new[] { lastVersion });

#if DEBUG
                        System.Windows.Forms.MessageBox.Show("DEBUG: Don't actually perform the update in debug mode");
                    }
#else
                    //await mgr.DownloadReleases(new[] {lastVersion});
                    await mgr.ApplyReleases(updates);
                    await mgr.UpdateApp();

                    System.Windows.Forms.MessageBox.Show("The application has been updated - please close and restart.");
                    mgr.CreateShortcutForThisExe();
                }

                UpdateManager.RestartApp();
#endif
            }
            catch (Exception e)
            {
                MessageBox.Show("Update check failed with an exception: " + e.Message);
            }
        }
    }
}
