using CrmCorner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CrmCorner.Controllers
{
    public class AdminLayoutFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userManager = context.HttpContext.RequestServices.GetService<UserManager<AppUser>>();
            if (userManager != null)
            {
                var user = await userManager.GetUserAsync(context.HttpContext.User);
                if (user != null)
                {
                    var roles = await userManager.GetRolesAsync(user);
                    if (roles.Contains("Admin"))
                    {
                        context.HttpContext.Items["Layout"] = "~/Areas/Admin/Views/Shared/_LayoutHomePageArea.cshtml";
                    }
                    else
                    {
                        context.HttpContext.Items["Layout"] = "~/Views/Shared/_LayoutHomePage.cshtml";
                    }
                }
                else
                {
                    context.HttpContext.Items["Layout"] = "~/Views/Shared/_LayoutHomePage.cshtml";
                }
            }
            else
            {
                context.HttpContext.Items["Layout"] = "~/Views/Shared/_LayoutHomePage.cshtml";
            }

            await next();
        }
    }
    }
