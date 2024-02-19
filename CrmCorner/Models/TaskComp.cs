using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CrmCorner.Models;

public partial class TaskComp
{

    public int TaskId { get; set; }

    public string Title { get; set; } = null!;

    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
    public decimal? ValueOrOffer { get; set; }

    public string? Description { get; set; }

    public DateTime? SalesDone { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? StatusId { get; set; }

    public string? UserId { get; set; }
    public virtual AppUser? AppUser { get; set; }

    public virtual Status? Status { get; set; }

    public int? CustomerId { get; set; }

    public virtual CustomerN? Customer { get; set; }

    // Dosya ekleri için koleksiyon
    public virtual ICollection<FileAttachment>? FileAttachments { get; set; }
    public virtual ICollection<TaskCompLog>? TaskCompLogs { get; set; }




    //public string? TaskCompcol { get; set; }

    //public byte[]? UploadedFile { get; set; }

    //public string? UploadedFileName { get; set; }
}
