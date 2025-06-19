﻿using System;
using System.Collections.Generic;

namespace WpfMqttSubApp.Models;

public partial class Schedule
{
    public int SchIdx { get; set; }

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
