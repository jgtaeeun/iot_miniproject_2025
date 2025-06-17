﻿using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfIotSimulatorApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {

        private string _greeting;

        public MainViewModel()
        {
            Greeting = "Iot Sorting Simulator";
        }

        public string Greeting
        {
            get => _greeting;
            set => SetProperty(ref _greeting, value);   
        }


    }
}
