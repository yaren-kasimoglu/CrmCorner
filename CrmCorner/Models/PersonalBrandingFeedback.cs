using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmCorner.Models
{
    public class PersonalBrandingFeedback
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PersonalBrandingContentId { get; set; }

        [ForeignKey("PersonalBrandingContentId")]
        public PersonalBrandingContent PersonalBrandingContent { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Opsiyonel: Geri bildirimi yapan kullanıcı
        public string? CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public AppUser? CreatedBy { get; set; }


    }
}
