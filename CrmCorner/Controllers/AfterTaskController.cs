using CrmCorner.Models;
using CrmCorner.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    [Authorize]
    public class AfterTaskController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly CrmCornerContext _context;
        public AfterTaskController(UserManager<AppUser> userManager, CrmCornerContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> AddPostSaleInfo(int taskId)
        {
            var task = await _context.TaskComps
                                     .Include(t => t.Status)
                                     .Include(t => t.AppUser)
                                     .Include(t => t.AssignedUser)
                                     .Include(t => t.Customer)
                                     .FirstOrDefaultAsync(t => t.TaskId == taskId && t.Outcomes == OutcomeType.Olumlu);

            if (task == null)
            {
                return NotFound();
            }

            var postSaleInfo = new PostSaleInfo
            {
                TaskCompId = taskId
            };

            return View(postSaleInfo);
        }

        [HttpPost]
        public async Task<IActionResult> AddPostSaleInfo(PostSaleInfo postSaleInfo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(postSaleInfo);
                await _context.SaveChangesAsync();
                return RedirectToAction("PositiveTasks"); // Kazanılan görevler listesine yönlendirme
            }

            return View(postSaleInfo); // Validasyon hatası varsa formu tekrar göster
        }

    }
}
