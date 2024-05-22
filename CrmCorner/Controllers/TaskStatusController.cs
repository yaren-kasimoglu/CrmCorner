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
        public IActionResult PositiveTasks()
        {
            try
            {
                var positiveTasks = _context.TaskComps
                    .Include(t => t.Status)
                    .Include(t => t.AppUser)
                    .Include(t => t.AssignedUser)
                    .Include(t => t.Customer)
                    .Where(t => t.Outcomes == OutcomeType.Olumlu) // 'Olumlu' enum değerine göre filtreleme
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
                var currentUserCompanyName = currentUser.CompanyName;

                var negativeTaks = _context.TaskComps
                    .Include(t => t.Status)
                    .Include(t => t.AppUser)
                    .Include(t => t.AssignedUser)
                    .Include(t => t.Customer)
                    .Where(u => u.AppUser.CompanyName == currentUserCompanyName)
                    .Where(t => t.Outcomes == OutcomeType.Olumsuz)
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
