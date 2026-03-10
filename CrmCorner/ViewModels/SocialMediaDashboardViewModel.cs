using CrmCorner.Models;

namespace CrmCorner.ViewModels
{
    public class SocialMediaDashboardViewModel
    {
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int WeeklyCount { get; set; }
        public int RevisionCount { get; set; }
        public int TodayCount { get; set; }
        public bool IsAdminView { get; set; }
        public List<SocialMediaContent> UpcomingContents { get; set; } = new List<SocialMediaContent>();

        public int PersonalBrandingTotalCount { get; set; }
        public int PersonalBrandingWeeklyCount { get; set; }
        public int PersonalBrandingPendingCount { get; set; }
        public int PersonalBrandingPublishedCount { get; set; }
    }
}
