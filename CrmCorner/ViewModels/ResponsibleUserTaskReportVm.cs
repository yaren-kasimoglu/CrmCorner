using CrmCorner.Models;

namespace CrmCorner.ViewModels
{
    public class ResponsibleUserTaskReportVm
    {
        public string ResponsibleUserName { get; set; }
        public int TotalTasks { get; set; }
        public int OngoingTasks { get; set; }
        public int SuccessfulTasks { get; set; }
        public int FailedTasks { get; set; }
        public List<PipelineTask> TaskList { get; set; } = new();
    }
}
