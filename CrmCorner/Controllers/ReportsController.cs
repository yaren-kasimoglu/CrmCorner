using CrmCorner.Models;
using CrmCorner.Models.Enums;
using CrmCorner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CrmCorner.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin,TeamLeader,TeamMember")]
    public class ReportsController : Controller
    {
        private readonly CrmCornerContext _context;

        public ReportsController(CrmCornerContext context)
        {
            _context = context;
        }

        public IActionResult ResponsibleUserTasks(DateTime? startDate, DateTime? endDate)
        {
            // Aktif kullanıcıyı bul
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var me = _context.Users.FirstOrDefault(u => u.Id == currentUserId);
            if (me == null) return Unauthorized();

            // Aynı şirketteki tüm kullanıcıları bul (domain bazlı güvenlik)
            var companyUsers = _context.Users
                .Where(u => u.EmailDomain == me.EmailDomain)
                .ToList();

            // Tarih aralığı belirlenmemişse default: son 30 gün
            startDate ??= DateTime.Now.AddDays(-30);
            endDate ??= DateTime.Now;

            // Task'ları çek
            var tasks = _context.PipelineTasks
                .Include(t => t.ResponsibleUser)
                .Where(t => t.CreatedDate >= startDate && t.CreatedDate <= endDate
                            && t.ResponsibleUserId != null
                            && companyUsers.Select(u => u.Id).Contains(t.ResponsibleUserId))
                .ToList();

            // Kullanıcı bazlı gruplama
            var model = tasks
                .GroupBy(t => t.ResponsibleUser)
                .Select(g => new ResponsibleUserTaskReportVm
                {
                    ResponsibleUserName = g.Key?.NameSurname ?? "Bilinmiyor",
                    TotalTasks = g.Count(),
                    OngoingTasks = g.Count(x => x.Outcomes == OutcomeType.Surecte),
                    SuccessfulTasks = g.Count(x => x.OutcomeStatus == OutcomeTypeSales.Won),
                    FailedTasks = g.Count(x => x.OutcomeStatus == OutcomeTypeSales.Lost),
                    TaskList = g.ToList()
                })
                .ToList();

            return View(model);
        }

        public IActionResult StageByUserReports(DateTime? startDate, DateTime? endDate)
{
    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var me = _context.Users.FirstOrDefault(u => u.Id == currentUserId);
    if (me == null) return Unauthorized();

    var companyUsers = _context.Users
        .Where(u => u.EmailDomain == me.EmailDomain)
        .ToList();

    startDate ??= DateTime.Now.AddDays(-30);
    endDate ??= DateTime.Now;

    var tasks = _context.PipelineTasks
        .Include(t => t.ResponsibleUser)
        .Where(t => t.CreatedDate >= startDate && t.CreatedDate <= endDate
                    && t.ResponsibleUserId != null
                    && companyUsers.Select(u => u.Id).Contains(t.ResponsibleUserId))
        .ToList();

            var reports = tasks
                .GroupBy(t => t.ResponsibleUser?.NameSurname)
                .Select(g => new StageByUserRowVm
                {
                    ResponsibleUserName = g.Key ?? "Bilinmiyor",
                    Degerlendirilen = g.Count(x => x.Stage == PipelineStage.Degerlendirilen),
                    IletisimKuruldu = g.Count(x => x.Stage == PipelineStage.IletisimKuruldu),
                    ToplantiDuzenlendi = g.Count(x => x.Stage == PipelineStage.ToplantiDuzenlendi),
                    TeklifSunuldu = g.Count(x => x.Stage == PipelineStage.TeklifSunuldu),
                    Sonuc = g.Count(x => x.Stage == PipelineStage.Sonuc),
                    TaskTitlesByStage = new Dictionary<string, List<string>>
                    {
                        ["Değerlendirilen"] = g.Where(x => x.Stage == PipelineStage.Degerlendirilen).Select(x => x.Title).ToList(),
                        ["İletişim Kuruldu"] = g.Where(x => x.Stage == PipelineStage.IletisimKuruldu).Select(x => x.Title).ToList(),
                        ["Toplantı Düzenlendi"] = g.Where(x => x.Stage == PipelineStage.ToplantiDuzenlendi).Select(x => x.Title).ToList(),
                        ["Teklif Sunuldu"] = g.Where(x => x.Stage == PipelineStage.TeklifSunuldu).Select(x => x.Title).ToList(),
                        ["Sonuç"] = g.Where(x => x.Stage == PipelineStage.Sonuc).Select(x => x.Title).ToList()
                    }
                })
                .ToList();


            return View(reports);
}

        [AllowAnonymous]
        public async Task<IActionResult> TeamDashboard(DateTime? startDate, DateTime? endDate)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var me = await _context.Users.FirstOrDefaultAsync(x => x.Id == currentUserId);
            if (me == null) return Unauthorized();

            var start = (startDate ?? DateTime.Now.AddDays(-30)).Date;
            var endExclusive = (endDate ?? DateTime.Now).Date.AddDays(1);

            var users = await _context.Users
                .Where(x => x.EmailDomain == me.EmailDomain)
                .ToListAsync();

            var userIds = users.Select(x => x.Id).ToList();

            var tasks = await _context.PipelineTasks
                .Where(x =>
                    x.CreatedDate >= start &&
                    x.CreatedDate < endExclusive &&
                    x.ResponsibleUserId != null &&
                    userIds.Contains(x.ResponsibleUserId))
                .ToListAsync();

            var totalCompanyBudget = GetBudgetByCurrency(tasks);
            var totalCompanyWonBudget = GetBudgetByCurrency(tasks.Where(x => x.OutcomeStatus == OutcomeTypeSales.Won));

            var userReports = users.Select(u =>
            {
                var userTasks = tasks.Where(t => t.ResponsibleUserId == u.Id).ToList();

                var userTotalBudget = GetBudgetByCurrency(userTasks);
                var userWonBudget = GetBudgetByCurrency(userTasks.Where(x => x.OutcomeStatus == OutcomeTypeSales.Won));

                return new TeamDashboardUserVm
                {
                    UserId = u.Id,

                    Name = !string.IsNullOrWhiteSpace(u.NameSurname)
                        ? u.NameSurname
                        : (!string.IsNullOrWhiteSpace(u.Email) ? u.Email : "İsimsiz Kullanıcı"),

                    Role = "Admin",

                    Total = userTasks.Count,
                    Won = userTasks.Count(x => x.OutcomeStatus == OutcomeTypeSales.Won),
                    Lost = userTasks.Count(x => x.OutcomeStatus == OutcomeTypeSales.Lost),
                    Ongoing = userTasks.Count(x => x.Outcomes == OutcomeType.Surecte),

                    TotalBudgetByCurrency = userTotalBudget,
                    WonBudgetByCurrency = userWonBudget,
                    LostBudgetByCurrency = GetBudgetByCurrency(userTasks.Where(x => x.OutcomeStatus == OutcomeTypeSales.Lost)),

                    TotalBudgetContributionByCurrency = userTotalBudget,
                    WonBudgetContributionByCurrency = userWonBudget,

                    TotalBudgetContributionRateByCurrency = GetContributionRates(userTotalBudget, totalCompanyBudget),
                    WonBudgetContributionRateByCurrency = GetContributionRates(userWonBudget, totalCompanyWonBudget)
                };
            })
            .OrderByDescending(x => x.Won)
            .ThenByDescending(x => x.Total)
            .ToList();

            var model = new TeamDashboardVm
            {
                TotalTasks = tasks.Count,
                Ongoing = tasks.Count(x => x.Outcomes == OutcomeType.Surecte),
                Won = tasks.Count(x => x.OutcomeStatus == OutcomeTypeSales.Won),
                Lost = tasks.Count(x => x.OutcomeStatus == OutcomeTypeSales.Lost),

                TotalBudgetByCurrency = GetBudgetByCurrency(tasks),
                WonBudgetByCurrency = GetBudgetByCurrency(tasks.Where(x => x.OutcomeStatus == OutcomeTypeSales.Won)),
                LostBudgetByCurrency = GetBudgetByCurrency(tasks.Where(x => x.OutcomeStatus == OutcomeTypeSales.Lost)),

                Users = userReports
            };

            ViewBag.StartDate = start.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endExclusive.AddDays(-1).ToString("yyyy-MM-dd");

            return View(model);
        }

        public async Task<IActionResult> TeamUserDetail(string userId, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest();

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var me = await _context.Users.FirstOrDefaultAsync(x => x.Id == currentUserId);
            if (me == null) return Unauthorized();

            var selectedUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (selectedUser == null) return NotFound();

            // Firma güvenliği
            if (selectedUser.EmailDomain != me.EmailDomain)
                return Forbid();

            // TeamMember sadece kendini görebilsin
            if (User.IsInRole("TeamMember") && userId != currentUserId)
                return Forbid();

            var start = (startDate ?? DateTime.Now.AddDays(-30)).Date;
            var endExclusive = (endDate ?? DateTime.Now).Date.AddDays(1);

            var tasks = await _context.PipelineTasks
                .Where(x =>
                    x.ResponsibleUserId == userId &&
                    x.CreatedDate >= start &&
                    x.CreatedDate < endExclusive)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            var model = new TeamUserDetailVm
            {
                UserId = selectedUser.Id,
                Name = !string.IsNullOrWhiteSpace(selectedUser.NameSurname)
                    ? selectedUser.NameSurname
                    : selectedUser.Email ?? "İsimsiz Kullanıcı",

                Role = "Kullanıcı",

                Total = tasks.Count,
                Ongoing = tasks.Count(x => x.Outcomes == OutcomeType.Surecte),
                Won = tasks.Count(x => x.OutcomeStatus == OutcomeTypeSales.Won),
                Lost = tasks.Count(x => x.OutcomeStatus == OutcomeTypeSales.Lost),

                Tasks = tasks
            };

            ViewBag.StartDate = start.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endExclusive.AddDays(-1).ToString("yyyy-MM-dd");

            return View(model);
        }


        private Dictionary<string, decimal> GetBudgetByCurrency(IEnumerable<PipelineTask> tasks)
        {
            return tasks
                .Where(x => x.Value.HasValue && x.Value.Value > 0)
                .GroupBy(x => string.IsNullOrWhiteSpace(x.Currency) ? "₺" : x.Currency)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.Value ?? 0)
                );
        }

        private Dictionary<string, decimal> GetContributionRates(
    Dictionary<string, decimal> userBudget,
    Dictionary<string, decimal> totalBudget)
        {
            var result = new Dictionary<string, decimal>();

            foreach (var item in userBudget)
            {
                var currency = item.Key;
                var userValue = item.Value;

                if (totalBudget.ContainsKey(currency) && totalBudget[currency] > 0)
                {
                    result[currency] = Math.Round((userValue / totalBudget[currency]) * 100, 2);
                }
                else
                {
                    result[currency] = 0;
                }
            }

            return result;
        }


        public async Task<IActionResult> WeeklyMeetingReport()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var me = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (me == null)
                return Unauthorized();

            var today = DateTime.Today;
            var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var weekStart = today.AddDays(-diff);
            var weekEnd = weekStart.AddDays(7);

            var companyUserIds = await _context.Users.AsNoTracking()
                .Where(u => u.EmailDomain == me.EmailDomain)
                .Select(u => u.Id)
                .ToListAsync();

            var report = await _context.PipelineTasks
             .AsNoTracking()
             .Where(t =>
                 t.MeetingUserId != null &&
                 companyUserIds.Contains(t.MeetingUserId) &&
                 t.CreatedDate >= weekStart &&
                 t.CreatedDate < weekEnd)
             .GroupBy(t => t.MeetingUserId)
             .Select(g => new WeeklyMeetingReportViewModel
             {
                 UserId = g.Key,
                 MeetingCount = g.Count()
             })
             .ToListAsync();

            var users = await _context.Users.AsNoTracking()
                .Where(u => companyUserIds.Contains(u.Id))
                .ToListAsync();

            foreach (var item in report)
            {
                var user = users.FirstOrDefault(u => u.Id == item.UserId);

                item.UserName = user == null
                    ? "Bilinmeyen Kullanıcı"
                    : string.IsNullOrWhiteSpace(user.NameSurname)
                        ? user.UserName
                        : user.NameSurname;

                var companies = await _context.PipelineTasks
                    .AsNoTracking()
                    .Where(t =>
                        t.MeetingUserId == item.UserId &&
                        t.CreatedDate >= weekStart &&
                        t.CreatedDate < weekEnd &&
                        !string.IsNullOrWhiteSpace(t.CompanyName))
                    .Select(t => t.CompanyName)
                    .Distinct()
                    .Take(10)
                    .ToListAsync();

                item.Companies = string.Join(", ", companies);
            }

            ViewBag.WeekStart = weekStart;
            ViewBag.WeekEnd = weekEnd.AddDays(-1);

            return View(report.OrderByDescending(x => x.MeetingCount).ToList());
        }

    }
}