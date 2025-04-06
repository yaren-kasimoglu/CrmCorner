using CrmCorner.Models;

namespace CrmCorner.ViewModels
{
    public class CalendarViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public List<SocialMediaContent> Contents { get; set; }
    }

}
