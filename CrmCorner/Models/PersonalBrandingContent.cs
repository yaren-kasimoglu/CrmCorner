using CrmCorner.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmCorner.Models
{
    public class PersonalBrandingContent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; } // LinkedIn yazısı

        public byte[]? MediaFile { get; set; } // Görsel veya video

        public int? CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public Company? Company { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? EstimatedPublishDate { get; set; } // Tahmini Paylaşım Tarihi

        public ContentStatus Status { get; set; } = ContentStatus.OnayBekliyor;

        public ICollection<PersonalBrandingFeedback>? Feedbacks { get; set; }


        public string? PersonalUserId { get; set; }

        [ForeignKey("PersonalUserId")]
        public AppUser? PersonalUser { get; set; }

    }
}
