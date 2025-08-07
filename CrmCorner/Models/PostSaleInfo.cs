using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmCorner.Models
{
    public class PostSaleInfo
    {
        public int Id { get; set; }

        // ✅ Artık PipelineTask ile ilişkilendirme
        public int PipelineTaskId { get; set; }

        [ForeignKey("PipelineTaskId")]
        public virtual PipelineTask? PipelineTask { get; set; }

        public bool IsFirstPaymentMade { get; set; } = false;

        public bool IsThereAProblem { get; set; } = false;

        public string? ProblemDescription { get; set; }

        public bool IsContinuationConsidered { get; set; } = false;

        public bool IsTrustpilotReviewed { get; set; } = false;

        public string? TrustPilotComment { get; set; }

        public bool CanUseLogo { get; set; } = false;
    }
}
