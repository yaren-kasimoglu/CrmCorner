using CrmCorner.Models.Enums;

namespace CrmCorner.Models
{
    public class UserModule
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public ModuleType Module { get; set; }

        public AppUser User { get; set; }
    }
}
