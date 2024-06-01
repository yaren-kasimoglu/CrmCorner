using CrmCorner.Models;
using CrmCorner.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    public class TaskStatusController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly CrmCornerContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;

        public TaskStatusController(CrmCornerContext context, UserManager<AppUser> userManager, IWebHostEnvironment environment, IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
        }
        public async Task<IActionResult> PositiveTasks()
        {
            try
            {
                var currentUserName = User.Identity.Name;
                var currentUser = await _userManager.FindByNameAsync(currentUserName);
                var currentUserEmailDomain = currentUser.EmailDomain;
                ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

                var positiveTasks = _context.TaskComps
                    .Include(t => t.Status)
                    .Include(t => t.AppUser)
                    .Include(t => t.AssignedUser)
                    .Include(t => t.Customer)
                    .Where(u => u.AppUser.EmailDomain == currentUserEmailDomain)
                    .Where(t => t.OutcomeStatus == OutcomeTypeSales.Won)
                    .Where(t => t.StatusId == 6)// 'Olumlu' enum değerine göre filtreleme
                    .ToList();

                return View(positiveTasks);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while retrieving positive tasks: " + ex.Message;
                return View("Error");
            }
        }

        public async Task<IActionResult> NegativeTasks()
        {
            try
            {
                var currentUserName = User.Identity.Name;
                var currentUser = await _userManager.FindByNameAsync(currentUserName);
                ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");
                var currentUserCompanyName = currentUser.CompanyName;
                var currentUserEmailDomain = currentUser.EmailDomain;

                var negativeTaks = _context.TaskComps
                    .Include(t => t.Status)
                    .Include(t => t.AppUser)
                    .Include(t => t.AssignedUser)
                    .Include(t => t.Customer)
                    .Where(u => u.AppUser.EmailDomain == currentUserEmailDomain)
                    .Where(t => t.OutcomeStatus == OutcomeTypeSales.Lost)
                    .Where(t => t.StatusId == 6)
                    .ToList();

                return View(negativeTaks);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while retrieving negative tasks: " + ex.Message;
                return View("Error");
            }
        }

    }
}
