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

        public IActionResult AllUserTaskStatusChartsView()
        {
            return View();
        }


        public async Task<IActionResult> AllUserTaskStatusCharts()
        {
            // Şirketin ID'sini al (örnek olarak CurrentUser'ın CompanyId'si üzerinden)
            var companyId = _userManager.Users.FirstOrDefault(u => u.Id == _userManager.GetUserId(User))?.CompanyId;

            if (companyId == null)
            {
                return Unauthorized();
            }

            // Şirket çalışanlarını al
            var users = await _userManager.Users
                                          .Where(u => u.CompanyId == companyId)
                                          .ToListAsync();

            // Her kullanıcı için görev durumlarını gruplandır
            var allChartsData = new List<object>();

            foreach (var user in users)
            {
                var userId = user.Id;

                // Kullanıcının görevlerini al
                var appUserTasks = await _context.TaskComps
                                                 .Include(tc => tc.Status)
                                                 .Where(tc => tc.UserId == userId)
                                                 .ToListAsync();

                var assignedUserTasks = await _context.TaskComps
                                                      .Include(tc => tc.Status)
                                                      .Where(tc => tc.AssignedUserId == userId)
                                                      .ToListAsync();

                var combinedTasks = appUserTasks.Concat(assignedUserTasks).ToList();

                var chartData = combinedTasks.GroupBy(tc => tc.Status.StatusName)
                                             .Select(group => new
                                             {
                                                 StatusName = group.Key,
                                                 TaskNames = group.Select(tc => tc.Title).Distinct().ToList(),
                                                 Count = group.Select(tc => tc.Title).Distinct().Count()
                                             }).ToList();

                allChartsData.Add(new
                {
                    UserName = user.UserName, // Kullanıcı ismi
                    ChartData = chartData
                });
            }

            return Json(allChartsData);
        }




    }
}
