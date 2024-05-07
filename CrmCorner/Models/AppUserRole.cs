namespace CrmCorner.Models
{
    public class AppUserRole
    {
        public string UserId { get; set; }
        public AppUser User { get; set; }

        public string RoleId { get; set; }
        public AppRole Role { get; set; }
    }
}
