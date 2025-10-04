using CrmCorner.Models;
using CrmCorner.Models.Enums;

namespace CrmCorner.ViewModels
{
    public class StageReportVm
    {
        public PipelineStage Stage { get; set; }
        public int Count { get; set; }
        public List<PipelineTask> TaskList { get; set; } = new();
    }
}
