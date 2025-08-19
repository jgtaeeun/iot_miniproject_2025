using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Standard;
using MahApps.Metro.Controls.Dialogs;
using MQTTnet;
using MySqlConnector;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Schema;
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
        private string subTopic;
        private string pubTopic;


       
        // -------------------MQTT 재접속용 변수
        private Timer _mqttMonitorTimer;
        private bool _isReconnecting = false;

        //------------------- 구독 데이터를 DB에 저장하기 위해, 
        private int tempPrcLoadTime =0;
        private string tempPrcCd = string.Empty;

        //--------------
        //현재의 schIdx와 선택한 게 같은지, 다른지 판단해서 다를 경우, amount을 0으로 초기화
        private int currSchIdx = 0;

        //현재의 schIdx와 선택한 게 같은지, 다른지 판단해서 다를 경우, search함수 날짜를 오늘로 초기화
        private int currSchIdx2 = 0;

        // search함수 쿼리의 날짜 선택
        private string SelectedDate = DateTime.Now.ToString("yyyy-MM-dd") + "%";
        private string maxDate = DateTime.Now.ToString("yyyy-MM-dd");
        private string minDate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");

        //search()함수가 최초 search인지 아니면 다음,이전버튼으로  search()인지에 따라 메시지 출력이 다르게 하기 위해서
        private bool isnotfirst = false;
        private bool isbuttonCommand = false;
        //-------------------
        public MonitoringViewModel(IDialogCoordinator coordinator)
        {
            this._dialogCoordinator = coordinator;
            ProductBrush = Brushes.Gray;
            SchIdx = 0;
            SucessAmount = FailAmount = 0;
            SuccessRate = string.Empty;

            //MQTT 설정
            brokerHost = "192.168.0.2";
            subTopic = "pknu/mes/Monitoring/CheckTrueFalse";   //iotsimulator에서 양품불량품 확인
            pubTopic = "pknu/mes/Monitoring/CheckSchId";      //mrs에서 schId선택되었는지 확인
            InitMqttClient();
            StartMqttMonitor();
        }

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
                                    .WithCleanSession(true) // 세션을 유지해서 구독 유지
                                    .Build();

            // 이벤트 핸들러는 한 번만 등록
            
             await mqttClient.ConnectAsync(mqttClientOptions);

             await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(subTopic).Build());
             Debug.WriteLine("MQTT 구독 완료");
            
            

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

                    if (data.Result == "FAIL")
                    {
                        FailAmount += 1;
                        ProductBrush = Brushes.Crimson;
                    }
                    else
                    {
                        SucessAmount += 1;
                        ProductBrush = Brushes.Green;

                    }

                    SuccessRate = String.Format("{0:0.0}", (SucessAmount * 100.0 / (SucessAmount + FailAmount))) + " %";

                    using (var db = new IotDbContext())
                    {
                        var exists = db.Processes.Any(p => p.SchIdx == data.PIdx && p.PrcDate == data.TimeStamp);
                        if (!exists)
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
            isnotfirst = false;
            isbuttonCommand = false;
            search();

        }

        private async Task search()
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

            
            currSchIdx2 = SchIdx;


            if (isnotfirst)
            {
             
                await this._dialogCoordinator.ShowMessageAsync(this, "공정조회(2)", $"{SelectedDate.Replace("%", "")} 기준일 조회를 시작합니다");
           

            }
            else
            {
                isnotfirst = true;
                SelectedDate = DateTime.Now.ToString("yyyy-MM-dd") + "%";
                await this._dialogCoordinator.ShowMessageAsync(this, "공정조회(1)", "금일 조회를 시작합니다");
            }


              





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
                                and prcDate like @selDate";
                DataSet ds = new DataSet();

                using (MySqlConnection conn = new MySqlConnection(Common.CONNSTR))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@schIdx", SchIdx);
                    cmd.Parameters.AddWithValue("@selDate", SelectedDate);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);


                    adapter.Fill(ds, "Result");
                    //Debug.WriteLine(ds.Tables["Result"].Rows.Count);   //1       schIdx가 pk이니 1행만 나올것이다.
                    //Debug.WriteLine(ds.Tables["Result"].Rows[0]);  //itemArray에 보면 데이터가 담겨져있다.

                    if (ds.Tables["Result"].Rows.Count != 0)
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
                       
                            if (isbuttonCommand)
                            {
                                await this._dialogCoordinator.ShowMessageAsync(this, "공정조회(2)", $"{SelectedDate.Replace("%", "")} 기준일 공정기록이 없습니다.");
                            }
                            else
                            {
                                await this._dialogCoordinator.ShowMessageAsync(this, "공정조회(1)", "해당 공정이 없습니다.");
                            }
                            
                     
                        


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
                await this._dialogCoordinator.ShowMessageAsync(this, "오류", ex.Message);
            }

            return;
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
            //SucessAmount = FailAmount = 0;
            SuccessRate = string.Empty;
            tempPrcLoadTime = 0;
            tempPrcCd = string.Empty;

            if (SchIdx == 0)
            {
                await this._dialogCoordinator.ShowMessageAsync(this, "알림", "순번을 입력하세요");
                return;
            }
           
            if (currSchIdx < SchIdx ||  currSchIdx > SchIdx)
            {
                SucessAmount = FailAmount = 0;
            }
            currSchIdx = SchIdx;
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


            try
            { 

            await this._dialogCoordinator.ShowMessageAsync(this, "공정시작", "공정을 시작합니다");
            if (tempPrcCd == string.Empty || tempPrcLoadTime <=0)
            {
                await this._dialogCoordinator.ShowMessageAsync(this, "공정조회", "해당 공정이 없습니다.");
                return;
            }

      
            
            
            
            //MQTT로 데이터 전송
           
            var payload = new CheckResult { PIdx = SchIdx, Result = "CheckSchId", TimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") , PlantCode = tempPrcCd, LoadTime = tempPrcLoadTime };
            var jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
            var message = new MqttApplicationMessageBuilder()
                        .WithTopic(pubTopic)
                        .WithPayload(jsonPayload)
                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
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
                var options = new MqttClientOptionsBuilder()
                        .WithTcpServer(brokerHost, 1883)
                        .WithCleanSession(true)
                        .Build();

                await mqttClient.ConnectAsync(options); // 재접속
                }

            ProductBrush = Brushes.Gray;
            StartHmiRequested?.Invoke();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }


        [RelayCommand]
        public async void ForwardDate()
        {
           if (SchIdx ==0) 
           {
                await this._dialogCoordinator.ShowMessageAsync(this, "알림", "순번을 선택하세요");
                return;
           }

            if (!isnotfirst || currSchIdx2 > SchIdx || currSchIdx2 < SchIdx)
            {
                await this._dialogCoordinator.ShowMessageAsync(this, "알림", "search버튼을 선택하세요");
                return;
            }


            DateTime tmpSelectedDate = Convert.ToDateTime(SelectedDate.Replace("%", ""));
            string tempSelectedDate = tmpSelectedDate.AddDays(1).ToString("yyyy-MM-dd");
            string tempMaxDate = maxDate;
            if (string.Compare(tempSelectedDate, tempMaxDate)   > 0 )
            {   await this._dialogCoordinator.ShowMessageAsync(this, "알림", "오늘 이후 데이터는 존재하지 않습니다.");
                return ;
            }
            SelectedDate = tempSelectedDate + "%";
            isbuttonCommand = true;
            search();
        }

        [RelayCommand]
        public async void BackwardDate()
        {
            if (SchIdx == 0)
            {
                await this._dialogCoordinator.ShowMessageAsync(this, "알림", "순번을 선택하세요");
                return;
            }

            if (!isnotfirst || currSchIdx2 > SchIdx || currSchIdx2 < SchIdx)
            {
                await this._dialogCoordinator.ShowMessageAsync(this, "알림", "search버튼을 선택하세요");
                return;
            }

            DateTime tmpSelectedDate = Convert.ToDateTime(SelectedDate.Replace("%", ""));
            string tempSelectedDate = tmpSelectedDate.AddDays(-1).ToString("yyyy-MM-dd");
            string tempMinDate = minDate;
            if (string.Compare(tempSelectedDate, tempMinDate) <0)
            {
                await this._dialogCoordinator.ShowMessageAsync(this, "알림", "더이상 과거의 데이터는 존재하지 않습니다.");
                return;
            }
            SelectedDate = tempSelectedDate + "%";
            isbuttonCommand = true;
            search();
        }
    }
}
