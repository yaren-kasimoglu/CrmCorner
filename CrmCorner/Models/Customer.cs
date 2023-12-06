using System;
using System.Collections.Generic;

namespace CrmCorner.Models;

public partial class Customer
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int? IdEmployee { get; set; }

    public virtual Employee? IdEmployeeNavigation { get; set; }
}
