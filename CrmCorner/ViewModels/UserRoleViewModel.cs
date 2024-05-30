using CrmCorner.Models;

namespace CrmCorner.ViewModels
{
    public class UserRoleViewModel
    {
        public AppUser User { get; set; }
        public IList<string> Roles { get; set; }
    }
}
