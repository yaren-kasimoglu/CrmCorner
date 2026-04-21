using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmCorner.Models
{
    public class TodoEntry
    {
        public int Id { get; set; }

        public int TodoBoardId { get; set; }
        public string UserId { get; set; }
        public string Text { get; set; }

        public bool IsDone { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? CompletedDate { get; set; }

        public string? AssigneeId { get; set; }
        public string? AssignedById { get; set; }

        public bool IsDayBoardTask { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool ExpirationWarningSent { get; set; }

        // Deadline bildirim flagleri
        public DateTime? Deadline { get; set; }
        public bool DeadlineReminderWeekSent { get; set; }
        public bool DeadlineReminder3DaysSent { get; set; }
        public bool DeadlineReminderLastDaySent { get; set; }
        public bool DeadlineReminder2HoursSent { get; set; }

        public bool IsImportant { get; set; }

        public virtual TodoBoard TodoBoard { get; set; }


        [ForeignKey(nameof(AssigneeId))]
        public virtual AppUser? AssigneeUser { get; set; }

        [ForeignKey(nameof(AssignedById))]
        public virtual AppUser? AssignedByUser { get; set; }
    }
}