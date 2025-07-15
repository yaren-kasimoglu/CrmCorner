using CrmCorner.Models.Enums;
using CrmCorner.Models;
using CrmCorner.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

//[Authorize(Roles = "Admin")]
public class RoleManagementController : Controller
{
    private readonly UserManager<AppUser> _userManager;

    public RoleManagementController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();
        var model = users.Select(user => new UserRoleViewModel
        {
            UserId = user.Id,
            Email = user.Email,
            CompanyName = user.CompanyName,
            //CurrentRole = user.Role.ToString(),
            //NewRole = user.Role.ToString()
        }).ToList();

        return View(model);
    }

    //[HttpPost]
    //public async Task<IActionResult> UpdateRoles(List<UserRoleViewModel> model)
    //{
    //    foreach (var userVm in model)
    //    {
    //        var user = await _userManager.FindByIdAsync(userVm.UserId);
    //        if (user != null && user.Role.ToString() != userVm.NewRole)
    //        {
    //            user.Role = Enum.Parse<UserRole>(userVm.NewRole);
    //            await _userManager.UpdateAsync(user);
    //        }
    //    }

    //    return RedirectToAction("Index");
    //}
}
