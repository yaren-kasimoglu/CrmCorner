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


//Scaffold-DbContext "server=94.73.148.165;database=u1613932_crmCor;user=u1613932_crmcorn;password=.8j:-6njA8WLDf7_;Convert Zero Datetime=True;" Pomelo.EntityFrameworkCore.MySql -OutputDir Models -force




namespace CrmCorner.Controllers
{
    public class HomeController : Controller
    {
        private readonly CrmCornerContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailServices _emailServices;


        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailServices emailServices, CrmCornerContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailServices = emailServices;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        //public async Task<IActionResult> GetWeather(string city)
        //{
        //    var url = $"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={_apiKey}&units=metric";
        //    var response = await _httpClient.GetAsync(url);
        //    if (response.IsSuccessStatusCode)
        //    {
        //        var content = await response.Content.ReadAsStringAsync();
        //        var weatherData = JsonConvert.DeserializeObject(content);
        //        return Ok(weatherData); // Hava durumu verilerini JSON olarak döndür
        //    }

        //    return BadRequest("Hava durumu bilgileri alınamadı.");
        //}
        public async Task<IActionResult> OutcomeStatusChart()
        {
            // Aktif kullanıcının ID'sini al
            var userId = _userManager.GetUserId(User);

            // Kullanıcı ID'sini kullanarak AppUser kaydını ve ilişkili TaskComps'ı bul
            var userTaskComps = await _context.Users
                                                 .Where(u => u.Id == userId)
                                                 .SelectMany(u => u.TaskComps)
                                                 .ToListAsync();

            if (userTaskComps == null || !userTaskComps.Any())
            {
                return NotFound();
            }

            var outcomeCounts = userTaskComps.GroupBy(tc => tc.Outcome)
                                             .Select(group => new {
                                                 Outcome = group.Key.ToString(), // Enum değerini string'e çevir
                                                 Count = group.Count()
                                             }).ToList();
            var chartData = new
            {
                labels = outcomeCounts.Select(oc => oc.Outcome.ToString()),
                data = outcomeCounts.Select(oc => oc.Count)
            };

            return Json(chartData);
        }
        public async Task<IActionResult> UserTaskStatusChart()
        {
            // Aktif kullanıcının ID'sini al
            var userId = _userManager.GetUserId(User);

            // Kullanıcı ID'sini kullanarak AppUser kaydını ve ilişkili TaskComps'ı bul
            var user = await _context.Users
                                     .Include(u => u.TaskComps)
                                         .ThenInclude(tc => tc.Status)
                                     .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            var chartData = new
            {
                labels = user.TaskComps.Select(tc => tc.Status.StatusName).Distinct(),
                data = user.TaskComps.GroupBy(tc => tc.Status.StatusName).Select(group => group.Count())
            };

            return Json(chartData);
        }


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
                CompanyName=request.CompanyName,   
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
            if (userId==null || token==null)
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

            return RedirectToAction("SignIn","Home");
        }



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