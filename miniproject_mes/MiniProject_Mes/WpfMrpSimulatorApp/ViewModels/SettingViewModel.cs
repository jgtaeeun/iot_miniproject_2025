using CommunityToolkit.Mvvm.ComponentModel;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfMrpSimulatorApp.ViewModels
{
    public partial class SettingViewModel : ObservableObject
    {
        private IDialogCoordinator _dialogCoordinator;


        //db연동
        private string _basicCode;
        public string BasicCode
        {
            get => _basicCode;
            set => SetProperty(ref _basicCode, value);
        }

        private string _codeName;
        public string CodeName
        {
            get => _codeName;
            set => SetProperty(ref _codeName, value);
        }

        private string? _codeDesc;
        public string? CodeDesc
        {
            get => _codeDesc;
            set => SetProperty(ref _codeDesc, value);
        }


        private DateTime? _reDt;
        public DateTime? ReDt
        {
            get => _reDt;
            set => SetProperty(ref _reDt, value);
        }

        private DateTime? _modDt;
        public DateTime? ModDt
        {
            get => _modDt;
            set => SetProperty(ref _modDt, value);
        }



        public SettingViewModel(IDialogCoordinator coordinator)
        {
            this._dialogCoordinator = coordinator;
        }

    }
}