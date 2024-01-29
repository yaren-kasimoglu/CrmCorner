using CrmCorner.Models;
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

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

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
            foreach (IdentityError item in identityResult.Errors)
            {
                ModelState.AddModelError(string.Empty, item.Description);
            }

            return View();
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