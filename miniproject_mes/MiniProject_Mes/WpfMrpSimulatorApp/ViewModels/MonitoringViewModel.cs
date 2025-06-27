using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Standard;
using MahApps.Metro.Controls.Dialogs;
using MQTTnet;
using MySqlConnector;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Windows.Media;
using WpfMrpSimulatorApp.Helpers;
using WpfMrpSimulatorApp.Models;

namespace WpfMrpSimulatorApp.ViewModels
{
    public partial class MonitoringViewModel : ObservableObject
    {   //메시지박스
        private IDialogCoordinator _dialogCoordinator;


        private readonly Random rand = new Random();  // 클래스 필드로 선언

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
        private bool isSubscribed = false;

        //-------------------



        //------------------- 구독 데이터를 DB에 저장하기 위해, 
        private int tempPrcLoadTime =0;
        private string tempPrcCd = string.Empty;
       
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
                                    .WithCleanSession(false) // 세션을 유지해서 구독 유지
                                    .Build();

            // 이벤트 핸들러는 한 번만 등록
            mqttClient.ApplicationMessageReceivedAsync += HandleReceivedMessageAsync;
            mqttClient.DisconnectedAsync += async e =>
            {
                Debug.WriteLine("MQTT 연결 끊김, 재접속 시도 중...");
                await Task.Delay(5000);

                try
                {
                    await mqttClient.ConnectAsync(mqttClientOptions);
                    await mqttClient.SubscribeAsync(topic);
                    Debug.WriteLine("MQTT 재접속 성공 및 구독 완료");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"MQTT 재접속 실패: {ex.Message}");
                }
            };
            try
            {
                await mqttClient.ConnectAsync(mqttClientOptions);

                if (!isSubscribed)
                {
                    await mqttClient.SubscribeAsync(topic);
                    isSubscribed = true;
                    Debug.WriteLine("MQTT 구독 완료");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MQTT 연결 실패: {ex.Message}");
            }
        }

        private async Task HandleReceivedMessageAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            var payload = e.ApplicationMessage.ConvertPayloadToString();
            Debug.WriteLine($"수신됨: {payload}");

            try
            {
                var data = JsonConvert.DeserializeObject<CheckResult>(payload);
                if (data != null)
                {
                    using (var db = new IotDbContext())
                    {
                        db.Processes.Add(new Models.Process
                        {
                            SchIdx = data.PIdx,
                            PrcDate = Convert.ToString(data.TimeStamp),
                            PrcResult = (sbyte?)(data.Result == "FAIL" ? 0 : 1),
                            PrcLoadTime = data.LoadTime,
                            PrcCd = data.PlantCode
                        });
                        await db.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"JSON 파싱 또는 DB 저장 오류: {ex.Message}");
            }
        }

        //---------------- db연동 함수
        [RelayCommand]
        public async Task SearchProcess()
        {
            PlantName = string.Empty;
            PrcDate = string.Empty;
            PrcLoadTime = string.Empty;
            PrcCodeDesc = string.Empty;
            SchAmount = 0;
            SucessAmount = FailAmount = 0;
            SuccessRate = string.Empty;

            if (SchIdx == 0)
            {
                await this._dialogCoordinator.ShowMessageAsync(this, "알림", "순번을 입력하세요");
                return;
            }


            await this._dialogCoordinator.ShowMessageAsync(this, "공정조회", "조회를 시작합니다");




            try
            {
                string query = @" SELECT sch.schIdx, sch.plantCode ,set1.codeName AS plantName,  
                                pr.prcDate AS prcDate, pr.prcLoadTime AS prcLoadTime, 
                                set1.codeDesc AS prcCodeDesc,  sch.schAmount AS prcAmount, pr.prcResult
                                FROM schedules AS sch
                                JOIN settings AS set1 
                                ON sch.plantCode = set1.BasicCode
                                JOIN process As pr
                                ON sch.schIdx = pr.schIdx
                                WHERE sch.schIdx = @schIdx
                                ORDER BY prcDate";
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

                        for (int i = 0; i < ds.Tables["Result"].Rows.Count; i++)
                        {
                            if (Convert.ToBoolean(ds.Tables["Result"].Rows[i]["prcResult"]))
                            {
                                SucessAmount++;
                               
                            }
                            else
                            {
                                FailAmount++;
                            }
                        
                        }
                        int total = SucessAmount + FailAmount;
                        SuccessRate = total > 0 ? $"{(SucessAmount * 100.0 / total):F1}%" : string.Empty;

                    }
                    else
                    {
                        await this._dialogCoordinator.ShowMessageAsync(this, "공정조회", "해당 공정이 없습니다.");
                        PlantName = string.Empty;
                        PrcDate = string.Empty;
                        PrcLoadTime = string.Empty;
                        PrcCodeDesc = string.Empty;
                        SchAmount = 0;
                        SucessAmount = FailAmount = 0;
                        SuccessRate = string.Empty;
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
            PlantName = string.Empty;
            PrcDate = string.Empty;
            PrcLoadTime = string.Empty;
            PrcCodeDesc = string.Empty;
            SchAmount = 0;
            SucessAmount = FailAmount = 0;
            SuccessRate = string.Empty;
            tempPrcLoadTime = 0;
            tempPrcCd = string.Empty;

            if (SchIdx == 0)
            {
                await this._dialogCoordinator.ShowMessageAsync(this, "알림", "순번을 입력하세요");
                return;
            }

            //db에서 선택된 schIdx의 기본정보 가져옴
            try
            {
                string query = "SELECT plantCode, loadTime FROM schedules where schIdx = @schidx";

               
                using (MySqlConnection conn = new MySqlConnection(Common.CONNSTR))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@schidx", SchIdx);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                         tempPrcLoadTime= reader.GetInt32("loadTime");
                         tempPrcCd = reader.GetString("plantCode"); 

                    }

                }

             }
            catch (Exception ex)
            {
                await this._dialogCoordinator.ShowMessageAsync(this, "오류", ex.Message);
            }

            await this._dialogCoordinator.ShowMessageAsync(this, "공정시작", "공정을 시작합니다");
            if (tempPrcCd == string.Empty || tempPrcLoadTime <=0)
            {
                await this._dialogCoordinator.ShowMessageAsync(this, "공정조회", "해당 공정이 없습니다.");
                return;
            }

            
            ProductBrush = Brushes.Gray;
            StartHmiRequested?.Invoke();

            await Task.Delay(3000);

            StartSensorCheckRequested?.Invoke();

            //양품불량품 판단
           
            int result = rand.Next(1, 3); //1~3 중 하나 선별

            ProductBrush = result switch
            {
                1 => Brushes.Green,        //양품
                2 => Brushes.Crimson,       //불량품
                _ => Brushes.Aqua,          //default
            };



            //MQTT로 데이터 전송
            var resultText = result == 1 ? "OK" : "FAIL";
            var payload = new CheckResult { PIdx = SchIdx, Result = resultText, TimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") , PlantCode = tempPrcCd, LoadTime = tempPrcLoadTime };
            var jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
            var message = new MqttApplicationMessageBuilder()
                        .WithTopic(topic)
                        .WithPayload(jsonPayload)
                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
                        .Build();

            //발행
            if (mqttClient.IsConnected)
            {
                await mqttClient.PublishAsync(message);
                Debug.WriteLine("MQTT 데이터 발행 완료");
            }
            else
            {
                Debug.WriteLine("MQTT 클라이언트가 연결되어 있지 않습니다.");
            }




        }
    }
}
