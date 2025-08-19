using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google.Protobuf.WellKnownTypes;
using MahApps.Metro.Controls.Dialogs;
using MQTTnet;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using NLog.LayoutRenderers.Wrappers;
using Org.BouncyCastle.Tls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using WpfMqttSubApp.Helpers;
using WpfMqttSubApp.Models;
using ZstdSharp.Unsafe;

namespace WpfMqttSubApp.ViewModels
{
    public partial class MainViewModel : ObservableObject , IDisposable
    {
       //강사pc에서 센서데이터 가져오기 위한 topic
        private readonly string TOPIC;

        //타이머
        private readonly DispatcherTimer _timer;
       
        //디버깅 확인 위한 테스트값
        private int LineCounter = 1; 

        //mqtt
        private IMqttClient _mqttClient;

        //MahApps.Metro 다이얼로그 코디네이터
        private readonly IDialogCoordinator dialogCoordinator;

        //db주소
        private string _dbHost;
        public string DBHost
        {
            get => _dbHost;
            set => SetProperty(ref _dbHost, value);
        }

        //mqtt 주소
        private string _brokerHost;
        public string BrokerHost
        {
            get => _brokerHost;
            set => SetProperty(ref _brokerHost, value);
        }

        //richboxtext에 값
        private string _logText;
        public string LogText
        {
            get => _logText;
            set => SetProperty(ref _logText, value);    
        }

        //db연결 conString
        private string _connString = string.Empty;
        private MySqlConnection connection;


        public MainViewModel(IDialogCoordinator coordiantor)
        {
            this.dialogCoordinator = coordiantor;
            BrokerHost = App.configuration.Mqtt.Broker;
            DBHost = App.configuration.Database.Server;
            connection = new MySqlConnection();
            TOPIC = App.configuration.Mqtt.Topic;

        }

        [RelayCommand]
        public async Task ConnectMqtt ()
        {
            if (string.IsNullOrEmpty(BrokerHost))
            {
                await this.dialogCoordinator.ShowMessageAsync(this, "브로커연결합니다.", "브로커연결주소없음");
                return;
            }

            //mqtt 브로커에 접속해서 데이터를 가져오기
            ConnectMqttBroker();
        }

        private async Task ConnectMqttBroker()
        {   //mqtt 클라이언트 생성
            var mqttFactory = new MqttClientFactory();
            _mqttClient = mqttFactory.CreateMqttClient();

            //matt 클라이언트 접속 설정
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(BrokerHost, App.configuration.Mqtt.Port)
                .WithClientId(App.configuration.Mqtt.ClientId)
                .WithCleanSession(true)
                .Build();

            //matt 접속 후 이벤트 처리
            _mqttClient.ConnectedAsync += async e =>
            {
                LogText += "MQTT Broker 연결성공\n";

                //연결이후 구독(subscribe)
                await _mqttClient.SubscribeAsync(TOPIC);
            };

            _mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = e.ApplicationMessage.ConvertPayloadToString(); //byte데이터를 utf-8문자열로 변환
            
                //json으로 변경하여 db에저장하기 위한 과정
                var data = JsonConvert.DeserializeObject<CheckResult>(payload);
                SaveSensingData(data);

                //richtextbox에 데이터 띄우기
                LogText += $"LineNumber:{LineCounter++}\n";
                LogText += $"{payload}\n";

                return Task.CompletedTask;
            };

            await _mqttClient.ConnectAsync(mqttClientOptions);
        }


        private async Task SaveSensingData(CheckResult data)
        {
            string query = "INSERT INTO process(schIdx,prcCd,prcDate,prcLoadTime,prcStartTime,prcEndTime,prcFacilityId,prcResult,regDt) VALUES (@schIdx,@prcCd,@prcDate,@prcLoadTime,@prcStartTime,@prcEndTime,@prcFacilityId,@prcResult,@regDt)";
            Debug.WriteLine(connection.State);
            Debug.WriteLine(System.Data.ConnectionState.Open);
           
            try
            {
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    using var cmd = new MySqlCommand(query, connection);
                   // cmd.Parameters.AddWithValue("@schIdx", data.L);
                    cmd.Parameters.AddWithValue("@prcCd", "MesMqttSub01");
                    cmd.Parameters.AddWithValue("@prcDate", data.TimeStamp);
                    cmd.Parameters.AddWithValue("@prcLoadTime", 2);
                    cmd.Parameters.AddWithValue("@prcStartTime", DateTime.Parse(data.TimeStamp).AddSeconds(-2).ToString("HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@prcEndTime", DateTime.Parse(data.TimeStamp).ToString("HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@prcFacilityId","IOT01");
                    cmd.Parameters.AddWithValue("@prcResult", data.Result == "FALI" ? false: true);
                    cmd.Parameters.AddWithValue("@regDt", data.TimeStamp);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"MySQL 오류: {ex.Message}");
            }


        }

        [RelayCommand]
        public async Task ConnectDB()
        {
            if (string.IsNullOrEmpty(DBHost))
            {
                await this.dialogCoordinator.ShowMessageAsync(this, "db연결합니다.", "db연결실패");
                return;
            }
            _connString = Common.CONNSTR;
            await ConnectDatabaseServer();
          
        }

        private async Task ConnectDatabaseServer()
        {
            try
            {
                connection = new MySqlConnection(_connString);
                connection.Open();
                LogText += $"{DBHost} DB서버 접속 성공 ! {connection.State}\n";

               //db에서 데이터 불러오기
            }
            catch (Exception ex)
            {
                LogText += $"{DBHost} DB서버 접속 실패 :{ex.Message}\n";
            }
        }


        public void Dispose()
        {   //리소스 해제 
            connection?.Dispose();
        }
    }
}
