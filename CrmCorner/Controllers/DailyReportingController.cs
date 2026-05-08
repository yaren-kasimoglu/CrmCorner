using CrmCorner.Migrations;
using CrmCorner.Models;
using CrmCorner.Models.Enums;
using CrmCorner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

                    row.TotalP = row.Days.Sum(x => x.Value.P);
                    row.TotalA = row.Days.Sum(x => x.Value.A);

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

    }
}