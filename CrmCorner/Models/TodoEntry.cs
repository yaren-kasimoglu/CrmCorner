using System;

namespace CrmCorner.Models
{
    public class TodoEntry
    {
        public int Id { get; set; }

        public int TodoBoardId { get; set; }  // foreign key

        public string UserId { get; set; }
        public string? AssigneeId { get; set; }

        public string? AssignedById { get; set; }

        public string Text { get; set; }

        public bool IsDone { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? CompletedDate { get; set; }

        public virtual TodoBoard TodoBoard { get; set; }
        public virtual AppUser? Assignee { get; set; }
        public AppUser? AssignedByUser { get; set; }
    }
}
