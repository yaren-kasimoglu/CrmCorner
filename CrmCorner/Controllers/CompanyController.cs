using CrmCorner.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class CompanyController : Controller
    {
        private readonly CrmCornerContext _context;
        private readonly UserManager<AppUser> _userManager;
        public CompanyController(CrmCornerContext context,UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
    
        //[Authorize(Roles ="Admin")]
        public async Task<IActionResult> CompanyList()
        {
            try
            {
            var currentUser = await _userManager.GetUserAsync(User);

                var roles = await _userManager.GetRolesAsync(currentUser);

                if (roles.Contains("Admin"))//areaya yönlendiriyorum
                {
                    return RedirectToAction("CompanyList", "Company", new { area = "Admin" });
                }

                ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

            var Companys = _context.Companies.ToList();
                return View(Companys);
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApproveCompany(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User); // Async olarak currentUser'ı al
                var roles = await _userManager.GetRolesAsync(currentUser);

                if (roles.Contains("Admin"))
                {
                    // Admin kullanıcıyı Admin alanına yönlendirme
                    return RedirectToAction("ApproveCompany", "Company", new { area = "Admin" });
                }

                var company = await _context.Companies.FindAsync(id); // Async olarak şirketi bul
                if (company != null)
                {
                    company.IsApproved = true;  // Onaylama işlemi
                    await _context.SaveChangesAsync(); // Async olarak değişiklikleri kaydet
                }

                return RedirectToAction("CompanyList");
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> RejectCompany(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User); // Async olarak currentUser'ı al
                var roles = await _userManager.GetRolesAsync(currentUser);

                if (roles.Contains("Admin"))
                {
                    // Admin kullanıcıyı Admin alanına yönlendirme
                    return RedirectToAction("RejectCompany", "Company", new { area = "Admin" });
                }
                var company = _context.Companies.Find(id);
                if (company != null)
                {
                    _context.Companies.Remove(company);  // Silme işlemi
                    _context.SaveChanges();
                }
                return RedirectToAction("CompanyList");
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }
    }
}
