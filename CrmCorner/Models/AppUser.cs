using CrmCorner.Models.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmCorner.Models
{
    [Table("users")]
    public class AppUser : IdentityUser
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

        public string EmailDomain { get; set; } 
        public int CompanyId { get; set; }

        //[NotMapped]
        //public UserRole Role { get; set; }



        public virtual ICollection<CustomerN> Customers { get; set; }
        public virtual ICollection<TaskComp> TaskComps { get; set; }
        public virtual ICollection<Calendar>? Calendars { get; set; }
        public virtual ICollection<Notification>? Notifications { get; set; }
        public virtual ICollection<TaskCompLog> TaskCompLogs { get; set; } = new List<TaskCompLog>();

        public virtual ICollection<AppUserRole> UserRoles { get; set; }

    }

}

