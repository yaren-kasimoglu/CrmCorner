using System;

namespace CrmCorner.Models
{
    public class DailyReport
    {
        public int Id { get; set; }

        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        public string CompanyName { get; set; }

        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }

        public DateTime ReportDate { get; set; }

        public string ActivityType { get; set; }

        public int ProspectTarget { get; set; }
        public int ActualValue { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}