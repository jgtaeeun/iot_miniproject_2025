using System;
using System.Collections.Generic;

namespace WpfMrpSimulatorApp.Models;

public partial class Setting
{
    public string BasicCode { get; set; } = null!;

    /// <summary>
    /// 코드명
    /// </summary>
    public string CodeName { get; set; } = null!;

    /// <summary>
    /// 코드설명
    /// </summary>
    public string? CodeDesc { get; set; }

    /// <summary>
    /// 최초등록
    /// </summary>
    public DateTime? ReDt { get; set; }

    /// <summary>
    /// 수정일
    /// </summary>
    public DateTime? ModDt { get; set; }
}
