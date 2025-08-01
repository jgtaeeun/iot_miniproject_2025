﻿using System.Configuration;
using System.Data;
using System.Windows;
using WpfIotSimulatorApp.ViewModels;
using WpfIotSimulatorApp.Views;

namespace WpfIotSimulatorApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var viewModel = new MainViewModel();
            var view = new MainView
            {
                DataContext = viewModel,
            };

            viewModel.StartHmiRequested += view.startHmiAni;
            viewModel.StartSensorCheckRequested += view.startCheckAni;
            
            view.ShowDialog();
        }
    }

}
