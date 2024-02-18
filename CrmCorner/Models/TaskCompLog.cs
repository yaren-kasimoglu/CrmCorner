using System;
using System.Collections.Generic;

namespace CrmCorner.Models;

public partial class TaskCompLog
{
    public int LogId { get; set; }

    public int? TaskId { get; set; }

    public string? UpdatedField { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual TaskComp? Task { get; set; }

    //public virtual Employee? UpdatedByNavigation { get; set; }
}
