using System;
using System.Collections.Generic;

namespace CrmCorner.Models;

public partial class Customer
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int? CompanyId { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual Company? Company { get; set; }

    public virtual ICollection<TaskComp> TaskComps { get; set; } = new List<TaskComp>();
}
