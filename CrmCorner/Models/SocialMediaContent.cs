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

        // Geri Bildirim Koleksiyonu
        public ICollection<Feedback> Feedbacks { get; set; }
        public DateTime? ScheduledPublishDate { get; set; }

    }

    public class Feedback
    {
        public int Id { get; set; }
        public int SocialMediaContentId { get; set; }  // İlgili içerik
        public string Message { get; set; }  // Geri bildirim mesajı
        public DateTime CreatedDate { get; set; }  // Geri bildirim oluşturulma tarihi

        // İlişkiler
        public SocialMediaContent SocialMediaContent { get; set; }
    }

}
