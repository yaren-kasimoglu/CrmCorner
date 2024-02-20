using System;
using System.Collections.Generic;

namespace CrmCorner.Models;

public partial class CustomerN
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string CustomerEmail { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string CompanyName { get; set; } = null!;

    public string CompanyEmail { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? AppUserId { get; set; }

    public virtual AppUser? AppUser { get; set; }

    public virtual ICollection<TaskComp> Taskcomps { get; set; } = new List<TaskComp>();
}
