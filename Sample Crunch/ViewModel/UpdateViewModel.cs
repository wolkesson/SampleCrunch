using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.ApplicationInsights;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Sample_Crunch.ViewModel
{
    public class UpdateViewModel:ViewModelBase
    {
        UpdateManager manager;
        public UpdateViewModel()
        {
        }

        public override void Cleanup()
        {
            base.Cleanup();
            if (manager != null)
            {
                manager.Dispose();
            }
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

        TelemetryClient telemetry = SimpleIoc.Default.GetInstance<TelemetryClient>();

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

                // Send Telemetry
                MainViewModel main = SimpleIoc.Default.GetInstance<MainViewModel>();
                Dictionary<string, string> props = new Dictionary<string, string>();
                props.Add("from", main.Version);
                props.Add("to", this.lastVersion.Version.ToString());
                Dictionary<string, double> metrics = new Dictionary<string, double>();
                metrics.Add("Elapse", watch.ElapsedMilliseconds);
                telemetry.TrackEvent("Updating", props, metrics);

                CurrentState = State.Installed;
                //System.Windows.Forms.MessageBox.Show("The application has been updated - please restart the app.");
                UpdateManager.RestartApp();
#endif
            }
            catch (Exception e)
            {
                telemetry.TrackException(e);
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
                if (manager == null)
                {
                    this.manager = await UpdateManager.GitHubUpdateManager("https://github.com/wolkesson/SampleCrunch", null, null, null, true);
                }

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
            catch (Exception e)
            {
                TelemetryClient cl = SimpleIoc.Default.GetInstance<TelemetryClient>();
                cl.TrackException(e);
                CurrentState = State.Failed;
            }
        }
    }
}
