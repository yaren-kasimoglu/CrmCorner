
using CrmCorner.Extensions;
using CrmCorner.Models;
using CrmCorner.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly CrmCornerContext _context;
        private readonly UserManager<AppUser> _userManager;
        public CustomerController(CrmCornerContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> CustomerList()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser != null)
            {
                var customers = _context.CustomerNs
                    .Include(c => c.AppUser)
                    .Where(c => c.AppUserId == currentUser.Id) // Mevcut kullanıcının Id'sine göre filtrele
                    .ToList();

                return View(customers);
            }
            else
            {
                ViewBag.ErrorMessage = "Geçerli kullanıcı bilgisi bulunamadı.";
                return View();
            }

        }

        [HttpGet]
        public async Task<IActionResult> CustomerAdd()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return View("SignIn", "Home"); // Kullanıcı giriş yapmamışsa giriş sayfasına yönlendir
            }
            var companyId = currentUser.CompanyId;

            var appUsers = _userManager.Users.Where(u => u.CompanyId == companyId).ToList();

            var appUserItems = appUsers
         .Select(u => new SelectListItem
         {
             Text = u.UserName,
             Value = u.Id
         })
         .ToList();

            // IndustryType enum'undan dropdown listesi için verileri hazırlama
            ViewBag.IndustryTypes = new SelectList(Enum.GetValues(typeof(IndustryType)).Cast<IndustryType>().Select(v => new SelectListItem
            {
                Text = v.GetDisplayName(), // Enum için Display Attribute'unu okuyan extension method
                Value = ((int)v).ToString()
            }).ToList(), "Value", "Text");


            ViewBag.EmployeeCountSelectList = Enum.GetValues(typeof(EmployeeCountRange))
        .Cast<EmployeeCountRange>()
        .Select(e => new SelectListItem
        {
            Text = e.GetDisplayName(),
            Value = ((int)e).ToString()
        }).ToList();


            ViewBag.AppUsers = appUserItems;
            return View();
        }
        [HttpPost]
        public IActionResult CustomerAdd(CustomerN customer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    customer.CreatedDate = DateTime.Now; // Oluşturulma tarihini şimdi olarak ayarla

                    var customers = _context.CustomerNs./*Include(e => e.IdEmployeeNavigation)*/ToList();

                    _context.CustomerNs.Add(customer);
                    _context.SaveChanges();

                    return RedirectToAction("CustomerList");
                }
            }
            catch (Exception ex)
            {

                throw;
            }


            var appUsers = _userManager.Users.ToList();
            var appUserItems = appUsers
         .Select(u => new SelectListItem
         {
             Text = u.UserName,
             Value = u.Id
         })
         .ToList();

            ViewBag.AppUsers = appUserItems;

            return View(customer);
        }


        [HttpGet]
        public async Task<IActionResult> CustomerEdit(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Challenge(); // Kullanıcı giriş yapmamışsa giriş sayfasına yönlendir
            }
            var companyId = currentUser.CompanyId;

            var appUsers = _userManager.Users.Where(u => u.CompanyId == companyId).ToList();

            var appUserItems = appUsers
         .Select(u => new SelectListItem
         {
             Text = u.UserName,
             Value = u.Id
         })
         .ToList();

            ViewBag.IndustryTypes = new SelectList(Enum.GetValues(typeof(IndustryType)).Cast<IndustryType>().Select(v => new SelectListItem
            {
                Text = v.GetDisplayName(),
                Value = ((int)v).ToString()
            }).ToList(), "Value", "Text");




            ViewBag.AppUsers = appUserItems;
            // id parametresini kullanarak düzenlenecek müşteriyi veritabanından al
            CustomerN customer = _context.CustomerNs.Find(id);

            ViewBag.EmployeeCountSelectList = Enum.GetValues(typeof(EmployeeCountRange))
.Cast<EmployeeCountRange>()
.Select(e => new SelectListItem
{
    Text = e.GetDisplayName(),
    Value = ((int)e).ToString(),
    Selected = e == customer.EmployeeCount // mevcut değeri işaretle
}).ToList();

            // Eğer müşteri bulunamazsa
            if (customer == null)
            {
                return NotFound(); // 404 Not Found dönülebilir
            }

            // Müşteriyi düzenleme sayfasına gönder
            return View(customer);
        }

        [HttpPost]
        public IActionResult CustomerEdit(CustomerN editedCustomer)
        {
            if (ModelState.IsValid)
            {
                _context.Entry(editedCustomer).State = EntityState.Modified;
                _context.SaveChanges();

                return RedirectToAction("CustomerList");
            }
            //ModelState.IsValid false ise
            return View(editedCustomer);
        }


        [HttpPost]
        public IActionResult CustomerDelete(int id)
        {
            CustomerN customer = _context.CustomerNs.Find(id);

            // Eğer müşteri bulunamazsa
            if (customer == null)
            {
                return NotFound();
            }
            _context.CustomerNs.Remove(customer);
            _context.SaveChanges();


            return RedirectToAction("CustomerList");
        }

    }
}
