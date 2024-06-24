using CrmCorner.Models;
using CrmCorner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{

    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly CrmCornerContext _context;

        public AdminController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, CrmCornerContext context)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToListAsync();

            var model = users.Select(u => new UserRoleViewModel
            {
                User = u,
                Roles = _userManager.GetRolesAsync(u).Result.ToList()
            }).ToList();

            return View(model);
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                AppRole role = new AppRole { Name = model.RoleName };
                IdentityResult result = await _roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    return RedirectToAction("RoleList");
                }

                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        public IActionResult RoleList()
        {
            var roles = _roleManager.Roles;
            return View(roles);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("RoleList");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            else
            {
                ModelState.AddModelError("", "Role Not Found");
            }
            return RedirectToAction("RoleList");
        }

        [HttpGet]
        public async Task<IActionResult> AssignRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var model = new AssignRoleViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync(),
                SelectedRoles = await _userManager.GetRolesAsync(user)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(AssignRoleViewModel model)
        {
            if (model == null)
            {
                ModelState.AddModelError("", "Invalid model");
                return View(model);
            }

            var user = await _context.Users
          .Include(u => u.UserRoles)
              .ThenInclude(ur => ur.Role)
          .Include(u => u.Customers)
          .Include(u => u.TaskComps)
          .Include(u => u.Calendars)
          .Include(u => u.Notifications)
          .Include(u => u.TaskCompLogs)
          .FirstOrDefaultAsync(u => u.Id == model.UserId)
          .ConfigureAwait(false);

            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            if (userRoles == null)
            {
                ModelState.AddModelError("", "User roles could not be loaded");
                return View(model);
            }

            var result = await _userManager.RemoveFromRolesAsync(user, userRoles).ConfigureAwait(false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }

            if (model.SelectedRoles == null || !model.SelectedRoles.Any())
            {
                ModelState.AddModelError("", "No roles selected");
                return View(model);
            }

            // Log the roles for debugging
            foreach (var role in model.SelectedRoles)
            {
                System.Diagnostics.Debug.WriteLine($"Role: {role}");
            }

            result = await _userManager.AddToRolesAsync(user, model.SelectedRoles).ConfigureAwait(false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(model);
            }

            return RedirectToAction("Index");
        }


    }
}