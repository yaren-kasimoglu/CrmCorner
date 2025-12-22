namespace CrmCorner.Models
{
    public class LatestTaskDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime CreatedDate { get; set; }
        public int BoardId { get; set; }
    }
}
