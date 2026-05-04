using System;
using System.Collections.Generic;

namespace CrmCorner.Models;

public partial class Company
{
    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = null!;

    public bool? IsApproved { get; set; } = false;
    public string EmailDomain { get; set; } = null!;

    public bool UseAppUser { get; set; } = true;          // Görüşmeyi alan
    public bool UseResponsibleUser { get; set; } = true;  // SDR
    public bool UseMeetingUser { get; set; } = false;     // Görüşmeyi gerçekleştiren
    public bool UseReporterUser { get; set; } = false;    // Raporlamacı

    public virtual ICollection<TableHeader> TableHeaders { get; set; } = new List<TableHeader>();

}
