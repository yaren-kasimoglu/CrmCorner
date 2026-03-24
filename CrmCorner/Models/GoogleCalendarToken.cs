namespace CrmCorner.Models
{
    public class GoogleCalendarToken
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime ExpiryUtc { get; set; }

        public virtual AppUser? AppUser { get; set; }
    }
}
