using Microsoft.AspNetCore.Identity;

namespace CrmCorner.Models
{
    public class AppRole:IdentityRole
    {
        public virtual ICollection<AppUserRole> UserRoles { get; set; }
    }
}
