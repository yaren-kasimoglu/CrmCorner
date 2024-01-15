using CrmCorner.Models;
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
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
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