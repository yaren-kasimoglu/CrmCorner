using CrmCorner.Models.Enums;
using Microsoft.AspNetCore.Identity;

namespace CrmCorner.Models
{
    public class AppUser:IdentityUser
    {
        public string? City { get; set; }
        public string? Picture { get; set; }
        public DateTime? BirthDate { get; set; }
        public Gender? Gender { get; set; }

        public string? NameSurname { get; set; }
        public string CompanyName { get; set; } = null!;
        public int? EmployeeCount { get; set; }
        public string? Sector { get; set; }
        public string? PositionName { get; set; }

        public int CompanyId { get; set; }

        public virtual ICollection<CustomerN> Customers { get; set; }
        public virtual ICollection<TaskComp> TaskComps { get; set; }
        public virtual ICollection<Calendar> Calendars { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<TaskCompLog> TaskCompLogs { get; set; } = new List<TaskCompLog>();
    }

}

