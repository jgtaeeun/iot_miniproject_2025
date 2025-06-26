using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MQTTnet;
using MQTTnet.Adapter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using WpfIotSimulatorApp.Models;

namespace WpfIotSimulatorApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {

        private string _greeting;
        public string Greeting
        {
            get => _greeting;
            set => SetProperty(ref _greeting, value);
        }


        //색상표시할 변수
        private Brush _productBrush;
        public Brush ProductBrush
        {
            get => _productBrush;
            set => SetProperty(ref _productBrush, value);
        }
            
        
        // MQTT
        private IMqttClient mqttClient;
        private string brokerHost;
        private string topic;
        private string clientId;
        private int logNum = 1;

        private string _logText;
        public string LogText
        {
            get => _logText;
            set => SetProperty(ref _logText, value);    
        }

        public MainViewModel()
        {
            Greeting = "Iot Sorting Simulator";
            LogText = "프로그램 실행";
            //MQTT 설정
            brokerHost = "210.119.12.110";
            clientId = "IOT01";
            topic = "pknu/mes/data";
            InitMqttClient();
        }

        // MQTT 클라이언트를 초기화
        private async void InitMqttClient()
        {
            var mqttFactory = new MqttClientFactory();   
            mqttClient = mqttFactory.CreateMqttClient();  //mqttClient 변수를 통해 MQTT 브로커와 통신할 수 있습니다.

            //MQTT 클라이언트 접속 설정
            var mqttClientOptions = new MqttClientOptionsBuilder()
                                    .WithTcpServer(brokerHost,1883)   //mqtt 포트번호
                                    .WithCleanSession(true)
                                    .Build();


            //matt 접속 후 이벤트 처리
            mqttClient.ConnectedAsync += async e =>
            {
                LogText += "MQTT Broker 연결성공\n";
            };

            
            await mqttClient.ConnectAsync(mqttClientOptions);

            ////테스트 메시지
            //var message = new MqttApplicationMessageBuilder()
            //            .WithTopic(topic)
            //            .WithPayload("HELLO FROM IOT Simulator")
            //            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
            //            .Build();

            ////발행
            //await mqttClient.PublishAsync(message);

            //LogText = "MQTT브로커에 초기메시지 전송";


        }


        public event Action? StartHmiRequested;
        public event Action? StartSensorCheckRequested;   //viewModel에서 View에 있는 이벤트를 호출


        [RelayCommand]
        public void Move()
        {
            ProductBrush = Brushes.Gray;
            StartHmiRequested?.Invoke();        //컨베이어 벨트 애니메이션 요청(view에서 처리)

        }

        [RelayCommand]
        public void Check()
        {
            StartSensorCheckRequested?.Invoke();

            //양품불량품 판단
            Random rand = new Random();
            int result = rand.Next(1, 3); //1~3 중 하나 선별

            ProductBrush = result switch
            {
                1 => Brushes.Green,        //양품
                2 => Brushes.Crimson,       //불량품
                _ => Brushes.Aqua,          //default
            };

            //MQTT로 데이터 전송
            var resultText = result == 1 ? "OK" : "FAIL";
            var payload = new CheckResult { ClientId = clientId, Result = resultText, TimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
            var jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
            var message = new MqttApplicationMessageBuilder()
                        .WithTopic(topic)
                        .WithPayload(jsonPayload)
                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
                        .Build();

            //발행
            mqttClient.PublishAsync(message);
            LogText = $"MQTT브로커에 양품불량품 판단 메시지 전송 : {logNum++}";
        }



    }
}
