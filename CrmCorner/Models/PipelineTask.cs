using CrmCorner.Models.CrmCorner.Models;
using CrmCorner.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CrmCorner.Models

{
    public class PipelineTask
    {
        public int Id { get; set; }

        // Görev Bilgileri
        [Required(ErrorMessage = "Başlık alanı zorunludur.")]
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Value { get; set; }
        public string? Currency { get; set; }

        [Required(ErrorMessage = "Aşama seçimi zorunludur.")]
        public PipelineStage? Stage { get; set; }
        public DateTime? ExpectedCloseDate { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        public string? CustomerName { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        public string? CustomerSurname { get; set; }

        [Required(ErrorMessage = "Şirket adı alanı zorunludur.")]
        public string? CompanyName { get; set; }
        public string? Phone { get; set; }

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        public string? Email { get; set; }
        public string? LinkedinUrl { get; set; }
        public string? Source { get; set; }

      //  [Required(ErrorMessage = "Görüşmeyi alan kişi seçilmelidir.")]
        [ForeignKey("ResponsibleUserId")]
        public AppUser? ResponsibleUser { get; set; }   // <<< EKLE
        public string? ResponsibleUserId { get; set; }


        public OutcomeType? Outcomes { get; set; } = OutcomeType.Surecte;

        public bool? ContactedViaLinkedIn { get; set; }
        public bool? ContactedViaColdCall { get; set; }

        public string? NegativeReason { get; set; }

        [Required(ErrorMessage = "Bu görüşme kaydının edinildiği kanal seçilmelidir.")]
        public SourceChannelType? SourceChannel { get; set; }



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

        public virtual PostSaleInfo? PostSaleInfo { get; set; }


        public virtual ICollection<PipelineTaskNote> Notes { get; set; } = new List<PipelineTaskNote>();

        public virtual ICollection<PipelineTaskFileAttachment> FileAttachments { get; set; } = new List<PipelineTaskFileAttachment>();



    }

}
