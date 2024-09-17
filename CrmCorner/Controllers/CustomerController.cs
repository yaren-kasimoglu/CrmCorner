
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
                if (currentUser == null)
                {
                    ErrorHelper.HandleError(this, "Geçerli kullanıcı bilgisi bulunamadı.");
                    return RedirectToAction("NotFound", "Error");
                }

                ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

                if (currentUser != null)
                {
                    var customersQuery = _context.CustomerNs
                        .Include(c => c.AppUser)
                        .AsQueryable();

                    var roles = await _userManager.GetRolesAsync(currentUser);
                    if (roles.Contains("Admin"))
                    {
                        // Admin ise, aynı şirketteki tüm kullanıcıların müşterilerini getir
                        customersQuery = customersQuery.Where(c => c.AppUser.CompanyId == currentUser.CompanyId);
                    }
                    else if (roles.Contains("TeamLeader"))
                    {
                        // TeamLeader ise, kendi ve TeamMember rolündeki kullanıcıların müşterilerini getir
                        var teamMembers = await _userManager.GetUsersInRoleAsync("TeamMember");
                        var teamMemberIds = teamMembers.Where(u => u.CompanyId == currentUser.CompanyId).Select(u => u.Id).ToList();
                        teamMemberIds.Add(currentUser.Id);

                        customersQuery = customersQuery.Where(c => teamMemberIds.Contains(c.AppUserId));
                    }
                    else
                    {
                        // Admin değil ise, sadece kendi müşterilerini getir
                        customersQuery = customersQuery.Where(c => c.AppUserId == currentUser.Id);
                    }

                    var customers = await customersQuery.ToListAsync();
                    return View(customers);
                }
                else
                {
                    ViewBag.ErrorMessage = "Geçerli kullanıcı bilgisi bulunamadı.";
                    return View();
                }
            }
            catch (NullReferenceException ex)
            {
                ErrorHelper.HandleError(this, "Veri bulunamadı: " + ex.Message);
                return RedirectToAction("NotFound", "Error");
            }
            catch (UnauthorizedAccessException ex)
            {
                ErrorHelper.HandleError(this, "Yetkisiz erişim: " + ex.Message);
                return RedirectToAction("Unauthorized", "Error");
            }
            catch (Exception ex)
            {
                ErrorHelper.HandleError(this, "Bir hata oluştu: " + ex.Message);
                return RedirectToAction("Error", "Error"); // Genel hata sayfası
            }
        }


        [HttpGet]
        public async Task<IActionResult> CustomerAdd()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

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

                //// IndustryType enum'undan dropdown listesi için verileri hazırlama
                //ViewBag.IndustryTypes = new SelectList(Enum.GetValues(typeof(IndustryType)).Cast<IndustryType>().Select(v => new SelectListItem
                //{
                //    Text = v.GetDisplayName(), // Enum için Display Attribute'unu okuyan extension method
                //    Value = ((int)v).ToString()
                //}).ToList(), "Value", "Text");

                var industryTypes = Enum.GetValues(typeof(CrmCorner.Models.Enums.IndustryType))
                        .Cast<CrmCorner.Models.Enums.IndustryType>()
                        .Select(e => new SelectListItem
                        {
                            Value = ((int)e).ToString(),
                            Text = e.GetDisplayName().ToString()
                        }).ToList();

                ViewBag.IndustryTypes = industryTypes;

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
                else
                {
                    // ModelState hatalarını yakala ve bunları kullanıcıya göster
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.ErrorMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Bir hata oluştu: " + ex.Message);
            }

            // ViewBag verilerini yeniden yükleyin, çünkü sayfa doğrulama hatası nedeniyle geri dönüyor
            var appUsers = _userManager.Users.ToList();
            var appUserItems = appUsers
                .Select(u => new SelectListItem
                {
                    Text = u.UserName,
                    Value = u.Id
                })
                .ToList();

            ViewBag.AppUsers = appUserItems;

            var industryTypes = Enum.GetValues(typeof(CrmCorner.Models.Enums.IndustryType))
                .Cast<CrmCorner.Models.Enums.IndustryType>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.GetDisplayName()
                }).ToList();

            ViewBag.IndustryTypes = new SelectList(industryTypes, "Value", "Text");

            ViewBag.EmployeeCountSelectList = Enum.GetValues(typeof(EmployeeCountRange))
                .Cast<EmployeeCountRange>()
                .Select(e => new SelectListItem
                {
                    Text = e.GetDisplayName(),
                    Value = ((int)e).ToString()
                }).ToList();

            return View(customer); // ModelState geçersiz olduğu için form yeniden yüklenir
        }

        [HttpGet]
        public async Task<IActionResult> CustomerEdit(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

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
