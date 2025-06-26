using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Standard;
using MahApps.Metro.Controls.Dialogs;
using MQTTnet;
using MySqlConnector;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;
using System.Windows.Media;
using WpfMrpSimulatorApp.Helpers;
using WpfMrpSimulatorApp.Models;

namespace WpfMrpSimulatorApp.ViewModels
{
    public partial class MonitoringViewModel : ObservableObject
    {   //메시지박스
        private IDialogCoordinator _dialogCoordinator;

        // -------------------애니메이션 관련

        //색상표시할 변수
        private Brush _productBrush;
        public Brush ProductBrush
        {
            get => _productBrush;
            set => SetProperty(ref _productBrush, value);
        }
        //viewModel에서 View에 있는 이벤트를 호출
        public event Action? StartHmiRequested;
        public event Action? StartSensorCheckRequested;
        // -------------------

        //------------groupbox데이터 속성
        private string _plantName;
        public string PlantName
        {
            get => _plantName;
            set =>SetProperty(ref _plantName, value);
        }

        private string _prcDate;
        public string PrcDate
        {
            get => _prcDate;
            set =>SetProperty(ref _prcDate, value); 
        }

        private string _prcLoadTime;
        public string PrcLoadTime
        {
            get => _prcLoadTime;
            set => SetProperty(ref _prcLoadTime, value);
        }

        private string _prcCodeDesc;
        public string PrcCodeDesc
        {
            get => _prcCodeDesc;
            set => SetProperty(ref _prcCodeDesc, value);
        }

        private int _schAmount;
        public int SchAmount
        {
            get => _schAmount;
            set => SetProperty(ref _schAmount, value);
        }

        private int _sucessAmount;
        public int SucessAmount
        {
            get => _sucessAmount;
            set => SetProperty(ref _sucessAmount, value);
        }
        private int _failAmount;
        public int FailAmount
        {
            get => _failAmount;
            set => SetProperty(ref _failAmount, value);
        }

        private string _successRate;
        public string SuccessRate
        {
            get => _successRate;
            set => SetProperty(ref _successRate, value);
        }


        private int _schIdx;
        public int  SchIdx
        {
            get => _schIdx;
            set => SetProperty(ref _schIdx, value);
        }

        //-------------------

        // ------------------- mqtt 관련 변수
        private IMqttClient mqttClient;
        private string brokerHost;
        private string topic;
        private string clientId;

      
        //-------------------

        public MonitoringViewModel(IDialogCoordinator coordinator)
        {
            this._dialogCoordinator = coordinator;
            ProductBrush = Brushes.Gray;
            SchIdx = 0;
            SucessAmount = FailAmount = 0;
            SuccessRate = string.Empty;

            //MQTT 설정
            brokerHost = "210.119.12.110";
            topic = "pknu/mes/Monitoring/data";
            InitMqttClient();
        }


        // MQTT 클라이언트를 초기화
        private async void InitMqttClient()
        {
            var mqttFactory = new MqttClientFactory();
            mqttClient = mqttFactory.CreateMqttClient();  //mqttClient 변수를 통해 MQTT 브로커와 통신할 수 있습니다.

            //MQTT 클라이언트 접속 설정
            var mqttClientOptions = new MqttClientOptionsBuilder()
                                    .WithTcpServer(brokerHost, 1883)   //mqtt 포트번호
                                    .WithCleanSession(true)
                                    .Build();

            await mqttClient.ConnectAsync(mqttClientOptions);

            await mqttClient.SubscribeAsync(topic);
        
            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = e.ApplicationMessage.ConvertPayloadToString(); //byte데이터를 utf-8문자열로 변환

                
                try
                {
                    var data = JsonConvert.DeserializeObject<CheckResult>(payload);
                    
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"JSON 파싱 오류: {ex.Message}");
                }

                return Task.CompletedTask;
            };

            

        }


        //---------------- db연동 함수
        [RelayCommand]
        public async Task SearchProcess()
        {
            if (SchIdx == 0)
            {
                await this._dialogCoordinator.ShowMessageAsync(this, "알림", "순번을 입력하세요");
                return;
            }

            
            await this._dialogCoordinator.ShowMessageAsync(this, "공정조회", "조회를 시작합니다");
            try
            {
                string query = @" SELECT sch.schIdx, sch.plantCode ,set1.codeName AS plantName,   sch.schDate AS prcDate, sch.loadTime AS prcLoadTime, 
                                set1.codeDesc AS prcCodeDesc,  sch.schAmount AS prcAmount
                                FROM schedules AS sch
                                JOIN settings AS set1 
                                ON sch.plantCode = set1.BasicCode
                                WHERE sch.schIdx = @schIdx";
                DataSet ds = new DataSet();

                using (MySqlConnection conn= new MySqlConnection(Common.CONNSTR))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@schIdx",SchIdx);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                   

                    adapter.Fill(ds,"Result");
                    //Debug.WriteLine(ds.Tables["Result"].Rows.Count);   //1       schIdx가 pk이니 1행만 나올것이다.
                    //Debug.WriteLine(ds.Tables["Result"].Rows[0]);  //itemArray에 보면 데이터가 담겨져있다.

                    if (ds.Tables["Result"].Rows.Count !=0)
                    {
                        DataRow row = ds.Tables["Result"].Rows[0];
                        PlantName = row["plantName"].ToString();
                        PrcDate = Convert.ToDateTime(row["prcDate"]).ToString("yyyy-MM-dd");
                        PrcLoadTime = row["prcLoadTime"].ToString();
                        PrcCodeDesc = row["prcCodeDesc"].ToString();
                        SchAmount = Convert.ToInt32(row["prcAmount"]);
                       // SucessAmount = FailAmount = 0;
                        //SuccessRate = "0.0%";

                    }
                    else
                    {
                        await this._dialogCoordinator.ShowMessageAsync(this, "공정조회", "해당 공정이 없습니다.");
                        PlantName = string.Empty;
                        PrcDate = string.Empty;
                        PrcLoadTime = string.Empty;
                        PrcCodeDesc = string.Empty;
                        SchAmount = 0;
                        //SucessAmount = FailAmount = 0;
                        //SuccessRate = string.Empty;
                        return;

                    }
                }
            }
            catch (Exception ex)
            {
                await this._dialogCoordinator.ShowMessageAsync(this, "오류", ex.Message );
            }

        }
        //----------------




        //---------------- 애니메이션 호출함수
        [RelayCommand]
        public async Task StartProcess()
        {
            if (SchIdx == 0)
            {
                await this._dialogCoordinator.ShowMessageAsync(this, "알림", "순번을 입력하세요");
                return;
            }

            await this._dialogCoordinator.ShowMessageAsync(this, "공정시작", "공정을 시작합니다");
            ProductBrush = Brushes.Gray;
            StartHmiRequested?.Invoke();

            await Task.Delay(3000);

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
            var payload = new CheckResult { PIdx =SchIdx, Result = resultText, TimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
            var jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
            var message = new MqttApplicationMessageBuilder()
                        .WithTopic(topic)
                        .WithPayload(jsonPayload)
                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
                        .Build();

            //발행
            await mqttClient.PublishAsync(message);
        }

        //----------------
    }
}
