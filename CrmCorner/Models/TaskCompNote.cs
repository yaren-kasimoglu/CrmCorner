using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmCorner.Models
{
    public class TaskCompNote
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("TaskComp")]
        public int TaskCompId { get; set; }

        public string Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual TaskComp TaskComp { get; set; }
    }
}
