using System;
using System.Collections.Generic;

namespace CrmCorner.Models
{
    public class TodoBoard
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string Title { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        public virtual List<TodoEntry> Entries { get; set; } = new();
    }
}
