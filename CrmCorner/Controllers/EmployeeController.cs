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
    [Authorize]
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
                // Giriş yapmış kullanıcının UserName'ini al
                var currentUserName = User.Identity.Name;

                // Giriş yapmış kullanıcının bilgilerini al
                var currentUser = await _userManager.FindByNameAsync(currentUserName);

                if (currentUser == null)
                {
                    return RedirectToAction("NotFound", "Error");
                }

                // Giriş yapmış kullanıcının CompanyName bilgisini al
                var currentUserCompanyName = currentUser.CompanyName;

                // Aynı CompanyName'e sahip olan kullanıcıları getir
                var userList = await _userManager.Users
                    .Where(u => u.CompanyName == currentUserCompanyName)
                    .ToListAsync();

                var dashboardEmpViewModelList = new List<DashboardEmpViewModel>();

                foreach (var user in userList)
                {
                    var taskComps = _context.TaskComps
                        .AsNoTracking()
                        .Include(tc => tc.Status) // Status nesnesini dahil et.
                        .Where(tc => (tc.AppUser.UserName == user.UserName || tc.AssignedUser.UserName == user.UserName) && tc.Status != null)
                        .ToList();

                    var uniqueTaskComps = taskComps
                        .GroupBy(tc => tc.TaskId)
                        .Select(g => g.First())
                        .ToList();

                    var taskStatusCounts = uniqueTaskComps
                        .GroupBy(tc => tc.Status.StatusName)
                        .Select(group => new { Status = group.Key, Count = group.Count() })
                        .ToDictionary(t => t.Status, t => t.Count);

                    var dashboardEmpViewModel = new DashboardEmpViewModel
                    {
                        User = new UserViewModel
                        {
                            UserName = user.UserName,
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber,
                            CompanyName = user.CompanyName,
                            PositionName = user.PositionName,
                            NameSurname = user.NameSurname
                        },
                        TaskStatusCountsByUser = taskStatusCounts
                    };

                    dashboardEmpViewModelList.Add(dashboardEmpViewModel);
                }

                return View(dashboardEmpViewModelList);
            }
            catch (Exception ex)
            {
             return RedirectToAction("NotFound", "Error");
            }
        }

        #endregion

    }
}
