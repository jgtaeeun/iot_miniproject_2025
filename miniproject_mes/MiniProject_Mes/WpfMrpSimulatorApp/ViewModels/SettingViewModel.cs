using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using MySqlConnector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using WpfMrpSimulatorApp.Helpers;
using WpfMrpSimulatorApp.Models;

namespace WpfMrpSimulatorApp.ViewModels
{
    public partial class SettingViewModel : ObservableObject
    {
        private readonly IDialogCoordinator _dialogCoordinator;


        //db연동
        private string _basicCode;
        public string BasicCode
        {
            get => _basicCode;
            set => SetProperty(ref _basicCode, value);
        }

        private string _codeName;
        public string CodeName
        {
            get => _codeName;
            set => SetProperty(ref _codeName, value);
        }

        private string? _codeDesc;
        public string? CodeDesc
        {
            get => _codeDesc;
            set => SetProperty(ref _codeDesc, value);
        }


        private DateTime? _reDt;
        public DateTime? ReDt
        {
            get => _reDt;
            set => SetProperty(ref _reDt, value);
        }

        private DateTime? _modDt;
        public DateTime? ModDt
        {
            get => _modDt;
            set => SetProperty(ref _modDt, value);
        }

        //dataGrid Setting 데이터
        private ObservableCollection<Setting> _settings;
        public ObservableCollection<Setting> Settings
        {
            get=> _settings;
            set => SetProperty(ref _settings, value);
        }

        // 선택한 Setting 데이터
        private Setting _selectedSetting;
        public Setting SelectedSetting
        {
            get => _selectedSetting;
            set {
                SetProperty(ref _selectedSetting, value);
                
                if (SelectedSetting != null)
                {
                    if(!string.IsNullOrEmpty(SelectedSetting.BasicCode))
                    {
                        IsUpdate = true;
                        CanSave= CanRemove = true;
                    }
                }
            }
              
        }


        //업데이트인지 최초저장인지 여부 
        private Boolean _isUpdate;
        public Boolean IsUpdate
        {
            get => _isUpdate;
            set =>SetProperty(ref _isUpdate, value);
        }

        //저장, 삭제버튼 활성화 여부
        private Boolean _canSave;
        public Boolean CanSave
        {
            get => _canSave;
            set => SetProperty(ref _canSave, value);
        }

        private Boolean _canRemove;
        public Boolean CanRemove
        {
            get => _canRemove;
            set => SetProperty(ref _canRemove, value);
        }


        public SettingViewModel(IDialogCoordinator coordinator)
        {
            this._dialogCoordinator = coordinator;
            LoadSettingData();
            IsUpdate = true;

            //최초에는 저장버튼, 삭제버튼이 비활성화 =>SelectedSettings가 있을 경우 버튼을 활성화
            CanSave = CanRemove = false;
        }

        private async void LoadSettingData()
        {
            try
            {
                string query = @"SELECT basicCode,codeName,codeDesc,reDt,modDt FROM settings";
                ObservableCollection<Setting> settings = new ObservableCollection<Setting>();

                //db연동 방식 1
                using (MySqlConnection conn = new MySqlConnection(Common.CONNSTR))
                {
                    conn.Open();    
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();   

                    while (reader.Read())
                    {
                        var basicCode = reader.GetString("basicCode");
                        var codeName = reader.GetString("codeName");
                        var codeDesc = reader.GetString("codeDesc");
                        var reDt = reader.GetDateTime("reDt");
                        var modDt =  reader.IsDBNull(reader.GetOrdinal("modDt"))    ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("modDt"));



                        settings.Add(new Setting
                        {
                            BasicCode = basicCode,
                            CodeName = codeName,
                            CodeDesc = codeDesc,
                            ReDt =  reDt,
                            ModDt =  modDt
                        });

                    }
                    Settings = settings; 

                }

            }
            catch (Exception ex) 
            {
                await this._dialogCoordinator.ShowMessageAsync(this, "오류",ex.Message);
            }
        }

        private void InitVariable()
        {
            SelectedSetting = new Setting();
            IsUpdate = false;
            CanSave = true;
            CanRemove = false; 
        }

        //버튼 이벤트
        [RelayCommand]
        public void New()
        {
            InitVariable();
        }

        [RelayCommand]
        public async Task Remove()

        {   if (SelectedSetting== null)
            {
                await this._dialogCoordinator.ShowMessageAsync(this, "알림", "삭제대상 선택하세요!!", MessageDialogStyle.Affirmative);
                return;
            }



            var result =  await this._dialogCoordinator.ShowMessageAsync(this, "삭제확인", "삭제하시겠습니까?", MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Affirmative)
            {
                //db 삭제 쿼리

                string query = "DELETE FROM settings WHERE basicCode=@Id";
                using (MySqlConnection conn = new MySqlConnection(Common.CONNSTR))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", SelectedSetting.BasicCode);

                    var resultCnt = cmd.ExecuteNonQuery();
                    if (resultCnt > 0)
                    {
                        await this._dialogCoordinator.ShowMessageAsync(this, "삭제", "삭제 성공");

                    }
                    else
                    {
                        await this._dialogCoordinator.ShowMessageAsync(this, "삭제", "삭제 실패");

                    }

                }

            }
            else
            {
               
                return;
            }
            LoadSettingData();
            CanSave = CanRemove = false;
            return;
        }


        [RelayCommand]
        public async Task Save()
        {
            try
            {
                string query = string.Empty;
                using (MySqlConnection conn = new MySqlConnection(Common.CONNSTR))
                {
                    conn.Open();
                    if (IsUpdate) query = "SET time_zone = 'Asia/Seoul';UPDATE settings SET codeName=@codeName , codeDesc=@codeDesc, modDt= now() WHERE basicCode=@basicCode"; //update 쿼리
                    else query = "INSERT INTO settings(basicCode, codeName, codeDesc, reDt) VALUES (@basicCode, @codeName, @codeDesc, CONVERT_TZ(NOW(), 'UTC', 'Asia/Seoul'))";    //insert 쿼리
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@basicCode", SelectedSetting.BasicCode);
                    cmd.Parameters.AddWithValue("@codeName", SelectedSetting.CodeName);
                    cmd.Parameters.AddWithValue("@codeDesc", SelectedSetting.CodeDesc);
                    var resultCnt = cmd.ExecuteNonQuery();
                    if (resultCnt > 0)
                    {
                        await this._dialogCoordinator.ShowMessageAsync(this, "저장", "저장 성공");

                    }
                    else
                    {
                        await this._dialogCoordinator.ShowMessageAsync(this, "저장", "저장 실패");

                    }

                }
            }
            catch (Exception ex) 
            {
                await this._dialogCoordinator.ShowMessageAsync(this, "오류", ex.Message);

            }
           



            LoadSettingData();
            // 💡 객체 자체를 초기화해서 UI가 인식하게 한다
            SelectedSetting = new Setting();
            IsUpdate = true;
            CanSave = CanRemove = false;

        }
    }
}