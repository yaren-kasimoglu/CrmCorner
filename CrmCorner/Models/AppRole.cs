using Microsoft.AspNetCore.Identity;

namespace CrmCorner.Models
{
    public class AppRole:IdentityRole
    {
        public int Id { get; set; }
        public string Name { get; set; } // Örn: SuperAdmin, Admin, TeamLeader, TeamMember
    }
}
