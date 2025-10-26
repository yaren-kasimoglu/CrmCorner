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

    }
}