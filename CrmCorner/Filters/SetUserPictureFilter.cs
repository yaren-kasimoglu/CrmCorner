using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Identity;
using CrmCorner.Models;
using System.Threading.Tasks;

public class SetUserPictureFilter : IAsyncActionFilter
{
    private readonly UserManager<AppUser> _userManager;

    public SetUserPictureFilter(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var controller = context.Controller as Controller;

        if (controller != null && controller.User.Identity.IsAuthenticated)
        {
            var user = await _userManager.GetUserAsync(controller.User);
            var picture = user?.Picture ?? "defaultpp.png";
            controller.ViewBag.PictureUrl = "/userprofilepicture/" + picture;
        }

        await next(); 
    }
}
