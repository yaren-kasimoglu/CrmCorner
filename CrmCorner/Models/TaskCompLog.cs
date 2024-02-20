using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmCorner.Models;

public partial class TaskCompLog
{
    public int LogId { get; set; }

    public int? TaskId { get; set; }

    public string? UpdatedField { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedById { get; set; }

    [ForeignKey("TaskId")]
    public virtual TaskComp? Task { get; set; }

    [ForeignKey("UpdatedById")]
    public virtual AppUser? UpdatedBy { get; set; }
}
