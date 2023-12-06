using CrmCorner.Models;
using Microsoft.AspNetCore.Mvc;

namespace CrmCorner.Controllers
{
    public class TaskController : Controller
    {
        private readonly CrmcornerContext _context;
        public TaskController(CrmcornerContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
