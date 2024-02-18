namespace CrmCorner.Models
{
    public class FileAttachment
    {

        public int FileAttachmentId { get; set; }
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public long FileSize { get; set; }
        public string FileType { get; set; } = null!;
        public DateTime UploadedDate { get; set; }

        // Görev ile ilişkilendirme
        public int TaskId { get; set; }
        public virtual TaskComp TaskComp { get; set; } = null!;

    }
}
