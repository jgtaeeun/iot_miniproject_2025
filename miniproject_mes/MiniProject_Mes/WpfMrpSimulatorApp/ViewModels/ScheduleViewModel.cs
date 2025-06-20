using CommunityToolkit.Mvvm.ComponentModel;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfMrpSimulatorApp.ViewModels
{
    public partial class ScheduleViewModel : ObservableObject
    {
      

        private IDialogCoordinator _dialogCoordinator;



        public ScheduleViewModel(IDialogCoordinator coordinator)
        {
            this._dialogCoordinator = coordinator;
        }


    }
}
