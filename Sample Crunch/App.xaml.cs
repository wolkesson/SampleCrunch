

namespace Sample_Crunch
{
    using System.Threading.Tasks;
    using GalaSoft.MvvmLight.Ioc;
    using Squirrel;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private const int MINIMUM_SPLASH_TIME = 2000; // Miliseconds   
        private const int MAXIMUM_SPLASH_TIME = 5000; // Miliseconds  

        //private const string V = "https://github.com/wolkesson/SampleCrunch";

        //private static void OnFirstRun()
        //{
        //    //Logging.Log.Info("Triggered OnFirstRun...");
        //}
        //private static void OnAppUpdate(System.Version version)
        //{
        //    //Logging.Log.Info("Triggered OnAppUpdate...");

        //    using (var manager = new UpdateManager(V))
        //    {
        //        manager.CreateShortcutsForExecutable("MyApp.exe", ShortcutLocation.Desktop, true);
        //        manager.CreateShortcutsForExecutable("MyApp.exe", ShortcutLocation.StartMenu, true);
        //        manager.CreateShortcutsForExecutable("MyApp.exe", ShortcutLocation.AppRoot, true);

        //        manager.RemoveUninstallerRegistryEntry();
        //        manager.CreateUninstallerRegistryEntry();
        //    }
        //}
        //private static void OnAppUninstall(System.Version version)
        //{
        //    //Logging.Log.Info("Triggered OnAppUninstall...");

        //    using (var manager = new UpdateManager(V))
        //    {
        //        manager.RemoveShortcutsForExecutable("MyApp.exe", ShortcutLocation.Desktop);
        //        manager.RemoveShortcutsForExecutable("MyApp.exe", ShortcutLocation.StartMenu);
        //        manager.RemoveShortcutsForExecutable("MyApp.exe", ShortcutLocation.AppRoot);

        //        manager.RemoveUninstallerRegistryEntry();
        //    }
        //}
        //private static void OnInitialInstall(System.Version version)
        //{
        //    //Logging.Log.Info("Triggered OnInitialInstall...");

        //    using (var manager = new UpdateManager(V))
        //    {
        //        manager.CreateShortcutForThisExe();

        //        manager.CreateShortcutsForExecutable("MyApp.exe", ShortcutLocation.Desktop, false);
        //        manager.CreateShortcutsForExecutable("MyApp.exe", ShortcutLocation.StartMenu, false);
        //        manager.CreateShortcutsForExecutable("MyApp.exe", ShortcutLocation.AppRoot, false);

        //        manager.CreateUninstallerRegistryEntry();
        //    }
        //}

    //public App()
    //    {
    //        Squirrel.SquirrelAwareApp.HandleEvents(
    //           onInitialInstall: OnInitialInstall,
    //           onAppUpdate: OnAppUpdate,
    //           onAppUninstall: OnAppUninstall,
    //           onFirstRun: OnFirstRun);
    //    }

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
