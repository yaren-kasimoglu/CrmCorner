using System;
using System.Collections.Generic;

namespace CrmCorner.Models;

public partial class Company
{
    public int Id { get; set; }

    public string CompanyName { get; set; } = null!;
    //public string ResponsibleNameSurname { get; set; }
    //public int EmployeeCount { get; set; }
    //public string Phone { get; set; }
    //public string Sector { get; set; }
    //public string PositionName { get; set; }

    public string? CompanyEmail { get; set; }

    public int? StatusId { get; set; }

    public int? IdEmployee { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual Employee? IdEmployeeNavigation { get; set; }

    public virtual Status? Status { get; set; }
}
