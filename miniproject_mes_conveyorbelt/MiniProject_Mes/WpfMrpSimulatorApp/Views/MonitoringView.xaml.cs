using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfMrpSimulatorApp.ViewModels;

namespace WpfMrpSimulatorApp.Views
{
    /// <summary>
    /// MonitoringView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MonitoringView : UserControl
    {
     
        public MonitoringView()
        {
            InitializeComponent();

            this.DataContextChanged += (s, e) =>
            {
                if (e.NewValue is MonitoringViewModel vm)
                {
                    vm.StartHmiRequested += startHmiAni;
                    vm.StartSensorCheckRequested += startCheckAni;
                }
            };
        }

        //wpf상의 객체 애니메이션 추가
        public void startHmiAni()

        {
            // name이 GearStart,GearEnd인 객체의 기어 움직임 애니메이션
            DoubleAnimation ga = new DoubleAnimation();
            ga.From = 0;
            ga.To = 360;
            ga.Duration = TimeSpan.FromSeconds(2);   //schedules테이블의 loadTime값이 실제로는 들어가야 함.

            RotateTransform rt = new RotateTransform();
            GearStart.RenderTransform = rt;
            GearStart.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            GearEnd.RenderTransform = rt;
            GearEnd.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            rt.BeginAnimation(RotateTransform.AngleProperty, ga);


            // name이  Prouct인  retacgle의 애니메이션 (제품 애니메이션)
            DoubleAnimation pa = new DoubleAnimation();
            pa.From = 127;
            pa.To = 417;
            pa.Duration = TimeSpan.FromSeconds(1);

            Product.BeginAnimation(Canvas.LeftProperty, pa);
        }


        public void startCheckAni()
        {
            DoubleAnimation sa = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(50),
                AutoReverse = true
            };
            //name이 SortingSensor인 센서역할 객체의 애니메이션
            SortingSensor.BeginAnimation(OpacityProperty, sa);
            
        }

    }
}
