using System;
using CrmCorner.Models;
using CrmCorner.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    public class ChatAppController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly CrmCornerContext _context;
        public ChatAppController(CrmCornerContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> ChatApp()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var peopleName = _context.Users
                                 //.Where(c => c.CompanyId == currentUser.CompanyId && c.Id != currentUser.Id)
                                 .Select(u => new UserViewModel
                                 {
                                     NameSurname = u.NameSurname,
                                     UserId = u.Id,
                                     PictureUrl = u.Picture ?? "/userprofilepicture/defaultpp.png"
                                 })
                                .ToList();
            ViewBag.UserNames = peopleName;
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> ChatApp(string search)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (search != null)
            {
                var searchPeople = _context.Users.
                Where(c => c.CompanyId == currentUser.CompanyId && c.Id != currentUser.Id && c.NameSurname.Contains(search))
                      .Select(u => new UserViewModel
                      {
                          NameSurname = u.NameSurname,
                          UserId = u.Id,
                          PictureUrl = u.Picture ?? "/userprofilepicture/defaultpp.png"
                      })
                                .ToList();
                ViewBag.UserNames = searchPeople;
                return View();

            }
            else
            {
                var peopleName = _context.Users
                                  //.Where(c => c.CompanyId == currentUser.CompanyId && c.Id != currentUser.Id)
                                  .Select(u => new UserViewModel
                                  {
                                      NameSurname = u.NameSurname,
                                      UserId = u.Id,
                                      PictureUrl = u.Picture ?? "/userprofilepicture/defaultpp.png"
                                  })
                                 .ToList();
                ViewBag.UserNames = peopleName;
                return View();
            }

        }

    }
}



