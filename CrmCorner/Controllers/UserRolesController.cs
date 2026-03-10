using CrmCorner.Models;
using CrmCorner.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class UserRolesController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly CrmCornerContext _context;

        public UserRolesController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, CrmCornerContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // Kullanıcı listesi
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // Kullanıcıya rol ata ekranı
        public async Task<IActionResult> AssignRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var roles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            // Kullanıcının modüllerini getir
            var userModules = await _context.UserModules
                .Where(x => x.UserId == userId)
                .Select(x => x.Module)
                .ToListAsync();

            var model = roles.Select(r => new UserRoleAssignViewModel
            {
                RoleId = r.Id,
                RoleName = r.Name,
                IsAssigned = userRoles.Contains(r.Name)
            }).ToList();

            ViewBag.UserId = userId;
            ViewBag.UserName = user.UserName;
            ViewBag.UserModules = userModules;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(string userId, List<UserRoleAssignViewModel> model, List<int> SelectedModules)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User {userId} bulunamadı!");
            }


            foreach (var role in model)
            {
                if (role.IsAssigned)
                {
                    if (!await _userManager.IsInRoleAsync(user, role.RoleName))
                    {
                        if (string.IsNullOrEmpty(role.RoleName))
                        {
                            ModelState.AddModelError("", "RoleName boş geldi!");
                            continue;
                        }

                        var result = await _userManager.AddToRoleAsync(user, role.RoleName);

                        if (!result.Succeeded)
                        {
                            ModelState.AddModelError("", $"Rol eklenemedi: {role.RoleName} - {string.Join(", ", result.Errors.Select(e => e.Description))}");
                            return View(model);
                        }
                    }
                }
                else
                {
                    if (await _userManager.IsInRoleAsync(user, role.RoleName))
                    {
                        await _userManager.RemoveFromRoleAsync(user, role.RoleName);
                    }
                }
            }

            var existingModules = _context.UserModules.Where(x => x.UserId == userId);
            _context.UserModules.RemoveRange(existingModules);

            if (SelectedModules != null && SelectedModules.Any())
            {
                foreach (var moduleId in SelectedModules)
                {
                    _context.UserModules.Add(new UserModule
                    {
                        UserId = userId,
                        Module = (ModuleType)moduleId
                    });
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

    }
}
