using CrmCorner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CrmCorner.Areas.Admin.Controllers
{
    public class RolesController : Controller
    {
        private readonly RoleManager<AppUser> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public RolesController(RoleManager<AppUser> roleManager, UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public IActionResult RoleList()
        {
            return View();
        }
    }
}
