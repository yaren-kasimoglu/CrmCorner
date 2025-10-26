using CrmCorner.Models;
using CrmCorner.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;

namespace CrmCorner.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,TeamLeader,TeamMember")]
    public class PipelineStatusController : Controller
    {
        private readonly CrmCornerContext _context;
        private readonly UserManager<AppUser> _userManager;

        public PipelineStatusController(CrmCornerContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // === KAZANILAN GÖREVLER ===
        public async Task<IActionResult> PositivePipelineTasks()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null) return Unauthorized();

                var roles = await _userManager.GetRolesAsync(currentUser);
                bool isSuperAdmin = roles.Contains("SuperAdmin");
                bool isAdmin = roles.Contains("Admin");

                IQueryable<PipelineTask> query = _context.PipelineTasks
                    .Include(t => t.AppUser)
                    .Include(t => t.ResponsibleUser)
                    .Include(t => t.Customer)
                    .Where(t => t.OutcomeStatus == OutcomeTypeSales.Won);

                if (isSuperAdmin)
                {
                    // Her şeyi görebilir
                }
                else if (isAdmin)
                {
                    // Admin kendi şirketindeki tüm görevleri görür
                    var companyUserIds = _context.Users
                        .Where(u => u.CompanyId == currentUser.CompanyId)
                        .Select(u => u.Id);
                    query = query.Where(t =>
                        (t.AppUserId != null && companyUserIds.Contains(t.AppUserId)) ||
                        (t.ResponsibleUserId != null && companyUserIds.Contains(t.ResponsibleUserId))
                    );
                }
                else
                {
                    // TeamLeader / TeamMember -> kendi oluşturduğu veya kendisine atanan görevler
                    query = query.Where(t =>
                        t.AppUserId == currentUser.Id || t.ResponsibleUserId == currentUser.Id
                    );
                }

                var positiveTasks = await query.ToListAsync();
                return View(positiveTasks);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Olumlu pipeline görevleri getirilirken hata oluştu: " + ex.Message;
                return View("Error");
            }
        }

        // === KAYBEDİLEN GÖREVLER ===
        public async Task<IActionResult> NegativePipelineTasks()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null) return Unauthorized();

                var roles = await _userManager.GetRolesAsync(currentUser);
                bool isSuperAdmin = roles.Contains("SuperAdmin");
                bool isAdmin = roles.Contains("Admin");

                IQueryable<PipelineTask> query = _context.PipelineTasks
                    .Include(t => t.AppUser)
                    .Include(t => t.ResponsibleUser)
                    .Include(t => t.Customer)
                    .Where(t => t.OutcomeStatus == OutcomeTypeSales.Lost);

                if (isSuperAdmin)
                {
                    // Her şeyi görebilir
                }
                else if (isAdmin)
                {
                    // Admin kendi şirketindeki tüm görevleri görür
                    var companyUserIds = _context.Users
                        .Where(u => u.CompanyId == currentUser.CompanyId)
                        .Select(u => u.Id);
                    query = query.Where(t =>
                        (t.AppUserId != null && companyUserIds.Contains(t.AppUserId)) ||
                        (t.ResponsibleUserId != null && companyUserIds.Contains(t.ResponsibleUserId))
                    );
                }
                else
                {
                    // TeamLeader / TeamMember -> kendi oluşturduğu veya kendisine atanan görevler
                    query = query.Where(t =>
                        t.AppUserId == currentUser.Id || t.ResponsibleUserId == currentUser.Id
                    );
                }

                var negativeTasks = await query.ToListAsync();
                return View(negativeTasks);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Olumsuz pipeline görevleri getirilirken hata oluştu: " + ex.Message;
                return View("Error");
            }
        }
    }
}
