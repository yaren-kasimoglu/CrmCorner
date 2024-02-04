using CrmCorner.Extensions;
using CrmCorner.Models;
using CrmCorner.Services;
using CrmCorner.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
//Scaffold-DbContext "server=92.204.221.160;database=crmcorner;user=yaren;password='yagmuryaren123';" Pomelo.EntityFrameworkCore.MySql -OutputDir Models -force
//Scaffold-DbContext "server=92.204.221.160;database=crmcorner;user=yaren;password=yagmuryaren123;" Pomelo.EntityFrameworkCore.MySql --context CrmCorner.Areas.Identity.Data.CrmcornerContext -o Models
//Scaffold-DbContext "connection-string" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context CrmCornerContext
// Scaffold-DbContext "server=92.204.221.160;database=crmcorner;user=yaren;password=yagmuryaren123;" Pomelo.EntityFrameworkCore.MySql -OutputDir Models -Context CrmCornerContext

//Scaffold - DbContext "server=92.204.221.160;database=crmcorner;user=yaren;password=yagmuryaren123;" Pomelo.EntityFrameworkCore.MySql--context CrmCorner.Areas.Identity.Data.CrmcornerContext - o Models

namespace CrmCorner.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailServices _emailServices;

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailServices emailServices)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailServices = emailServices;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region Giriş
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(SignInViewModel model, string? returnUrl=null)
        {
           

            returnUrl = returnUrl ?? Url.Action("WelcomePage", "Home");

            var hasUser=await _userManager.FindByEmailAsync(model.Email);
            if (hasUser==null)
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
            var identityResult = await _userManager.CreateAsync(new() { UserName = request.UserName, Email = request.Email, PhoneNumber = request.Phone }, request.Password);

      

            if (identityResult.Succeeded)
            {
                TempData["SucceesMessage"] = "Kayıt işlemi başarıyla tamamlandı.";
                return View();                 
            }
            ModelState.AddModelErrorList(identityResult.Errors.Select(x=>x.Description).ToList());
            return View();
        }
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

                await _emailServices.SendResetPasswordEmail(passwordResetLink, hasUser.Email);


                TempData["SuccessMessage"] = "Şifre yenileme linki, e posta adresinize gönderilmiştir.";
            }
            catch (Exception ex)
            {

                throw;
            }

            return RedirectToAction(nameof(ForgetPassword));

        }

      

        public IActionResult WelcomePage()
        {
            return View();
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




    }
}