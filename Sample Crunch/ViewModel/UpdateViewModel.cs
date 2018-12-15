using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using Squirrel;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Sample_Crunch.ViewModel
{
    public class UpdateViewModel:ViewModelBase
    {
        public UpdateViewModel()
        {

        }

        public override void Cleanup()
        {
            base.Cleanup();
        }

        public Task CheckForUpdates(int timeout)
        {
            var task = CheckForUpdate();
            
            return Task.Run(async () =>
            {
                if (await Task.WhenAny(task, Task.Delay(timeout)) != task)
                {
                    // timeout logic
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
                return updateCommand ?? (updateCommand = new RelayCommand(Execute_UpdateCommand, ()=> { return this.UpdateAvailable && !this.updating; }));
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
                    await manager.ApplyReleases(updates);
                    await manager.UpdateApp();

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

                    CurrentState = State.Installed;
                    //System.Windows.Forms.MessageBox.Show("The application has been updated - please restart the app.");
                    UpdateManager.RestartApp();
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
                Updating = false;
            }
        }

        private async Task CheckForUpdate()
        {
            try
            {
                CurrentState = State.Checking;
                using (var manager = await UpdateManager.GitHubUpdateManager("https://github.com/wolkesson/SampleCrunch", null, null, null, true))
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
    }
}
