using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace CrmCorner.Models
{
    public class AppRole : IdentityRole
    {
        public string? Description { get; set; } // Ekstra alan (opsiyonel)
        public virtual ICollection<AppUserRole> UserRoles { get; set; }
    }
}
