namespace CrmCorner.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace CrmCorner.Models
    {
        public class PipelineTaskFileAttachment
        {
            [Key]
            public int Id { get; set; }

            [Required]
            public string FileName { get; set; }

            [Required]
            public string FilePath { get; set; }

            [Required]
            public string FileType { get; set; }

            public long FileSize { get; set; }

            public DateTime UploadedDate { get; set; }

            // Foreign Key
            public int PipelineTaskId { get; set; }

            [ForeignKey("PipelineTaskId")]
            public virtual PipelineTask PipelineTask { get; set; }
        }
    }

}
