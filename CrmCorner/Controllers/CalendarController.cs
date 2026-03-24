using CrmCorner.Models;
using CrmCorner.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Calendar = CrmCorner.Models.Calendar;

namespace CrmCorner.Controllers
{
    public class CalendarController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly CrmCornerContext _context;
        private readonly IGoogleCalendarService _googleCalendarService;
        private readonly IConfiguration _configuration;
        private readonly SmtpSettings _smtpSettings;



        public CalendarController(CrmCornerContext context, IGoogleCalendarService service, UserManager<AppUser> userManager, IConfiguration configuration, IOptions<SmtpSettings> options)
        {
            _context = context;
            _userManager = userManager;
            _googleCalendarService = service;
            _configuration = configuration;
            _smtpSettings = options.Value;
        }


        [Authorize]
        public async Task<IActionResult> Calendar()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                TempData["ErrorMessage"] = "Geçerli kullanıcı bilgisi bulunamadı.";
                return RedirectToAction("SignIn", "Home");
            }

            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

            var crmCalendars = _context.Calendars
                .Include(e => e.AppUser)
                .Where(e => e.UserId == currentUser.Id)
                .ToList();

            var googleCalendars = await _googleCalendarService.GetEventsAsync(currentUser);

            var allCalendars = crmCalendars.ToList();

            if (googleCalendars != null && googleCalendars.Any())
            {
                allCalendars.AddRange(googleCalendars);
            }

            var calendarItemsFilter = allCalendars
                .OrderByDescending(x => x.StartDate)
                .Take(5)
                .ToList();

            ViewBag.Calendar = allCalendars;
            ViewBag.CalendarFilter = calendarItemsFilter;
            ViewBag.Users = _context.Users
                .Where(u => u.Id != currentUser.Id && u.CompanyId == currentUser.CompanyId)
                .Select(u => new SelectListItem
                {
                    Value = u.Email,
                    Text = u.UserName
                }).ToList();

            return View("Calendar");
        }
  
        [Authorize]
        public IActionResult ConnectGoogleCalendar()
        {
            var clientId = _configuration["GoogleCalendar:ClientId"];
            var redirectUri = _configuration["GoogleCalendar:RedirectUri"];
            var scope = "https://www.googleapis.com/auth/calendar";

            var authUrl = "https://accounts.google.com/o/oauth2/v2/auth" +
                          "?client_id=" + Uri.EscapeDataString(clientId) +
                          "&redirect_uri=" + Uri.EscapeDataString(redirectUri) +
                          "&response_type=code" +
                          "&scope=" + Uri.EscapeDataString(scope) +
                          "&access_type=offline" +
                          "&prompt=consent";

            return Redirect(authUrl);
        }

        [Authorize]
        public async Task<IActionResult> GoogleCallback(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                TempData["ErrorMessage"] = "Google yetkilendirme kodu alınamadı.";
                return RedirectToAction("Calendar");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("Calendar");
            }

            using var httpClient = new HttpClient();

            var values = new Dictionary<string, string>
    {
        { "code", code },
        { "client_id", _configuration["GoogleCalendar:ClientId"] },
        { "client_secret", _configuration["GoogleCalendar:ClientSecret"] },
        { "redirect_uri", _configuration["GoogleCalendar:RedirectUri"] },
        { "grant_type", "authorization_code" }
    };

            var response = await httpClient.PostAsync(
                "https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(values)
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = "Google token alınamadı: " + errorContent;
                return RedirectToAction("Calendar");
            }

            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JObject.Parse(content);

            var accessToken = tokenResponse["access_token"]?.ToString();
            var refreshToken = tokenResponse["refresh_token"]?.ToString();
            var expiresIn = tokenResponse["expires_in"]?.ToObject<int>() ?? 3600;

            var existingToken = await _context.GoogleCalendarTokens
                .FirstOrDefaultAsync(x => x.UserId == currentUser.Id);

            if (existingToken == null)
            {
                existingToken = new GoogleCalendarToken
                {
                    UserId = currentUser.Id,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiryUtc = DateTime.UtcNow.AddSeconds(expiresIn)
                };

                _context.GoogleCalendarTokens.Add(existingToken);
            }
            else
            {
                existingToken.AccessToken = accessToken;

                if (!string.IsNullOrEmpty(refreshToken))
                    existingToken.RefreshToken = refreshToken;

                existingToken.ExpiryUtc = DateTime.UtcNow.AddSeconds(expiresIn);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Google Calendar başarıyla bağlandı.";
            return RedirectToAction("Calendar");
        }


    }
}