using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MySqlConnector;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using WpfMrpSimulatorApp.Helpers;
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
        public ObservableCollection<ScheduleNew> Schedules
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



        //플랜트코드 콤보박스
        private ObservableCollection<PlantGroup> _plantList;
        public ObservableCollection<PlantGroup> PlantList
        {
            get => _plantList;
            set => SetProperty(ref _plantList, value);

        }

        //설비코드 콤보박스
        private ObservableCollection<string> _schFacilityIdList;
        public ObservableCollection<string> SchFacilityIdList
        {
            get => _schFacilityIdList;
            set => SetProperty(ref _schFacilityIdList, value);

        }
       
        //시작시간, 종료시간 속성
        public ObservableCollection<TimeOption> TimeOptions
        {
            get;
        }
            = new ObservableCollection<TimeOption>(
                Enumerable.Range(0, 48).Select(i => new TimeOption
                {
                    Time = new TimeOnly(i / 2, (i % 2) * 30),
                    Display = $"{i / 2:00}:{(i % 2) * 30:00}"
                })
            );


        //입력을 위한 활성화 여부
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


        public ScheduleViewModel(IDialogCoordinator coordinator)
        {
            this._dialogCoordinator = coordinator;
            this._dbContext = new IotDbContext();

            InitializeAsync(); // ✅ 비동기 초기화 호출

            //신규버튼이나 선택한 SelectedSchedule가 있어야만 폼 작성 가능
            IsUpdate = false;
            
            //최초에는 저장버튼, 삭제버튼이 비활성화 =>SelectedSchedule가 있을 경우 버튼을 활성화
            CanSave = CanRemove = false;
        }

        // 비동기 초기화 메서드
        private async void InitializeAsync()
        {
            await LoadGridFromDb(); // ✅ 실제 비동기 로딩
            await InitializeInfo();

        }
        private async Task LoadGridFromDb()
        {

            try
            {
                using (var db = new IotDbContext())
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

                                    SchFacilityId = sch.SchFacilityId,

                                    RegDt = sch.RegDt,
                                    ModDt = sch.ModDt,
                                })
                                ;




                    Schedules = new ObservableCollection<ScheduleNew>(results);


                }


            }
            catch (Exception ex)
            {
                await this._dialogCoordinator.ShowMessageAsync(this, "오류", ex.Message);
            }

        }

        private async Task InitializeInfo()
        {
            //settings 테이블의 데이터 가져오기
            var i = _dbContext.Settings.ToList();

            //플랜트코드 콤보박스
            var plantGroups = i
                             .GroupBy(x => x.BasicCode)
                             .Select(g => new PlantGroup
                             {
                                 BasicCode = g.Key,
                                 CodeDesc = g.First().CodeDesc     // 마찬가지로 CodeDesc 하나
                             })
                            .OrderBy(pg => pg.CodeDesc)  // CodeDesc 기준 오름차순 정렬
                             .ToList();


            PlantList = new ObservableCollection<PlantGroup>(plantGroups);

            //공장 세부번호

            SchFacilityIdList = new ObservableCollection<string>
            {
                "설비" , "공정"
            };




        }

        private void InitVariable()
        {
            SelectedSchedule = new ScheduleNew();
            IsUpdate = true;
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

        {
            if (SelectedSchedule == null)
            {
                await this._dialogCoordinator.ShowMessageAsync(this, "알림", "삭제대상 선택하세요!!", MessageDialogStyle.Affirmative);
                return;
            }



            var result = await this._dialogCoordinator.ShowMessageAsync(this, "삭제확인", "삭제하시겠습니까?", MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Affirmative)
            {
                try
                {

                    //db 삭제 쿼리
                    using (var db = new IotDbContext())
                    {
                        var entity = db.Schedules.Find(SelectedSchedule.SchIdx);
                        if (entity != null)
                        {
                            db.Schedules.Remove(entity);
                            db.SaveChanges();
                            await this._dialogCoordinator.ShowMessageAsync(this, "삭제", "데이터가 삭제되었습니다.");
                        }

                    }
                }
                catch (Exception ex)
                {
                    await this._dialogCoordinator.ShowMessageAsync(this, "오류",ex.Message);
                }

            }
            else
            {

                return;
            }
              
               
            LoadGridFromDb();
            CanSave = CanRemove = false;
            IsUpdate = false;
            return;
        }


        [RelayCommand]
        public async Task Save()
        {
            if (SelectedSchedule.PlantCode == null || SelectedSchedule ==null)
            {
                await this._dialogCoordinator.ShowMessageAsync(this, "알림", "데이터 입력하세요!!", MessageDialogStyle.Affirmative);
                return;
            }

            try
            {
                using (var db = new IotDbContext())

                

                {
                   // insert , update분류 필요
                    var entity = db.Schedules.Find(SelectedSchedule.SchIdx);
                    if (entity != null)
                    {
                        //update
                        entity.PlantCode = SelectedSchedule.PlantCode;
                        entity.SchDate = SelectedSchedule.SchDate;
                        entity.LoadTime = SelectedSchedule.LoadTime;
                        entity.SchStartTime = SelectedSchedule.SchStartTime;
                        entity.SchEndTime = SelectedSchedule.SchEndTime;
                        entity.SchAmount = SelectedSchedule.SchAmount;
                        entity.SchFacilityId = SelectedSchedule.SchFacilityId;
                        entity.ModDt = DateTime.Now;
                        

                        if (entity.PlantCode != null)
                        {
                            db.SaveChanges();
                            await this._dialogCoordinator.ShowMessageAsync(this, "수정", "데이터가 수정되었습니다.");
                        }
                        else
                        {
                            await this._dialogCoordinator.ShowMessageAsync(this, "알림", "플랜트코드는 필수입니다.");
                        }
                       
                    }
                    else
                    {
                        //insert
                        var newSchedule = new Schedule
                        {
                            // 필요한 필드 값 채우기
                            PlantCode = SelectedSchedule.PlantCode,
                            SchDate = SelectedSchedule.SchDate,
                            LoadTime = SelectedSchedule.LoadTime,
                            SchStartTime = SelectedSchedule.SchStartTime,
                            SchEndTime = SelectedSchedule.SchEndTime,
                            SchAmount = SelectedSchedule.SchAmount,
                            SchFacilityId = SelectedSchedule.SchFacilityId,
                            RegDt = DateTime.Now



                        };

                        if (newSchedule.PlantCode != null)
                        {
                            db.Schedules.Add(newSchedule);
                            db.SaveChanges();

                            await this._dialogCoordinator.ShowMessageAsync(this, "저장", "데이터가 저장되었습니다.");

                        }
                        else
                        {
                            await this._dialogCoordinator.ShowMessageAsync(this, "알림", "플랜트코드는 필수입니다.");
                        }

                    }



                }
            }
            catch (Exception ex)
            {
                await this._dialogCoordinator.ShowMessageAsync(this, "오류", ex.Message);

            }




            LoadGridFromDb();
            SelectedSchedule = new ScheduleNew();
            CanSave = CanRemove = false;
            IsUpdate = false;
            return;
        }

    }
}
