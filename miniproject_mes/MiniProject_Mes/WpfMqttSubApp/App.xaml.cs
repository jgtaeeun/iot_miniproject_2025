using MahApps.Metro.Controls.Dialogs;
using System.Configuration;
using System.Data;
using System.Windows;
using WpfMqttSubApp.Helpers;
using WpfMqttSubApp.ViewModels;
using WpfMqttSubApp.Views;

namespace WpfMqttSubApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var dIDialogCoordinator = DialogCoordinator.Instance;
            var viewModel = new MainViewModel(dIDialogCoordinator);
            var view = new MainView
            {
                DataContext = viewModel,
            };
            view.ShowDialog();
           
        }
    }

}
