namespace CrmCorner.Models.ChatCorner
{
    public class ChatIntentParseResultDto
    {
        public string Intent { get; set; }

        public string TargetName { get; set; }

        public string TargetEmail { get; set; }

        public string PeriodType { get; set; }

        public int? Year { get; set; }

        public int? Month { get; set; }

        public string Metric { get; set; }

        public string RawQuestion { get; set; }
    }
}