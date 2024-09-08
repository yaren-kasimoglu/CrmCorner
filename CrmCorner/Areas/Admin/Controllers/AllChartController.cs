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

       public async Task<IActionResult> AllCharts()
        {
            return View();
        }

        public async Task<IActionResult> CompanyUserTaskCharts()
        {
            try
            {
                // Şu anki kullanıcı ID'sini al
                var currentUserId = _userManager.GetUserId(User);
                if (currentUserId == null)
                {
                    ViewData["Error"] = "Geçerli kullanıcı ID'si alınamadı.";
                    return View("AllCharts");
                }

                // Şu anki kullanıcının bilgilerini al
                var currentUser = await _context.Users
                                                .FirstOrDefaultAsync(u => u.Id == currentUserId);

                if (currentUser == null)
                {
                    ViewData["Error"] = "Kullanıcı bulunamadı.";
                    return View("AllCharts");
                }

                // Şu anki kullanıcı ile aynı şirketteki tüm kullanıcıları al
                var companyUsers = await _context.Users
                                                 .Where(u => u.CompanyId == currentUser.CompanyId)
                                                 .Include(u => u.TaskComps)
                                                 .ThenInclude(tc => tc.Status)
                                                 .ToListAsync();

                var chartDataList = new List<object>();

                // Her kullanıcı için görev verilerini grupla ve grafik verisi oluştur
                foreach (var user in companyUsers)
                {
                    var taskComps = user.TaskComps;

                    var chartData = taskComps.GroupBy(tc => tc.Status?.StatusName ?? "Durum Yok")
                                             .Select(group => new
                                             {
                                                 StatusName = group.Key,
                                                 TaskNames = group.Select(tc => tc.Title).Distinct().ToList(),
                                                 Count = group.Count()
                                             }).ToList();

                    chartDataList.Add(new { UserName = user.UserName, ChartData = chartData });
                }

                return Json(chartDataList);
            }
            catch (Exception ex)
            {
                // Hata mesajını ViewData'ya ekle ve view'i döndür
                ViewData["Error"] = $"Hata oluştu: {ex.Message}";
                return View("AllCharts");
            }
        }



    }
}
