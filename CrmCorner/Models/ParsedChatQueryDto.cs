using System;

namespace CrmCorner.Models.ChatCorner
{
    public class ParsedChatQueryDto
    {
        public string Intent { get; set; }
        public string TargetEmail { get; set; }
        public string TargetName { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string PeriodLabel { get; set; }
    }
}