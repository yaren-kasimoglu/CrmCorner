using CrmCorner.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{

    //  CompanyController
    // Şirket kayıtlarını listeleme, onaylama ve silme işlemlerini yönetir.
    // Erişim: SuperAdmin, Admin
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class CompanyController : Controller
    {
        private readonly CrmCornerContext _context;
        private readonly UserManager<AppUser> _userManager;

        public CompanyController(CrmCornerContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> CompanyList()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

            // Admin ise tüm şirketleri görsün
            if (roles.Contains("Admin") || roles.Contains("SuperAdmin"))
            {
                var allCompanies = await _context.Companies.ToListAsync();
                return View(allCompanies);
            }

            // Diğer kullanıcı sadece kendi şirketini görsün
            var company = await _context.Companies
                .Where(x => x.CompanyId == currentUser.CompanyId)
                .ToListAsync();

            return View(company);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> ApproveCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                company.IsApproved = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("CompanyList");
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> RejectCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("CompanyList");
        }
    }
}
