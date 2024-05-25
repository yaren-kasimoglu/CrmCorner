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
            ViewBag.GetUserId = currentUser.Id;
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
                                  .Where(c => c.CompanyId == currentUser.CompanyId && c.Id != currentUser.Id)
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

        [HttpGet]
        public async Task<IActionResult> GetUserMessages(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);


            var messagesSent = _context.ChatHistories
                                .Where(c => c.SenderId == currentUser.Id && c.ReceiverId == userId)
                                .OrderByDescending(c => c.MessageTime)
                                .ToList();

            var messagesReceived = _context.ChatHistories
                                            .Where(c => c.SenderId == userId && c.ReceiverId == currentUser.Id)
                                            .OrderByDescending(c => c.MessageTime)
                                            .ToList();

            var allMessages = messagesSent.Concat(messagesReceived)
                                            .OrderBy(c => c.MessageTime)
                                            .ToList();

            ViewBag.GetUserMessage = allMessages;
            ViewBag.GetUserId = currentUser.Id;
            return Json(new { Message = allMessages });
        }

        [HttpPost]
        public async Task<IActionResult> GetUserName(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(new { Message = "Current user not found" });
            }

            var peopleName = _context.Users
                                     .Where(c => c.CompanyId == currentUser.CompanyId && c.Id == userId)
                                     .FirstOrDefault();

            if (peopleName == null)
            {
                return Json(new { Message = "User not found" });
            }

            return Json(new { Message = peopleName.NameSurname });
        }

    }
}



