using CrmCorner.Models;
using CrmCorner.Models.Enums;
using CrmCorner.Services;
using CrmCorner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;



namespace CrmCorner.Controllers
{
    [Authorize]
    public class ApolloPullController : Controller
    {
        private readonly CrmCornerContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ApolloService _apolloService;
        private readonly IHttpClientFactory _httpClientFactory;

        public ApolloPullController(CrmCornerContext context, UserManager<AppUser> userManager, ApolloService apolloService, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _userManager = userManager;
            _apolloService = apolloService;
            _httpClientFactory = httpClientFactory;
        }

        //[Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult FetchContacts()
        {
            // İlk açılışta boş model ve boş liste
            ViewBag.Labels = new List<SelectListItem>();
            return View(new ApolloApiViewModel());
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> FetchContacts(ApolloApiViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ApiKey))
            {
                ModelState.AddModelError("", "API anahtarı zorunludur.");
                ViewBag.Labels = new List<SelectListItem>();
                return View(model);
            }

            string currentUserId = _userManager.GetUserId(User);

            var allContacts = new List<ApolloContactViewModel>();
            var labelList = new List<SelectListItem>();
            int page = 1;
            int perPage = 100;
            int retryDelay = 1000;

            using var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.apollo.io/");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("x-api-key", model.ApiKey);

            // 🔹 Etiketleri çek
            var labelRequest = new StringContent("{}", Encoding.UTF8, "application/json");
            var labelResponse = await client.PostAsync("api/v1/labels/search", labelRequest);

            if (labelResponse.IsSuccessStatusCode)
            {
                var labelJson = await labelResponse.Content.ReadAsStringAsync();
                var parsedLabel = JObject.Parse(labelJson);
                var labels = parsedLabel["labels"]?.ToObject<List<ApolloLabelViewModel>>();
                if (labels != null)
                {
                    labelList = labels.Select(l => new SelectListItem
                    {
                        Value = l.Id,
                        Text = l.Name
                    }).ToList();
                }
            }
            else
            {
                ViewBag.Error = $"Etiketler yüklenemedi. Status: {labelResponse.StatusCode}";
            }

            ViewBag.Labels = labelList;

            // 🔄 Kişileri sayfa sayfa çek
            while (true)
            {
                var requestBody = new
                {
                    page = page,
                    per_page = perPage,
                    label_ids = string.IsNullOrEmpty(model.SelectedLabelId) ? null : new[] { model.SelectedLabelId },

        //            // 🔽 Tarih filtresini Apollo API'ye ekle
        //            updated_at = (model.StartDate.HasValue && model.EndDate.HasValue)
        //? new
        //{
        //    from = model.StartDate.Value.ToString("yyyy-MM-dd"),
        //    to = model.EndDate.Value.ToString("yyyy-MM-dd")
        //}
        //: null

                };

                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("api/v1/contacts/search", content);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        await Task.Delay(retryDelay);
                        retryDelay *= 2;
                        continue;
                    }

                    ViewBag.Error = $"Kişiler alınamadı. Status: {response.StatusCode}";
                    break;
                }

                retryDelay = 1000;

                var resultString = await response.Content.ReadAsStringAsync();
                var parsed = JObject.Parse(resultString);
                var contactsJson = parsed["contacts"]?.ToString();
                var contactList = JsonConvert.DeserializeObject<List<ApolloContactViewModel>>(contactsJson);

                if (contactList == null || contactList.Count == 0)
                    break;

                allContacts.AddRange(contactList);

                int totalPages = (int)Math.Ceiling(
                    (parsed["pagination"]?["total_entries"]?.Value<double>() ?? 0) / perPage
                );

                if (page >= totalPages)
                    break;

                page++;
            }

            // ✅ TOPLU KONTROL İÇİN PERSON ID VE EMAIL LİSTESİ
            var personIdList = allContacts
                .Where(c => !string.IsNullOrWhiteSpace(c.PersonId))
                .Select(c => c.PersonId)
                .ToList();

            var emailList = allContacts
                .Where(c => !string.IsNullOrWhiteSpace(c.Email))
                .Select(c => c.Email.ToLower())
                .ToList();

            // ✅ VAR OLANLARI ÖNCEDEN ÇEK
            var existingContacts = await _context.ApolloContacts
                .Where(x =>
                    (personIdList.Contains(x.PersonId) || emailList.Contains(x.Email.ToLower()))
                    && x.UserId == currentUserId)
                .ToListAsync();

            var existingByPersonId = existingContacts
                .Where(x => !string.IsNullOrWhiteSpace(x.PersonId))
                .GroupBy(x => x.PersonId)
                .ToDictionary(g => g.Key, g => g.First());

            var existingByEmail = existingContacts
                .Where(x => !string.IsNullOrWhiteSpace(x.Email))
                .GroupBy(x => x.Email.ToLower())
                .ToDictionary(g => g.Key, g => g.First());

            // 💾 KAYDET / GÜNCELLE
            foreach (var contact in allContacts)
            {
                ApolloContactDbModel existing = null;

                if (!string.IsNullOrWhiteSpace(contact.PersonId) &&
                    existingByPersonId.TryGetValue(contact.PersonId, out var pidMatch))
                {
                    existing = pidMatch;
                }
                else if (!string.IsNullOrWhiteSpace(contact.Email) &&
                    existingByEmail.TryGetValue(contact.Email.ToLower(), out var emailMatch))
                {
                    existing = emailMatch;
                }

                if (existing == null)
                {
                    _context.ApolloContacts.Add(new ApolloContactDbModel
                    {
                        Email = contact.Email,
                        PersonId = contact.PersonId,
                        FirstName = contact.FirstName,
                        LastName = contact.LastName,
                        Title = contact.Title,
                        CompanyName = contact.CompanyName,
                        UpdatedAt = contact.UpdatedAt,
                        SourceLabelId = model.SelectedLabelId,
                        SourceLabelName = labelList.FirstOrDefault(l => l.Value == model.SelectedLabelId)?.Text,
                        UserId = currentUserId,
                        CreatedAt = DateTime.UtcNow,
                        LinkedinUrl = contact.LinkedinUrl,
                        Headline = contact.Headline,
                        Location = contact.Location
                    });
                }
                else if (existing.UpdatedAt < contact.UpdatedAt)
                {
                    existing.FirstName = contact.FirstName;
                    existing.LastName = contact.LastName;
                    existing.Title = contact.Title;
                    existing.CompanyName = contact.CompanyName;
                    existing.UpdatedAt = contact.UpdatedAt;
                    existing.SourceLabelId = model.SelectedLabelId;
                    existing.SourceLabelName = labelList.FirstOrDefault(l => l.Value == model.SelectedLabelId)?.Text;
                    existing.LinkedinUrl = contact.LinkedinUrl;
                    existing.Headline = contact.Headline;
                    existing.Location = contact.Location;
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Burada kullanıcıya uyarı gösterebilirsin
                ModelState.AddModelError("", "Bu kişi zaten sistemde mevcut.");
            }

            ViewBag.ContactList = allContacts;
            ViewBag.TotalCount = allContacts.Count;
            return View(model);
        }


        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> ContactsList()
        {
            var currentUserId = _userManager.GetUserId(User);

            var contacts = await _context.ApolloContacts
                .Where(x => x.UserId == currentUserId)
                .OrderByDescending(x => x.UpdatedAt)
                .ToListAsync();

            return View(contacts);
        }


        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var token = await _context.ApolloSettings
                .Where(x => x.UserId == userId)
                .Select(x => x.ApolloApiToken)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Apollo API Tokenı bulunamadı.";
                return RedirectToAction("Index", "ApolloSettings");
            }

            try
            {
                var users = await _apolloService.GetContactsAsync(token);
                return View("Index", users); // View name 'GetUsers.cshtml' olacak
            }
            catch (Exception ex)
            {
                return Content($"HATA: {ex.Message}");
            }
        }


        [HttpPost]
        public async Task<IActionResult> TransferToTasks(List<int> selectedIds)
        {
            var contacts = _context.ApolloContacts
                .Where(c => selectedIds.Contains(c.Id))
                .ToList();

            var currentUserId = _userManager.GetUserId(User);

            foreach (var contact in contacts)
            {
                var customer = new CustomerN
                {
                    Name = contact.FirstName,
                    Surname = contact.LastName,
                    CompanyName = contact.CompanyName,
                    PhoneNumber = contact.Phone,
                    CustomerEmail = contact.Email,
                    LinkedinUrl = contact.LinkedinUrl,
                    AppUserId = currentUserId,
                    CreatedDate = DateTime.UtcNow,
                    Source = "Apollo"
                };

                _context.CustomerNs.Add(customer);
                await _context.SaveChangesAsync(); // Müşteri kaydedildi, Id oluştu

                var pipelineTask = new PipelineTask
                {
                    Title = $"{contact.FirstName} {contact.LastName} - {contact.CompanyName}",
                    Description = "Apollo'dan aktarıldı.",
                    CreatedDate = DateTime.UtcNow,
                    CustomerName = contact.FirstName,
                    CustomerSurname = contact.LastName,
                    CompanyName = contact.CompanyName,
                    Phone = contact.Phone,
                    Email = contact.Email,
                    LinkedinUrl = contact.LinkedinUrl,
                    AppUserId = currentUserId,
                    CustomerId = customer.Id,
                    Stage = PipelineStage.Degerlendirilen, // varsayılan ilk aşama
                    Source = "Apollo",
                    SourceChannel = SourceChannelType.Apollo
                };

                _context.PipelineTasks.Add(pipelineTask);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Seçilen kişiler başarıyla görev olarak aktarıldı.";
            return RedirectToAction("ContactsList");
        }








    }
}