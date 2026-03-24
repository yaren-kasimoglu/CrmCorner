using System;

namespace CrmCorner.Models
{
    public class Calendar
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? UserId { get; set; }

        public virtual AppUser? AppUser { get; set; }

        public string? GoogleEventId { get; set; }

        public bool IsSyncedWithGoogle { get; set; } = false;
    }
}