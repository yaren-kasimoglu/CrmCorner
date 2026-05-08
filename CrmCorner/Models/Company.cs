using System;
using System.Collections.Generic;

namespace CrmCorner.Models;

public partial class Company
{
    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = null!;

    public bool? IsApproved { get; set; } = false;

    public string EmailDomain { get; set; } = null!;

    // -------------------------
    //  TRIAL (14 gün)
   
    public bool IsTrialActive { get; set; } = false;

    public DateTime? TrialStartDate { get; set; }

    public DateTime? TrialEndDate { get; set; }

    // -------------------------
    //  PAYMENT / SUBSCRIPTION

    public bool IsPaymentActive { get; set; } = false;

    public string? PlanName { get; set; }

    public int PaidUserCount { get; set; } = 0;

    public DateTime? SubscriptionStartDate { get; set; }

    public DateTime? SubscriptionEndDate { get; set; }

    public DateTime? LastPaymentDate { get; set; }

    public decimal LastPaymentAmount { get; set; } = 0;

    // -------------------------
    //  Mevcut yapı

    public bool UseAppUser { get; set; } = true;
    public bool UseResponsibleUser { get; set; } = true;
    public bool UseMeetingUser { get; set; } = false;
    public bool UseReporterUser { get; set; } = false;

    public bool IsLegacyCustomer { get; set; } = false;

    public virtual ICollection<TableHeader> TableHeaders { get; set; } = new List<TableHeader>();
}
