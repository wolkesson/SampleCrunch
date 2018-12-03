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
            SimpleIoc.Default.Register<UpdateViewModel>();
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

        public static UpdateViewModel Updater
        {
            get
            {
                return SimpleIoc.Default.GetInstance<UpdateViewModel>();
            }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
            Updater.Cleanup();
            Main.Cleanup();
        }
    }
}