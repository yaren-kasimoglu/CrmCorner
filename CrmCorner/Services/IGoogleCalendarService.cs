using CrmCorner.Models;
using CalendarModel = CrmCorner.Models.Calendar;

namespace CrmCorner.Services
{
    public interface IGoogleCalendarService
    {
        Task<string?> CreateEventAsync(AppUser user, CalendarModel model);
        Task<List<CalendarModel>> GetEventsAsync(AppUser user);
    }
}