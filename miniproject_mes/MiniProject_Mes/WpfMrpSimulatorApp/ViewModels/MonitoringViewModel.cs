using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WpfMrpSimulatorApp.ViewModels
{
    public partial class MonitoringViewModel : ObservableObject
    {
        private IDialogCoordinator _dialogCoordinator;



        public MonitoringViewModel(IDialogCoordinator coordinator)
        {
            this._dialogCoordinator = coordinator;
        }

        [RelayCommand]
        public async Task SearchProcess()
        {
            await this._dialogCoordinator.ShowMessageAsync(this, "공정조회", "조회를 시작합니다");
        }
        [RelayCommand]
        public async Task StartProcess()
        {
            await this._dialogCoordinator.ShowMessageAsync(this, "공정시작", "공정을 시작합니다");
        }
                
    }
}
