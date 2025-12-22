namespace CrmCorner.Models
{
    public class ApolloLabelSync
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string LabelId { get; set; }
        public DateTime? LastSyncUtc { get; set; }
    }

}
