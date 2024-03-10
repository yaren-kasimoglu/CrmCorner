using CrmCorner.Models;
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
        public CalendarController(CrmCornerContext context, IGoogleCalendarService service, UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _googleCalendarService = service;
            _configuration = configuration;
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
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    if (!String.IsNullOrEmpty(Calendar.Email.ToString()))
                    {
                        var emailSend = sendEmailAsync(Calendar);

                    }
                    Calendar.UserId = currentUser.Id;
                    _context.Calendars.Add(Calendar);
                    _context.SaveChanges();

                    return RedirectToAction("Calendar");
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
        public ActionResult OauthRedirect()
        {
            string jsonDosyaYolu = "/Users/oznurkaraburun/Documents/GitHub/CrmCorner/CrmCorner/credentials.json";
            JObject tokens = JObject.Parse(System.IO.File.ReadAllText(jsonDosyaYolu));
            var redirectUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize?" +
                "&scope" + tokens["scopes"].ToString() + "&response_type=code" +
                "&response_mode=query" + "&state=themessydeveloper" +
                "&redirect_uri" + tokens["redirect_url"].ToString() +
                "&client_id=" + tokens["client_id"].ToString();
            return Redirect(redirectUrl);
        }

        public async Task<ActionResult> sendEmailAsync(Calendar calendar)
        {

            string userEmail = "oznurr03@hotmail.com";

            string clientId = "514f1749-aa2d-4ec2-8605-424d6ecb8e21";
            string clientSecret = "pPa8Q~WdI1jY6EHQmBzGYJAIGMgy2VVBJG_.aak3";
            string tenantId = "d8ece3f8-f16b-4d0c-86b1-69c00f4af99b";
            //var confidentialClientApplication = ConfidentialClientApplicationBuilder
            //    .Create(clientId)
            //    .WithClientSecret(clientSecret)
            //    .WithTenantId(tenantId)
            //    .Build();

            try
            {
                GraphClient clienta= new GraphClient();

                clienta.ClientId = "db2f7fd4-7abb-4052-a15f-f2ebdea3d3ba";
                clienta.Tenant = "d8ece3f8-f16b-4d0c-86b1-69c00f4af99b";
                clienta.ClientSecret = "e9w8Q~6y7XYLDxHbEcQF5ebS26tzBQ9LaHbRXc3K";

                var confidentialClient = ConfidentialClientApplicationBuilder
                    .Create(clienta.ClientId)
                    .WithClientSecret(clienta.ClientSecret)
                    .WithAuthority(new Uri($"https://login.microsoftonline.com/{clienta.Tenant}"))
                    .WithRedirectUri("https://localhost")
                    .Build();
                string[] scopess = new string[] { "https://graph.microsoft.com/.default" };

                // Retrieve an access token for Microsoft Graph (gets a fresh token if needed).
                var authResult = await confidentialClient
                        .AcquireTokenForClient(scopess)
                        .ExecuteAsync();
                var token = authResult.AccessToken;
                Console.WriteLine(token);
                string accessTokens = token;
                string endpoint = "https://graph.microsoft.com/v1.0/me/events";

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessTokens);

                var requestBody = "{\"subject\": \"Meeting\",\"start\": {\"dateTime\": \"2024-03-01T12:00:00\",\"timeZone\": \"UTC\"},\"end\": {\"dateTime\": \"2024-03-01T13:00:00\",\"timeZone\": \"UTC\"}}";
                var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(endpoint, content);            // Build the Microsoft Graph client. As the authentication provider, set an async lambda
                // which uses the MSAL client to obtain an app-only access token to Microsoft Graph,
                // and inserts this access token in the Authorization header of each API request. 
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Event created successfully.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to create event. Status code: {response.StatusCode}, Error: {errorContent}");
                }
                Independentsoft.Graph.Calendars.Event appointment = new Independentsoft.Graph.Calendars.Event();
                appointment.Subject = "Development meeting";
                appointment.Body = new Independentsoft.Graph.ItemBody("Body text can be html or plain text. We set it now as a plain text.");
                appointment.Start = new Independentsoft.Graph.DateTimeTimeZone(DateTime.Now);
                appointment.End = new Independentsoft.Graph.DateTimeTimeZone(DateTime.Now);
                //  googleCalendarEvent.Attendees = new List<EventAttendee>()
                //{
                //  new EventAttendee() { Email=emails }
                //};
                List<Attendee> attendees = new List<Attendee>();

             
                Attendee attendee1 = new Attendee();
                attendee1.EmailAddress = new Independentsoft.Graph.EmailAddress("oznurkaraburun@hotmail.com");
                attendees.Add(attendee1);
                appointment.Attendees.Add(attendee1);


                Independentsoft.Graph.Calendars.Event createdAppointment = await clienta.CreateEvent(appointment);

                Console.WriteLine("Id = " + createdAppointment.Id);

                Console.Read();
            }
            catch (GraphException ex)
            {
                Console.WriteLine("Error: " + ex.Code);
                Console.WriteLine("Message: " + ex.Message);
                Console.Read();
            }
            // var t=OauthRedirect();
            string jsonDosyaYolu = "/Users/oznurkaraburun/Documents/GitHub/CrmCorner/CrmCorner/credentials.json";
            JObject tokens = JObject.Parse(System.IO.File.ReadAllText(jsonDosyaYolu));

            RestClient client = new RestClient("https://graph.microsoft.com/v1.0/me/calendar/events");
            RestRequest restRequest = new RestRequest();

            restRequest.AddHeader("Authorization", "Bearer " + tokens["access_token"].ToString());
            restRequest.AddHeader("Content-Type", "application/json");
            restRequest.AddParameter("application/json", JsonConvert.SerializeObject(calendar)); //model yap

            var responses = client.Post(restRequest);
            if (responses.StatusCode == System.Net.HttpStatusCode.Created)
                ViewBag.Success = "başarılı";


            string accessToken = "YourAccessToken"; // Outlook REST API'ye erişim için alınan erişim belirtecini içerir
            string calendarId = "YourCalendarId"; // Randevunun ekleneceği takvimin kimliği

            // OutlookCalendarService sınıfını kullanarak randevu oluştur
            OutlookCalendarService outlookService = new OutlookCalendarService(accessToken, calendarId);
            DateTime startTime = DateTime.UtcNow; // Randevunun başlangıç zamanı (UTC)
            DateTime endTime = startTime.AddHours(1); // Randevunun bitiş zamanı (1 saat sonrası)
            await outlookService.AddAppointmentToOutlookAsync("Toplantı", startTime, endTime, "Ofis");            // Takvim etkinliği verilerini JSON formatına çevir
            //var jsonEventData = JsonConvert.SerializeObject(eventData);

            //// HTTP POST isteği oluştur
            //var httpClient = new HttpClient();
            //httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + AccessToken);
            //var content = new StringContent(jsonEventData, Encoding.UTF8, "application/json");

            //// POST isteğini gönder
            //var response = await httpClient.PostAsync(ApiBaseUrl, content);

            //// Yanıtı kontrol et
            //if (response.IsSuccessStatusCode)
            //{
            //    Console.WriteLine("Takvim etkinliği başarıyla eklendi.");
            //}
            //else
            //{
            //    Console.WriteLine("Takvim etkinliği eklenirken bir hata oluştu. Hata kodu: " + response.StatusCode);
            //}








            //  string clientId = "928553971028-n6q5dv2pjg12mvnajbrj1q8g7kee6djp.apps.googleusercontent.com";
            //  string clientSecret = "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";

            //  string[] scopes = { "https://www.googleapis.com/auth/calendar" };

            //  var credentials = GoogleWebAuthorizationBroker.AuthorizeAsync(
            //      new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
            //      scopes, "user", CancellationToken.None).Result;

            //  string date = calendar.Date;
            //  //calendar.Date;
            //  IFormatProvider culture = new System.Globalization.CultureInfo("en-US", true);
            //  DateTime dt2 = DateTime.Parse(date, culture, System.Globalization.DateTimeStyles.AssumeLocal);
            //  var service = new CalendarService(new BaseClientService.Initializer()
            //  {
            //      HttpClientInitializer = credentials
            //  }
            //  );

            //  var googleCalendarEvent = new Event();

            //  googleCalendarEvent.Start = new EventDateTime()
            //  {
            //      DateTimeDateTimeOffset = dt2
            //  };

            //  googleCalendarEvent.End = new EventDateTime()
            //  {
            //      DateTimeDateTimeOffset = dt2
            //  };
            //  var emails = Convert.ToString(calendar.Email.Email);
            //  googleCalendarEvent.Summary = calendar.Title;
            //  googleCalendarEvent.Description = calendar.Description;
            //  googleCalendarEvent.Attendees = new List<EventAttendee>()
            //{
            //  new EventAttendee() { Email=emails }
            //};

            //  var calendarId = "primary";
            //  Event results = await service.Events.Insert(googleCalendarEvent, calendarId).ExecuteAsync();
            return View("Calendar");
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


