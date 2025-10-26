using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using CrmCorner.Models;
using CrmCorner.Models.Enums;

// Gerek duyulursa alias (projedeki isim çakışmalarını önlemek için)
using PipelineStageEnum = CrmCorner.Models.Enums.PipelineStage;

namespace CrmCorner.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,TeamLeader,TeamMember")]
    public class PipelineAnalysisController : Controller
    {
        private readonly CrmCornerContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public PipelineAnalysisController(
            CrmCornerContext context,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "SuperAdmin,Admin,TeamLeader,TeamMember")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 1) Aktif kullanıcı + rol
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is null) return Unauthorized();

            var roles = await _userManager.GetRolesAsync(currentUser);
            bool isSuperAdmin = roles.Contains("SuperAdmin");
            bool isAdmin = roles.Contains("Admin");
            bool isTeamLeader = roles.Contains("TeamLeader");
            bool isTeamMember = roles.Contains("TeamMember");

            // 2) Kapsama girecek kullanıcılar
            IQueryable<AppUser> usersQ = _context.Users.AsNoTracking();

            if (isSuperAdmin)
            {
                // SuperAdmin -> tüm kullanıcılar
                // Herhangi bir filtre uygulanmaz
            }
            else if (isAdmin)
            {
                // Admin -> kendi şirketindeki herkes
                usersQ = usersQ.Where(u => u.CompanyId == currentUser.CompanyId);
            }
            else if (isTeamLeader)
            {
                // TeamLeader -> kendi şirketindeki TeamMember'lar ve kendisi
                var teamMembers = await _userManager.GetUsersInRoleAsync("TeamMember");
                var teamMemberIds = teamMembers
                    .Where(u => u.CompanyId == currentUser.CompanyId)
                    .Select(u => u.Id)
                    .ToList();

                // kendi ID'sini de ekle
                teamMemberIds.Add(currentUser.Id);

                usersQ = usersQ.Where(u => teamMemberIds.Contains(u.Id));
            }
            else
            {
                // TeamMember -> sadece kendi verileri
                usersQ = usersQ.Where(u => u.Id == currentUser.Id);
            }

            var users = await usersQ
                .Select(u => new { u.Id, u.UserName, u.NameSurname })
                .ToListAsync();

            var userIds = users.Select(u => u.Id).ToList();

            // 3) Görev sahipliği: ResponsibleUserId öncelikli; yoksa AppUserId
            var tasks = await _context.PipelineTasks
                .AsNoTracking()
                .Where(p =>
                    (p.ResponsibleUserId != null && userIds.Contains(p.ResponsibleUserId)) ||
                    (p.ResponsibleUserId == null && p.AppUserId != null && userIds.Contains(p.AppUserId))
                )
                .Select(p => new
                {
                    OwnerId = p.ResponsibleUserId ?? p.AppUserId!,
                    p.Stage,
                    p.OutcomeStatus
                })
                .ToListAsync();

            // 4) Tüm enum başlıkları (tabloda sütun sırası için)
            var allStages = Enum.GetValues(typeof(PipelineStageEnum)).Cast<PipelineStageEnum>().ToArray();
            var allOutcomes = Enum.GetValues(typeof(OutcomeTypeSales)).Cast<OutcomeTypeSales>().ToArray();

            // 5) Satırlar
            var rows = new List<UserPipelineSummaryViewModel>();
            foreach (var u in users)
            {
                var myTasks = tasks.Where(t => t.OwnerId == u.Id).ToList();

                var row = new UserPipelineSummaryViewModel
                {
                    UserId = u.Id,
                    UserName = u.UserName,
                    NameSurname = u.NameSurname,
                    Total = myTasks.Count,
                    StageCounts = new Dictionary<PipelineStageEnum, int>(),
                    OutcomeCounts = new Dictionary<OutcomeTypeSales, int>()
                };

                // Stage kırılımları
                foreach (var st in allStages)
                    row.StageCounts[st] = myTasks.Count(t => t.Stage == st);

                // Outcome kırılımları (null'ları None say)
                foreach (var oc in allOutcomes)
                    row.OutcomeCounts[oc] = myTasks.Count(t => (t.OutcomeStatus ?? OutcomeTypeSales.None) == oc);

                rows.Add(row);
            }

            // 6) ViewModel
            var vm = new PipelineAnalysisPageViewModel
            {
                Rows = rows
                    .OrderByDescending(r => r.Total)
                    .ThenBy(r => r.NameSurname ?? r.UserName)
                    .ToList(),
                AllStages = allStages,
                AllOutcomes = allOutcomes,
            };

            return View(vm); // Views/PipelineAnalysis/Index.cshtml
        }
    }

    // ===========================
    //  ViewModel'ler (aynı dosyada)
    // ===========================
    public class UserPipelineSummaryViewModel
    {
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string? NameSurname { get; set; }
        public int Total { get; set; }

        public Dictionary<PipelineStageEnum, int> StageCounts { get; set; } = new();
        public Dictionary<OutcomeTypeSales, int> OutcomeCounts { get; set; } = new();
    }

    public class PipelineAnalysisPageViewModel
    {
        public List<UserPipelineSummaryViewModel> Rows { get; set; } = new();
        public PipelineStageEnum[] AllStages { get; set; } = Array.Empty<PipelineStageEnum>();
        public OutcomeTypeSales[] AllOutcomes { get; set; } = Array.Empty<OutcomeTypeSales>();
        public bool IsAdminOrManager { get; set; }
    }
}
