using CrmCorner.Areas.Admin.Models;
using CrmCorner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CrmCorner.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RolesController : Controller
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<RolesController> _logger;

        public RolesController(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager, ILogger<RolesController> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> RoleList()
        {
            var roles = await _roleManager.Roles.Select(x => new RoleVM()
            {
                Id = x.Id,
                Name = x.Name!
            }).ToListAsync();


            return View(roles);
        }

        [HttpGet]
        public IActionResult RoleCreate()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RoleCreate(RoleCreateVM request)
        {
            var result = await _roleManager.CreateAsync(new AppRole() { Name = request.RoleName });

            if (!result.Succeeded)
            {
                ModelState.AddModelErrorList(result.Errors);
                return View();
            }


            return RedirectToAction(nameof(RolesController.RoleList));
        }


        [HttpGet]
        public async Task<IActionResult> RoleUpdate(string id)
        {
            var roleToUpdate = await _roleManager.FindByIdAsync(id);

            if (roleToUpdate == null)
            {
                throw new Exception("Güncellemek istediğiniz role bulunamamıştır");
            }

            return View(new RoleUpdateVM() { Id = roleToUpdate.Id, Name = roleToUpdate.Name });
        }

        [HttpPost]
        public async Task<IActionResult> RoleUpdate(RoleUpdateVM request)
        {
            var roleToUpdate = await _roleManager.FindByIdAsync(request.Id);


            if (roleToUpdate == null)
            {
                throw new Exception("Güncellemek istediğiniz role bulunamamıştır");
            }

            roleToUpdate.Name = request.Name;

            await _roleManager.UpdateAsync(roleToUpdate);

            return RedirectToAction(nameof(RolesController.RoleList));
        }

        public async Task<IActionResult> RoleDelete(string id)
        {
            var roleRoDelete = await _roleManager.FindByIdAsync(id);

            if (roleRoDelete == null)
            {
                throw new Exception("Silmek istediğiniz role bulunamamıştır");
            }

            var result = await _roleManager.DeleteAsync(roleRoDelete);

            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.Select(x => x.Description).First());
            }

            return RedirectToAction(nameof(RolesController.RoleList));
        }


        public async Task<IActionResult> AssignRoleTouser(string id)
        {
            var currentUser = await _userManager.FindByIdAsync(id);
            ViewBag.userId = id;
            var roles = await _roleManager.Roles.ToListAsync();
            var userRoller = await _userManager.GetRolesAsync(currentUser);
            var roleVMList = new List<AssignRoleToUserVM>();

            foreach (var role in roles)
            {
                var assignRoleToUserVM = new AssignRoleToUserVM() { Id = role.Id, Name = role.Name! };
                if (userRoller.Contains(role.Name!))
                {
                    assignRoleToUserVM.Exist = true;
                }
                roleVMList.Add(assignRoleToUserVM);
            }
            return View(roleVMList);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRoleTouser(string userId, List<AssignRoleToUserVM> requestList)
        {
            _logger.LogInformation("Assigning roles to user. UserID: {UserId}, RequestListCount: {Count}", userId, requestList.Count);
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID must not be null or empty.");
            }

            if (requestList == null || requestList.Count == 0)
            {
                return BadRequest("Request list must not be null or empty.");
            }

            try
            {
                var userToAssignRoles = await _userManager.Users
     .Include(u => u.UserRoles)
     .ThenInclude(ur => ur.Role)
     .FirstOrDefaultAsync(u => u.Id == userId);


                if (userToAssignRoles == null)
                {
                    return NotFound($"User with ID {userId} not found.");
                }

                foreach (var role in requestList)
                {
                    if (role == null || string.IsNullOrEmpty(role.Name))
                    {
                        // Hatalı veri durumunda hata
                        return BadRequest("Invalid role data.");
                    }

                    if (role.Exist)
                    {
                        _logger.LogInformation("Adding user {UserId} to role {RoleName}", userToAssignRoles.Id, role.Name);

                        var addResult = await _userManager.AddToRoleAsync(userToAssignRoles, role.Name);
                        if (!addResult.Succeeded)
                        {
                            _logger.LogError("Failed to add role. Role: {RoleName}, UserID: {UserId}, Errors: {Errors}", role.Name, userId, string.Join(", ", addResult.Errors.Select(e => e.Description)));
                            return BadRequest($"Failed to add role {role.Name} to user {userId}. Errors: {string.Join(", ", addResult.Errors.Select(e => e.Description))}");
                        }
                    }
                    else
                    {
                        var removeResult = await _userManager.RemoveFromRoleAsync(userToAssignRoles, role.Name);

                        if (!removeResult.Succeeded)
                        {
                            return BadRequest($"Failed to remove role {role.Name} from user {userId}. Errors: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
                        }
                    }
                }

                return RedirectToAction(nameof(HomeController.UserList), "Home");
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier, Message = ex.Message });
            }
        }

    }
}
