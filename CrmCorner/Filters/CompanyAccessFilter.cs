using CrmCorner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CrmCorner.Filters
{
    public class CompanyAccessFilter : IAsyncActionFilter
    {
        private readonly CrmCornerContext _context;

        public CompanyAccessFilter(CrmCornerContext context)
        {
            _context = context;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;

            if (user?.Identity?.IsAuthenticated != true)
            {
                await next();
                return;
            }

            // ✅ SuperAdmin ödeme/trial kontrolüne takılmasın
            if (user.IsInRole("SuperAdmin"))
            {
                await next();
                return;
            }

            var controllerName = context.RouteData.Values["controller"]?.ToString();

            var allowedControllers = new[]
            {
        "Account",
        "Login",
        "Register",
        "Payment",
        "Home"
    };

            if (allowedControllers.Contains(controllerName))
            {
                await next();
                return;
            }

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            var appUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (appUser == null || appUser.CompanyId == null)
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
                return;
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(x => x.CompanyId == appUser.CompanyId);

            if (company == null)
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
                return;
            }

            //  Eski firmalar ödeme/trial kontrolüne takılmasın
            if (company.IsLegacyCustomer)
            {
                await next();
                return;
            }

            var now = DateTime.Now;

            var hasActiveTrial =
                company.IsTrialActive &&
                company.TrialEndDate.HasValue &&
                company.TrialEndDate.Value >= now;

            var hasActiveSubscription =
                company.IsPaymentActive &&
                company.SubscriptionEndDate.HasValue &&
                company.SubscriptionEndDate.Value >= now;

            var canUseSystem =
                company.IsApproved == true &&
                (hasActiveTrial || hasActiveSubscription);

            if (!canUseSystem)
            {
                context.Result = new RedirectToActionResult("Pricing", "Home", null);
                return;
            }

            await next();
        }
    }
}