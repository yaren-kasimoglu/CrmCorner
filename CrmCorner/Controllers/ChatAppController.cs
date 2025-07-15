using System;
using CrmCorner.Models;
using CrmCorner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    //[Authorize(Roles = "Admin")]
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
            var currentUser = await _userManager.FindByNameAsync(User.Identity!.Name!);
            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

            var userViewModel = new UserViewModel
            {
                Email = currentUser!.Email,
                UserName = currentUser!.UserName,
                PhoneNumber = currentUser!.PhoneNumber,
                PictureUrl = currentUser.Picture
            };
            ViewBag.UserNames = userViewModel;
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> ChatApp(string search)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");
            ViewBag.GetUserId = currentUser.Id;
            if (search != null)
            {
                var searchPeople = _context.Users.
                Where(c => c.CompanyId == currentUser.CompanyId && c.Id != currentUser.Id && c.NameSurname.Contains(search))
                .ToList();

                List<UserViewModel> userViewModels = new List<UserViewModel>();
                foreach (var item in searchPeople)
                {
                    var currentUsers = await _userManager.FindByNameAsync(item.UserName);
                    if (currentUsers != null)
                    {
                        var userViewModel = new UserViewModel
                        {
                            Email = currentUsers!.Email,
                            UserName = currentUsers!.UserName,
                            NameSurname = currentUsers!.NameSurname,
                            PhoneNumber = currentUsers!.PhoneNumber,
                            UserId=currentUsers.Id,
                            PictureUrl = "/userprofilepicture/" + (currentUsers.Picture ?? "defaultpp.png")
                        };
                        userViewModels.Add(userViewModel);
                    }
                }

                ViewBag.UserNames = userViewModels;

                return View();

            }
            else
            {
                var searchPeople = _context.Users.
               Where(c => c.CompanyId == currentUser.CompanyId && c.Id != currentUser.Id)
                  .ToList();
                List<UserViewModel> userViewModels = new List<UserViewModel>();
                foreach (var item in searchPeople)
                {
                    var currentUsers = await _userManager.FindByNameAsync(item.UserName);
                    if (currentUsers != null)
                    {
                        bool hasUnreadMessages = _context.ChatHistories.Any(m => m.ReceiverId == currentUser.Id && !m.IsRead && m.SenderId== item.Id);

                        var userViewModel = new UserViewModel
                        {
                            Email = currentUsers!.Email,
                            UserName = currentUsers!.UserName,
                            NameSurname= currentUsers!.NameSurname,
                            PhoneNumber = currentUsers!.PhoneNumber,
                            UserId = currentUsers.Id,
                            PictureUrl = "/userprofilepicture/" + (currentUsers.Picture ?? "defaultpp.png"),
                            HasUnreadMessages = hasUnreadMessages
                        };
                        userViewModels.Add(userViewModel);
                    }
                }

                ViewBag.UserNames = userViewModels;
                               
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
            #region Update
            var notReadMessages = allMessages.Where(m => !m.IsRead).ToList();
            if (notReadMessages.Any())  // notReadMessages boş olup olmadığını kontrol et
            {
                foreach (var message in notReadMessages)
                {
                    message.IsRead = true;  // Mesajı okundu olarak işaretle
                }

                // Değişiklikleri veritabanına uygula
                _context.UpdateRange(notReadMessages);
                await _context.SaveChangesAsync();

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = false, // JavaScript tarafından erişilebilir yapmak için HttpOnly 'false'
                    Expires = DateTime.Now.AddDays(1), // Çerezin geçerlilik süresi 1 gün
                    Path = "/" // Çerezin tüm site genelinde geçerli olması
                };

                // "HasUnreadMessages" çerezini "false" olarak güncelle
                Response.Cookies.Append("HasUnreadMessages", "false", cookieOptions);
            }
            #endregion Update
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



