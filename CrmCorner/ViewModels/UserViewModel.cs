namespace CrmCorner.ViewModels
{
    public class UserViewModel
    {
        public string? UserName { get; set; }
        public string? NameSurname { get; set; }
        public string? Email { get; set; } 
        public string? PhoneNumber { get; set; }
        public string? PictureUrl { get; set; }
        public string CompanyName { get; set; }
        public string? PositionName { get; set; }
        public string? UserId { get; set; }
        public bool HasUnreadMessages { get; set; }
    }
}
