using CrmCorner.Models.CrmCorner.Models;
using CrmCorner.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
namespace CrmCorner.Models
{
    public class PipelineTask
    {
        public int Id { get; set; }

        // Görev Bilgileri
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Value { get; set; }
        public string? Currency { get; set; }
        public PipelineStage? Stage { get; set; }
        public DateTime? ExpectedCloseDate { get; set; }

        public string? CustomerName { get; set; }
        public string? CustomerSurname { get; set; }
        public string? CompanyName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? LinkedinUrl { get; set; }
        public string? Source { get; set; }
        public string? SourceChannel { get; set; }
        public string? ResponsibleUserId { get; set; }
        public OutcomeType? Outcomes { get; set; }

        public bool? ContactedViaLinkedIn { get; set; }
        public bool? ContactedViaColdCall { get; set; }


        public OutcomeTypeSales? OutcomeStatus { get; set; } = OutcomeTypeSales.None;


        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Outcome durumu
    
        // Görev sahibi
        [ForeignKey("AppUserId")]
        public AppUser? AppUser { get; set; }
        public string? AppUserId { get; set; }  // Foreign key

        // Müşteri bilgisi
        [ForeignKey("CustomerId")]
        public CustomerN? Customer { get; set; }
        public int? CustomerId { get; set; }  // Foreign key

        public virtual ICollection<PipelineTaskNote> Notes { get; set; } = new List<PipelineTaskNote>();

        public virtual ICollection<PipelineTaskFileAttachment> FileAttachments { get; set; } = new List<PipelineTaskFileAttachment>();



    }

}
