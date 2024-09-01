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
using static Google.Protobuf.Reflection.UninterpretedOption.Types;

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
                List<string> validEmails = new List<string>();

                if (Calendar.SelectedEmails != null && Calendar.SelectedEmails.Count > 0 && Calendar.SelectedEmails[0] != null)
                {
                    string lastEmailString = Calendar.SelectedEmails[Calendar.SelectedEmails.Count - 1];
                    emailArray = lastEmailString.Split(',');

                    foreach (string email in emailArray)
                    {
                        var currentUsers = await _userManager.FindByEmailAsync(email);
                        if (currentUsers != null)
                        {

                            Calendar newCalendar = new Calendar
                            {
                                UserId = currentUsers.Id,
                                Title = Calendar.Title,
                                Description = Calendar.Description,
                                Date = Calendar.Date,
                                EmailProperty = Calendar.EmailProperty,
                                Email = email,
                            };


                            DateTime datePart = DateTime.ParseExact(Calendar.Date, dateFormat, null);
                            DateTime emailPropertyDateTime = Calendar.EmailProperty.StartDate;
                            DateTime emailPropertyDateTimeEnd = Calendar.EmailProperty.EndDate;

                            newCalendar.StartDate = new DateTime(
                                datePart.Year, datePart.Month, datePart.Day,
                                emailPropertyDateTime.Hour, emailPropertyDateTime.Minute, emailPropertyDateTime.Second
                            );
                            newCalendar.EndDate = new DateTime(
                                datePart.Year, datePart.Month, datePart.Day,
                                emailPropertyDateTimeEnd.Hour, emailPropertyDateTimeEnd.Minute, emailPropertyDateTimeEnd.Second
                            );

                            _context.Calendars.Add(newCalendar);
                            _context.SaveChanges();//
                        
                            validEmails.Add(email);
                        }
                        else
                        {
                            validEmails.Add(email);
                        }
                    }

                    // Tüm geçerli e-posta adreslerine toplu e-posta gönderimi
                    if (validEmails.Count > 0)
                    {
                        foreach (var email in validEmails)
                        {
                            await sendEmailAsync2(email, Calendar);
                        }
                    }
                }
                string mail = null;
                if (validEmails.Count > 0){
                    mail = string.Join(",", validEmails);

                }
                Calendar newCalendars = new Calendar
                {
                    UserId = currentUser.Id,
                    Title = Calendar.Title,
                    Description = Calendar.Description,
                    Date = Calendar.Date,
                    EmailProperty = Calendar.EmailProperty,
                    Email = mail!=null?mail:"bos",
                };


                DateTime dateParts = DateTime.ParseExact(Calendar.Date, dateFormat, null);
                DateTime emailPropertyDateTimes = Calendar.EmailProperty.StartDate;
                DateTime emailPropertyDateTimeEnds = Calendar.EmailProperty.EndDate;

                newCalendars.StartDate = new DateTime(
                    dateParts.Year, dateParts.Month, dateParts.Day,
                    emailPropertyDateTimes.Hour, emailPropertyDateTimes.Minute, emailPropertyDateTimes.Second
                );
                newCalendars.EndDate = new DateTime(
                    dateParts.Year, dateParts.Month, dateParts.Day,
                    emailPropertyDateTimeEnds.Hour, emailPropertyDateTimeEnds.Minute, emailPropertyDateTimeEnds.Second
                );

                AddCompany(newCalendars);
                return RedirectToAction("Calendar");
            }

            return Json(new { Message = true });
        }

        [HttpPost]
        public bool AddCompany(Calendar model)
        {
            
            _context.Calendars.Add(model);
            _context.SaveChanges();
            return true;
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Calendar model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            string[] emailArray = null;

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
            
                if (model.SelectedEmails != null && model.SelectedEmails.Count > 0 && model.SelectedEmails[0] != null)
                {
                    var filteredEmails = model.SelectedEmails
                                    .Where(email => !string.IsNullOrWhiteSpace(email) && email!= "bos")
                                  .ToList();
                    model.SelectedEmails = filteredEmails;
                    foreach (string email in filteredEmails)
                    {
                        var currentUsers = await _userManager.FindByEmailAsync(email);
                        if (currentUsers != null)
                        {
                            model.UserId = currentUsers.Id;
                            model.Id = 0;
                            _context.Calendars.Add(model);
                            _context.SaveChanges();
                        }
                    }
                    var emailSend = sendEmailAsync2(currentUser.Email, model);
                    model.Email = filteredEmails != null ? string.Join(",", filteredEmails) : "bos";
                    eventToUpdate.Email = model.Email;
                }
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
        public async Task<IActionResult> GetDescription(int? ID)
        {
            Calendar calendar = _context.Calendars.Find(ID);
            List<string> findUserCompany = new List<string>();
            List<string> notfindUserCompany = new List<string>();

            if (calendar == null)
            {
                return Json(new { Message = "error" });
            }
            if (calendar.Email != null)
            {
                var emailArray = calendar.Email.Split(',');
                foreach (var item in emailArray)
                {
                    if (item != "bos")
                    {
                        var name = await _userManager.FindByEmailAsync(item);
                        if (name != null)
                        {
                            findUserCompany.Add(item);
                        }
                        else
                        {
                            notfindUserCompany.Add(item);
                        }
                    }
                }
            }
           
            string resultString = string.Join(",", findUserCompany);
            string resultStringNot= string.Join(",", notfindUserCompany);

            var calendarDto = new Calendar
            {
                Id = calendar.Id,
                Description = calendar.Description,
                StartDate = calendar.StartDate,
                EndDate = calendar.EndDate,
                Email = resultString,
                NotEmail = resultStringNot
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
            TimeSpan gmtPlus3 = TimeSpan.FromHours(3);
            DateTime startTimeUtc = startTime - gmtPlus3;
            DateTime endTimeUtc = endTime - gmtPlus3;
            string[] emailsArray = toEmails.Split(',');

            StringBuilder ics = new StringBuilder();
            ics.AppendLine("BEGIN:VCALENDAR");
            ics.AppendLine("VERSION:2.0");
            ics.AppendLine("PRODID:-//Your Company//Your Product//EN");
            ics.AppendLine("METHOD:REQUEST");
            ics.AppendLine("BEGIN:VEVENT");
            ics.AppendLine($"DTSTART:{startTimeUtc:yyyyMMddTHHmmssZ}");
            ics.AppendLine($"DTEND:{endTimeUtc:yyyyMMddTHHmmssZ}");
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