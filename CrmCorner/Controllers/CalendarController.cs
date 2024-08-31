﻿using CrmCorner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Calendar = CrmCorner.Models.Calendar;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Azure.Identity;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using System.Text;
using System.Net.Http;
using Microsoft.Exchange.WebServices.Data;
using Independentsoft.Graph;
using Independentsoft.Graph.Calendars;
using Attendee = Independentsoft.Graph.Calendars.Attendee;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Net;
using CrmCorner.OptionsModels;
using Microsoft.Extensions.Options;
using CrmCorner.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Humanizer;
using Microsoft.Office.Interop.Outlook;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.VisualBasic;

namespace CrmCorner.Controllers
{
    public class CalendarController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly CrmCornerContext _context;
        private readonly IGoogleCalendarService _googleCalendarService;
        private readonly IConfiguration _configuration;
        private const string AccessToken = "9f9f7969-0154-4719-af42-66600536dec4";
        private const string ApiBaseUrl = "https://outlook.office.com/api/v2.0/me/events";
        private readonly string calendarId;
        private readonly SmtpSettings _smtpSettings;



        public CalendarController(CrmCornerContext context, IGoogleCalendarService service, UserManager<AppUser> userManager, IConfiguration configuration, IOptions<SmtpSettings> options)
        {
            _context = context;
            _userManager = userManager;
            _googleCalendarService = service;
            _configuration = configuration;
            _smtpSettings = options.Value;
        }
        public async Task<IActionResult> Calendar()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

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
                ViewBag.Users = _context.Users
                    .Where(u=>u.Id!=currentUser.Id && u.CompanyId==currentUser.CompanyId)
                    .Select(u => new SelectListItem
                {
                    Value = u.Email,
                    Text = u.UserName // veya başka bir kullanıcı adı alanı
                }).ToList();
                return View("Calendar");
            }

            else
            {
                ViewBag.ErrorMessage = "Geçerli kullanıcı bilgisi bulunamadı.";
                return View();


            }

        }




        [HttpPost]
        public async Task<IActionResult> CalendarAdd(Calendar Calendar)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                string[] emailArray = null;
                var dateFormat = "yyyy-MM-dd";
                if (Calendar.SelectedEmails!=null &&Calendar.SelectedEmails.Count>0 && Calendar.SelectedEmails[0]!=null)
                {
                    string lastEmailString = Calendar.SelectedEmails[Calendar.SelectedEmails.Count - 1];
                    emailArray = lastEmailString.Split(',');
                    foreach (string email in emailArray)
                    {
                        var currentUsers = await _userManager.FindByEmailAsync(email);
                        if (currentUsers != null)
                        {
                            Calendar.UserId = currentUsers.Id;
                            Calendar.Id = 0;
                            _context.Calendars.Add(Calendar);
                            _context.SaveChanges();
                        }
                    }
                    var emailSend = sendEmailAsync2(currentUser.Email,Calendar);
                }
                string dateStr = Calendar.Date;

                DateTime emailPropertyDateTime = Calendar.EmailProperty.StartDate;
                DateTime emailPropertyDateTimeEnd = Calendar.EmailProperty.EndDate;
                DateTime datePart = DateTime.ParseExact(dateStr, dateFormat, null);
                Calendar.UserId = currentUser.Id;
                Calendar.Email = emailArray != null ? string.Join(",", emailArray) : "bos";
                Calendar.StartDate = new DateTime(
                    datePart.Year, // Yıl
                    datePart.Month, // Ay
                    datePart.Day, // Gün
                    emailPropertyDateTime.Hour, // Saat
                    emailPropertyDateTime.Minute, // Dakika
                    emailPropertyDateTime.Second // Saniye
                );
                Calendar.EndDate = new DateTime(
                                   datePart.Year, // Yıl
                                   datePart.Month, // Ay
                                   datePart.Day, // Gün
                                   emailPropertyDateTimeEnd.Hour, // Saat
                                   emailPropertyDateTimeEnd.Minute, // Dakika
                                   emailPropertyDateTimeEnd.Second // Saniye
                               );
                _context.Calendars.Add(Calendar);
                _context.SaveChanges();

                return RedirectToAction("Calendar");
            }

            return Json(new { Message = true });

        }

        [HttpPost]
        public async Task<IActionResult> Edit(Calendar model)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var eventToUpdate = await _context.Set<Calendar>().FindAsync(model.Id);
                if (eventToUpdate != null)
                {
                    eventToUpdate.Title = model.Title;
                    eventToUpdate.Description = model.Description;
                    eventToUpdate.Date = model.Date;
                    eventToUpdate.UserId = currentUser.Id;
                string dateStr = model.Date;
                var dateFormat = "yyyy-MM-dd";
                DateTime emailPropertyDateTime = model.EmailProperty.StartDate;
                DateTime emailPropertyDateTimeEnd = model.EmailProperty.EndDate;
                DateTime datePart = DateTime.ParseExact(dateStr, dateFormat, null);
                model.StartDate = new DateTime(
                    datePart.Year, // Yıl
                    datePart.Month, // Ay
                    datePart.Day, // Gün
                    emailPropertyDateTime.Hour, // Saat
                    emailPropertyDateTime.Minute, // Dakika
                    emailPropertyDateTime.Second // Saniye
                );
                model.EndDate = new DateTime(
                     datePart.Year, // Yıl
                     datePart.Month, // Ay
                     datePart.Day, // Gün
                     emailPropertyDateTimeEnd.Hour, // Saat
                     emailPropertyDateTimeEnd.Minute, // Dakika
                     emailPropertyDateTimeEnd.Second // Saniye
                );
                eventToUpdate.StartDate = model.StartDate;
                eventToUpdate.EndDate= model.EndDate;
                await _context.SaveChangesAsync();
                    return RedirectToAction("Calendar"); // Başarılı işlemi belirten bir sayfaya yönlendirin.
                }
            return RedirectToAction("Calendar"); // Başarılı işlemi belirten bir sayfaya yönlendirin.

        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var eventToDelete = await _context.Set<Calendar>().FindAsync(id);
            if (eventToDelete != null)
            {
                _context.Set<Calendar>().Remove(eventToDelete);
                await _context.SaveChangesAsync();
                return RedirectToAction("Calendar"); // Başarılı işlemi belirten bir sayfaya yönlendirin.
            }
            return NotFound(); // Event bulunamazsa
        }
        [HttpPost]
        //public async Task<IActionResult> CalendarUpdate(int? ID, string title,string description, string date,DateTime startdate,DateTime enddate)
        //{
        //    var currentUser = await _userManager.GetUserAsync(User);
        //    Calendar calendar = new Calendar { Id = ID.Value, Title = title, Date = date ,UserId= currentUser.Id,Description=description};
        //    if (ModelState.IsValid)
        //    {
        //        _context.Calendars.Update(calendar);
        //        _context.SaveChanges();

        //        return Json(new { Message = "success" });
        //    }
        //    return View("Calendar");
        //}

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
            
                var calendarDto = new Calendar
                {
                    Id = calendar.Id,
                    Description = calendar.Description,
                    StartDate=calendar.StartDate,
                    EndDate=calendar.EndDate,
                    Email=calendar.Email
                    // Diğer gerekli alanlar
                };
                return Json(new { Message = calendarDto });

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
        
        public async Task<ActionResult> sendEmailAsync2(string from , Calendar calendar)
        {
            try
            {
                
                string fromEmail = from;
                string subject = calendar.Title;               
                string body = "Merhaba,Yukarıda gönderilen event size "+calendar.Description+" açıklaması ile atanmıştır. Lütfen takviminize ekleyiniz.. ";

                DateTime startTime = calendar.EmailProperty.StartDate;

                DateTime endTime = calendar.EmailProperty.EndDate;
                string toEmails = null;
                if (calendar.SelectedEmails.Any())
                {
                    var lastSelectedEmail = calendar.SelectedEmails.Last();
                   toEmails = string.Join(",", lastSelectedEmail);
                    // lastSelectedEmail'i kullan
                }
            

                // iCalendar dosyası oluşturma
                string icsContent = CreateICS(subject, body, startTime, endTime, fromEmail, toEmails);
                System.Net.Mail.Attachment calendarAttachment = new System.Net.Mail.Attachment(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(icsContent)), "invite.ics", "text/calendar");

                // SMTP sunucusu bilgileri
                var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
                {
                    Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                    EnableSsl = _smtpSettings.EnableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpSettings.Username),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };

                //MailMessage mailMessage = new MailMessage
                //{
                //    From = new MailAddress(fromEmail), // İstediğiniz "From" adresini burada belirleyin
                //    Subject = subject,
                //    Body = body,
                //    IsBodyHtml = true
                //};
                foreach (var email in calendar.SelectedEmails)
                {
                    mailMessage.To.Add(email);
                }

                mailMessage.Attachments.Add(calendarAttachment);

                mailMessage.To.Add(fromEmail);
                // E-posta gönderimi
                client.Send(mailMessage);

                Console.WriteLine("Toplantı daveti başarıyla gönderildi.");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Toplantı daveti gönderimi sırasında bir hata oluştu: " + ex.Message);
            }
            return RedirectToAction("Calendar");
        }
        static string CreateICS(string subject, string description, DateTime startTime, DateTime endTime, string fromEmail, string toEmails)
        {
            string[] emailsArray = toEmails.Split(',');

            StringBuilder ics = new StringBuilder();
            ics.AppendLine("BEGIN:VCALENDAR");
            ics.AppendLine("VERSION:2.0");
            ics.AppendLine("PRODID:-//Your Company//Your Product//EN");
            ics.AppendLine("METHOD:REQUEST");
            ics.AppendLine("BEGIN:VEVENT");
            ics.AppendLine($"DTSTART:{startTime:yyyyMMddTHHmmssZ}");
            ics.AppendLine($"DTEND:{endTime:yyyyMMddTHHmmssZ}");
            ics.AppendLine($"SUMMARY:{subject}");
            ics.AppendLine($"DESCRIPTION:{description}");
            ics.AppendLine($"UID:{Guid.NewGuid()}");
            foreach (var toEmail in emailsArray)
            {
                ics.AppendLine($"ATTENDEE;CN=\"Takvim\";RSVP=TRUE:mailto:{toEmail}");
            }
            ics.AppendLine($"ORGANIZER;CN=\"{fromEmail}\":mailto:{fromEmail}");
            ics.AppendLine("END:VEVENT");
            ics.AppendLine("END:VCALENDAR");



            return ics.ToString();
        }
       

       



    }
}