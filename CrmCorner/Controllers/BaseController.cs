using CrmCorner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CrmCorner.Controllers
{
    public class BaseController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public BaseController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task SetLayout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                {
                    ViewData["Layout"] = "~/Areas/Admin/Views/Shared/_LayoutHomePageArea.cshtml";
                }
                else
                {
                    ViewData["Layout"] = "~/Views/Shared/_LayoutHomePage.cshtml";
                }
            }
            else
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutHomePage.cshtml";
            }
        }


    }
}
