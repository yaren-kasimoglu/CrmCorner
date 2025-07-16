using CrmCorner.Models;
using CrmCorner.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CrmCorner.Controllers
{
    public class PipelineStatusController : Controller
    {
        private readonly CrmCornerContext _context;
        private readonly UserManager<AppUser> _userManager;

        public PipelineStatusController(CrmCornerContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> PositivePipelineTasks()
        {
            try
            {
                var currentUserName = User.Identity.Name;
                var currentUser = await _userManager.FindByNameAsync(currentUserName);
                var emailDomain = currentUser.EmailDomain;

                var positiveTasks = _context.PipelineTasks
                    .Include(t => t.AppUser)
                    .Include(t => t.Customer)
                    .Where(t => t.AppUser.EmailDomain == emailDomain)
                    .Where(t => t.OutcomeStatus == OutcomeTypeSales.Won)
                    .ToList();

                return View(positiveTasks);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Olumlu pipeline görevleri getirilirken hata oluştu: " + ex.Message;
                return View("Error");
            }
        }

        public async Task<IActionResult> NegativePipelineTasks()
        {
            try
            {
                var currentUserName = User.Identity.Name;
                var currentUser = await _userManager.FindByNameAsync(currentUserName);
                var emailDomain = currentUser.EmailDomain;

                var negativeTasks = _context.PipelineTasks
                    .Include(t => t.AppUser)
                    .Include(t => t.Customer)
                    .Where(t => t.AppUser.EmailDomain == emailDomain)
                    .Where(t => t.OutcomeStatus == OutcomeTypeSales.Lost)
                    .ToList();

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
