using ClosedXML.Excel;
using System.Globalization;
using CrmCorner.Extensions;
using CrmCorner.Models;
using CrmCorner.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using PipelineStage = CrmCorner.Models.Enums.PipelineStage;

namespace CrmCorner.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,TeamLeader,TeamMember")]
    [ModuleAuthorize(ModuleType.CRM)]
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
            else if (isTeamLeader)
            {
                // Bu TeamLeader'a bağlı takım üyeleri
                var teamMemberIds = await _context.TeamLeaderMembers
                    .Where(x => x.TeamLeaderId == currentUserId)
                    .Select(x => x.TeamMemberId)
                    .Distinct()
                    .ToListAsync();

                // Lider kendisini de görsün
                teamMemberIds.Add(currentUserId);

                tasksQuery = tasksQuery.Where(t =>
                    (t.AppUserId != null && teamMemberIds.Contains(t.AppUserId)) ||
                    (t.ResponsibleUserId != null && teamMemberIds.Contains(t.ResponsibleUserId))
                );
            }
            else if (isTeamMember)
            {
                tasksQuery = tasksQuery.Where(t =>
                    t.AppUserId == currentUserId || t.ResponsibleUserId == currentUserId
                );
            }

            // 🔹 Görevleri listele
            var tasks = await tasksQuery
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();

            // 🔹 Görev sayısı (kişisel bazda)
            if (isSuperAdmin)
            {
                ViewBag.PipelineTaskCount = await _context.PipelineTasks.CountAsync();
            }
            else if (isAdmin)
            {
                var sameCompanyUserIds = companyUsers
                    .Where(u => u.CompanyId == me.CompanyId)
                    .Select(u => u.Id)
                    .ToList();

                ViewBag.PipelineTaskCount = await _context.PipelineTasks.CountAsync(t =>
                    (t.AppUserId != null && sameCompanyUserIds.Contains(t.AppUserId)) ||
                    (t.ResponsibleUserId != null && sameCompanyUserIds.Contains(t.ResponsibleUserId))
                );
            }
            else if (isTeamLeader)
            {
                var teamMemberIds = await _context.TeamLeaderMembers
                    .Where(x => x.TeamLeaderId == currentUserId)
                    .Select(x => x.TeamMemberId)
                    .Distinct()
                    .ToListAsync();

                teamMemberIds.Add(currentUserId);

                ViewBag.PipelineTaskCount = await _context.PipelineTasks.CountAsync(t =>
                    (t.AppUserId != null && teamMemberIds.Contains(t.AppUserId)) ||
                    (t.ResponsibleUserId != null && teamMemberIds.Contains(t.ResponsibleUserId))
                );
            }
            else
            {
                ViewBag.PipelineTaskCount = await _context.PipelineTasks.CountAsync(t =>
                    t.AppUserId == currentUserId || t.ResponsibleUserId == currentUserId);
            }

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
                else if (isTeamLeader)
                {
                    var teamMemberIds = _context.TeamLeaderMembers
                        .Where(x => x.TeamLeaderId == currentUserId)
                        .Select(x => x.TeamMemberId)
                        .Distinct()
                        .ToList();

                    teamMemberIds.Add(currentUserId);

                    bool canAccess =
                        (task.AppUserId != null && teamMemberIds.Contains(task.AppUserId)) ||
                        (task.ResponsibleUserId != null && teamMemberIds.Contains(task.ResponsibleUserId));

                    if (!canAccess)
                        return Forbid();
                }
                else
                {
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
            if (!isSuperAdmin)
            {
                if (isAdmin)
                {
                    var taskCompanyId =
                        existing.ResponsibleUser?.CompanyId
                        ?? existing.AppUser?.CompanyId;

                    if (taskCompanyId != me.CompanyId)
                    {
                        TempData["ErrorMessage"] = "Bu görevi düzenleme yetkiniz yok.";
                        return RedirectToAction("PipelineIndex");
                    }
                }
                else if (isTeamLeader)
                {
                    var accessibleUserIds = _context.TeamLeaderMembers
                        .Where(x => x.TeamLeaderId == currentUserId)
                        .Select(x => x.TeamMemberId)
                        .Distinct()
                        .ToList();

                    if (!accessibleUserIds.Contains(currentUserId))
                        accessibleUserIds.Add(currentUserId);

                    bool canAccess =
                        (existing.AppUserId != null && accessibleUserIds.Contains(existing.AppUserId)) ||
                        (existing.ResponsibleUserId != null && accessibleUserIds.Contains(existing.ResponsibleUserId));

                    if (!canAccess)
                    {
                        TempData["ErrorMessage"] = "Bu görevi düzenleme yetkiniz yok.";
                        return RedirectToAction("PipelineIndex");
                    }
                }
                else if (isTeamMember)
                {
                    bool iAmInTask =
                        existing.AppUserId == currentUserId ||
                        existing.ResponsibleUserId == currentUserId;

                    if (!iAmInTask)
                    {
                        TempData["ErrorMessage"] = "Bu görevi düzenleme yetkiniz yok.";
                        return RedirectToAction("PipelineIndex");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Bu görevi düzenleme yetkiniz yok.";
                    return RedirectToAction("PipelineIndex");
                }
            }

            // --- Domain güvenliği (SuperAdmin hariç) ---
            bool IsCompanyUser(string? uid) =>
                isSuperAdmin || (
                    !string.IsNullOrWhiteSpace(uid) &&
                    _context.Users.Any(u => u.Id == uid && u.EmailDomain == me.EmailDomain)
                );

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

            // =========================================================
            // FORMDA GELMEYEN ALANLARI KORU (gereksiz log oluşmasın)
            // =========================================================
            bool HasFormValue(string key) => Request.Form.ContainsKey(key);

            if (!HasFormValue(nameof(model.ContactedViaLinkedIn)))
                model.ContactedViaLinkedIn = existing.ContactedViaLinkedIn;

            if (!HasFormValue(nameof(model.ContactedViaColdCall)))
                model.ContactedViaColdCall = existing.ContactedViaColdCall;

            if (!HasFormValue(nameof(model.Value)))
                model.Value = existing.Value;

            if (!HasFormValue(nameof(model.Source)))
                model.Source = existing.Source;

            if (!HasFormValue(nameof(model.SourceChannel)))
                model.SourceChannel = existing.SourceChannel;

            if (!HasFormValue(nameof(model.Outcomes)))
                model.Outcomes = existing.Outcomes;

            if (!HasFormValue(nameof(model.OutcomeStatus)))
                model.OutcomeStatus = existing.OutcomeStatus;

            if (!HasFormValue(nameof(model.NegativeReason)))
                model.NegativeReason = existing.NegativeReason;

            if (!HasFormValue(nameof(model.LinkedinUrl)))
                model.LinkedinUrl = existing.LinkedinUrl;

            if (!HasFormValue(nameof(model.ExpectedCloseDate)))
                model.ExpectedCloseDate = existing.ExpectedCloseDate;

            // --- LOG setup ---
            var now = DateTime.Now;
            var updaterUserId = currentUserId;

            string GetUserDisplayName(string? userId)
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return "Boş";

                var user = _context.Users.AsNoTracking().FirstOrDefault(x => x.Id == userId);
                if (user == null)
                    return "Boş";

                return string.IsNullOrWhiteSpace(user.NameSurname) ? user.UserName : user.NameSurname;
            }

            string GetCustomerDisplayName(int? customerId)
            {
                if (customerId == null)
                    return "Boş";

                var customer = _context.CustomerNs.AsNoTracking().FirstOrDefault(x => x.Id == customerId.Value);
                if (customer == null)
                    return "Boş";

                return $"{customer.Name} {customer.Surname} / {customer.CompanyName}";
            }

            string GetFieldDisplayName(string fieldName)
            {
                return fieldName switch
                {
                    nameof(PipelineTask.AppUserId) => "Görüşmeyi Alan",
                    nameof(PipelineTask.ResponsibleUserId) => "Sorumlu Kişi",
                    nameof(PipelineTask.Title) => "Başlık",
                    nameof(PipelineTask.Description) => "Açıklama",
                    nameof(PipelineTask.Value) => "Değer Teklifi",
                    nameof(PipelineTask.Currency) => "Para Birimi",
                    nameof(PipelineTask.Stage) => "Aşama",
                    nameof(PipelineTask.ExpectedCloseDate) => "Planlanan Kapanış Tarihi",
                    nameof(PipelineTask.CustomerName) => "Müşteri Adı",
                    nameof(PipelineTask.CustomerSurname) => "Müşteri Soyadı",
                    nameof(PipelineTask.CompanyName) => "Şirket Adı",
                    nameof(PipelineTask.Phone) => "Telefon",
                    nameof(PipelineTask.Email) => "E-posta",
                    nameof(PipelineTask.LinkedinUrl) => "LinkedIn URL",
                    nameof(PipelineTask.OutcomeStatus) => "Sonuç",
                    nameof(PipelineTask.NegativeReason) => "Olumsuz Sebep",
                    nameof(PipelineTask.Source) => "Kaynak",
                    nameof(PipelineTask.SourceChannel) => "Kaynak Kanalı",
                    nameof(PipelineTask.Outcomes) => "Genel Durum",
                    nameof(PipelineTask.ContactedViaLinkedIn) => "LinkedIn ile iletişim kuruldu",
                    nameof(PipelineTask.ContactedViaColdCall) => "Soğuk arama yapıldı",
                    nameof(PipelineTask.CustomerId) => "Müşteri",
                    _ => fieldName
                };
            }

            string FormatLogValue(string fieldName, object? value)
            {
                if (value == null)
                    return "Boş";

                var str = value.ToString();
                if (string.IsNullOrWhiteSpace(str))
                    return "Boş";

                if (fieldName == nameof(PipelineTask.AppUserId) || fieldName == nameof(PipelineTask.ResponsibleUserId))
                    return GetUserDisplayName(str);

                if (fieldName == nameof(PipelineTask.CustomerId))
                {
                    if (int.TryParse(str, out var customerId))
                        return GetCustomerDisplayName(customerId);

                    return "Boş";
                }

                if (fieldName == nameof(PipelineTask.ContactedViaLinkedIn) ||
                    fieldName == nameof(PipelineTask.ContactedViaColdCall))
                {
                    if (value is bool b)
                        return b ? "Evet" : "Hayır";

                    if (bool.TryParse(str, out var parsedBool))
                        return parsedBool ? "Evet" : "Hayır";
                }

                if (fieldName == nameof(PipelineTask.Stage))
                {
                    if (value is PipelineStage stageEnum)
                        return stageEnum.GetDisplayName();

                    if (Enum.TryParse<PipelineStage>(str, out var parsedStage))
                        return parsedStage.GetDisplayName();
                }

                if (fieldName == nameof(PipelineTask.Outcomes))
                {
                    if (value is OutcomeType outcomeEnum)
                        return outcomeEnum.GetDisplayName();

                    if (Enum.TryParse<OutcomeType>(str, out var parsedOutcome))
                        return parsedOutcome.GetDisplayName();
                }

                if (fieldName == nameof(PipelineTask.OutcomeStatus))
                {
                    if (value is OutcomeTypeSales salesEnum)
                        return salesEnum.GetDisplayName();

                    if (Enum.TryParse<OutcomeTypeSales>(str, out var parsedSales))
                        return parsedSales.GetDisplayName();
                }

                if (fieldName == nameof(PipelineTask.SourceChannel))
                {
                    if (value is SourceChannelType sourceEnum)
                        return sourceEnum.GetDisplayName();

                    if (Enum.TryParse<SourceChannelType>(str, out var parsedSource))
                        return parsedSource.GetDisplayName();
                }

                if (fieldName == nameof(PipelineTask.ExpectedCloseDate))
                {
                    if (value is DateTime dtValue)
                        return dtValue.ToString("dd.MM.yyyy");

                    if (DateTime.TryParse(str, out var dt))
                        return dt.ToString("dd.MM.yyyy");
                }

                if (fieldName == nameof(PipelineTask.Value))
                {
                    if (decimal.TryParse(str, out var decimalValue))
                    {
                        var currencyText = existing.Currency?.ToString();
                        return string.IsNullOrWhiteSpace(currencyText)
                            ? decimalValue.ToString("N2")
                            : $"{decimalValue:N2} {currencyText}";
                    }
                }

                return str;
            }

            void LogIfChanged(string fieldName, object? oldVal, object? newVal)
            {
                var oldFormatted = FormatLogValue(fieldName, oldVal);
                var newFormatted = FormatLogValue(fieldName, newVal);

                if (oldFormatted == newFormatted)
                    return;

                _context.PipelineTaskLogs.Add(new PipelineTaskLog
                {
                    PipelineTaskId = model.Id,
                    UpdatedField = GetFieldDisplayName(fieldName),
                    OldValue = oldFormatted,
                    NewValue = newFormatted,
                    UpdatedAt = now,
                    UpdatedById = updaterUserId
                });
            }

            LogIfChanged(nameof(model.AppUserId), existing.AppUserId, model.AppUserId);
            LogIfChanged(nameof(model.ResponsibleUserId), existing.ResponsibleUserId, model.ResponsibleUserId);
            LogIfChanged(nameof(model.Title), existing.Title, model.Title);
            LogIfChanged(nameof(model.Description), existing.Description, model.Description);
            LogIfChanged(nameof(model.Value), existing.Value, model.Value);
            LogIfChanged(nameof(model.Currency), existing.Currency, model.Currency);
            LogIfChanged(nameof(model.Stage), existing.Stage, model.Stage);
            LogIfChanged(nameof(model.ExpectedCloseDate), existing.ExpectedCloseDate, model.ExpectedCloseDate);
            LogIfChanged(nameof(model.CustomerName), existing.CustomerName, model.CustomerName);
            LogIfChanged(nameof(model.CustomerSurname), existing.CustomerSurname, model.CustomerSurname);
            LogIfChanged(nameof(model.CompanyName), existing.CompanyName, model.CompanyName);
            LogIfChanged(nameof(model.Phone), existing.Phone, model.Phone);
            LogIfChanged(nameof(model.Email), existing.Email, model.Email);
            LogIfChanged(nameof(model.LinkedinUrl), existing.LinkedinUrl, model.LinkedinUrl);
            LogIfChanged(nameof(model.OutcomeStatus), existing.OutcomeStatus, model.OutcomeStatus);
            LogIfChanged(nameof(model.NegativeReason), existing.NegativeReason, model.NegativeReason);
            LogIfChanged(nameof(model.Source), existing.Source, model.Source);
            LogIfChanged(nameof(model.SourceChannel), existing.SourceChannel, model.SourceChannel);
            LogIfChanged(nameof(model.Outcomes), existing.Outcomes, model.Outcomes);
            LogIfChanged(nameof(model.ContactedViaLinkedIn), existing.ContactedViaLinkedIn, model.ContactedViaLinkedIn);
            LogIfChanged(nameof(model.ContactedViaColdCall), existing.ContactedViaColdCall, model.ContactedViaColdCall);
            LogIfChanged(nameof(model.CustomerId), existing.CustomerId, model.CustomerId);

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
            var task = _context.PipelineTasks
                .Include(t => t.AppUser)
                .Include(t => t.ResponsibleUser)
                .FirstOrDefault(t => t.Id == id);

            if (task == null)
                return NotFound();

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var me = _context.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (me == null)
                return Unauthorized();

            var myRoles = _context.UserRoles
                .Where(x => x.UserId == currentUserId)
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => r.Name)
                .ToList();

            bool isSuperAdmin = myRoles.Contains("SuperAdmin");
            bool isAdmin = myRoles.Contains("Admin");
            bool isTeamLeader = myRoles.Contains("TeamLeader");

            bool canAccess = false;

            if (isSuperAdmin)
            {
                canAccess = true;
            }
            else if (isAdmin)
            {
                var taskCompanyId =
                    task.ResponsibleUser?.CompanyId ??
                    task.AppUser?.CompanyId;

                canAccess = taskCompanyId == me.CompanyId;
            }
            else if (isTeamLeader)
            {
                var accessibleUserIds = _context.TeamLeaderMembers
                    .Where(x => x.TeamLeaderId == currentUserId)
                    .Select(x => x.TeamMemberId)
                    .Distinct()
                    .ToList();

                accessibleUserIds.Add(currentUserId);

                canAccess =
                    (task.AppUserId != null && accessibleUserIds.Contains(task.AppUserId)) ||
                    (task.ResponsibleUserId != null && accessibleUserIds.Contains(task.ResponsibleUserId));
            }
            else
            {
                canAccess =
                    task.AppUserId == currentUserId ||
                    task.ResponsibleUserId == currentUserId;
            }

            if (!canAccess)
                return Forbid();

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
                    var taskCompanyId = task.ResponsibleUser?.CompanyId ?? task.AppUser?.CompanyId;

                    if (taskCompanyId != me.CompanyId)
                    {
                        TempData["ErrorMessage"] = "Bu görevin detayını görüntüleme yetkiniz yok.";
                        return RedirectToAction("PipelineIndex");
                    }
                }
                else if (myRoles.Contains("TeamLeader"))
                {
                    var accessibleUserIds = _context.TeamLeaderMembers
                        .Where(x => x.TeamLeaderId == currentUserId)
                        .Select(x => x.TeamMemberId)
                        .Distinct()
                        .ToList();

                    if (!accessibleUserIds.Contains(currentUserId))
                        accessibleUserIds.Add(currentUserId);

                    bool canAccess =
                        (task.AppUserId != null && accessibleUserIds.Contains(task.AppUserId)) ||
                        (task.ResponsibleUserId != null && accessibleUserIds.Contains(task.ResponsibleUserId));

                    if (!canAccess)
                    {
                        TempData["ErrorMessage"] = "Bu görevin detayını görüntüleme yetkiniz yok.";
                        return RedirectToAction("PipelineIndex");
                    }
                }
                else
                {
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



        [HttpGet]
        public IActionResult DownloadPipelineTemplate()
        {
            using var workbook = new XLWorkbook();

            var ws = workbook.Worksheets.Add("Template");

            ws.Cell(1, 1).Value = "Baslik *";
            ws.Cell(1, 2).Value = "DegerTeklifi";
            ws.Cell(1, 3).Value = "MusteriAd *";
            ws.Cell(1, 4).Value = "MusteriSoyad *";
            ws.Cell(1, 5).Value = "SirketAdi *";
            ws.Cell(1, 6).Value = "Telefon";
            ws.Cell(1, 7).Value = "Eposta *";
            ws.Cell(1, 9).Value = "GorusmeyiAlanEmail";
            ws.Cell(1, 10).Value = "SorumluKisiEmail *";

            var headerRange = ws.Range("A1:J1");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            int[] requiredCols = { 1, 3, 4, 5, 7, 8, 10 };
            foreach (var col in requiredCols)
            {
                ws.Cell(1, col).Style.Fill.BackgroundColor = XLColor.LightYellow;
            }

            ws.SheetView.FreezeRows(1);
            ws.Columns().AdjustToContents();

            var info = workbook.Worksheets.Add("Aciklamalar");

            info.Cell(1, 1).Value = "ZORUNLU ALANLAR";
            info.Cell(2, 1).Value = "Baslik, MusteriAd, MusteriSoyad, SirketAdi, Eposta, KaynakKanal, SorumluKisiEmail";

            info.Cell(4, 1).Value = "ASAMA";
            info.Cell(5, 1).Value = "Import edilen tum kayitlar otomatik olarak Degerlendirilen asamasinda olusur.";

            info.Cell(7, 1).Value = "GORUSMEYI ALAN";
            info.Cell(8, 1).Value = "GorusmeyiAlanEmail bos birakilirsa import eden kullanici atanir.";

            info.Cell(10, 1).Value = "SORUMLU KISI";
            info.Cell(11, 1).Value = "SorumluKisiEmail zorunludur ve sistemde kayitli bir kullanici olmalidir.";

            info.Cell(13, 1).Value = "DEGER TEKLIFI";
            info.Cell(14, 1).Value = "Bos birakilabilir. Sonradan ekran uzerinden duzenlenebilir.";

            info.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "PipelineTemplate.xlsx"
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportPipelineExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Lütfen geçerli bir Excel dosyası seçin.";
                return RedirectToAction("PipelineIndex");
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var me = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (me == null) return Unauthorized();

            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == me.Id)
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .ToListAsync();

            bool isSuperAdmin = userRoles.Contains("SuperAdmin");

            bool IsSameCompanyUser(AppUser user)
            {
                if (isSuperAdmin) return true;
                return user.EmailDomain == me.EmailDomain;
            }

            var successCount = 0;
            var errorMessages = new List<string>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var ws = workbook.Worksheet(1);

            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;
            if (lastRow < 2)
            {
                TempData["ErrorMessage"] = "Excel dosyasında aktarılacak veri bulunamadı.";
                return RedirectToAction("PipelineIndex");
            }

            for (int row = 2; row <= lastRow; row++)
            {
                try
                {
                    string GetText(int col)
                    {
                        return ws.Cell(row, col).GetValue<string>()?.Trim();
                    }

                    decimal? GetDecimal(int col)
                    {
                        var txt = GetText(col);
                        if (string.IsNullOrWhiteSpace(txt)) return null;

                        txt = txt.Replace(",", ".");
                        if (decimal.TryParse(txt, NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
                            return val;

                        return null;
                    }

                    var title = GetText(1);
                    var value = GetDecimal(2);
                    var customerName = GetText(3);
                    var customerSurname = GetText(4);
                    var companyName = GetText(5);
                    var phone = GetText(6);
                    var email = GetText(7);
                    var takenByEmail = GetText(8);
                    var responsibleEmail = GetText(9);

                    if (string.IsNullOrWhiteSpace(title) ||
                        string.IsNullOrWhiteSpace(customerName) ||
                        string.IsNullOrWhiteSpace(customerSurname) ||
                        string.IsNullOrWhiteSpace(companyName) ||
                        string.IsNullOrWhiteSpace(email) ||
                        string.IsNullOrWhiteSpace(responsibleEmail))
                    {
                        errorMessages.Add($"Satır {row}: Zorunlu alanlardan biri boş.");
                        continue;
                    }

                    var responsibleUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == responsibleEmail.ToLower());

                    if (responsibleUser == null)
                    {
                        errorMessages.Add($"Satır {row}: SorumluKisiEmail sistemde bulunamadı.");
                        continue;
                    }

                    if (!IsSameCompanyUser(responsibleUser))
                    {
                        errorMessages.Add($"Satır {row}: Sorumlu kişi aynı şirket/domain içinde değil.");
                        continue;
                    }

                    AppUser takenByUser = null;

                    if (string.IsNullOrWhiteSpace(takenByEmail))
                    {
                        takenByUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
                    }
                    else
                    {
                        takenByUser = await _context.Users
                            .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == takenByEmail.ToLower());

                        if (takenByUser == null)
                        {
                            errorMessages.Add($"Satır {row}: GorusmeyiAlanEmail sistemde bulunamadı.");
                            continue;
                        }

                        if (!IsSameCompanyUser(takenByUser))
                        {
                            errorMessages.Add($"Satır {row}: Görüşmeyi alan kişi aynı şirket/domain içinde değil.");
                            continue;
                        }
                    }

                    var newCustomer = new CustomerN
                    {
                        Name = customerName,
                        Surname = customerSurname,
                        CompanyName = companyName,
                        PhoneNumber = phone,
                        CustomerEmail = email,
                        CreatedDate = DateTime.Now,
                        AppUserId = responsibleUser.Id
                    };

                    _context.CustomerNs.Add(newCustomer);
                    await _context.SaveChangesAsync();

                    var newTask = new PipelineTask
                    {
                        Title = title,
                        Value = value,
                        CustomerName = customerName,
                        CustomerSurname = customerSurname,
                        CompanyName = companyName,
                        Phone = phone,
                        Email = email,
                        AppUserId = takenByUser?.Id,
                        ResponsibleUserId = responsibleUser.Id,
                        CustomerId = newCustomer.Id,
                        CreatedDate = DateTime.Now,

                        // otomatik alanlar
                        Stage = PipelineStage.Degerlendirilen,
                        Outcomes = OutcomeType.Surecte,
                        OutcomeStatus = OutcomeTypeSales.None,

                        // TODO: sende hangi enum uygunsa onunla değiştir
                        SourceChannel = SourceChannelType.Apollo
                    };

                    _context.PipelineTasks.Add(newTask);
                    await _context.SaveChangesAsync();

                    successCount++;
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"Satır {row}: Beklenmeyen hata - {ex.Message}");
                }
            }

            if (errorMessages.Any())
            {
                TempData["ErrorMessage"] =
                    $"Aktarılan kayıt: {successCount}. Hatalı satır: {errorMessages.Count}. İlk hatalar: {string.Join(" | ", errorMessages.Take(5))}";
            }
            else
            {
                TempData["SuccessMessage"] = $"{successCount} kayıt başarıyla içe aktarıldı.";
            }

            return RedirectToAction("PipelineIndex");
        }

    }
}
