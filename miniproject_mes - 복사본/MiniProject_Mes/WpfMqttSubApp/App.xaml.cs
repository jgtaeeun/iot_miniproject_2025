using MahApps.Metro.Controls.Dialogs;
using System.Configuration;
using System.Data;
using System.Windows;
using WpfMqttSubApp.Helpers;
using WpfMqttSubApp.Models;
using WpfMqttSubApp.ViewModels;
using WpfMqttSubApp.Views;

namespace WpfMqttSubApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static TotalConfig? configuration {  get; private set; }
        
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //config.json 파일 로드
            configuration = ConfigLoader.Load();


            //view화면 로드 후 화면 띄우기
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
