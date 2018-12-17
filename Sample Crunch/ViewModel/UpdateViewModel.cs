using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using Squirrel;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Sample_Crunch.ViewModel
{
    public class UpdateViewModel : ViewModelBase
    {
        public UpdateViewModel()
        {
            RestoreSettings();
        }

        public Task CheckForUpdates(int timeout)
        {
            var task = CheckForUpdate();

            return Task.Run(async () =>
            {
                if (await Task.WhenAny(task, Task.Delay(timeout)) != task)
                {
                    CurrentState = State.Timedout;
                }
            });
        }

        public enum State
        {
            Idle,
            Checking,
            UpdateAvailable,
            NoUpdateAvailable,
            Downloading,
            Installing,
            Installed,
            Timedout,
            Failed
        }

        public bool PreRelease
        {
            get { return Properties.Settings.Default.PreRelease; }
            set
            {
                Properties.Settings.Default.PreRelease = value;
                Properties.Settings.Default.Save();
                RaisePropertyChanged<bool>(nameof(PreRelease));
                CheckForUpdates(10000).Start();
            }
        }

        private State state = State.Idle;
        public State CurrentState
        {
            get { return state; }
            private set
            {
                this.state = value;
                RaisePropertyChanged<bool>(nameof(CurrentState));
            }
        }

        private bool updateAvailable = false;
        public bool UpdateAvailable
        {
            get { return updateAvailable; }
            private set
            {
                this.updateAvailable = value;
                RaisePropertyChanged<bool>(nameof(UpdateAvailable));
            }
        }

        public string AvailableVersion
        {
            get { return (lastVersion == null ? "Checking..." : lastVersion.Version.ToString()); }
        }

        private ICommand updateCommand;
        private ReleaseEntry lastVersion = null;

        public ICommand UpdateCommand
        {
            get
            {
                return updateCommand ?? (updateCommand = new RelayCommand(Execute_UpdateCommand, () => { return this.UpdateAvailable && !this.updating; }));
            }
        }
        private bool updating = false;
        public bool Updating
        {
            get { return this.updating; }
            private set
            {
                this.updating = value;
                RaisePropertyChanged<bool>(nameof(Updating));
            }
        }

        private async void Execute_UpdateCommand()
        {
            if (this.updating) return;

            Updating = true;
            CurrentState = State.Checking;

            try
            {
                Stopwatch watch = Stopwatch.StartNew();
                using (var manager = await UpdateManager.GitHubUpdateManager("https://github.com/wolkesson/SampleCrunch", null, null, null, true))
                {
                    var updates = await manager.CheckForUpdate();
                    var lastVersion = updates?.ReleasesToApply?.OrderBy(x => x.Version).LastOrDefault();
                    CurrentState = State.Downloading;
                    await manager.DownloadReleases(new[] { lastVersion });
#if DEBUG
            System.Windows.Forms.MessageBox.Show("DEBUG: Don't actually perform the update in debug mode");

#else
                    CurrentState = State.Installing;

                    //manager.CreateShortcutForThisExe();

                    MainViewModel main = SimpleIoc.Default.GetInstance<MainViewModel>();

                    // Send Telemetry
                    System.Collections.Specialized.NameValueCollection data = new System.Collections.Specialized.NameValueCollection
                    {
                        { "from", main.Version },
                        { "to", this.lastVersion.Version.ToString() },
                        { "elapse", watch.ElapsedMilliseconds.ToString() }
                    };
                    AppTelemetry.ReportEvent("Updating", data);

                    //System.Windows.Forms.MessageBox.Show("The application has been updated - please restart the app.");
                    await manager.ApplyReleases(updates);
                    await manager.UpdateApp();
                    BackupSettings();

                    CurrentState = State.Installed;
#endif
                }
            }
            catch (Exception e)
            {
                AppTelemetry.ReportError("Update", e);
                CurrentState = State.Failed;
            }
            finally
            {
                if (CurrentState == State.Installed)
                {
                    UpdateManager.RestartApp();
                }
                Updating = false;
            }
        }

        private async Task CheckForUpdate()
        {
            try
            {
                CurrentState = State.Checking;
                using (var manager = await UpdateManager.GitHubUpdateManager("https://github.com/wolkesson/SampleCrunch", null, null, null, Properties.Settings.Default.PreRelease))
                {
                    var updates = await manager.CheckForUpdate();
                    this.lastVersion = updates?.ReleasesToApply?.OrderBy(x => x.Version).LastOrDefault();

                    if (this.lastVersion == null)
                    {
                        CurrentState = State.NoUpdateAvailable;
                        UpdateAvailable = false;
                    }
                    else
                    {
                        UpdateAvailable = true;
                        CurrentState = State.UpdateAvailable;
                        RaisePropertyChanged<string>(nameof(AvailableVersion));
                    }
                }
            }
            catch (Exception e)
            {
                AppTelemetry.ReportError("Update", e);
                CurrentState = State.Failed;
            }
        }

        /// <summary>
        /// Make a backup of our settings.
        /// Used to persist settings across updates.
        /// </summary>
        public static void BackupSettings()
        {
            string settingsFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            string destination = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\..\\last.config";
            File.Copy(settingsFile, destination, true);
        }

        /// <summary>
        /// Restore our settings backup if any.
        /// Used to persist settings across updates.
        /// </summary>
        private static void RestoreSettings()
        {
            //Restore settings after application update            
            string destFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            string sourceFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\..\\last.config";
            // Check if we have settings that we need to restore
            if (!File.Exists(sourceFile))
            {
                // Nothing we need to do
                return;
            }
            // Create directory as needed
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destFile));
            }
            catch (Exception) { }

            // Copy our backup file in place 
            try
            {
                File.Copy(sourceFile, destFile, true);
            }
            catch (Exception) { }

            // Delete backup file
            try
            {
                File.Delete(sourceFile);
            }
            catch (Exception) { }

        }
    }
}
