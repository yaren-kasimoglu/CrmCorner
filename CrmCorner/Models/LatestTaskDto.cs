namespace CrmCorner.Models
{
    public class LatestTaskDto
    {
        public int Id { get; set; }
        public int BoardId { get; set; }

        public string Text { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? Deadline { get; set; }

        public bool IsUrgent { get; set; }

        public string UrgencyText { get; set; }

        public bool IsAssignedToMe { get; set; }
    }
}
