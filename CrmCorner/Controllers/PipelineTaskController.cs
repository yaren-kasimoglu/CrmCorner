using CrmCorner.Extensions;
using CrmCorner.Models;
using CrmCorner.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

using Microsoft.EntityFrameworkCore;

using PipelineStage = CrmCorner.Models.Enums.PipelineStage;

namespace CrmCorner.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,TeamLeader,TeamMember")]
    public class PipelineTaskController : Controller
    {
        private readonly CrmCornerContext _context;

        public PipelineTaskController(CrmCornerContext context)
        {
            _context = context;
        }

        // 1. Görevleri Listeleme
        public async Task<IActionResult> PipelineIndex()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var me = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (me == null) return Unauthorized();

            // 🔹 Rolleri al
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == me.Id)
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .ToListAsync();

            bool isSuperAdmin = userRoles.Contains("SuperAdmin");
            bool isAdmin = userRoles.Contains("Admin");
            bool isTeamLeader = userRoles.Contains("TeamLeader");
            bool isTeamMember = userRoles.Contains("TeamMember");

            // 🔹 Status List
            ViewBag.StatusList = Enum.GetValues(typeof(PipelineStage))
                .Cast<PipelineStage>()
                .ToDictionary(e => e, e => e.GetDisplayName());

            // 🔹 Şirket kullanıcıları
            var companyUsers = await _context.Users.AsNoTracking()
                .Where(u => u.EmailDomain == me.EmailDomain)
                .Select(u => new { u.Id, u.UserName, u.NameSurname, u.CompanyId })
                .ToListAsync();

            ViewBag.UserMap = companyUsers.ToDictionary(
                x => x.Id,
                x => string.IsNullOrWhiteSpace(x.NameSurname) ? x.UserName : x.NameSurname
            );

            var companyUserIds = companyUsers.Select(u => u.Id).ToList();

            // 🔹 Görevleri çek (rol bazlı filtreleme)
            var tasksQuery = _context.PipelineTasks
                .Include(t => t.AppUser)
                .Include(t => t.ResponsibleUser)
                .AsNoTracking();

            if (isSuperAdmin)
            {
                // 🔸 SuperAdmin -> tüm görevleri görür
            }
            else if (isAdmin)
            {
                // 🔸 Admin -> kendi şirketindeki herkesin görevleri
                var sameCompanyUserIds = companyUsers
                    .Where(u => u.CompanyId == me.CompanyId)
                    .Select(u => u.Id)
                    .ToList();

                tasksQuery = tasksQuery.Where(t =>
                    (t.AppUserId != null && sameCompanyUserIds.Contains(t.AppUserId)) ||
                    (t.ResponsibleUserId != null && sameCompanyUserIds.Contains(t.ResponsibleUserId))
                );
            }
            else if (isTeamLeader || isTeamMember)
            {
                // 🔸 TeamLeader & TeamMember -> sadece kendi görevleri (AppUser veya ResponsibleUser)
                tasksQuery = tasksQuery.Where(t =>
                    t.AppUserId == currentUserId || t.ResponsibleUserId == currentUserId
                );
            }

            // 🔹 Görevleri listele
            var tasks = await tasksQuery
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();

            // 🔹 Görev sayısı (kişisel bazda)
            ViewBag.PipelineTaskCount = await _context.PipelineTasks.CountAsync(t =>
                t.AppUserId == currentUserId || t.ResponsibleUserId == currentUserId);

            return View(tasks);
        }


        #region GÖREV EKLEME
        // 2. Yeni Görev Formu (GET)
        public IActionResult PipelineTaskCreate()
        {
            ViewBag.StageList = Enum.GetValues(typeof(PipelineStage))
                .Cast<PipelineStage>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.GetDisplayName()
                }).ToList();

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 🔒 sadece aynı domain kullanıcıları
            ViewBag.Users = BuildCompanyUsersSelectList(currentUserId ?? string.Empty);

            ViewBag.SourceChannels = Enum.GetValues(typeof(SourceChannelType))
                .Cast<SourceChannelType>()
                .Select(e => new SelectListItem
                {
                    Text = e.GetDisplayName(),
                    Value = e.ToString()
                }).ToList();

            // Görüşmeyi alan varsayılan olarak mevcut kullanıcı
            var model = new PipelineTask { AppUserId = currentUserId };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PipelineTaskCreate(PipelineTask task)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 🧠 “Görüşmeyi alan” boş ise mevcut kullanıcıyı ata
            if (string.IsNullOrWhiteSpace(task.AppUserId))
                task.AppUserId = currentUserId;

            // 🔒 Sunucu tarafı domain güvenliği
            var me = _context.Users.AsNoTracking().FirstOrDefault(u => u.Id == currentUserId);
            if (me == null) return Unauthorized();

            bool IsCompanyUser(string? uid) =>
                !string.IsNullOrWhiteSpace(uid) &&
                _context.Users.Any(u => u.Id == uid && u.EmailDomain == me.EmailDomain);

            if (!IsCompanyUser(task.AppUserId))
                ModelState.AddModelError(nameof(task.AppUserId), "Sadece kendi şirketinizdeki kullanıcıları seçebilirsiniz.");

            if (!IsCompanyUser(task.ResponsibleUserId))
                ModelState.AddModelError(nameof(task.ResponsibleUserId), "Sadece kendi şirketinizdeki kullanıcıları seçebilirsiniz.");


            // === Outcomes ↔ OutcomeStatus tutarlılık kuralları ===
            bool invalid = false;

            // 1) Süreçte iken sonuç Won/Lost olamaz (None olmalı)
            if (task.Outcomes == OutcomeType.Surecte &&
               (task.OutcomeStatus == OutcomeTypeSales.Won || task.OutcomeStatus == OutcomeTypeSales.Lost))
            {
                ModelState.AddModelError(nameof(task.OutcomeStatus),
                    "Süreç durumu 'Süreçte' iken sonuç 'Kazanıldı/Kaybedildi' olamaz. Lütfen 'Hiçbiri' seçin.");
                invalid = true;
            }

            // 2) Olumlu iken Kaybedildi olamaz
            if (task.Outcomes == OutcomeType.Olumlu && task.OutcomeStatus == OutcomeTypeSales.Lost)
            {
                ModelState.AddModelError(nameof(task.OutcomeStatus),
                    "Süreç durumu 'Olumlu' iken sonuç 'Kaybedildi' olamaz.");
                invalid = true;
            }

            // 3) Olumsuz iken Kazanıldı olamaz
            if (task.Outcomes == OutcomeType.Olumsuz && task.OutcomeStatus == OutcomeTypeSales.Won)
            {
                ModelState.AddModelError(nameof(task.OutcomeStatus),
                    "Süreç durumu 'Olumsuz' iken sonuç 'Kazanıldı' olamaz.");
                invalid = true;
            }

            // 4) Negatif gerekçe zorunlu: Outcomes=Olumsuz veya OutcomeStatus=Lost
            if ((task.Outcomes == OutcomeType.Olumsuz || task.OutcomeStatus == OutcomeTypeSales.Lost)
                && string.IsNullOrWhiteSpace(task.NegativeReason))
            {
                ModelState.AddModelError(nameof(task.NegativeReason),
                    "Olumsuz durumda 'Olumsuz Sebep' alanı zorunludur.");
                invalid = true;
            }




            if (!ModelState.IsValid)
            {
                // ViewBag'leri tekrar doldur (filtreli!)
                ViewBag.StageList = Enum.GetValues(typeof(PipelineStage))
                    .Cast<PipelineStage>()
                    .Select(e => new SelectListItem { Value = ((int)e).ToString(), Text = e.GetDisplayName() })
                    .ToList();

                ViewBag.Users = BuildCompanyUsersSelectList(currentUserId ?? string.Empty);

                ViewBag.SourceChannels = Enum.GetValues(typeof(SourceChannelType))
                    .Cast<SourceChannelType>()
                    .Select(e => new SelectListItem { Text = e.GetDisplayName(), Value = e.ToString() })
                    .ToList();

                return View(task);
            }

            // 1) Müşteriyi oluştur (müşterinin sahibi = sorumlu kişi)
            var newCustomer = new CustomerN
            {
                Name = task.CustomerName,
                Surname = task.CustomerSurname,
                CompanyName = task.CompanyName,
                PhoneNumber = task.Phone,
                CustomerEmail = task.Email,
                LinkedinUrl = task.LinkedinUrl,
                CreatedDate = DateTime.Now,
                AppUserId = task.ResponsibleUserId
            };

            _context.CustomerNs.Add(newCustomer);
            _context.SaveChanges();

            // 2) Görevi kaydet
            task.CustomerId = newCustomer.Id;
            task.CreatedDate = DateTime.Now;

            _context.PipelineTasks.Add(task);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Görev ve müşteri başarıyla eklendi.";
            return RedirectToAction("PipelineIndex");
        }
        #endregion


        #region GÖREV GÜNCELLEME
        [HttpGet]
        public IActionResult PipelineEdit(int id)
        {
            var task = _context.PipelineTasks
                .Include(t => t.AppUser)
                .Include(t => t.ResponsibleUser)
                .Include(t => t.Customer)
                .FirstOrDefault(t => t.Id == id);

            if (task == null)
                return NotFound();

            // Aktif kullanıcı bilgisi
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var me = _context.Users.AsNoTracking().FirstOrDefault(u => u.Id == currentUserId);
            if (me == null) return Unauthorized();

            // --- Roller ---
            var userRoles = _context.UserRoles
                .Where(ur => ur.UserId == me.Id)
                .Select(ur => ur.RoleId)
                .ToList();

            var roleNames = _context.Roles
                .Where(r => userRoles.Contains(r.Id))
                .Select(r => r.Name)
                .ToList();

            bool isSuperAdmin = roleNames.Contains("SuperAdmin");
            bool isAdmin = roleNames.Contains("Admin");
            bool isTeamLeader = roleNames.Contains("TeamLeader");
            bool isTeamMember = roleNames.Contains("TeamMember");

            // --- Erişim kontrolü ---
            if (!isSuperAdmin)
            {
                if (isAdmin)
                {
                    // admin yalnızca kendi şirketindeki görevleri görebilir
                    if (task.AppUser?.CompanyId != me.CompanyId && task.ResponsibleUser?.CompanyId != me.CompanyId)
                        return Forbid(); // erişim yok
                }
                else
                {
                    // teamleader veya member yalnızca kendine ait görevleri görebilir
                    if (task.AppUserId != currentUserId && task.ResponsibleUserId != currentUserId)
                        return Forbid();
                }
            }

            // --- ViewBag doldurma kısmı (aynı kalıyor) ---
            var companyUsers = _context.Users.AsNoTracking()
                .Where(u => u.EmailDomain == me.EmailDomain)
                .OrderBy(u => u.UserName)
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = (u.Id == currentUserId)
                        ? $"{(string.IsNullOrWhiteSpace(u.NameSurname) ? u.UserName : u.NameSurname)} (ben)"
                        : (string.IsNullOrWhiteSpace(u.NameSurname) ? u.UserName : u.NameSurname)
                })
                .ToList();

            // gizli kullanıcıları listeye ekleme
            var extraUserIds = new[] { task.AppUserId, task.ResponsibleUserId }
                .Where(x => !string.IsNullOrWhiteSpace(x) && !companyUsers.Any(c => c.Value == x))
                .Distinct()
                .ToList();

            if (extraUserIds.Any())
            {
                var extras = _context.Users.AsNoTracking()
                    .Where(u => extraUserIds.Contains(u.Id))
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id,
                        Text = $"{(string.IsNullOrWhiteSpace(u.NameSurname) ? u.UserName : u.NameSurname)} (gizli)"
                    })
                    .ToList();

                companyUsers.InsertRange(0, extras);
            }

            ViewBag.Users = companyUsers;

            // --- müşteri listesi ---
            var companyUserIds = _context.Users.AsNoTracking()
                .Where(u => u.EmailDomain == me.EmailDomain)
                .Select(u => u.Id)
                .ToList();

            var customerItems = _context.CustomerNs.AsNoTracking()
                .Where(c => c.AppUserId != null && companyUserIds.Contains(c.AppUserId))
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Name} {c.Surname} / {c.CompanyName}"
                })
                .ToList();

            // gizli müşteri ekleme
            if (task.CustomerId != null && !customerItems.Any(x => x.Value == task.CustomerId.ToString()))
            {
                var missingCustomer = _context.CustomerNs.AsNoTracking().FirstOrDefault(c => c.Id == task.CustomerId);
                if (missingCustomer != null)
                {
                    customerItems.Insert(0, new SelectListItem
                    {
                        Value = missingCustomer.Id.ToString(),
                        Text = $"{missingCustomer.Name} {missingCustomer.Surname} / {missingCustomer.CompanyName} (gizli)"
                    });
                }
            }

            ViewBag.Customer = new SelectList(customerItems, "Value", "Text", task.CustomerId?.ToString());

            // Kaynak kanallar
            ViewBag.SourceChannels = Enum.GetValues(typeof(SourceChannelType))
                .Cast<SourceChannelType>()
                .Select(e => new SelectListItem
                {
                    Text = e.GetDisplayName(),
                    Value = e.ToString()
                }).ToList();

            // Aşamalar
            ViewBag.StageList = Enum.GetValues(typeof(PipelineStage))
                .Cast<PipelineStage>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.GetDisplayName()
                }).ToList();

            return View(task);
        }

        // EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PipelineEdit(PipelineTask model)
        {
            var existing = _context.PipelineTasks
                .Include(t => t.AppUser)
                .Include(t => t.ResponsibleUser)
                .FirstOrDefault(t => t.Id == model.Id);

            if (existing == null)
            {
                TempData["ErrorMessage"] = "Görev bulunamadı.";
                return RedirectToAction("PipelineIndex");
            }

            // Aktif kullanıcı
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var me = _context.Users.AsNoTracking().FirstOrDefault(u => u.Id == currentUserId);
            if (me == null) return Unauthorized();

            // ====== ROL ÇEK ======
            var roleIdsOfMe = _context.UserRoles
                .Where(ur => ur.UserId == me.Id)
                .Select(ur => ur.RoleId)
                .ToList();

            var myRoles = _context.Roles
                .Where(r => roleIdsOfMe.Contains(r.Id))
                .Select(r => r.Name)
                .ToList();

            bool isSuperAdmin = myRoles.Contains("SuperAdmin");
            bool isAdmin = myRoles.Contains("Admin");
            bool isTeamLeader = myRoles.Contains("TeamLeader");
            bool isTeamMember = myRoles.Contains("TeamMember");

            // ====== YETKİ KONTROLÜ ======
            // SuperAdmin: sınırsız
            if (!isSuperAdmin)
            {
                if (isAdmin)
                {
                    // Admin yalnızca kendi şirketindeki görevleri düzenleyebilir
                    var taskCompanyId =
                        existing.ResponsibleUser?.CompanyId
                        ?? existing.AppUser?.CompanyId;

                    if (taskCompanyId != me.CompanyId)
                    {
                        TempData["ErrorMessage"] = "Bu görevi düzenleme yetkiniz yok.";
                        return RedirectToAction("PipelineIndex");
                    }
                }
                else
                {
                    // TeamLeader / TeamMember sadece kendisi dahilse düzenleyebilir
                    bool iAmInTask =
                        existing.AppUserId == currentUserId ||
                        existing.ResponsibleUserId == currentUserId;

                    if (!iAmInTask)
                    {
                        TempData["ErrorMessage"] = "Bu görevi düzenleme yetkiniz yok.";
                        return RedirectToAction("PipelineIndex");
                    }
                }
            }
            // === Buraya kadar: erişim OK ===


            // --- Domain güvenliği (SuperAdmin hariç) ---
            bool IsCompanyUser(string? uid) =>
                isSuperAdmin || (
                    !string.IsNullOrWhiteSpace(uid) &&
                    _context.Users.Any(u => u.Id == uid && u.EmailDomain == me.EmailDomain)
                );

            // Burada sadece "zorunlu alan seçilmemişse" hata koy
            if (string.IsNullOrWhiteSpace(model.AppUserId))
                ModelState.AddModelError(nameof(model.AppUserId), "Görüşmeyi alan kişi seçilmelidir.");
            else if (!IsCompanyUser(model.AppUserId))
                ModelState.AddModelError(nameof(model.AppUserId), "Sadece kendi şirketinizdeki kullanıcıları seçebilirsiniz.");

            if (string.IsNullOrWhiteSpace(model.ResponsibleUserId))
                ModelState.AddModelError(nameof(model.ResponsibleUserId), "Sorumlu kullanıcı seçilmelidir.");
            else if (!IsCompanyUser(model.ResponsibleUserId))
                ModelState.AddModelError(nameof(model.ResponsibleUserId), "Sadece kendi şirketinizdeki kullanıcıları seçebilirsiniz.");


            // Müşteri domain kontrolü (SuperAdmin hariç)
            var companyUserIds = _context.Users.AsNoTracking()
                .Where(u => u.EmailDomain == me.EmailDomain)
                .Select(u => u.Id)
                .ToList();

            bool IsCompanyCustomer(int? cid) =>
                isSuperAdmin || (
                    cid != null &&
                    _context.CustomerNs.Any(c =>
                        c.Id == cid &&
                        c.AppUserId != null &&
                        companyUserIds.Contains(c.AppUserId))
                );

            if (!IsCompanyCustomer(model.CustomerId))
                ModelState.AddModelError(nameof(model.CustomerId), "Sadece kendi şirketinizdeki müşterileri seçebilirsiniz.");


            // Stage/Outcome validasyonları
            if (model.Stage == PipelineStage.Sonuc &&
                model.OutcomeStatus != OutcomeTypeSales.Won &&
                model.OutcomeStatus != OutcomeTypeSales.Lost)
            {
                ModelState.AddModelError(nameof(model.OutcomeStatus),
                    "Sonuç aşamasına geçmek için lütfen 'Kazanıldı' veya 'Kaybedildi' seçiniz.");
            }

            if (model.OutcomeStatus == OutcomeTypeSales.Lost &&
                string.IsNullOrWhiteSpace(model.NegativeReason))
            {
                ModelState.AddModelError(nameof(model.NegativeReason),
                    "Olumsuz sebep girilmesi zorunludur.");
            }

            // === Outcomes ↔ OutcomeStatus tutarlılık kuralları ===
            if (model.Outcomes == OutcomeType.Surecte &&
               (model.OutcomeStatus == OutcomeTypeSales.Won ||
                model.OutcomeStatus == OutcomeTypeSales.Lost))
            {
                ModelState.AddModelError(nameof(model.OutcomeStatus),
                    "Süreç durumu 'Süreçte' iken sonuç 'Kazanıldı/Kaybedildi' olamaz. Lütfen 'Hiçbiri' seçin.");
            }

            if (model.Outcomes == OutcomeType.Olumlu &&
                model.OutcomeStatus == OutcomeTypeSales.Lost)
            {
                ModelState.AddModelError(nameof(model.OutcomeStatus),
                    "Süreç durumu 'Olumlu' iken sonuç 'Kaybedildi' olamaz.");
            }

            if (model.Outcomes == OutcomeType.Olumsuz &&
                model.OutcomeStatus == OutcomeTypeSales.Won)
            {
                ModelState.AddModelError(nameof(model.OutcomeStatus),
                    "Süreç durumu 'Olumsuz' iken sonuç 'Kazanıldı' olamaz.");
            }

            if ((model.Outcomes == OutcomeType.Olumsuz ||
                 model.OutcomeStatus == OutcomeTypeSales.Lost) &&
                string.IsNullOrWhiteSpace(model.NegativeReason))
            {
                ModelState.AddModelError(nameof(model.NegativeReason),
                    "Olumsuz durumda 'Olumsuz Sebep' alanı zorunludur.");
            }

            // Model hatalıysa aynı geri dönüş bloğun aynen kalıyor ↓
            if (!ModelState.IsValid)
            {
                var usersSelect = _context.Users.AsNoTracking()
                    .Where(u => u.EmailDomain == me.EmailDomain)
                    .OrderBy(u => u.UserName)
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id,
                        Text = u.Id == currentUserId ? $"{u.UserName} (ben)" : u.UserName
                    })
                    .ToList();

                if (!string.IsNullOrWhiteSpace(model.AppUserId) &&
                    !usersSelect.Any(x => x.Value == model.AppUserId))
                {
                    var au = _context.Users.AsNoTracking().FirstOrDefault(u => u.Id == model.AppUserId);
                    usersSelect.Insert(0, new SelectListItem
                    {
                        Value = model.AppUserId,
                        Text = au != null ? $"{au.UserName} (gizli)" : $"{model.AppUserId} (gizli)"
                    });
                }

                if (!string.IsNullOrWhiteSpace(model.ResponsibleUserId) &&
                    !usersSelect.Any(x => x.Value == model.ResponsibleUserId))
                {
                    var ru = _context.Users.AsNoTracking().FirstOrDefault(u => u.Id == model.ResponsibleUserId);
                    usersSelect.Insert(0, new SelectListItem
                    {
                        Value = model.ResponsibleUserId,
                        Text = ru != null ? $"{ru.UserName} (gizli)" : $"{model.ResponsibleUserId} (gizli)"
                    });
                }
                ViewBag.Users = usersSelect;

                var customerItems = _context.CustomerNs.AsNoTracking()
                    .Where(c => c.AppUserId != null && companyUserIds.Contains(c.AppUserId))
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = $"{c.Name} {c.Surname} / {c.CompanyName}"
                    })
                    .ToList();

                if (model.CustomerId != null &&
                    !customerItems.Any(x => x.Value == model.CustomerId.ToString()))
                {
                    var missingCustomer = _context.CustomerNs.AsNoTracking()
                        .FirstOrDefault(c => c.Id == model.CustomerId);
                    if (missingCustomer != null)
                    {
                        customerItems.Insert(0, new SelectListItem
                        {
                            Value = missingCustomer.Id.ToString(),
                            Text = $"{missingCustomer.Name} {missingCustomer.Surname} / {missingCustomer.CompanyName} (gizli)"
                        });
                    }
                }

                var errs = ModelState
                    .Where(x => x.Value?.Errors.Any() == true)
                    .Select(x => $"{x.Key}: {string.Join(", ", x.Value!.Errors.Select(e => e.ErrorMessage))}");

                TempData["ErrorMessage"] = string.Join(" | ", errs);

                var allErrors = ModelState
                    .Where(ms => ms.Value.Errors.Any())
                    .Select(ms => new
                    {
                        Field = ms.Key,
                        Errors = ms.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    });
                ViewBag.DebugErrors = allErrors;

                ViewBag.Customer = new SelectList(customerItems, "Value", "Text",
                    model.CustomerId?.ToString());

                ViewBag.SourceChannels = Enum.GetValues(typeof(SourceChannelType))
                    .Cast<SourceChannelType>()
                    .Select(e => new SelectListItem
                    {
                        Text = e.GetDisplayName(),
                        Value = e.ToString()
                    }).ToList();

                ViewBag.StageList = Enum.GetValues(typeof(PipelineStage))
                    .Cast<PipelineStage>()
                    .Select(e => new SelectListItem
                    {
                        Value = ((int)e).ToString(),
                        Text = e.GetDisplayName()
                    }).ToList();

                return View(model);
            }

            // --- LOG setup (senin kodun) ---
            var now = DateTime.Now;
            var updaterUserId = currentUserId;

            void LogIfChanged(string fieldName, object? oldVal, object? newVal)
            {
                var oldStr = oldVal?.ToString();
                var newStr = newVal?.ToString();

                if (oldStr == newStr ||
                    (string.IsNullOrWhiteSpace(oldStr) && string.IsNullOrWhiteSpace(newStr)))
                    return;

                _context.PipelineTaskLogs.Add(new PipelineTaskLog
                {
                    PipelineTaskId = model.Id,
                    UpdatedField = fieldName,
                    OldValue = oldStr,
                    NewValue = newStr,
                    UpdatedAt = now,
                    UpdatedById = updaterUserId
                });
            }

            LogIfChanged(nameof(model.AppUserId), existing.AppUserId, model.AppUserId);
            LogIfChanged(nameof(model.ResponsibleUserId), existing.ResponsibleUserId, model.ResponsibleUserId);
            LogIfChanged(nameof(model.Title), existing.Title, model.Title);
            LogIfChanged(nameof(model.Description), existing.Description, model.Description);
            LogIfChanged(nameof(model.Value), existing.Value?.ToString("N2"), model.Value?.ToString("N2"));
            LogIfChanged(nameof(model.Currency), existing.Currency, model.Currency);
            LogIfChanged(nameof(model.Stage), existing.Stage?.ToString(), model.Stage?.ToString());
            LogIfChanged(nameof(model.ExpectedCloseDate), existing.ExpectedCloseDate?.ToString("yyyy-MM-dd"), model.ExpectedCloseDate?.ToString("yyyy-MM-dd"));
            LogIfChanged(nameof(model.CustomerName), existing.CustomerName, model.CustomerName);
            LogIfChanged(nameof(model.CustomerSurname), existing.CustomerSurname, model.CustomerSurname);
            LogIfChanged(nameof(model.CompanyName), existing.CompanyName, model.CompanyName);
            LogIfChanged(nameof(model.Phone), existing.Phone, model.Phone);
            LogIfChanged(nameof(model.Email), existing.Email, model.Email);
            LogIfChanged(nameof(model.LinkedinUrl), existing.LinkedinUrl, model.LinkedinUrl);
            LogIfChanged(nameof(model.OutcomeStatus), existing.OutcomeStatus?.ToString(), model.OutcomeStatus?.ToString());
            LogIfChanged(nameof(model.NegativeReason), existing.NegativeReason, model.NegativeReason);
            LogIfChanged(nameof(model.Source), existing.Source, model.Source);
            LogIfChanged(nameof(model.SourceChannel), existing.SourceChannel, model.SourceChannel);
            LogIfChanged(nameof(model.Outcomes), existing.Outcomes?.ToString(), model.Outcomes?.ToString());
            LogIfChanged(nameof(model.ContactedViaLinkedIn), existing.ContactedViaLinkedIn?.ToString(), model.ContactedViaLinkedIn?.ToString());
            LogIfChanged(nameof(model.ContactedViaColdCall), existing.ContactedViaColdCall?.ToString(), model.ContactedViaColdCall?.ToString());
            LogIfChanged(nameof(model.CustomerId), existing.CustomerId?.ToString(), model.CustomerId?.ToString());

            // Güncelle
            existing.AppUserId = model.AppUserId;
            existing.ResponsibleUserId = model.ResponsibleUserId;
            existing.Title = model.Title;
            existing.Description = model.Description;
            existing.Value = model.Value;
            existing.Currency = model.Currency;
            existing.Stage = model.Stage;
            existing.ExpectedCloseDate = model.ExpectedCloseDate;
            existing.CustomerName = model.CustomerName;
            existing.CustomerSurname = model.CustomerSurname;
            existing.CompanyName = model.CompanyName;
            existing.Phone = model.Phone;
            existing.Email = model.Email;
            existing.LinkedinUrl = model.LinkedinUrl;
            existing.OutcomeStatus = model.OutcomeStatus;
            existing.NegativeReason = model.NegativeReason;
            existing.Source = model.Source;
            existing.SourceChannel = model.SourceChannel;
            existing.Outcomes = model.Outcomes;
            existing.ContactedViaLinkedIn = model.ContactedViaLinkedIn;
            existing.ContactedViaColdCall = model.ContactedViaColdCall;
            existing.CustomerId = model.CustomerId;

            // Satış kazanıldıysa AfterSales kaydı aç
            if (existing.OutcomeStatus == OutcomeTypeSales.Won)
            {
                var existingPostSale = _context.PostSaleInfos
                    .FirstOrDefault(p => p.PipelineTaskId == existing.Id);
                if (existingPostSale == null)
                {
                    var newPostSale = new PostSaleInfo
                    {
                        PipelineTaskId = existing.Id
                    };
                    _context.PostSaleInfos.Add(newPostSale);
                }
            }

            _context.SaveChanges();

            return RedirectToAction("PipelineIndex");
        }

        #endregion

        [HttpPost]
        public IActionResult UpdateContactMethods(int taskId, bool contactedViaLinkedIn, bool contactedViaColdCall)
        {
            var task = _context.PipelineTasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
                return NotFound();

            task.ContactedViaLinkedIn = contactedViaLinkedIn;
            task.ContactedViaColdCall = contactedViaColdCall;
            _context.SaveChanges();

            return Ok();
        }

        [HttpPost]
        public IActionResult UpdateStage(int id, int newStage)
        {
            var task = _context.PipelineTasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
                return NotFound();

            task.Stage = (PipelineStage)newStage;
            _context.SaveChanges();

            return Ok();
        }

        public IActionResult PipelineDetails(int id)
        {
            var task = _context.PipelineTasks
                .Include(t => t.Notes)
                .Include(t => t.AppUser)
                .Include(t => t.ResponsibleUser)
                .Include(t => t.Customer)
                .Include(t => t.FileAttachments)
                .FirstOrDefault(t => t.Id == id);

            if (task == null)
                return NotFound();

            // --- Aktif kullanıcı ve rol bilgisi ---
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var me = _context.Users.AsNoTracking().FirstOrDefault(u => u.Id == currentUserId);
            if (me == null) return Unauthorized();

            var roleIdsOfMe = _context.UserRoles
                .Where(ur => ur.UserId == me.Id)
                .Select(ur => ur.RoleId)
                .ToList();

            var myRoles = _context.Roles
                .Where(r => roleIdsOfMe.Contains(r.Id))
                .Select(r => r.Name)
                .ToList();

            bool isSuperAdmin = myRoles.Contains("SuperAdmin");
            bool isAdmin = myRoles.Contains("Admin");

            // --- Erişim kontrolü ---
            if (!isSuperAdmin)
            {
                if (isAdmin)
                {
                    // Admin sadece kendi şirketindeki görevleri görebilir
                    var taskCompanyId = task.ResponsibleUser?.CompanyId ?? task.AppUser?.CompanyId;
                    if (taskCompanyId != me.CompanyId)
                    {
                        TempData["ErrorMessage"] = "Bu görevin detayını görüntüleme yetkiniz yok.";
                        return RedirectToAction("PipelineIndex");
                    }
                }
                else
                {
                    // TeamLeader / TeamMember sadece kendiyle ilgili görevleri görebilir
                    bool iAmInTask =
                        task.AppUserId == currentUserId ||
                        task.ResponsibleUserId == currentUserId;

                    if (!iAmInTask)
                    {
                        TempData["ErrorMessage"] = "Bu görevin detayını görüntüleme yetkiniz yok.";
                        return RedirectToAction("PipelineIndex");
                    }
                }
            }

            // --- Loglar ---
            var logs = _context.PipelineTaskLogs
                .Include(l => l.UpdatedBy)
                .Where(l => l.PipelineTaskId == id)
                .OrderByDescending(l => l.UpdatedAt)
                .ToList();

            ViewBag.Logs = logs;

            return View(task);
        }


        [HttpPost]
        public IActionResult AddNote(int taskId, string note)
        {
            var task = _context.PipelineTasks.Include(t => t.Notes).FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                var newNote = new PipelineTaskNote
                {
                    PipelineTaskId = taskId,
                    Note = note,
                    CreatedAt = DateTime.Now
                };

                task.Notes.Add(newNote);
                _context.SaveChanges();
            }

            return RedirectToAction("PipelineDetails", new { id = taskId });
        }

        [HttpPost]
        public IActionResult CheckProposalConditions(int id)
        {
            var task = _context.PipelineTasks
                .Include(t => t.FileAttachments)
                .FirstOrDefault(t => t.Id == id);

            if (task == null)
            {
                return Json(new { isValid = false, message = "Görev bulunamadı." });
            }

            if (task.Value == null || task.Value <= 0)
            {
                return Json(new { isValid = false, message = "Değer girilmeden 'Teklif Sunuldu' aşamasına geçilemez." });
            }

            if (task.FileAttachments == null || !task.FileAttachments.Any())
            {
                return Json(new { isValid = false, message = "Sözleşme veya dosya yüklenmeden 'Teklif Sunuldu' aşamasına geçilemez." });
            }

            return Json(new { isValid = true });
        }


        [HttpPost]
        public async Task<IActionResult> UploadPipelineFile(IFormFile file, int pipelineTaskId)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Lütfen geçerli bir dosya seçin.";
                return RedirectToAction("PipelineDetails", new { id = pipelineTaskId });
            }

            var fileName = Path.GetFileName(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var relativePath = Path.Combine("uploads/pipeline", uniqueFileName);
            var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

            // Klasörü oluştur (varsa atlar)
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

            using (var stream = new FileStream(absolutePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileAttachment = new PipelineTaskFileAttachment
            {
                PipelineTaskId = pipelineTaskId,
                FileName = fileName,
                FilePath = "/" + relativePath.Replace("\\", "/"),
                FileSize = file.Length,
                FileType = file.ContentType,
                UploadedDate = DateTime.Now
            };

            _context.PipelineTaskFileAttachments.Add(fileAttachment);
            await _context.SaveChangesAsync();

            return RedirectToAction("PipelineDetails", new { id = pipelineTaskId });
        }


        public async Task<IActionResult> DownloadPipelineFile(int id)
        {
            var file = await _context.PipelineTaskFileAttachments.FirstOrDefaultAsync(f => f.Id == id);
            if (file == null)
                return NotFound();

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.FilePath.TrimStart('/'));

            if (!System.IO.File.Exists(path))
                return NotFound();

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, file.FileType, file.FileName);
        }

        public async Task<IActionResult> DeletePipelineFile(int id)
        {
            var file = await _context.PipelineTaskFileAttachments.FirstOrDefaultAsync(f => f.Id == id);
            if (file == null)
                return NotFound();

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.FilePath.TrimStart('/'));

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            _context.PipelineTaskFileAttachments.Remove(file);
            await _context.SaveChangesAsync();

            return RedirectToAction("PipelineDetails", new { id = file.PipelineTaskId });
        }



        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            var task = _context.PipelineTasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
                return NotFound();

            // 1. İlişkili log kayıtlarını sil
            var logs = _context.PipelineTaskLogs.Where(l => l.PipelineTaskId == id).ToList();
            _context.PipelineTaskLogs.RemoveRange(logs);

            // 2. İlişkili müşteri kaydını sil (CustomerN)
            if (task.CustomerId != null)
            {
                var customer = _context.CustomerNs.FirstOrDefault(c => c.Id == task.CustomerId);
                if (customer != null)
                {
                    _context.CustomerNs.Remove(customer);
                }
            }

            // 3. Görevi sil
            _context.PipelineTasks.Remove(task);

            _context.SaveChanges();

            return Json(new { success = true });
        }



        private List<SelectListItem> BuildCompanyUsersSelectList(string currentUserId)
        {
            var me = _context.Users.AsNoTracking().FirstOrDefault(u => u.Id == currentUserId);
            if (me == null) return new();

            return _context.Users.AsNoTracking()
                .Where(u => u.EmailDomain == me.EmailDomain)             // 🔒 sadece aynı domain
                .OrderBy(u => u.UserName)
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.Id == currentUserId ? $"{u.UserName} (ben)" : u.UserName
                })
                .ToList();
        }


    }
}
