using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfMrpSimulatorApp.Helpers;
using WpfMrpSimulatorApp.Models;
using WpfMrpSimulatorApp.Views;

namespace WpfMrpSimulatorApp.ViewModels
{
    public partial class MainViewModel : ObservableObject, IDisposable
    {
        private object currentViewModel;
        public object CurrentViewModel
        {
            get => currentViewModel;
            set => SetProperty(ref currentViewModel, value);
        }

        // 메세지박스대신에 다이얼로그로 표현하기 위해서
        private IDialogCoordinator _dialogCoordinator;

     

        public MainViewModel()
        {

        }

        public MainViewModel(IDialogCoordinator coordinator)
        {
            this._dialogCoordinator = coordinator;
        }

        [RelayCommand]
        public void  Setting()
        {
            CurrentViewModel = new SettingViewModel(Common.DIALOGCOORDINATOR);
        }

        [RelayCommand]
        public void Schedule()
        {
            CurrentViewModel = new ScheduleViewModel(Common.DIALOGCOORDINATOR);
          
        }

        [RelayCommand]
        public void Monitoring()
        {
            CurrentViewModel = new MonitoringViewModel(Common.DIALOGCOORDINATOR);
           
        }
        

        [RelayCommand]
        public void Analysis()
        {

        }

    

        [RelayCommand]
        public async Task AppExit()
        {
            //var result = MessageBox.Show("종료하시겠습니까?", "종료확인", MessageBoxButton.YesNo, MessageBoxImage.Question);
            //if (result == MessageBoxResult.Yes)
            //{
            //    Application.Current.Shutdown(); 
            //}
            //else
            //{
            //    return;
            //}
            var result = await this._dialogCoordinator.ShowMessageAsync(this, "종료확인", "종료하시겠습니까?", MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Affirmative)
            {
                Application.Current.Shutdown();
            }
            else
            {
                return;
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
