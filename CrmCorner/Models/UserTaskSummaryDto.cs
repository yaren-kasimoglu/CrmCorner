using System;

namespace CrmCorner.Models.ChatCorner
{
    public class UserTaskSummaryDto
    {
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }

        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        public int TotalTasks { get; set; }

        public int DegerlendirilenCount { get; set; }
        public int GorusmeBaslatildiCount { get; set; }
        public int ToplantiDuzenlendiCount { get; set; }
        public int TeklifSunulduCount { get; set; }
        public int SonucAsamasiCount { get; set; }

        public int WonCount { get; set; }
        public int LostCount { get; set; }

        public decimal? WinRate { get; set; }
    }
}