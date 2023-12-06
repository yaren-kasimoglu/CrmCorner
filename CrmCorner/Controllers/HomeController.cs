using CrmCorner.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
//Scaffold-DbContext "server=92.204.221.160;database=crmcorner;user=yaren;password='yagmuryaren123';" Pomelo.EntityFrameworkCore.MySql -OutputDir Models -force
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