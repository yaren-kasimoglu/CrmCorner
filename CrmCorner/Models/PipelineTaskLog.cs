using CrmCorner.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmCorner.Models
{
    public class PipelineTaskLog
    {
        [Key]
        public int LogId { get; set; }

        public int? PipelineTaskId { get; set; }

        public string? UpdatedField { get; set; }

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? UpdatedById { get; set; }

        [ForeignKey("PipelineTaskId")]
        public virtual PipelineTask? PipelineTask { get; set; }

        [ForeignKey("UpdatedById")]
        public virtual AppUser? UpdatedBy { get; set; }
    }
}
