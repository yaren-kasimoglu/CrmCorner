using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmCorner.Models
{
    public class PipelineTaskNote
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("PipelineTask")]
        public int PipelineTaskId { get; set; }
        public virtual PipelineTask PipelineTask { get; set; }

        public string Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
