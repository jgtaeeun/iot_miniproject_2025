using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public MainViewModel()
        {
            Schedule();
        }

        [RelayCommand]
        public void Schedule()
        {
            CurrentViewModel = new ScheduleView();
        }

        [RelayCommand]
        public void Monitoring()
        {
            
        }


        [RelayCommand]
        public void Analysis()
        {

        }

        [RelayCommand]
        public void Setting()
        {

        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
