using System;
using System.Collections.Generic;

namespace CrmCorner.ViewModels
{
    public class DailyReportingUserListViewModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
    }

    public class DailyReportingUserDetailViewModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }

        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }

        public List<DailyReportingCompanyTableViewModel> CompanyTables { get; set; }
            = new List<DailyReportingCompanyTableViewModel>();
    }

    public class DailyReportingCompanyTableViewModel
    {
        public string CompanyName { get; set; }

        public List<DailyReportingActivityRowViewModel> Rows { get; set; }
            = new List<DailyReportingActivityRowViewModel>();
    }

    public class DailyReportingActivityRowViewModel
    {
        public string ActivityType { get; set; }
        public int TotalDifference { get; set; }

        public Dictionary<DayOfWeek, DailyReportingDayCellViewModel> Days { get; set; }
            = new Dictionary<DayOfWeek, DailyReportingDayCellViewModel>();

        public int TotalP { get; set; }
        public int TotalA { get; set; }

        public int TotalGap
        {
            get { return TotalA - TotalP; }
        }

        public decimal TotalPercent
        {
            get
            {
                if (TotalP == 0) return 0;
                return Math.Round(((decimal)TotalA / TotalP) * 100, 0);
            }
        }
    }

    public class DailyReportingDayCellViewModel
    {
        public int P { get; set; }
        public int A { get; set; }

        public int Gap
        {
            get { return A - P; }
        }

        public decimal Percent
        {
            get
            {
                if (P == 0) return 0;
                return Math.Round(((decimal)A / P) * 100, 0);
            }
        }
    }

    public class DailyReportingWeeklySummaryViewModel
    {
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }

        public string UserId { get; set; }
        public string FullName { get; set; }

        public string CompanyName { get; set; }

        public int LinkedinConnectionCount { get; set; }
        public int Emails { get; set; }
        public int LinkedinMessages { get; set; }
        public int LinkedinSentConnections { get; set; }
        public int Calls { get; set; }
        public int MeetingPlanned { get; set; }
        public int MeetingCompleted { get; set; }
    }
}