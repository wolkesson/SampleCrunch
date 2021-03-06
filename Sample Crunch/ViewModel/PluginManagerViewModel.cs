﻿namespace Sample_Crunch.ViewModel
{
    using GalaSoft.MvvmLight.CommandWpf;
    using GalaSoft.MvvmLight.Ioc;
    using PluginFramework;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Input;

    public class PluginManagerViewModel
    {
        public string PluginPath { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins"); } }

        public IDialogServiceExt DialogService
        {
            get
            {
                return SimpleIoc.Default.GetInstance<IDialogServiceExt>();
            }
        }

        public List<PluginFactory.PluginInfo> Plugins { get { return PluginFactory.Info; } }

        private ICommand importPluginCommand;
        public ICommand ImportPluginCommand
        {
            get
            {
                return importPluginCommand ?? (importPluginCommand = new RelayCommand(Execute_ImportPluginCommand));
            }
        }

        private void Execute_ImportPluginCommand()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.Multiselect = true;

            // Create filter extension filter string
            dlg.DefaultExt = ".dll"; // Default file extension
            dlg.Filter = "Plugin DLL|*.dll"; // Filter files by extension

            // Show open file dialog box 
            Nullable<bool> result = dlg.ShowDialog();

            // Create directory if it does not exist
            if (!Directory.Exists(PluginPath)) {
                Directory.CreateDirectory(PluginPath);
            }

            // Process open file dialog box results 
            if (result == true)
            {
                // All files
                foreach (var filename in dlg.FileNames)
                {
                    try
                    {
                        // Copy file to Plugin path
                        File.Copy(filename, Path.Combine(PluginPath, Path.GetFileName(filename)));
                    }

                    catch (Exception ex)
                    {
                        DialogService.ShowError(ex, "Could not open file", null, null).Wait();
                        return;
                    }
                }

                DialogService.ShowMessage("You need to restart the application for changes to take effect!", "Restart required");
            }
        }

        private ICommand deletePluginCommand;
        public ICommand DeletePluginCommand
        {
            get
            {
                return deletePluginCommand ?? (deletePluginCommand = new RelayCommand<PluginFactory.PluginInfo>(Execute_DeletePluginCommand));
            }
        }

        private async void Execute_DeletePluginCommand(PluginFactory.PluginInfo obj)
        {
            try
            {
                bool proceed = await DialogService.ShowMessage("This will permanently delete the plugin file! Any project depending on components from this plugin might become invalid! Do you want to proceed and delete the file?", "Are you sure?", "Delete Plugin File", "Cancel", null);
                if (proceed)
                {
                    File.Delete(obj.AssemblyPath);
                    await DialogService.ShowMessage("You need to restart the application for changes to take effect!", "Restart required");
                }
            }

            catch (Exception ex)
            {
                DialogService.ShowError(ex, "Could not delete the plugin file", null, null).Wait();
                return;
            }
        }
    }
}
