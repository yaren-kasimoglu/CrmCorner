using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmCorner.Models;

public partial class FileAttachment
{
    public int FileAttachmentId { get; set; }

    public string FileName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public long FileSize { get; set; }

    public string FileType { get; set; } = null!;

    public DateTime UploadedDate { get; set; }


    public int TaskId { get; set; }
    [ForeignKey("TaskId")]
    public virtual TaskComp? Task { get; set; }
}
