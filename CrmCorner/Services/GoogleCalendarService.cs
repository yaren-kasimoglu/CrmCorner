using CrmCorner.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.EntityFrameworkCore;
using CalendarModel = CrmCorner.Models.Calendar;

namespace CrmCorner.Services
{
    public class GoogleCalendarService : IGoogleCalendarService
    {
        private readonly CrmCornerContext _context;

        public GoogleCalendarService(CrmCornerContext context)
        {
            _context = context;
        }

        public async Task<string?> CreateEventAsync(AppUser user, CalendarModel model)
        {
            if (user == null || string.IsNullOrEmpty(user.Id))
                return null;

            if (model == null || model.StartDate == null || model.EndDate == null)
                return null;

            var token = await _context.GoogleCalendarTokens
                .FirstOrDefaultAsync(x => x.UserId == user.Id);

            if (token == null || string.IsNullOrEmpty(token.AccessToken))
                return null;

            var credential = GoogleCredential.FromAccessToken(token.AccessToken);

            var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "CrmCorner"
            });

            var googleEvent = new Event
            {
                Summary = model.Title,
                Description = model.Description,
                Start = new EventDateTime
                {
                    DateTime = model.StartDate,
                    TimeZone = "Europe/Istanbul"
                },
                End = new EventDateTime
                {
                    DateTime = model.EndDate,
                    TimeZone = "Europe/Istanbul"
                }
            };

            var request = service.Events.Insert(googleEvent, "primary");
            var createdEvent = await request.ExecuteAsync();

            return createdEvent.Id;
        }

        public async Task<List<CalendarModel>> GetEventsAsync(AppUser user)
        {
            var result = new List<CalendarModel>();

            if (user == null || string.IsNullOrEmpty(user.Id))
                return result;

            var token = await _context.GoogleCalendarTokens
                .FirstOrDefaultAsync(x => x.UserId == user.Id);

            if (token == null || string.IsNullOrEmpty(token.AccessToken))
                return result;

            var credential = GoogleCredential.FromAccessToken(token.AccessToken);

            var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "CrmCorner"
            });

            var request = service.Events.List("primary");
            request.SingleEvents = true;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
            request.TimeMinDateTimeOffset = DateTime.Now.AddMonths(-1);
            request.TimeMaxDateTimeOffset = DateTime.Now.AddMonths(3);

            var events = await request.ExecuteAsync();

            if (events?.Items == null)
                return result;

            foreach (var item in events.Items)
            {
                DateTime? start = item.Start?.DateTimeDateTimeOffset?.DateTime;
                DateTime? end = item.End?.DateTimeDateTimeOffset?.DateTime;

                if (start == null && !string.IsNullOrEmpty(item.Start?.Date))
                    start = DateTime.Parse(item.Start.Date);

                if (end == null && !string.IsNullOrEmpty(item.End?.Date))
                    end = DateTime.Parse(item.End.Date);

                result.Add(new CalendarModel
                {
                    Title = item.Summary ?? "(Başlıksız)",
                    Description = item.Description ?? "",
                    StartDate = start ?? DateTime.MinValue,
                    EndDate = end ?? (start ?? DateTime.MinValue).AddHours(1),
                    GoogleEventId = item.Id,
                    IsSyncedWithGoogle = true
                });
            }

            return result;
        }
    }
}