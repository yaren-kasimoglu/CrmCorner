namespace CrmCorner.ViewModels
{
    public class CombinedReportVm
    {
        public List<ResponsibleUserTaskReportVm> ResponsibleUserReports { get; set; } = new();
        public List<StageReportVm> StageReports { get; set; } = new();
     //   public List<StageByUserReportVm> StageByUserReports { get; set; } = new();
        public List<StageByUserRowVm> StageByUserPivot { get; set; } = new();


    }

}
