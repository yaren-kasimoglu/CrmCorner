using CrmCorner.Models.Enums;
using CrmCorner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class ModuleAuthorizeAttribute : Attribute, IAsyncActionFilter
{
    private readonly ModuleType _module;

    public ModuleAuthorizeAttribute(ModuleType module)
    {
        _module = module;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var db = context.HttpContext.RequestServices.GetService(typeof(CrmCornerContext)) as CrmCornerContext;
        var userManager = context.HttpContext.RequestServices.GetService(typeof(UserManager<AppUser>)) as UserManager<AppUser>;

        var user = await userManager.GetUserAsync(context.HttpContext.User);

        if (user == null)
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        var modules = await db.UserModules
            .Where(x => x.UserId == user.Id)
            .Select(x => x.Module)
            .ToListAsync();

        if (!modules.Contains(_module))
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
            return;
        }

        await next();
    }
}