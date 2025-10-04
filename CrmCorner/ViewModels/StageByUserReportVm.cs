using CrmCorner.Models.Enums;

namespace CrmCorner.ViewModels
{
    public class StageByUserReportVm
    {
        public string ResponsibleUserName { get; set; }
        public PipelineStage Stage { get; set; }
        public string StageName { get; set; }
        public int Count { get; set; }
        public List<string> TaskTitles { get; set; } = new(); // <<< EKLENDİ
    }

}
