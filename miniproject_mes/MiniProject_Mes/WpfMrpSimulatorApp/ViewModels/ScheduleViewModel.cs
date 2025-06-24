using CommunityToolkit.Mvvm.ComponentModel;
using MahApps.Metro.Controls.Dialogs;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfMrpSimulatorApp.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WpfMrpSimulatorApp.ViewModels
{
    public partial class ScheduleViewModel : ObservableObject
    {
      

        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IotDbContext _dbContext;

        //dataGrid Schedules 데이터
        private ObservableCollection<ScheduleNew> _schedules;
        public ObservableCollection<ScheduleNew>  Schedules
        {
            get => _schedules;
            set => SetProperty(ref _schedules, value);
        }


        // 선택한 Schedule 데이터
        private ScheduleNew _selectedSchedule;
        public ScheduleNew SelectedSchedule
        {
            get => _selectedSchedule;
            set 
            {
                SetProperty(ref _selectedSchedule, value);
                if (SelectedSchedule != null)
                {
                        IsUpdate = true;
                        CanSave = CanRemove = true;
                    
                }
            }
        }

        //공장번호
        private ObservableCollection<string> _plantNumberList;
        public ObservableCollection< string> PlantNumberList
        {
            get => _plantNumberList;
            set =>SetProperty(ref _plantNumberList, value); 
        
        }


        //공장id
        private ObservableCollection<KeyValuePair<string, string>> _plantList;
        public ObservableCollection<KeyValuePair<string, string>> PlantList
        {
            get => _plantList;
            set => SetProperty(ref _plantList, value);

        }


        //업데이트인지 최초저장인지 여부 
        private Boolean _isUpdate;
        public Boolean IsUpdate
        {
            get => _isUpdate;
            set => SetProperty(ref _isUpdate, value);
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


        public ScheduleViewModel(IDialogCoordinator coordinator)
        {
            this._dialogCoordinator = coordinator;
            this._dbContext = new IotDbContext();
            PlantNumberList = new ObservableCollection<String>
            {
                "1공장", "2공장","3공장","4공장"
            };
            InitializeAsync(); // ✅ 비동기 초기화 호출

            IsUpdate = true;
            //최초에는 저장버튼, 삭제버튼이 비활성화 =>SelectedSchedule가 있을 경우 버튼을 활성화
            CanSave = CanRemove = false;
        }

        // 비동기 초기화 메서드
        private async void InitializeAsync()
        {
            await LoadGridFromDb(); // ✅ 실제 비동기 로딩

        }
        private async Task LoadGridFromDb()
        {
            
            try
            {   using (var db = new IotDbContext())
                {
                    var results = db.Schedules
                                .Join(db.Settings,
                                sch => sch.PlantCode,
                                setting => setting.BasicCode,
                                (sch, setting) => new ScheduleNew
                                {
                                        SchIdx = sch.SchIdx,
                                        PlantCode = sch.PlantCode,
                                        PlantName = setting.CodeName,  // 공장 이름 가져오기
                                        SchDate = sch.SchDate,
                                        LoadTime = sch.LoadTime,
                                        SchStartTime = sch.SchStartTime,
                                        SchEndTime = sch.SchEndTime,
                                        SchAmount = sch.SchAmount,

                                        SchFacilityId = setting.CodeDesc.Replace("(","").Replace(")","").Substring(4),

                                        RegDt = sch.RegDt,
                                        ModDt = sch.ModDt,
                                    })
                                ;

                   
                   

                    Schedules = new ObservableCollection<ScheduleNew>(results);


                    var distinctCodes = results
                                        .GroupBy(x => x.PlantCode)
                                        .Select(g => new KeyValuePair<string, string>(g.Key, g.First().PlantName));

                    ObservableCollection<KeyValuePair<string, string>> plantCodes = new ObservableCollection<KeyValuePair<string, string>>(distinctCodes);

                    PlantList = plantCodes;
                }
                
                
            }
            catch (Exception ex)
            {
                await this._dialogCoordinator.ShowMessageAsync(this, "오류", ex.Message);
            }

        }
    }
}
