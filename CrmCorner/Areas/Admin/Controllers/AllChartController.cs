using CrmCorner.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class AllChartController : Controller
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly CrmCornerContext _context;

        public AllChartController(UserManager<AppUser> userManager, CrmCornerContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> CompanyUserTaskCharts()
        {
            var currentUserId = _userManager.GetUserId(User);
            var currentUser = await _context.Users.Include(u => u.CompanyId).FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (currentUser == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            var companyUsers = await _context.Users
                                             .Where(u => u.CompanyId == currentUser.CompanyId)
                                             .Include(u => u.TaskComps)
                                             .ToListAsync();

            var chartDataList = new List<object>();

            foreach (var user in companyUsers)
            {
                var taskComps = user.TaskComps;

                var chartData = taskComps.GroupBy(tc => tc.Status.StatusName)
                                         .Select(group => new {
                                             StatusName = group.Key,
                                             TaskNames = group.Select(tc => tc.Title).Distinct().ToList(),
                                             Count = group.Count()
                                         }).ToList();

                chartDataList.Add(new { UserName = user.UserName, ChartData = chartData });
            }

            return Json(chartDataList);
        }

    }
}
