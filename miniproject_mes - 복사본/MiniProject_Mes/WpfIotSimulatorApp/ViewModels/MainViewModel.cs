using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MQTTnet;
using MQTTnet.Adapter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using WpfIotSimulatorApp.Models;
using System.Windows; // Application 참조용

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

        // MQTT 재접속용 변수

        private Timer _mqttMonitorTimer;
        private bool _isReconnecting = false;



        // MQTT
        private IMqttClient mqttClient;
        private string brokerHost;
        private string pubTopic;
        private string subTopic;
        private int logNum = 1;

        //임시정보 저장하는 변수
        private string pcode;
        private int ploadtime;
        private int pid;

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
            pubTopic = "pknu/mes/Monitoring/CheckTrueFalse";   //iotsimulator에서 양품불량품 확인
            subTopic = "pknu/mes/Monitoring/CheckSchId";      //mrs에서 schId선택되었는지 확인
            InitMqttClient();
            StartMqttMonitor();
        }

    

        // 핵심. MQTTClient 접속이 끊어지면 재접속
        private void StartMqttMonitor()
        {
            _mqttMonitorTimer = new Timer(async _ =>
            {
                try
                {
                    await CheckMqttConnectionAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"MQTT 연결 모니터링 중 오류: {ex.Message}");
                }
            }, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10));

        }

        private async Task CheckMqttConnectionAsync()
        {
            if (!mqttClient.IsConnected && !_isReconnecting)
            {
                _isReconnecting = true;

                try
                {
                    var options = new MqttClientOptionsBuilder()
                                        .WithTcpServer(brokerHost, 1883)
                                        .WithCleanSession(true)
                                        .Build();
                    await mqttClient.ConnectAsync(options);
                    _isReconnecting = false;  // 성공 시 false로 변경
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"MQTT 재접속 실패 : {ex.Message}");
                    _isReconnecting = false;  // 실패 후에도 false로 변경해서 재시도 가능하도록
                }
            }
        }


        // MQTT 클라이언트를 초기화
        private async void InitMqttClient()
        {
            var mqttFactory = new MqttClientFactory();
            mqttClient = mqttFactory.CreateMqttClient();  //mqttClient 변수를 통해 MQTT 브로커와 통신할 수 있습니다.
            mqttClient.ApplicationMessageReceivedAsync += HandleReceivedMessageAsync;

            //MQTT 클라이언트 접속 설정
            var mqttClientOptions = new MqttClientOptionsBuilder()
                                    .WithTcpServer(brokerHost, 1883)   //mqtt 포트번호
                                    .WithCleanSession(true)
                                    .Build();




     


            await mqttClient.ConnectAsync(mqttClientOptions);
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                           .WithTopic(subTopic)
                            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
                           .Build());
            Debug.WriteLine("MQTT 구독 완료");





        }

        private  Task HandleReceivedMessageAsync(MqttApplicationMessageReceivedEventArgs e)
        {

            try
            {
                Debug.WriteLine($"HandleReceivedMessageAsync called - Topic: {e.ApplicationMessage.Topic}");
                var payload = e.ApplicationMessage.ConvertPayloadToString();
                Debug.WriteLine($"수신됨: {payload}");

                var data = JsonConvert.DeserializeObject<CheckResult>(payload);
                if (data.Result == "CheckSchId")
                {
                   
                        pid = data.PIdx;
                        pcode = data.PlantCode;
                        ploadtime = data.LoadTime;
                        Move();
                        Thread.Sleep(2200);
                        CheckAsync(); // UI 관련
                   

                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"JSON 파싱 또는 DB 저장 오류: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        public event Action? StartHmiRequested;
        public event Action? StartSensorCheckRequested;   //viewModel에서 View에 있는 이벤트를 호출


        [RelayCommand]
        public void Move()
        {
            ProductBrush = Brushes.Gray;

            Application.Current.Dispatcher.Invoke(() =>  // UI스레드와 VM스레드간 분리
            {
                StartHmiRequested?.Invoke();  // 컨베이어벨트 애니메이션 요청(View에서 처리)
            });


        }

        [RelayCommand]
        public void CheckAsync()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StartSensorCheckRequested?.Invoke();
            });

            //양품불량품 판단
            Random rand = new Random();
            int result = rand.Next(1, 3); //1~3 중 하나 선별

            ProductBrush = result switch
            {
                1 => Brushes.Green,        //양품
                2 => Brushes.Crimson,       //불량품
                _ => Brushes.Aqua,          //default
            };

            try
            {
                //MQTT로 데이터 전송
                var resultText = result == 1 ? "OK" : "FAIL";
                var payload = new CheckResult { PIdx = pid, Result = resultText, TimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), LoadTime = ploadtime, PlantCode = pcode };
                var jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
                var message = new MqttApplicationMessageBuilder()
                            .WithTopic(pubTopic)
                            .WithPayload(jsonPayload)
                            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
                            .Build();

                //발행
                mqttClient.PublishAsync(message);
                LogText = $"MQTT브로커에 양품불량품 판단 메시지 전송 : {logNum++}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }

    

    }
}