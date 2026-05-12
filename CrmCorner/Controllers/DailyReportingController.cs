using CrmCorner.Migrations;
using CrmCorner.Models;
using CrmCorner.Models.Enums;
using CrmCorner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CrmCorner.Controllers
{
    [Authorize]
    public class DailyReportingController : Controller
    {
        private readonly CrmCornerContext _context;
        private readonly UserManager<AppUser> _userManager;

        public DailyReportingController(
            CrmCornerContext context,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Unauthorized();

            var roles = await _userManager.GetRolesAsync(currentUser);

            bool isSuperAdmin = roles.Contains("SuperAdmin");
            bool isAdmin = roles.Contains("Admin");
            bool isTeamLeader = roles.Contains("TeamLeader");
            bool isTeamMember = roles.Contains("TeamMember");

            IQueryable<AppUser> usersQuery = _context.Users.AsNoTracking();

            // SuperAdmin herkesi görür
            if (isSuperAdmin)
            {
                usersQuery = usersQuery
                    .Where(x => x.EmailDomain == currentUser.EmailDomain);
            }
            // Admin ve TeamLeader sadece kendi firmasındaki herkesi görür
            else if (isAdmin || isTeamLeader)
            {
                usersQuery = usersQuery
                    .Where(x => x.EmailDomain == currentUser.EmailDomain);
            }
            // TeamMember sadece kendi firmasındaki TeamMember kullanıcıları görür
            else if (isTeamMember)
            {
                var teamMemberUserIds = await _context.UserRoles
                    .Join(_context.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => new { ur.UserId, RoleName = r.Name })
                    .Where(x => x.RoleName == "TeamMember")
                    .Select(x => x.UserId)
                    .ToListAsync();

                usersQuery = usersQuery
                    .Where(x =>
                        x.EmailDomain == currentUser.EmailDomain &&
                        teamMemberUserIds.Contains(x.Id));
            }
            else
            {
                usersQuery = usersQuery
                    .Where(x => x.Id == currentUser.Id);
            }

            var users = await usersQuery
                .OrderBy(x => x.NameSurname)
                .Select(x => new DailyReportingUserListViewModel
                {
                    UserId = x.Id,
                    FullName = x.NameSurname,
                    Email = x.Email
                })
                .ToListAsync();

            return View(users);
        }


        [HttpPost]
        public async Task<IActionResult> SaveCell([FromBody] SaveDailyCellRequest request)
        {
            var existing = await _context.DailyReports.FirstOrDefaultAsync(x =>
                x.AppUserId == request.UserId &&
                x.CompanyName == request.CompanyName &&
                x.ReportDate == request.Date &&
                x.ActivityType == request.ActivityType);

            if (existing == null)
            {
                existing = new DailyReport
                {
                    AppUserId = request.UserId,
                    CompanyName = request.CompanyName,
                    ReportDate = request.Date,
                    ActivityType = request.ActivityType,
                    CreatedAt = DateTime.Now
                };

                _context.DailyReports.Add(existing);
            }

            if (request.Type == "P")
                existing.ProspectTarget = request.Value;

            if (request.Type == "A")
                existing.ActualValue = request.Value;

            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        public async Task<IActionResult> Detail(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return RedirectToAction("Index");

            var selectedUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (selectedUser == null)
                return NotFound();

            var today = DateTime.Today;

            var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var weekStart = today.AddDays(-diff).Date;
            var weekEnd = weekStart.AddDays(4).Date; // Pazartesi - Cuma

            var companies = new List<string>
    {
        "SAAS Corner"
    };

            var pipelineCompanies = await _context.PipelineTasks
            .AsNoTracking()
            .Where(x =>
                x.ResponsibleUserId == userId &&
               x.OutcomeStatus == CrmCorner.Models.Enums.OutcomeTypeSales.Won)
            .Select(x => x.CompanyName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToListAsync();

            companies.AddRange(pipelineCompanies);

            var customCompanies = await _context.DailyReports
                .AsNoTracking()
                .Where(x =>
                    x.AppUserId == userId &&
                    x.ReportDate >= weekStart &&
                    x.ReportDate <= weekEnd)
                .Select(x => x.CompanyName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToListAsync();

            companies.AddRange(customCompanies);
            companies = companies.Distinct().ToList();

            var activityTypes = new List<string>
{
                "LinkedIn Bağlantı Sayısı",
    "E-Mails",
    "LinkedIn Gönderilen Mesaj",
    "LinkedIn Gönderilen Bağlantı",
    "Arama",
    "Meeting Planlama",
    "Meeting Gerçekleşen"
};

            var existingReports = await _context.DailyReports
                .AsNoTracking()
                .Where(x =>
                    x.AppUserId == userId &&
                    x.ReportDate >= weekStart &&
                    x.ReportDate <= weekEnd)
                .ToListAsync();

            var model = new DailyReportingUserDetailViewModel
            {
                UserId = selectedUser.Id,
                FullName = selectedUser.NameSurname,
                WeekStartDate = weekStart,
                WeekEndDate = weekEnd
            };

            foreach (var company in companies)
            {
                var companyTable = new DailyReportingCompanyTableViewModel
                {
                    CompanyName = company
                };

                foreach (var activity in activityTypes)
                {
                    var row = new DailyReportingActivityRowViewModel
                    {
                        ActivityType = activity
                    };

                    for (var date = weekStart; date <= weekEnd; date = date.AddDays(1))
                    {
                        var report = existingReports.FirstOrDefault(x =>
                            x.CompanyName == company &&
                            x.ActivityType == activity &&
                            x.ReportDate.Date == date.Date);

                        row.Days[date.DayOfWeek] = new DailyReportingDayCellViewModel
                        {
                            P = report?.ProspectTarget ?? 0,
                            A = report?.ActualValue ?? 0
                        };
                    }

                    if (activity == "LinkedIn Bağlantı Sayısı")
                    {
                        var mondayCell = row.Days.ContainsKey(DayOfWeek.Monday)
                            ? row.Days[DayOfWeek.Monday]
                            : null;

                        var fridayCell = row.Days.ContainsKey(DayOfWeek.Friday)
                            ? row.Days[DayOfWeek.Friday]
                            : null;

                        row.TotalP = mondayCell?.P ?? 0;
                        row.TotalA = fridayCell?.A ?? 0;
                        row.TotalDifference = row.TotalA - row.TotalP;
                    }
                    else
                    {
                        row.TotalP = row.Days.Sum(x => x.Value.P);
                        row.TotalA = row.Days.Sum(x => x.Value.A);
                        row.TotalDifference = row.TotalA - row.TotalP;
                    }

                    companyTable.Rows.Add(row);
                }

                model.CompanyTables.Add(companyTable);
            }

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> AddCustomTable(string userId, string tableName)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(tableName))
                return RedirectToAction("Detail", new { userId });

            tableName = tableName.Trim();

            var today = DateTime.Today;
            var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var weekStart = today.AddDays(-diff).Date;
            var weekEnd = weekStart.AddDays(4).Date;

            var activityTypes = new List<string>
    {
        "LinkedIn Bağlantı Sayısı",
        "E-Mails",
        "LinkedIn Gönderilen Mesaj",
        "LinkedIn Gönderilen Bağlantı",
        "Arama",
        "Meeting Planlama",
        "Meeting Gerçekleşen"
    };

            var alreadyExists = await _context.DailyReports.AnyAsync(x =>
                x.AppUserId == userId &&
                x.CompanyName == tableName &&
                x.ReportDate >= weekStart &&
                x.ReportDate <= weekEnd);

            if (!alreadyExists)
            {
                for (var date = weekStart; date <= weekEnd; date = date.AddDays(1))
                {
                    foreach (var activity in activityTypes)
                    {
                        _context.DailyReports.Add(new DailyReport
                        {
                            AppUserId = userId,
                            CompanyName = tableName,
                            ReportDate = date,
                            ActivityType = activity,
                            ProspectTarget = 0,
                            ActualValue = 0,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Detail", new { userId });
        }


        [HttpPost]
        public async Task<IActionResult> DeleteCustomTable(string userId, string tableName)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(tableName))
                return RedirectToAction("Detail", new { userId });

            var today = DateTime.Today;
            var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var weekStart = today.AddDays(-diff).Date;
            var weekEnd = weekStart.AddDays(4).Date;

            var reports = await _context.DailyReports
                .Where(x =>
                    x.AppUserId == userId &&
                    x.CompanyName == tableName &&
                    x.ReportDate >= weekStart &&
                    x.ReportDate <= weekEnd)
                .ToListAsync();

            if (reports.Any())
            {
                _context.DailyReports.RemoveRange(reports);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Detail", new { userId });
        }

        public async Task<IActionResult> WeeklySummary(DateTime? startDate, DateTime? endDate, string userId, string companyName, string summaryType)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Unauthorized();

            var today = DateTime.Today;

            var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var currentWeekStart = today.AddDays(-diff).Date;
            var currentWeekEnd = currentWeekStart.AddDays(4).Date; // Cuma

            var start = startDate?.Date ?? today.AddDays(-30);
            var end = endDate?.Date ?? currentWeekEnd;

            var query = _context.DailyReports
                .AsNoTracking()
                .Where(x => x.ReportDate >= start && x.ReportDate <= end);

            if (!string.IsNullOrWhiteSpace(userId))
                query = query.Where(x => x.AppUserId == userId);

            if (!string.IsNullOrWhiteSpace(companyName))
                query = query.Where(x => x.CompanyName == companyName);

            var reports = await query.ToListAsync();

            var users = await _context.Users
                .AsNoTracking()
                .Where(x => reports.Select(r => r.AppUserId).Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    FullName = x.NameSurname
                })
                .ToListAsync();

            var result = reports
               .GroupBy(x => new
               {
                   x.AppUserId,
                   x.CompanyName,

                   PeriodStart =
        summaryType == "year"
            ? new DateTime(x.ReportDate.Year, 1, 1)
            : summaryType == "month"
                ? new DateTime(x.ReportDate.Year, x.ReportDate.Month, 1)
                : x.ReportDate.AddDays(-((7 + (x.ReportDate.DayOfWeek - DayOfWeek.Monday)) % 7)).Date
               })
                .Select(g =>
                {
                    var weekStart = g.Key.PeriodStart;

                    var weekEnd =
                        summaryType == "year"
                            ? new DateTime(weekStart.Year, 12, 31)
                            : summaryType == "month"
                                ? weekStart.AddMonths(1).AddDays(-1)
                                : weekStart.AddDays(4);

                    int GetTotal(string activity)
                    {
                        return g
                            .Where(x => x.ActivityType == activity)
                            .Sum(x => x.ActualValue);
                    }

                    int GetLinkedinConnectionDifference()
                    {
                        var linkedinReports = g
                            .Where(x => x.ActivityType == "LinkedIn Bağlantı Sayısı")
                            .OrderBy(x => x.ReportDate)
                            .ToList();

                        if (!linkedinReports.Any())
                            return 0;

                        var first = linkedinReports.First();
                        var last = linkedinReports.Last();

                        var startValue = first.ProspectTarget;
                        var endValue = last.ActualValue;

                        return endValue - startValue;
                    }

                    var user = users.FirstOrDefault(u => u.Id == g.Key.AppUserId);

                    return new DailyReportingWeeklySummaryViewModel
                    {
                        UserId = g.Key.AppUserId,
                        FullName = user?.FullName ?? "-",
                        CompanyName = g.Key.CompanyName,

                        WeekStartDate = weekStart,
                        WeekEndDate = weekEnd,

                        LinkedinConnectionCount = GetLinkedinConnectionDifference(),
                        Emails = GetTotal("E-Mails"),
                        LinkedinMessages = GetTotal("LinkedIn Gönderilen Mesaj"),
                        LinkedinSentConnections = GetTotal("LinkedIn Gönderilen Bağlantı"),
                        Calls = GetTotal("Arama"),
                        MeetingPlanned = GetTotal("Meeting Planlama"),
                        MeetingCompleted = GetTotal("Meeting Gerçekleşen")
                    };
                })
                .OrderByDescending(x => x.WeekStartDate)
                .ThenBy(x => x.FullName)
                .ToList();

            ViewBag.Users = await _context.Users
        .AsNoTracking()
        .OrderBy(x => x.NameSurname)
        .Select(x => new SelectListItem
        {
            Value = x.Id,
            Text = x.NameSurname
        })
        .ToListAsync();

            ViewBag.SelectedUserId = userId;

            return View(result);
        }

    }
}