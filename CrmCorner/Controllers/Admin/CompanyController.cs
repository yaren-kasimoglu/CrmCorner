//using CrmCorner.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;

//namespace CrmCorner.Controllers.Admin
//{
//    //[Authorize(Roles = "Admin")]
//    //[Route("Admin/[controller]/[action]")]
//    public class CompanyController : Controller
//    {
//        private readonly CrmCornerContext _context;
//        private readonly UserManager<AppUser> _userManager;
//        public CompanyController(CrmCornerContext context,UserManager<AppUser> userManager)
//        {
//            _context = context;
//            _userManager = userManager;
//        }
    

//        public async Task<IActionResult> CompanyList()
//        {
//            try
//            {
//            var currentUser = await _userManager.GetUserAsync(User);

//            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

//            var Companys = _context.Companies.ToList();
//                return View(Companys);
//            }
//            catch (Exception ex)
//            {
//                return RedirectToAction("NotFound", "Error");
//            }
//        }


//        [HttpPost]
//        public IActionResult ApproveCompany(int id)
//        {
//            try
//            {
//                var company = _context.Companies.Find(id);
//                if (company != null)
//                {
//                    company.IsApproved = true;  // Onaylama işlemi
//                    _context.SaveChanges();
//                }
//                return RedirectToAction("CompanyList");
//            }
//            catch (Exception ex)
//            {
//                return RedirectToAction("NotFound", "Error");
//            }
//        }

  
//        [HttpPost]
//        public IActionResult RejectCompany(int id)
//        {
//            try
//            {
//                var company = _context.Companies.Find(id);
//                if (company != null)
//                {
//                    _context.Companies.Remove(company);  // Silme işlemi
//                    _context.SaveChanges();
//                }
//                return RedirectToAction("CompanyList");
//            }
//            catch (Exception ex)
//            {
//                return RedirectToAction("NotFound", "Error");
//            }
//        }
//    }
//}
