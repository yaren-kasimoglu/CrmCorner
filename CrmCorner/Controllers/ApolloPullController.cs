using CrmCorner.Models;
using CrmCorner.Models.Enums;
using CrmCorner.Services;
using CrmCorner.ViewModels;
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
    // ApolloPullController
    // Apollo API'den kişi çekme ve pipeline aktarma işlemleri
    // Erişim: SuperAdmin, Admin, TeamLeader, TeamMember
    //[Authorize(Roles = "SuperAdmin,Admin,TeamLeader,TeamMember")]
    public class ApolloPullController : Controller
    {
        private readonly CrmCornerContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ApolloService _apolloService;
        private readonly IHttpClientFactory _httpClientFactory;

        public ApolloPullController(
            CrmCornerContext context,
            UserManager<AppUser> userManager,
            ApolloService apolloService,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _userManager = userManager;
            _apolloService = apolloService;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult FetchContacts()
        {
            ViewBag.Labels = new List<SelectListItem>();
            return View(new ApolloApiViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> FetchLabels(ApolloApiViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ApiKey))
            {
                ModelState.AddModelError("", "API anahtarı zorunludur.");
                model.Labels = new List<SelectListItem>();
                return View("FetchContacts", model);
            }

            using var client = CreateApolloClient(model.ApiKey);

            var labelRequest = new StringContent("{}", Encoding.UTF8, "application/json");
            var labelResponse = await client.PostAsync("api/v1/labels/search", labelRequest);

            if (!labelResponse.IsSuccessStatusCode)
            {
                var err = await labelResponse.Content.ReadAsStringAsync();
                ViewBag.Error = $"Etiketler alınamadı. Status: {(int)labelResponse.StatusCode} {labelResponse.StatusCode} | Body: {err}";
                model.Labels = new List<SelectListItem>();
                return View("FetchContacts", model);
            }

            var labelJson = await labelResponse.Content.ReadAsStringAsync();
            var parsedLabel = JObject.Parse(labelJson);
            var labels = parsedLabel["labels"]?.ToObject<List<ApolloLabelViewModel>>() ?? new List<ApolloLabelViewModel>();

            model.Labels = labels.Select(l => new SelectListItem
            {
                Value = l.Id,
                Text = l.Name
            }).ToList();

            return View("FetchContacts", model);
        }

        [HttpPost]
        public async Task<IActionResult> FetchContactsByLabel(ApolloApiViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ApiKey))
            {
                ModelState.AddModelError("", "API anahtarı zorunludur.");
                model.Labels = new List<SelectListItem>();
                return View("FetchContacts", model);
            }

            model.Labels = await GetLabels(model.ApiKey);

            if (string.IsNullOrWhiteSpace(model.SelectedLabelId))
            {
                ModelState.AddModelError("", "Lütfen bir liste (label) seçin.");
                return View("FetchContacts", model);
            }

            var currentUserId = _userManager.GetUserId(User);

            var me = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            var myCompanyId = me?.CompanyId;

            var allContacts = new List<ApolloContactViewModel>();
            int page = 1;
            int perPage = 100;
            int retryDelay = 1000;

            using var client = CreateApolloClient(model.ApiKey);

            while (true)
            {
                var requestBody = new
                {
                    page,
                    per_page = perPage,
                    label_ids = new[] { model.SelectedLabelId }
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(requestBody),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync("api/v1/contacts/search", content);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        await Task.Delay(retryDelay);
                        retryDelay *= 2;
                        continue;
                    }

                    var err = await response.Content.ReadAsStringAsync();
                    ViewBag.Error = $"Kişiler alınamadı. Status: {(int)response.StatusCode} {response.StatusCode} | Body: {err}";
                    break;
                }

                retryDelay = 1000;

                var resultString = await response.Content.ReadAsStringAsync();
                var parsed = JObject.Parse(resultString);

                var contactsJson = parsed["contacts"]?.ToString();
                var contactList = JsonConvert.DeserializeObject<List<ApolloContactViewModel>>(contactsJson ?? "[]");

                if (contactList == null || contactList.Count == 0)
                    break;

                allContacts.AddRange(contactList);

                if (contactList.Count < perPage)
                    break;

                page++;
            }

            await SaveOrUpdateApolloContacts(allContacts, model, currentUserId, myCompanyId);

            var missingEmailCount = allContacts.Count(c => !HasRealEmail(c.Email));

            ViewBag.SuccessMessage = missingEmailCount > 0
                ? $"{allContacts.Count} kişi çekildi ve işlendi. {missingEmailCount} kişide e-posta bulunamadığı için sistem fallback e-posta oluşturdu."
                : $"{allContacts.Count} kişi çekildi ve işlendi.";

            ViewBag.TotalCount = allContacts.Count;
            ViewBag.MissingEmailCount = missingEmailCount;
            ViewBag.ContactList = allContacts.Take(200).ToList();

            return View("FetchContacts", model);
        }

        [HttpPost]
        public async Task<IActionResult> FetchContacts(ApolloApiViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ApiKey))
            {
                ModelState.AddModelError("", "API anahtarı zorunludur.");
                ViewBag.Labels = new List<SelectListItem>();
                return View(model);
            }

            var currentUserId = _userManager.GetUserId(User);

            var me = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            var myCompanyId = me?.CompanyId;

            var allContacts = new List<ApolloContactViewModel>();
            var labelList = new List<SelectListItem>();

            int page = 1;
            int perPage = 100;
            int retryDelay = 1000;

            var setting = await _context.ApolloSettings
                .FirstOrDefaultAsync(x => x.UserId == currentUserId);

            if (setting == null)
            {
                setting = new ApolloSettings
                {
                    UserId = currentUserId
                };

                _context.ApolloSettings.Add(setting);
                await _context.SaveChangesAsync();
            }

            int lastDays = model.LastDays.HasValue && (model.LastDays.Value == 15 || model.LastDays.Value == 30)
                ? model.LastDays.Value
                : 30;

            DateTime fromDate = setting.LastContactsSyncUtc ?? DateTime.UtcNow.Date.AddDays(-lastDays);
            DateTime toDate = DateTime.UtcNow.Date.AddDays(1);

            using var client = CreateApolloClient(model.ApiKey, useUpperCaseApiHeader: true);

            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

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
                var labelError = await labelResponse.Content.ReadAsStringAsync();
                ViewBag.Error = $"Etiketler yüklenemedi. Status: {(int)labelResponse.StatusCode} {labelResponse.StatusCode} | Body: {labelError}";
            }

            ViewBag.Labels = labelList;

            while (true)
            {
                object requestBody;
                var updatedAtFilter = new
                {
                    from = fromDate.ToString("yyyy-MM-dd"),
                    to = toDate.ToString("yyyy-MM-dd")
                };

                if (string.IsNullOrEmpty(model.SelectedLabelId))
                {
                    requestBody = new
                    {
                        page,
                        per_page = perPage,
                        updated_at = updatedAtFilter
                    };
                }
                else
                {
                    requestBody = new
                    {
                        page,
                        per_page = perPage,
                        label_ids = new[] { model.SelectedLabelId },
                        updated_at = updatedAtFilter
                    };
                }

                var json = JsonConvert.SerializeObject(requestBody, jsonSettings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/v1/contacts/search", content);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        await Task.Delay(retryDelay);
                        retryDelay *= 2;
                        continue;
                    }

                    var errorBody = await response.Content.ReadAsStringAsync();

                    if ((int)response.StatusCode == 422 &&
                        errorBody != null &&
                        errorBody.Contains("over threshold"))
                    {
                        ViewBag.Info = $"Apollo limiti nedeniyle en fazla {allContacts.Count} kayıt çekilebildi. Daha fazlası için label seçin veya tarih aralığını daraltın.";
                        break;
                    }

                    ViewBag.Error = $"Kişiler alınamadı. Status: {(int)response.StatusCode} {response.StatusCode} | Body: {errorBody}";
                    break;
                }

                retryDelay = 1000;

                var resultString = await response.Content.ReadAsStringAsync();
                var parsed = JObject.Parse(resultString);

                var contactsToken = parsed["contacts"];
                if (contactsToken == null)
                {
                    ViewBag.Error = $"Beklenen alan bulunamadı (contacts). Response: {resultString}";
                    break;
                }

                var contactsJson = contactsToken.ToString();
                var contactList = JsonConvert.DeserializeObject<List<ApolloContactViewModel>>(contactsJson);

                if (contactList == null || contactList.Count == 0)
                    break;

                allContacts.AddRange(contactList);

                if (contactList.Count < perPage)
                    break;

                page++;
            }

            var personIdList = allContacts
                .Where(c => !string.IsNullOrWhiteSpace(c.PersonId))
                .Select(c => c.PersonId)
                .ToList();

            var emailList = allContacts
                .Where(c => !string.IsNullOrWhiteSpace(c.Email))
                .Select(c => c.Email!.ToLower())
                .ToList();

            var existingContacts = await _context.ApolloContacts
                .Where(x =>
                    x.CompanyId == myCompanyId &&
                    (
                        (!string.IsNullOrEmpty(x.PersonId) && personIdList.Contains(x.PersonId)) ||
                        (!string.IsNullOrEmpty(x.Email) && emailList.Contains(x.Email.ToLower()))
                    ))
                .ToListAsync();

            var existingByPersonId = existingContacts
                .Where(x => !string.IsNullOrWhiteSpace(x.PersonId))
                .GroupBy(x => x.PersonId!)
                .ToDictionary(g => g.Key, g => g.First());

            var existingByEmail = existingContacts
                .Where(x => !string.IsNullOrWhiteSpace(x.Email))
                .GroupBy(x => x.Email!.ToLower())
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var contact in allContacts)
            {
                ApolloContactDbModel? existing = null;

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

                var safeEmail = HasRealEmail(contact.Email)
                    ? contact.Email!.Trim()
                    : BuildFallbackEmail(contact);

                if (existing == null)
                {
                    _context.ApolloContacts.Add(new ApolloContactDbModel
                    {
                        Email = safeEmail,
                        PersonId = contact.PersonId,
                        CompanyId = myCompanyId,
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
                else
                {
                    if (contact.UpdatedAt != DateTime.MinValue && existing.UpdatedAt < contact.UpdatedAt)
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
                        existing.CompanyId = myCompanyId;
                        existing.Location = contact.Location;

                        if (HasRealEmail(contact.Email))
                            existing.Email = contact.Email!.Trim();
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();

                setting.LastContactsSyncUtc = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Bazı kişiler zaten sistemde mevcut olabilir.");
            }

            var missingEmailCount = allContacts.Count(c => !HasRealEmail(c.Email));

            ViewBag.ContactList = allContacts;
            ViewBag.TotalCount = allContacts.Count;
            ViewBag.LastDaysUsed = lastDays;
            ViewBag.FromDateUsed = fromDate.ToString("yyyy-MM-dd");
            ViewBag.ToDateUsed = toDate.ToString("yyyy-MM-dd");
            ViewBag.MissingEmailCount = missingEmailCount;

            if (missingEmailCount > 0)
            {
                ViewBag.Info = $"{missingEmailCount} kişide e-posta bulunamadı. Bu kayıtlar fallback e-posta ile işlendi.";
            }

            return View(model);
        }

        public async Task<IActionResult> ContactsList()
        {
            var currentUserId = _userManager.GetUserId(User);

            var me = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            var myCompanyId = me?.CompanyId;

            var contacts = await _context.ApolloContacts
                .Where(x => x.CompanyId == myCompanyId)
                .OrderByDescending(x => x.UpdatedAt)
                .ToListAsync();

            return View(contacts);
        }

        [HttpPost]
        public async Task<IActionResult> ContactsListData()
        {
            var userId = _userManager.GetUserId(User);

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
            var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "25");
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            var orderColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
            var orderDir = Request.Form["order[0][dir]"].FirstOrDefault();

            var me = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            var myCompanyId = me?.CompanyId;

            var query = _context.ApolloContacts
                .AsNoTracking()
                .Where(x => x.CompanyId == myCompanyId);

            var recordsTotal = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                var s = searchValue.ToLower();
                query = query.Where(x =>
                    (x.FirstName != null && x.FirstName.ToLower().Contains(s)) ||
                    (x.LastName != null && x.LastName.ToLower().Contains(s)) ||
                    (x.Email != null && x.Email.ToLower().Contains(s)) ||
                    (x.CompanyName != null && x.CompanyName.ToLower().Contains(s)) ||
                    (x.Title != null && x.Title.ToLower().Contains(s)));
            }

            var recordsFiltered = await query.CountAsync();

            bool desc = (orderDir ?? "desc").ToLower() == "desc";
            switch (orderColumnIndex)
            {
                case "1":
                    query = desc ? query.OrderByDescending(x => x.FirstName) : query.OrderBy(x => x.FirstName);
                    break;
                case "2":
                    query = desc ? query.OrderByDescending(x => x.LastName) : query.OrderBy(x => x.LastName);
                    break;
                case "3":
                    query = desc ? query.OrderByDescending(x => x.Title) : query.OrderBy(x => x.Title);
                    break;
                case "4":
                    query = desc ? query.OrderByDescending(x => x.CompanyName) : query.OrderBy(x => x.CompanyName);
                    break;
                case "5":
                    query = desc ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email);
                    break;
                case "6":
                default:
                    query = desc
                        ? query.OrderByDescending(x => x.UpdatedAt)
                        : query.OrderBy(x => x.UpdatedAt);
                    break;
            }

            var data = await query
                .Skip(start)
                .Take(length)
                .Select(x => new
                {
                    id = x.Id,
                    firstName = x.FirstName,
                    lastName = x.LastName,
                    email = x.Email,
                    companyName = x.CompanyName,
                    title = x.Title,
                    updatedAt = x.UpdatedAt,
                    sourceLabelName = x.SourceLabelName,
                    linkedinUrl = x.LinkedinUrl
                })
                .ToListAsync();

            return Json(new
            {
                draw,
                recordsTotal,
                recordsFiltered,
                data
            });
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
                return View("Index", users);
            }
            catch (Exception ex)
            {
                return Content($"HATA: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> TransferToTasks(List<int> selectedIds)
        {
            var contacts = await _context.ApolloContacts
                .Where(c => selectedIds.Contains(c.Id))
                .ToListAsync();

            var currentUserId = _userManager.GetUserId(User);
            int missingEmailCount = 0;

            foreach (var contact in contacts)
            {
                var safeEmail = HasRealEmail(contact.Email)
                    ? contact.Email!.Trim()
                    : BuildFallbackEmail(contact);

                if (!HasRealEmail(contact.Email))
                    missingEmailCount++;

                var customer = new CustomerN
                {
                    Name = contact.FirstName,
                    Surname = contact.LastName,
                    CompanyName = contact.CompanyName,
                    PhoneNumber = contact.Phone,
                    CustomerEmail = safeEmail,
                    LinkedinUrl = contact.LinkedinUrl,
                    AppUserId = currentUserId,
                    CreatedDate = DateTime.UtcNow,
                    Source = "Apollo"
                };

                _context.CustomerNs.Add(customer);
                await _context.SaveChangesAsync();

                var pipelineTask = new PipelineTask
                {
                    Title = $"{contact.FirstName} {contact.LastName} - {contact.CompanyName}",
                    Description = HasRealEmail(contact.Email)
                        ? "Apollo'dan aktarıldı."
                        : "Apollo'dan aktarıldı. Orijinal e-posta bilgisi bulunamadığı için sistem fallback e-posta oluşturdu.",
                    CreatedDate = DateTime.UtcNow,
                    CustomerName = contact.FirstName,
                    CustomerSurname = contact.LastName,
                    CompanyName = contact.CompanyName,
                    Phone = contact.Phone,
                    Email = safeEmail,
                    LinkedinUrl = contact.LinkedinUrl,
                    AppUserId = currentUserId,
                    CustomerId = customer.Id,
                    Stage = PipelineStage.Degerlendirilen,
                    Source = "Apollo",
                    SourceChannel = SourceChannelType.Apollo
                };

                _context.PipelineTasks.Add(pipelineTask);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = missingEmailCount > 0
                ? $"Seçilen kişiler başarıyla aktarıldı. {missingEmailCount} kişide e-posta olmadığı için sistem fallback e-posta oluşturdu."
                : "Seçilen kişiler başarıyla görev olarak aktarıldı.";

            return RedirectToAction("ContactsList");
        }

        private async Task<List<SelectListItem>> GetLabels(string apiKey)
        {
            using var client = CreateApolloClient(apiKey);

            var labelRequest = new StringContent("{}", Encoding.UTF8, "application/json");
            var labelResponse = await client.PostAsync("api/v1/labels/search", labelRequest);

            if (!labelResponse.IsSuccessStatusCode)
                return new List<SelectListItem>();

            var labelJson = await labelResponse.Content.ReadAsStringAsync();
            var parsedLabel = JObject.Parse(labelJson);

            var labels = parsedLabel["labels"]?.ToObject<List<ApolloLabelViewModel>>() ?? new List<ApolloLabelViewModel>();

            return labels.Select(l => new SelectListItem
            {
                Value = l.Id,
                Text = l.Name
            }).ToList();
        }

        private async Task SaveOrUpdateApolloContacts(
            List<ApolloContactViewModel> allContacts,
            ApolloApiViewModel model,
            string currentUserId,
            int? companyId)
        {
            var labelList = model.Labels ?? new List<SelectListItem>();

            var normalized = allContacts
                .Where(c => c != null)
                .Select(c => new ApolloContactViewModel
                {
                    PersonId = c.PersonId?.Trim(),
                    Email = c.Email?.Trim(),
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Title = c.Title,
                    CompanyName = c.CompanyName,
                    UpdatedAt = c.UpdatedAt,
                    LinkedinUrl = c.LinkedinUrl,
                    Headline = c.Headline,
                    Location = c.Location
                })
                .ToList();

            var withPersonId = normalized
                .Where(c => !string.IsNullOrWhiteSpace(c.PersonId))
                .GroupBy(c => c.PersonId!)
                .Select(g => g.OrderByDescending(x => x.UpdatedAt).First())
                .ToList();

            var withoutPersonId = normalized
                .Where(c => string.IsNullOrWhiteSpace(c.PersonId) && !string.IsNullOrWhiteSpace(c.Email))
                .GroupBy(c => c.Email!.ToLowerInvariant())
                .Select(g => g.OrderByDescending(x => x.UpdatedAt).First())
                .ToList();

            var noPersonIdNoEmail = normalized
                .Where(c => string.IsNullOrWhiteSpace(c.PersonId) && string.IsNullOrWhiteSpace(c.Email))
                .ToList();

            var dedupedContacts = withPersonId
                .Concat(withoutPersonId)
                .Concat(noPersonIdNoEmail)
                .ToList();

            var personIdList = dedupedContacts
                .Where(c => !string.IsNullOrWhiteSpace(c.PersonId))
                .Select(c => c.PersonId!)
                .Distinct()
                .ToList();

            var emailList = dedupedContacts
                .Where(c => !string.IsNullOrWhiteSpace(c.Email))
                .Select(c => c.Email!.ToLowerInvariant())
                .Distinct()
                .ToList();

            var existingContacts = await _context.ApolloContacts
                .Where(x =>
                    (!string.IsNullOrEmpty(x.PersonId) && personIdList.Contains(x.PersonId)) ||
                    (!string.IsNullOrEmpty(x.Email) && emailList.Contains(x.Email.ToLower())))
                .ToListAsync();

            var existingByPersonId = existingContacts
                .Where(x => !string.IsNullOrWhiteSpace(x.PersonId))
                .GroupBy(x => x.PersonId!)
                .ToDictionary(g => g.Key, g => g.First());

            var existingByEmail = existingContacts
                .Where(x => !string.IsNullOrWhiteSpace(x.Email))
                .GroupBy(x => x.Email!.ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.First());

            var selectedLabelName = labelList.FirstOrDefault(l => l.Value == model.SelectedLabelId)?.Text;

            foreach (var contact in dedupedContacts)
            {
                ApolloContactDbModel? existing = null;

                var pid = contact.PersonId?.Trim();
                var emailLower = contact.Email?.Trim()?.ToLowerInvariant();
                var safeEmail = HasRealEmail(contact.Email)
                    ? contact.Email!.Trim()
                    : BuildFallbackEmail(contact);

                if (!string.IsNullOrWhiteSpace(pid) && existingByPersonId.TryGetValue(pid, out var pidMatch))
                {
                    existing = pidMatch;
                }
                else if (!string.IsNullOrWhiteSpace(emailLower) && existingByEmail.TryGetValue(emailLower, out var emailMatch))
                {
                    existing = emailMatch;
                }

                var incomingUpdatedAt = contact.UpdatedAt;

                if (existing == null)
                {
                    _context.ApolloContacts.Add(new ApolloContactDbModel
                    {
                        Email = safeEmail,
                        PersonId = pid,
                        FirstName = contact.FirstName,
                        LastName = contact.LastName,
                        Title = contact.Title,
                        CompanyName = contact.CompanyName,
                        UpdatedAt = incomingUpdatedAt,
                        SourceLabelId = model.SelectedLabelId,
                        SourceLabelName = selectedLabelName,
                        UserId = currentUserId,
                        CompanyId = companyId,
                        CreatedAt = DateTime.UtcNow,
                        LinkedinUrl = contact.LinkedinUrl,
                        Headline = contact.Headline,
                        Location = contact.Location
                    });
                }
                else
                {
                    var existingUpdatedAt = existing.UpdatedAt;

                    if (existingUpdatedAt < incomingUpdatedAt)
                    {
                        existing.FirstName = contact.FirstName;
                        existing.LastName = contact.LastName;
                        existing.Title = contact.Title;
                        existing.CompanyName = contact.CompanyName;
                        existing.UpdatedAt = incomingUpdatedAt;
                        existing.SourceLabelId = model.SelectedLabelId;
                        existing.SourceLabelName = selectedLabelName;
                        existing.LinkedinUrl = contact.LinkedinUrl;
                        existing.Headline = contact.Headline;
                        existing.Location = contact.Location;

                        if (HasRealEmail(contact.Email))
                            existing.Email = contact.Email!.Trim();
                    }

                    if (existing.CompanyId == null)
                        existing.CompanyId = companyId;

                    if (string.IsNullOrWhiteSpace(existing.UserId))
                        existing.UserId = currentUserId;
                }
            }

            await _context.SaveChangesAsync();
        }

        private HttpClient CreateApolloClient(string apiKey, bool useUpperCaseApiHeader = false)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://api.apollo.io/");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (useUpperCaseApiHeader)
                client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
            else
                client.DefaultRequestHeaders.Add("x-api-key", apiKey);

            return client;
        }

        private static bool HasRealEmail(string? email)
        {
            return !string.IsNullOrWhiteSpace(email);
        }

        private static string BuildFallbackEmail(ApolloContactDbModel contact)
        {
            var key = !string.IsNullOrWhiteSpace(contact.PersonId)
                ? contact.PersonId.Trim()
                : contact.Id.ToString();

            return $"apollo-noemail-{key}@crmcorner.local";
        }

        private static string BuildFallbackEmail(ApolloContactViewModel contact)
        {
            var key = !string.IsNullOrWhiteSpace(contact.PersonId)
                ? contact.PersonId.Trim()
                : Guid.NewGuid().ToString("N");

            return $"apollo-noemail-{key}@crmcorner.local";
        }
    }
}