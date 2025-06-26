using MahApps.Metro.Controls.Dialogs;
using System.Configuration;
using System.Data;
using System.Windows;
using WpfMrpSimulatorApp.Helpers;
using WpfMrpSimulatorApp.Models;
using WpfMrpSimulatorApp.ViewModels;
using WpfMrpSimulatorApp.Views;

namespace WpfMrpSimulatorApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static TotalConfig? configuration { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
           

            //config.json 파일 로드
            configuration = ConfigLoader.Load();

            Common.DIALOGCOORDINATOR = DialogCoordinator.Instance;

            //view화면 로드 후 화면 띄우기
          
            var viewModel = new MainViewModel(Common.DIALOGCOORDINATOR);
            var view = new MainView
            {
                DataContext = viewModel,
            };
           
            view.ShowDialog();

        }
    }

}
