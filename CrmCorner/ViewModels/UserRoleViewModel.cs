using CrmCorner.Models;

namespace CrmCorner.ViewModels
{
    public class UserRoleViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string CompanyName { get; set; }
        public string CurrentRole { get; set; }
        public string NewRole { get; set; }
    }

}
