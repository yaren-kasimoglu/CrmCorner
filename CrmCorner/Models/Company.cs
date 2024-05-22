using System;
using System.Collections.Generic;

namespace CrmCorner.Models;

public partial class Company
{
    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = null!;

    public bool? IsApproved { get; set; } = false;
    public string EmailDomain { get; set; } = null!;

}
