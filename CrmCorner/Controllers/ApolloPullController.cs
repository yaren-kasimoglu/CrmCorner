using CrmCorner.Models;
using CrmCorner.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    [Authorize]
    public class ApolloPullController : Controller
    {
        private readonly CrmCornerContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ApolloService _apolloService;

        public ApolloPullController(CrmCornerContext context, UserManager<AppUser> userManager, ApolloService apolloService)
        {
            _context = context;
            _userManager = userManager;
            _apolloService = apolloService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var token = await _context.ApolloSettings
                .Where(x => x.UserId == userId)
                .Select(x => x.ApolloApiToken)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Apollo API Tokenı bulunamadı.";
                return RedirectToAction("Index", "ApolloSettings");
            }

            try
            {
                var users = await _apolloService.GetContactsAsync(token);
                return View("Index", users); // View name 'GetUsers.cshtml' olacak
            }
            catch (Exception ex)
            {
                return Content($"HATA: {ex.Message}");
            }
        }


        public async Task<IActionResult> Lists()
        {
            var userId = _userManager.GetUserId(User);
            var token = await _context.ApolloSettings
                .Where(x => x.UserId == userId)
                .Select(x => x.ApolloApiToken)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Apollo API Token bulunamadı.";
                return RedirectToAction("Index", "ApolloSettings");
            }

            var lists = await _apolloService.GetContactListsAsync(token);
            return View("Lists", lists);
        }

        public async Task<IActionResult> MatchPerson()
        {
            var userId = _userManager.GetUserId(User);
            var token = await _context.ApolloSettings
                .Where(x => x.UserId == userId)
                .Select(x => x.ApolloApiToken)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Apollo API Token bulunamadı.";
                return RedirectToAction("Index", "ApolloSettings");
            }

            try
            {
                var matchedPerson = await _apolloService.MatchPersonAsync(token, "Yaren", "Kasimoğlu", "yaren@exporty.co");
                return View("MatchPerson", matchedPerson);
            }
            catch (Exception ex)
            {
                return Content($"HATA: {ex.Message}");
            }
        }

        public async Task<IActionResult> SearchPeople()
        {
            var userId = _userManager.GetUserId(User);
            var token = await _context.ApolloSettings
                .Where(x => x.UserId == userId)
                .Select(x => x.ApolloApiToken)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Apollo API Tokenı bulunamadı.";
                return RedirectToAction("Index", "ApolloSettings");
            }

            try
            {
                var people = await _apolloService.SearchPeopleAsync(token);
                return View("SearchPeople", people);
            }
            catch (Exception ex)
            {
                return Content($"HATA: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult SearchAccounts()
        {
            return View(new AccountSearchModel());
        }

        [HttpPost]
        public async Task<IActionResult> SearchAccounts(AccountSearchModel model)
        {
            var userId = _userManager.GetUserId(User);
            var token = await _context.ApolloSettings
                .Where(x => x.UserId == userId)
                .Select(x => x.ApolloApiToken)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Apollo API Tokenı bulunamadı.";
                return RedirectToAction("Index", "ApolloSettings");
            }

            try
            {
                var companies = await _apolloService.SearchAccountsAsync(token, model);
                return View("SearchAccountResults", companies);
            }
            catch (Exception ex)
            {
                return Content("HATA: " + ex.Message);
            }
        }

        [HttpGet]
        public IActionResult SearchCompanies()
        {
            return View(new ApolloCompanySearchDto());
        }

        [HttpPost]
        public async Task<IActionResult> SearchCompanies(ApolloCompanySearchDto dto)
        {
            var userId = _userManager.GetUserId(User);
            var token = await _context.ApolloSettings
                .Where(x => x.UserId == userId)
                .Select(x => x.ApolloApiToken)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Apollo API Tokenı bulunamadı.";
                return RedirectToAction("Index", "ApolloSettings");
            }

            try
            {
                var results = await _apolloService.SearchCompaniesAsync(token, dto);
                ViewBag.Query = dto;
                return View("SearchCompaniesResults", results);
            }
            catch (Exception ex)
            {
                return Content($"HATA: {ex.Message}");
            }
        }

        public async Task<IActionResult> TestPeopleSearch()
        {
            var result = await _apolloService.SearchPeopleWithSessionTokenAsync();
            return Content(result, "application/json");
        }



    }
}