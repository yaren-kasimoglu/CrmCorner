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

            // TeamLeader rolündeki kullanıcıları getir
            var teamLeaderUsers = await _userManager.GetUsersInRoleAsync("TeamLeader");

            // Bu kullanıcıya atanmış TeamLeader'ları getir
            var selectedTeamLeaderIds = await _context.TeamLeaderMembers
                .Where(x => x.TeamMemberId == userId)
                .Select(x => x.TeamLeaderId)
                .ToListAsync();

            ViewBag.UserId = userId;
            ViewBag.UserName = user.UserName;
            ViewBag.UserModules = userModules;
            ViewBag.TeamLeaderUsers = teamLeaderUsers;
            ViewBag.SelectedTeamLeaderIds = selectedTeamLeaderIds;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(
    string userId,
    List<UserRoleAssignViewModel> model,
    List<int> SelectedModules,
    List<string> SelectedTeamLeaderIds)
        {
            if (!ModelState.IsValid)
            {
                var userModules = await _context.UserModules
                    .Where(x => x.UserId == userId)
                    .Select(x => x.Module)
                    .ToListAsync();

                var teamLeaderUsers = await _userManager.GetUsersInRoleAsync("TeamLeader");
                var selectedTeamLeaderIdsFromDb = await _context.TeamLeaderMembers
                    .Where(x => x.TeamMemberId == userId)
                    .Select(x => x.TeamLeaderId)
                    .ToListAsync();

                ViewBag.UserId = userId;
                ViewBag.UserModules = userModules;
                ViewBag.TeamLeaderUsers = teamLeaderUsers;
                ViewBag.SelectedTeamLeaderIds = selectedTeamLeaderIdsFromDb;

                return View(model);
            }

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

                            var userModules = await _context.UserModules
                                .Where(x => x.UserId == userId)
                                .Select(x => x.Module)
                                .ToListAsync();

                            var teamLeaderUsers = await _userManager.GetUsersInRoleAsync("TeamLeader");
                            var selectedTeamLeaderIdsFromDb = await _context.TeamLeaderMembers
                                .Where(x => x.TeamMemberId == userId)
                                .Select(x => x.TeamLeaderId)
                                .ToListAsync();

                            ViewBag.UserId = userId;
                            ViewBag.UserName = user.UserName;
                            ViewBag.UserModules = userModules;
                            ViewBag.TeamLeaderUsers = teamLeaderUsers;
                            ViewBag.SelectedTeamLeaderIds = selectedTeamLeaderIdsFromDb;

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

            // Module kayıtlarını güncelle
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

            // Kullanıcı TeamMember mi kontrol et
            var isTeamMemberSelected = model.Any(x => x.RoleName == "TeamMember" && x.IsAssigned);

            // Önce eski eşleşmeleri sil
            var existingTeamLeaderRelations = _context.TeamLeaderMembers.Where(x => x.TeamMemberId == userId);
            _context.TeamLeaderMembers.RemoveRange(existingTeamLeaderRelations);

            // Eğer TeamMember ise seçilen TeamLeader'ları kaydet
            if (isTeamMemberSelected && SelectedTeamLeaderIds != null && SelectedTeamLeaderIds.Any())
            {
                foreach (var teamLeaderId in SelectedTeamLeaderIds.Distinct())
                {
                    if (teamLeaderId != userId) // kendisini lider seçemesin
                    {
                        _context.TeamLeaderMembers.Add(new TeamLeaderMember
                        {
                            TeamMemberId = userId,
                            TeamLeaderId = teamLeaderId
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


    }
}
