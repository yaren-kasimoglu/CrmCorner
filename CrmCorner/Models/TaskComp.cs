using System;
using System.Collections.Generic;

namespace CrmCorner.Models;

public partial class TaskComp
{
    public int TaskId { get; set; }

    public string Title { get; set; } = null!;

    public decimal? ValueOrOffer { get; set; }

    public int? CustomerId { get; set; }

    public int? EmployeeId { get; set; }

    public int? StatusId { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? TaskCompcol { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual Status? Status { get; set; }
}
