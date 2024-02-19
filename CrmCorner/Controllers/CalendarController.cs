using System;
using CrmCorner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using System.Xml.Serialization;
using System.Data.Common;
using System.Net.Mail;
using MySqlX.XDevAPI;
using System.Net;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.Auth;
using Azure.Core;
using Org.BouncyCastle.Cms;
using System.Net.Mail;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Google;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Calendar = CrmCorner.Models.Calendar;
using MySqlX.XDevAPI.Relational;
using Microsoft.AspNetCore.Identity;

namespace CrmCorner.Controllers
{
    public class CalendarController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly CrmCornerContext _context;
        private readonly IGoogleCalendarService _googleCalendarService;


        public CalendarController(CrmCornerContext context, IGoogleCalendarService service, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _googleCalendarService = service;
        }
        public async Task<IActionResult> Calendar()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser != null)
            {
                var calendars = _context.Calendars
                       .Include(e => e.AppUser)
                       .Where(e => e.UserId == currentUser.Id)
                       .ToList();
            List<Calendar> calendarItems = calendars
               .Select(c => new Calendar
               {

                   Date = c.Date,
                   Id = c.Id,
                   Title = c.Title,
                   Description = c.Description
               }).ToList();
            List<Calendar> calendarItemsFilter = calendars
         .Select(c => new Calendar
         {
             Date = c.Date,
             Id = c.Id,
             Title = c.Title,
             Description = c.Description
         }).ToList();

            ViewBag.Calendar = calendars;
            ViewBag.CalendarFilter = calendarItemsFilter.Take(5);
            return View();
            }

            else
            {
                ViewBag.ErrorMessage = "Geçerli kullanıcı bilgisi bulunamadı.";
                return View();
            }

        }

        [HttpPost]
        public async Task<IActionResult> CalendarAdd(Calendar calendar)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {

                    calendar.UserId = currentUser.Id;

                    _context.Calendars.Add(calendar);
                    await _context.SaveChangesAsync();

                    if (!String.IsNullOrEmpty(calendar.Email.ToString()))
                    {
                        var emailSend = sendEmailAsync(calendar);
                    }
                    return RedirectToAction("Calendar");
                }
                else
                {
                 
                    ViewBag.ErrorMessage = "Geçerli kullanıcı bilgisi bulunamadı.";
                    return View(calendar); 
                }
            }
            return RedirectToAction("Calendar");
        }
        [HttpPost]
        public IActionResult CalendarUpdate(int? ID, string title, string date)
        {
            var htmlAttributes = ViewBag.Id;
            Calendar calendar = new Calendar { Id = ID.Value, Title = title, Date = date };
            if (ModelState.IsValid)
            {
                _context.Calendars.Update(calendar);
                _context.SaveChanges();

                return Json(new { Message = "success" });
            }
            return View("Calendar");
        }

        [HttpPost]
        public IActionResult CalendarDelete(int? ID)
        {
            Calendar calendar = _context.Calendars.Find(ID);

            if (calendar == null)
            {
                return NotFound();
            }
            _context.Calendars.Remove(calendar);
            _context.SaveChanges();
            return Json(new { Message = "success" });

        }
        [HttpPost]
        public IActionResult GetDescription(int? ID)
        {
            Calendar calendar = _context.Calendars.Find(ID);


            if (calendar == null)
            {
                return Json(new { Message = "error" });
            }
            return Json(new { Message = calendar.Description });

        }

        [HttpPost]
        public async Task<IActionResult> GetFilterCalendar(bool ischecked)
        {
            List<Calendar> calendars;
            if (ischecked == true)
            {
                var currentUser = await _userManager.GetUserAsync(User);

                calendars = _context.Calendars
                          .Include(e => e.AppUser)
                          .Where(e => e.UserId == currentUser.Id)
                          .ToList();
            }
            else
            {
                calendars = _context.Calendars.ToList();
            }
            List<Calendar> calendarItems = calendars
                  .Select(c => new Calendar
                  {

                      Date = c.Date,
                      Id = c.Id,
                      Title = c.Title,
                      Description = c.Description
                  }).ToList();
            return Json(new { Message = true });

        }

        public  async Task<bool> sendEmailAsync(Calendar calendar)
        {
            string clientId = "928553971028-n6q5dv2pjg12mvnajbrj1q8g7kee6djp.apps.googleusercontent.com";
            string clientSecret = "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";

            string[] scopes = { "https://www.googleapis.com/auth/calendar" };

            var credentials = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
                scopes, "user", CancellationToken.None).Result;

            string date = calendar.Date;
            //calendar.Date;
            IFormatProvider culture = new System.Globalization.CultureInfo("en-US", true);
            DateTime dt2 = DateTime.Parse(date, culture, System.Globalization.DateTimeStyles.AssumeLocal);
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials
            }
            );

            var googleCalendarEvent = new Event();

            googleCalendarEvent.Start = new EventDateTime()
            {
                DateTimeDateTimeOffset = dt2
            };

            googleCalendarEvent.End = new EventDateTime()
            {
                DateTimeDateTimeOffset = dt2
            };
            var emails = Convert.ToString(calendar.Email.Email);
            googleCalendarEvent.Summary = calendar.Title;
            googleCalendarEvent.Description = calendar.Description;
            googleCalendarEvent.Attendees = new List<EventAttendee>()
          {
            new EventAttendee() { Email=emails }
          };

            var calendarId = "primary";
            Event results = await service.Events.Insert(googleCalendarEvent, calendarId).ExecuteAsync();
            return true;
        }

        private static string Base64UrlEncode(string input)
        {
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            // Special "url-safe" base64 encode.
            return Convert.ToBase64String(inputBytes)
              .Replace('+', '-')
              .Replace('/', '_')
              .Replace("=", "");
        }
        [HttpPost]
        public async Task<IActionResult> CalendarSearch(string email)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var employess = _context.Users
                       .Include(e => e.Customers)
                       .Select(m => m.Email).ToList();
            var filteremployes = employess.Where(word => word.Contains(email)).ToList();
            ViewBag.Filteremployes = filteremployes;
            if (filteremployes == null)
            {
                return NotFound();
            }
            return Json(new { Message = filteremployes });
            //var employess = _context.Employees.Select(m => m.EmployeeEmail).ToList();
            //var filteremployes = employess.Where(word => word.Contains(email)).ToList();
            //ViewBag.Filteremployes = filteremployes;
            //if (filteremployes == null)
            //{
            //    return NotFound();
            //}
            //return Json(new { Message = filteremployes });
       
        }
    }


}


