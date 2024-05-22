using CrmCorner.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    [Authorize]
    public class CompanyController : Controller
    {
        private readonly CrmCornerContext _context;
        public CompanyController(CrmCornerContext context)
        {
            _context = context;
        }
        [Authorize]
        public IActionResult CompanyList()
        {
            try
            {
                var Companys = _context.Companies.ToList();
                return View(Companys);
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }

        [HttpPost]
        public IActionResult ApproveCompany(int id)
        {
            try
            {
                var company = _context.Companies.Find(id);
                if (company != null)
                {
                    company.IsApproved = true;  // Onaylama işlemi
                    _context.SaveChanges();
                }
                return RedirectToAction("CompanyList");
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }

        [HttpPost]
        public IActionResult RejectCompany(int id)
        {
            try
            {
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
