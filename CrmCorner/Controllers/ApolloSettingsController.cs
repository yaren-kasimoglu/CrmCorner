using CrmCorner.Models;
using CrmCorner.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    //api kaydetme ve kontrol etme controllerı
    //[Authorize(Roles = "Admin")]
    public class ApolloSettingsController : Controller
    {
        private readonly CrmCornerContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ApolloHealthService _apolloHealthService;

        public ApolloSettingsController(CrmCornerContext context, UserManager<AppUser> userManager, ApolloHealthService apolloHealthService)
        {
            _context = context;
            _userManager = userManager;
            _apolloHealthService = apolloHealthService;
        }

        public async Task<IActionResult> ApolloHealthCheck()
        {
            var userId = _userManager.GetUserId(User);
            var token = await _context.ApolloSettings
                .Where(x => x.UserId == userId)
                .Select(x => x.ApolloApiToken)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Apollo API Token bulunamadı.";
                return RedirectToAction("Index");
            }

            var result = await _apolloHealthService.CheckHealthAsync(token);

            if (!result.Healthy)
                TempData["Error"] = $"❌ Apollo API erişilemiyor. Sebep: {result.Message}";
            else
                TempData["Success"] = "✅ Apollo API erişimi başarılı!";

            return RedirectToAction("Index");
        }




        // GET: ApolloSettings
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var setting = await _context.ApolloSettings
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (setting == null)
            {
                setting = new ApolloSettings(); // Boş model döneriz
            }

            return View(setting);
        }

        // POST: ApolloSettings/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(ApolloSettings model)
        {
            var userId = _userManager.GetUserId(User);

            var setting = await _context.ApolloSettings
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (setting == null)
            {
                setting = new ApolloSettings
                {
                    UserId = userId,
                    ApolloApiToken = model.ApolloApiToken
                };
                _context.ApolloSettings.Add(setting);
            }
            else
            {
                setting.ApolloApiToken = model.ApolloApiToken;
                _context.ApolloSettings.Update(setting);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Apollo API Token kaydedildi!";
            return RedirectToAction(nameof(Index));
        }
    }
}
