using CrmCorner.Areas.Admin.Models;
using CrmCorner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CrmCorner.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CrmCorner.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RolesController : Controller
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<RolesController> _logger;
        private readonly CrmCornerContext _context;


        public RolesController(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager, ILogger<RolesController> logger, CrmCornerContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> RoleList()
        {
            var roleList = await _roleManager.Roles.ToListAsync();
            var roleViewModelList = roleList.Select(x => new RoleVM()
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
            return View(roleViewModelList);
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(RoleCreateVM model)
        {
            if (ModelState.IsValid)
            {
                AppRole appRole = new AppRole
                {
                    Name = model.RoleName
                };

                IdentityResult result = await _roleManager.CreateAsync(appRole);

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



        [HttpGet]
        public async Task<IActionResult> AssignRoleToUserVM()
        {
            var users = await _userManager.Users.ToListAsync();
            var roles = await _roleManager.Roles.ToListAsync();

            var model = new AssignRoleToUserVM
            {
                Users = users.Select(u => new SelectListItem { Value = u.Id, Text = u.UserName }).ToList(),
                Roles = roles.Select(r => new SelectListItem { Value = r.Id, Text = r.Name }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRoleToUserVM(AssignRoleToUserVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userRole = new IdentityUserRole<string>
                    {
                        UserId = model.UserId,
                        RoleId = model.RoleId
                    };

                    _context.UserRoles.Add(userRole);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("UserList", "Home");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while adding the role.");
                }
            }

            model.Users = await GetUserSelectListItemsAsync();
            model.Roles = await GetRoleSelectListItemsAsync();

            return View(model);
        }

        private async Task<List<SelectListItem>> GetUserSelectListItemsAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            return users.Select(u => new SelectListItem { Value = u.Id, Text = u.UserName }).ToList();
        }

        private async Task<List<SelectListItem>> GetRoleSelectListItemsAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return roles.Select(r => new SelectListItem { Value = r.Id, Text = r.Name }).ToList();
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


        //[HttpGet]
        //public async Task<IActionResult> AssignRoleToUser(string id)
        //{
        //    var currentUser = await _userManager.FindByIdAsync(id);
        //    ViewBag.userId = id;
        //    var roles = await _roleManager.Roles.ToListAsync();
        //    var userRoles = await _userManager.GetRolesAsync(currentUser);
        //    var roleVMList = new List<AssignRoleToUserVM>();

        //    foreach (var role in roles)
        //    {
        //        var assignRoleToUserVM = new AssignRoleToUserVM() { Id = role.Id, Name = role.Name };
        //        if (userRoles.Contains(role.Name))
        //        {
        //            assignRoleToUserVM.Exist = true;
        //        }
        //        roleVMList.Add(assignRoleToUserVM);
        //    }

        //    return View(roleVMList);
        //}

        //[HttpPost]
        //public async Task<IActionResult> AssignRoleToUser(string userId, List<AssignRoleToUserVM> requestList)
        //{
        //    _logger.LogInformation("Assigning roles to user. UserID: {UserId}, RequestListCount: {Count}", userId, requestList.Count);
        //    if (string.IsNullOrEmpty(userId))
        //    {
        //        return BadRequest("User ID must not be null or empty.");
        //    }

        //    if (requestList == null || requestList.Count == 0)
        //    {
        //        return BadRequest("Request list must not be null or empty.");
        //    }

        //    try
        //    {
        //        var userToAssignRoles = await _userManager.Users
        //            .Include(u => u.UserRoles)
        //            .ThenInclude(ur => ur.Role)
        //            .FirstOrDefaultAsync(u => u.Id == userId);

        //        if (userToAssignRoles == null)
        //        {
        //            return NotFound($"User with ID {userId} not found.");
        //        }

        //        foreach (var role in requestList)
        //        {
        //            if (role == null || string.IsNullOrEmpty(role.Name))
        //            {
        //                return BadRequest("Invalid role data.");
        //            }

        //            if (role.Exist)
        //            {
        //                if (!_userManager.IsInRoleAsync(userToAssignRoles, role.Name).Result)
        //                {
        //                    var addResult = await _userManager.AddToRoleAsync(userToAssignRoles, role.Name);
        //                    if (!addResult.Succeeded)
        //                    {
        //                        _logger.LogError("Failed to add role. Role: {RoleName}, UserID: {UserId}, Errors: {Errors}", role.Name, userId, string.Join(", ", addResult.Errors.Select(e => e.Description)));
        //                        return BadRequest($"Failed to add role {role.Name} to user {userId}. Errors: {string.Join(", ", addResult.Errors.Select(e => e.Description))}");
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                if (_userManager.IsInRoleAsync(userToAssignRoles, role.Name).Result)
        //                {
        //                    var removeResult = await _userManager.RemoveFromRoleAsync(userToAssignRoles, role.Name);
        //                    if (!removeResult.Succeeded)
        //                    {
        //                        return BadRequest($"Failed to remove role {role.Name} from user {userId}. Errors: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
        //                    }
        //                }
        //            }
        //        }

        //        return RedirectToAction(nameof(HomeController.UserList), "Home");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error while assigning roles.");
        //        return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier, Message = ex.Message });
        //    }
        //}
    }

}

