using CrmCorner.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmCorner.Models;

public partial class TaskComp
{
    public int TaskId { get; set; }

    public string Title { get; set; } = null!;

    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
    public decimal? ValueOrOffer { get; set; }

    public string? Description { get; set; }

    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
    public DateTime? SalesDone { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
    public DateTime? FinalSalesDone { get; set; }

    public int? StatusId { get; set; }

    public string? UserId { get; set; }
    public virtual AppUser? AppUser { get; set; }

    public string? AssignedUserId { get; set; } // Görüşmeyi gerçekleştiren kişinin UserId'si
    public virtual AppUser? AssignedUser { get; set; } // Görüşmeyi gerçekleştiren kişi

    public virtual Status? Status { get; set; }

    public int? CustomerId { get; set; }
    public virtual CustomerN? Customer { get; set; }

    public bool IsFinalDecisionMaker { get; set; } = true;

    public OutcomeType Outcomes { get; set; }
    public string? NegativeReason { get; set; }

    public OutcomeTypeSales? OutcomeStatus { get; set; }=OutcomeTypeSales.None;

    public int? CompanyId { get; set; }


    public string? HeardFrom { get; set; } // Nereden duydunuz?



    // Dosya ekleri için koleksiyon
    public virtual ICollection<FileAttachment>? FileAttachments { get; set; }
    public virtual ICollection<TaskCompLog>? TaskCompLogs { get; set; }
    public virtual Notification? Notification { get; set; }

    public virtual ICollection<TaskCompNote> Notes { get; set; } = new List<TaskCompNote>();


    public string? SelectedCurrency { get; set; } 


}


