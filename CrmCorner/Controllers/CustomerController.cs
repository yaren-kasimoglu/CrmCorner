
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
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

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
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CustomerAdd()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                if (currentUser == null)
                {
                    return RedirectToAction("SignIn", "Home"); // Kullanıcı giriş yapmamışsa giriş sayfasına yönlendir
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
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }

        [HttpPost]
        public IActionResult CustomerAdd(CustomerN customer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    customer.CreatedDate = DateTime.Now; // Oluşturulma tarihini şimdi olarak ayarla

                    var customers = _context.CustomerNs.ToList();

                    _context.CustomerNs.Add(customer);
                    _context.SaveChanges();

                    return RedirectToAction("CustomerList");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
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
            try
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
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }

        [HttpPost]
        public IActionResult CustomerEdit(CustomerN editedCustomer)
        {
            try
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
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }

        [HttpPost]
        public IActionResult CustomerDelete(int id)
        {
            try
            {
                CustomerN customer = _context.CustomerNs.Find(id);

                // Eğer müşteri bulunamazsa
                if (customer == null)
                {
                    return NotFound();
                }

                // İlgili tüm taskcomps kayıtlarını alın
                var taskComps = _context.TaskComps.Where(tc => tc.CustomerId == id).ToList();

                // İlgili tüm taskcomplogs kayıtlarını alın ve sil
                foreach (var taskComp in taskComps)
                {
                    var taskCompLogs = _context.TaskCompLogs.Where(tcl => tcl.TaskId == taskComp.TaskId).ToList();
                    _context.TaskCompLogs.RemoveRange(taskCompLogs);
                }

                // İlgili tüm taskcomps kayıtlarını sil
                _context.TaskComps.RemoveRange(taskComps);

                // Müşteriyi sil
                _context.CustomerNs.Remove(customer);

                // Değişiklikleri kaydet
                _context.SaveChanges();

                return RedirectToAction("CustomerList");
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }
    }
}
