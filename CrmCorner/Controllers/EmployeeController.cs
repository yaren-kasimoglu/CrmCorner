using CrmCorner.Models;
using CrmCorner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace CrmCorner.Controllers
{
    // 🔹 EmployeeController
    // Şirket içindeki çalışanları listeler. (SuperAdmin tüm şirketleri görebilir.)
    // Erişim: SuperAdmin, Admin, TeamLeader, TeamMember
    [Authorize(Roles = "SuperAdmin,Admin,TeamLeader,TeamMember")]
    public class EmployeeController : Controller
    {
        private readonly CrmCornerContext _context;
        private readonly UserManager<AppUser> _userManager;
        public EmployeeController(CrmCornerContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        #region EMPLOYEE
        public async Task<IActionResult> EmployeeList()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToAction("NotFound", "Error");
                }

                ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

                var roles = await _userManager.GetRolesAsync(currentUser);

                List<AppUser> employees;

                if (roles.Contains("SuperAdmin"))
                {
                    // SuperAdmin tüm kullanıcıları görebilir
                    employees = await _userManager.Users
                        .OrderBy(u => u.CompanyName)
                        .ToListAsync();
                }
                else
                {
                    // Diğer roller sadece kendi şirketindeki kullanıcıları görebilir
                    employees = await _userManager.Users
                        .Where(u => u.CompanyId == currentUser.CompanyId)
                        .OrderBy(u => u.NameSurname)
                        .ToListAsync();
                }

                return View(employees);
            }
            catch (Exception)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }


        #endregion

    }
}
