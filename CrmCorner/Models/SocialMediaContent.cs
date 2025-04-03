using CrmCorner.Models.Enums;

namespace CrmCorner.Models
{
    public class SocialMediaContent
    {
        public int Id { get; set; }

        public string Title { get; set; } // İçerik başlığı
        public string Description { get; set; } // Açıklama (caption)

        public string MediaPath { get; set; } // Görsel veya video yolu
        public ContentType ContentType { get; set; } // Post, Story, Reels

        public DateTime CreatedDate { get; set; }
        public ContentStatus Status { get; set; } // OnayBekliyor, Onaylandı, FeedbackVerildi

        public string? FeedbackMessage { get; set; } // Kullanıcının geri bildirimi varsa
        public DateTime? ScheduledPublishDate { get; set; }

    }
}
