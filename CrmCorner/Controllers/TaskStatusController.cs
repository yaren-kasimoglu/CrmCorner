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
            var positiveTasks = _context.TaskComps
        .Include(t => t.Status)
        .Include(t => t.AppUser)
        .Include(t => t.AssignedUser)
        .Include(t => t.Customer)
        .Where(t => t.Outcomes == OutcomeType.Olumlu) // 'Olumlu' enum değerine göre filtreleme
        .ToList();



            return View(positiveTasks);
        }

        public async Task<IActionResult> NegativeTasks()
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
    }
}
