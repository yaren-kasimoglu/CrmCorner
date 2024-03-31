using CrmCorner.Extensions;
using CrmCorner.Models;
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
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return View();
            }

            currentUser = await _context.Users
                             .Include(u => u.Customers)
                             .Include(u => u.TaskComps)
                             .FirstOrDefaultAsync(u => u.Id == currentUser.Id);

        
            var companyUsers = await _context.Users
                                             .Where(u => u.CompanyId == currentUser.CompanyId)
                                             .ToListAsync();

            var taskComps = await _context.TaskComps.ToListAsync(); // Bu satırı görevleri yüklemek için ekledim

            var viewModel = new CompanyUsersViewModel
            {
                CurrentUser = currentUser,
                CompanyUsers = companyUsers,
                TaskComps = taskComps // ViewModel'e TaskComps ekleyin
            };

            return View(viewModel);
        }

        #region CHARTS

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

        //public async Task<IActionResult> OutcomeStatusChart()
        //{
        //    var userId = _userManager.GetUserId(User);

        //    // AppUser olarak atanan TaskComps'ı bul
        //    var appUserTasks = await _context.TaskComps
        //                                      .Where(tc => tc.UserId == userId)
        //                                      .ToListAsync();

        //    // AssignedUser olarak atanan TaskComps'ı bul
        //    var assignedUserTasks = await _context.TaskComps
        //                                           .Where(tc => tc.AssignedUserId == userId)
        //                                           .ToListAsync();

        //    // İki listeyi birleştir
        //    var combinedTasks = appUserTasks.Concat(assignedUserTasks).ToList();

        //    if (!combinedTasks.Any())
        //    {
        //        return NotFound();
        //    }

        //    // Olumlu ve olumsuz sonuçların sayısını hesapla
        //    var positiveCount = combinedTasks.Count(tc => tc.IsPositiveOutcome);
        //    var negativeCount = combinedTasks.Count(tc => !tc.IsPositiveOutcome);

        //    var chartData = new
        //    {
        //        labels = new[] { "Olumlu", "Olumsuz" },
        //        data = new[] { positiveCount, negativeCount },
        //    };

        //    return Json(chartData);
        //}
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
            if (!ModelState.IsValid)
            {
                return View();
            }

            returnUrl ??= Url.Action("Index", "Home"); //null olma durumuna karşılık bu değerin atanması . Basit bir kullanım yöntemi

            var hasUser = await _userManager.FindByEmailAsync(model.Email);
            if (hasUser == null)
            {
                ModelState.AddModelError(string.Empty, "Email veya şifre yanlış");
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
        #endregion

        #region Kayıt
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> SignUp(SignUpViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Firma adını Company tablosuna ekle veya varsa mevcut olanını kullan
            //harfleri küçülterek kaydetsin ki büyük küçük kaydedildiğinde yeniden eklenmesin
            var company = _context.Companies.FirstOrDefault(c => c.CompanyName.ToLower() == request.CompanyName.ToLower());
            if (company == null)
            {
                company = new Company { CompanyName = request.CompanyName };
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();
            }

            // Kullanıcıyı kaydedin
            var user = new AppUser
            {
                UserName = request.UserName,
                Email = request.Email,
                PhoneNumber = request.Phone,
                NameSurname = request.NameSurname,
                PositionName = request.PositionName,
                CompanyName = request.CompanyName,
            };

            user.CompanyId = company.CompanyId;

            var identityResult = await _userManager.CreateAsync(user, request.Password);

            if (identityResult.Succeeded)
            {
                TempData["SucceesMessage"] = "Kayıt işlemi başarıyla tamamlandı.";
                return RedirectToAction("SignIn", "Home");
            }

            ModelState.AddModelErrorList(identityResult.Errors.Select(x => x.Description).ToList());
            return View();
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



                //https://localhost:7145

                await _emailServices.SendResetPasswordEmail(passwordResetLink!, hasUser.Email!);


                TempData["SuccessMessage"] = "Şifre yenileme linki, e posta adresinize gönderilmiştir.";
            }
            catch (Exception ex)
            {

                throw;
            }

            return RedirectToAction(nameof(ForgetPassword));

        }

        public IActionResult ResetPassword(string userId, string token)
        {
            TempData["userId"] = userId;
            TempData["token"] = token;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel request)
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
            }
            else
            {
                ModelState.AddModelErrorList(result.Errors.Select(x => x.Description).ToList());

            }

            return RedirectToAction("SignIn", "Home");
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