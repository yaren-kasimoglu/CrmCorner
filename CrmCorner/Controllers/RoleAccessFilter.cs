using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CrmCorner.Filters
{
    public class RoleAccessFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (!user.Identity.IsAuthenticated)
                return; // Giriş yoksa login sayfasına yönlenir (Identity halleder)

            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();

         
            if (controller == "Member")
                return;

            // Sadece SocialMediaUser için kontrol yap
            if (user.IsInRole("SocialMediaUser"))
            {
                var allowed = new List<(string Controller, string Action)>
                {
                    ("SocialMedia", "Index"),
                    ("SocialMedia", "Calendar"),
                    ("SocialMedia", "SendFeedback"),
                    ("SocialMedia", "Details"),
                    ("SocialMedia", "GetMedia"),
                    ("SocialMedia", "GetFileExtension"),
                    ("SocialMedia", "GetMimeType"),
                    ("SocialMedia", "CancelApproval")
                };

                bool isAllowed = allowed.Any(a =>
                    string.Equals(a.Controller, controller, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(a.Action, action, StringComparison.OrdinalIgnoreCase));

                if (!isAllowed)
                {
                    context.Result = new RedirectToActionResult("AccessDenied", "Member", null);
                }
            }
        }
    }
}
