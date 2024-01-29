using CrmCorner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    public class UserController : Controller
    {
        private readonly CrmCornerContext _context;
        public UserController(CrmCornerContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MyProfile(int id)
        {
            User user = _context.User.FirstOrDefault(t => t.UserId == id);

            if (user == null)
            {
                return NotFound();
            }
            return View("MyProfile", user);
        }
    }
}
