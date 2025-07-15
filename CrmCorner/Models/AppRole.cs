using Microsoft.AspNetCore.Identity;

namespace CrmCorner.Models
{
    public class AppRole:IdentityRole
    {
        public string Name { get; set; } // Örn: SuperAdmin, Admin, TeamLeader, TeamMember
        public ICollection<AppUserRole> UserRoles { get; set; }

    }
}
