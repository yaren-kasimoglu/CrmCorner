using CrmCorner.Extensions;
using CrmCorner.Models;
using CrmCorner.Models.Enums;
using CrmCorner.Services;
using CrmCorner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Common;
using System.Diagnostics;
using System.Security.Claims;

namespace CrmCorner.Controllers
{
    public class HomeController : Controller
    {
        private readonly CrmCornerContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailServices _emailServices;

        //deneme yorum
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailServices emailServices, CrmCornerContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailServices = emailServices;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                if (!User.Identity!.IsAuthenticated)
                {
                    return RedirectToAction("SignIn");
                }

                var currentUser = await _userManager.GetUserAsync(User);

                if (currentUser == null)
                {
                    return View();
                }

                try
                {
                    currentUser = await _context.Users
                                        .Include(u => u.Customers)
                                        .Include(u => u.TaskComps)
                                        .FirstOrDefaultAsync(u => u.Id == currentUser.Id);

                    var companyUsers = await _context.Users
                                                     .Where(u => u.CompanyId == currentUser.CompanyId)
                                                     .ToListAsync();

                    var taskComps = await _context.TaskComps.ToListAsync(); // Bu satırı görevleri yüklemek için ekledim
                    var email = currentUser?.Email;

                    var viewModel = new CompanyUsersViewModel
                    {
                        CurrentUser = currentUser,
                        CompanyUsers = companyUsers,
                        TaskComps = taskComps // ViewModel'e TaskComps ekleyin
                    };
                    ViewData["UserEmail"] = email;

                    return View(viewModel);
                }
                catch (Exception ex)
                {
                    return RedirectToAction("NotFound", "Error");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }


        #region CHARTS

        [Authorize]
        public async Task<IActionResult> IndustryChart()
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                var user = await _context.Users
                                          .Include(u => u.Customers)
                                          .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                // Sektörleri gruplayıp sayılarına göre chart verisi oluşturma
                var chartData = user.Customers
                                     .GroupBy(c => c.Industry)
                                     .Select(group => new { Industry = group.Key, Count = group.Count() })
                                     .ToList();

                // labels ve data alanlarını doldur
                var labels = chartData.Select(data => data.Industry.GetDisplayName().ToString()).ToArray();
                var dataValues = chartData.Select(data => data.Count).ToArray();

                return Json(new { labels, data = dataValues });
            }
            catch (Exception ex)
            {
                // Hata günlüğüne yaz
                _logger.LogError(ex, "An error occurred while fetching industry chart data.");

                // Hata mesajıyla birlikte bir server hatası döndür
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [Authorize]
        public async Task<IActionResult> IsFinalDesicionMaker()
        {
            var userId = _userManager.GetUserId(User);

            var userTaskComps = await _context.Users
                                                 .Where(u => u.Id == userId)
                                                 .SelectMany(u => u.TaskComps)
                                                 .ToListAsync();

            if (userTaskComps == null || !userTaskComps.Any())
            {
                return NotFound();
            }

            var isFinalDesicionMaker = userTaskComps.Count(tc => tc.IsFinalDecisionMaker);
            var IsNotFinalDesicionMaker = userTaskComps.Count(tc => !tc.IsFinalDecisionMaker);

            var chartData = new
            {
                labels = new[] { "Evet", "Hayır" },
                data = new[] { isFinalDesicionMaker, IsNotFinalDesicionMaker },
            };


            return Json(chartData);
        }

        [Authorize]
        public async Task<IActionResult> OutcomeStatusChart()
        {
            var userId = _userManager.GetUserId(User);

            // AppUser olarak atanan TaskComps'ı bul
            var appUserTasks = await _context.TaskComps
                                              .Where(tc => tc.UserId == userId)
                                              .ToListAsync();

            // AssignedUser olarak atanan TaskComps'ı bul
            var assignedUserTasks = await _context.TaskComps
                                                   .Where(tc => tc.AssignedUserId == userId)
                                                   .ToListAsync();

            // İki listeyi birleştir
            var combinedTasks = appUserTasks.Concat(assignedUserTasks).ToList();

            if (!combinedTasks.Any())
            {
                return NotFound();
            }


            var outcomeCounts = combinedTasks.GroupBy(tc => tc.Outcomes)
                                     .ToDictionary(g => g.Key.ToString(), g => g.Count());

            // Tüm olası enum değerlerini döngü ile işle
            var labels = Enum.GetNames(typeof(OutcomeType)).ToList();
            var data = new List<int>();
            foreach (var label in labels)
            {
                outcomeCounts.TryGetValue(label, out var count);
                data.Add(count);
            }

            var chartData = new
            {
                labels = labels.ToArray(),
                data = data.ToArray(),
            };
            return Json(chartData);
        }
        [Authorize]
        public async Task<IActionResult> UserTaskStatusChart()
        {
            // Aktif kullanıcının ID'sini al
            var userId = _userManager.GetUserId(User);

            // AppUser olarak atanan TaskComps'ı bul
            var appUserTasks = await _context.TaskComps
                                              .Include(tc => tc.Status)
                                              .Where(tc => tc.UserId == userId)
                                              .ToListAsync();

            // AssignedUser olarak atanan TaskComps'ı bul
            var assignedUserTasks = await _context.TaskComps
                                                   .Include(tc => tc.Status)
                                                   .Where(tc => tc.AssignedUserId == userId)
                                                   .ToListAsync();

            // İki listeyi birleştir
            var combinedTasks = appUserTasks.Concat(assignedUserTasks).ToList();

            var chartData = new
            {
                labels = combinedTasks.Select(tc => tc.Status.StatusName).Distinct(),
                data = combinedTasks.GroupBy(tc => tc.Status.StatusName).Select(group => group.Count())
            };

            return Json(chartData);
        }
        #endregion

        public IActionResult Giris()
        {
            return View();
        }

        #region Giriş
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(SignInViewModel model, string? returnUrl = null)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }

                returnUrl ??= Url.Action("Index", "Home"); // null olma durumuna karşılık bu değerin atanması. Basit bir kullanım yöntemi

                var hasUser = await _userManager.FindByEmailAsync(model.Email);
                if (hasUser == null)
                {
                    ModelState.AddModelError(string.Empty, "Email veya şifre yanlış");
                    return View();
                }

                // Şirket onaylı mı değil mi kontrol edildiği yer
                var company = _context.Companies.FirstOrDefault(c => c.CompanyId == hasUser.CompanyId);
                if (company == null || !(company.IsApproved ?? false))
                {
                    ModelState.AddModelError(string.Empty, "Şirketiniz sistem tarafından henüz onaylanmamış.");
                    return View();
                }

                var signInresult = await _signInManager.PasswordSignInAsync(hasUser, model.Password, model.RememberMe, true);

                if (signInresult.Succeeded)
                {
                    return Redirect(returnUrl);
                }

                if (signInresult.IsLockedOut)
                {
                    ModelState.AddModelErrorList(new List<string>() { "3 dakika boyunca giriş yapamazsınız." });
                    return View();
                }

                ModelState.AddModelErrorList(new List<string>() { $"Email veya şifre hatalı.", $"(Başarısız giriş sayısı : {await _userManager.GetAccessFailedCountAsync(hasUser)})" });

                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }

        #endregion

        #region Kayıt
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }

                // Kullanıcının e-posta adresinden domain'i çıkarma
                var emailDomain = request.Email.Split('@')[1].ToLower();

                // Domain'e göre bir firma var mı kontrol et
                var company = _context.Companies.FirstOrDefault(c => c.EmailDomain.ToLower() == emailDomain);

                if (company == null)
                {
                    try
                    {
                        // Yeni firma oluştur ve ekle
                        company = new Company
                        {
                            CompanyName = request.CompanyName,
                            EmailDomain = emailDomain,
                            IsApproved = false
                        };
                        _context.Companies.Add(company);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        return RedirectToAction("NotFound", "Error");
                    }
                }

                try
                {
                    // Kullanıcıyı kaydetme
                    var user = new AppUser
                    {
                        UserName = request.UserName,
                        Email = request.Email,
                        PhoneNumber = request.Phone,
                        NameSurname = request.NameSurname,
                        PositionName = request.PositionName,
                        CompanyName = company.CompanyName,  // Firma adını Company'den al
                        EmailDomain = emailDomain,
                        CompanyId = company.CompanyId
                    };

                    var identityResult = await _userManager.CreateAsync(user, request.Password);

                    if (identityResult.Succeeded)
                    {
                        TempData["SuccessMessage"] = "Kayıt işlemi başarıyla tamamlandı.";
                        return RedirectToAction("SignIn", "Home");
                    }

                    ModelState.AddModelErrorList(identityResult.Errors.Select(x => x.Description).ToList());
                    return View();
                }
                catch (Exception ex)
                {
                    return RedirectToAction("NotFound", "Error");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }


        //public async Task<IActionResult> SignUp(SignUpViewModel request)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View();
        //    }
        //    var identityResult = await _userManager.CreateAsync(new() { UserName = request.UserName, Email = request.Email, PhoneNumber = request.Phone,NameSurname=request.NameSurname,PositionName=request.PositionName,CompanyName=request.CompanyName,EmployeeCount=request.EmployeeCount,Sector=request.Sector }, request.Password);



        //    if (identityResult.Succeeded)
        //    {
        //        TempData["SucceesMessage"] = "Kayıt işlemi başarıyla tamamlandı.";
        //        return RedirectToAction("SignIn", "Home");

        //    }
        //    ModelState.AddModelErrorList(identityResult.Errors.Select(x => x.Description).ToList());
        //    return View();
        //}
        #endregion

        #region ŞİFRE İŞLEMLERİ
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel request)
        {
            try
            {
                var hasUser = await _userManager.FindByEmailAsync(request.Email);
                if (hasUser == null)
                {
                    ModelState.AddModelError(String.Empty, "Bu email adresine sahip kullanıcı bulunamamıştır.");
                    return View();
                }

                string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(hasUser);
                var passwordResetLink = Url.Action("ResetPassword", "Home", new { userId = hasUser.Id, Token = passwordResetToken }, HttpContext.Request.Scheme);

                await _emailServices.SendResetPasswordEmail(passwordResetLink!, hasUser.Email!);
                TempData["SuccessMessage"] = "Şifre yenileme linki, e-posta adresinize gönderilmiştir.";
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }

            return RedirectToAction(nameof(ForgetPassword));
        }

        public IActionResult ResetPassword(string userId, string token)
        {
            try
            {
                TempData["userId"] = userId;
                TempData["token"] = token;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel request)
        {
            try
            {
                var userId = TempData["userId"];
                var token = TempData["token"];
                if (userId == null || token == null)
                {
                    throw new Exception("Bir hata meydana geldi.");
                }

                var hasUser = await _userManager.FindByIdAsync(userId.ToString()!);
                if (hasUser == null)
                {
                    ModelState.AddModelError(String.Empty, "Kullanıcı bulunamamıştır.");
                    return View();
                }

                IdentityResult result = await _userManager.ResetPasswordAsync(hasUser, token.ToString()!, request.Password);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Şifreniz başarıyla yenilenmiştir.";
                    return RedirectToAction("SignIn", "Home");
                }
                else
                {
                    ModelState.AddModelErrorList(result.Errors.Select(x => x.Description).ToList());
                    return View();
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }
        #endregion

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Notifications()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id; // Kullanıcı ID'sini alın

            if (userId == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            var notifications = await _context.Notifications
                                              .Where(n => n.UserId == userId)
                                              .ToListAsync(); // Kullanıcıya ait bildirimleri çekin

            return View(notifications); // Bildirimleri view'a gönderin
        }


        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Notifications));
            }
            return NotFound();
        }

    }
}