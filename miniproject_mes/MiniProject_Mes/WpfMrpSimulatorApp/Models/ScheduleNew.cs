using System;
using System.Collections.Generic;

namespace WpfMrpSimulatorApp.Models;

public partial class ScheduleNew
{
    public int SchIdx { get; set; }

    //데이터 그리드에 넣기 위해서 속성 필요
    public string? PlantName { get; set; }   
    public string PlantCode { get; set; } = null!;

    /// <summary>
    /// 공정처리 계획일
    /// </summary>
    public DateOnly SchDate { get; set; }

    public int LoadTime { get; set; }

    public TimeOnly? SchStartTime { get; set; }

    public TimeOnly? SchEndTime { get; set; }

    public string? SchFacilityId { get; set; }

    public int? SchAmount { get; set; }

    public DateTime? RegDt { get; set; }

    public DateTime? ModDt { get; set; }

    public virtual ICollection<Process> Processes { get; set; } = new List<Process>();
}
