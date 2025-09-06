// UserPipelineSummaryViewModel.cs
using CrmCorner.Models.Enums;

namespace CrmCorner.ViewModels
{
    public class UserPipelineSummaryViewModel
    {
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string? NameSurname { get; set; }
        public int Total { get; set; }

        // Stage ve Outcome kırılımları
        public Dictionary<PipelineStage, int> StageCounts { get; set; } = new();
        public Dictionary<OutcomeTypeSales, int> OutcomeCounts { get; set; } = new();
    }

    public class PipelineAnalysisPageViewModel
    {
        public List<UserPipelineSummaryViewModel> Rows { get; set; } = new();
        public PipelineStage[] AllStages { get; set; } = Array.Empty<PipelineStage>();
        public OutcomeTypeSales[] AllOutcomes { get; set; } = Array.Empty<OutcomeTypeSales>();
        public bool IsAdminOrManager { get; set; }
    }
}
