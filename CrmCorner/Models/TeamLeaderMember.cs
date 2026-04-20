namespace CrmCorner.Models
{
    public class TeamLeaderMember
    {
        public int Id { get; set; }

        public string TeamMemberId { get; set; }
        public AppUser TeamMember { get; set; }

        public string TeamLeaderId { get; set; }
        public AppUser TeamLeader { get; set; }
    }
}