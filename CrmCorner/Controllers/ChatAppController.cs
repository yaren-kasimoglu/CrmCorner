using CrmCorner.Models;
using CrmCorner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CrmCorner.Services;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI;
using System;

namespace CrmCorner.Controllers
{
    //  ChatAppController
    // Şirket içi mesajlaşma (chat) işlemlerini yönetir.
    // Erişim: SuperAdmin, Admin, TeamLeader, TeamMember (sosyal medya rollerine kapalı)
    [Authorize(Roles = "SuperAdmin,Admin,TeamLeader,TeamMember")]
    public class ChatAppController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly CrmCornerContext _context;
        private readonly AiChatService _aiChatService;
        public ChatAppController(CrmCornerContext context, UserManager<AppUser> userManager, AiChatService aiChatService)
        {
            _context = context;
            _userManager = userManager;
            _aiChatService = aiChatService;
        }

        public async Task<IActionResult> ChatApp()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("SignIn", "Home");

            ViewBag.PictureUrl = string.IsNullOrWhiteSpace(currentUser.Picture)
                ? "/userprofilepicture/defaultpp.png"
                : "/userprofilepicture/" + currentUser.Picture;

            ViewBag.GetUserId = currentUser.Id;

            var searchPeople = _context.Users
                .Where(c => c.CompanyId == currentUser.CompanyId && c.Id != currentUser.Id)
                .ToList();

            List<UserViewModel> userViewModels = new List<UserViewModel>();

            foreach (var item in searchPeople)
            {
                bool hasUnreadMessages = _context.ChatHistories.Any(m =>
                    m.ReceiverId == currentUser.Id &&
                    !m.IsRead &&
                    m.SenderId == item.Id);

                var userViewModel = new UserViewModel
                {
                    Email = item.Email,
                    UserName = item.UserName,
                    NameSurname = item.NameSurname,
                    PhoneNumber = item.PhoneNumber,
                    UserId = item.Id,
                    PictureUrl = string.IsNullOrWhiteSpace(item.Picture)
                        ? "/userprofilepicture/defaultpp.png"
                        : "/userprofilepicture/" + item.Picture,
                    HasUnreadMessages = hasUnreadMessages
                };

                userViewModels.Add(userViewModel);
            }

            ViewBag.UserNames = userViewModels;

            return View();
        }


        [HttpGet]
        public async Task<IActionResult> ChatApp(string search)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("SignIn", "Home");

            ViewBag.PictureUrl = string.IsNullOrWhiteSpace(currentUser.Picture)
                ? "/userprofilepicture/defaultpp.png"
                : "/userprofilepicture/" + currentUser.Picture;

            ViewBag.GetUserId = currentUser.Id;

            IQueryable<AppUser> query = _context.Users
                .Where(c => c.CompanyId == currentUser.CompanyId && c.Id != currentUser.Id);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.NameSurname.Contains(search));
            }

            var searchPeople = query.ToList();

            List<UserViewModel> userViewModels = new List<UserViewModel>();

            foreach (var item in searchPeople)
            {
                bool hasUnreadMessages = _context.ChatHistories.Any(m =>
                    m.ReceiverId == currentUser.Id &&
                    !m.IsRead &&
                    m.SenderId == item.Id);

                var userViewModel = new UserViewModel
                {
                    Email = item.Email,
                    UserName = item.UserName,
                    NameSurname = item.NameSurname,
                    PhoneNumber = item.PhoneNumber,
                    UserId = item.Id,
                    PictureUrl = string.IsNullOrWhiteSpace(item.Picture)
                        ? "/userprofilepicture/defaultpp.png"
                        : "/userprofilepicture/" + item.Picture,
                    HasUnreadMessages = hasUnreadMessages
                };

                userViewModels.Add(userViewModel);
            }

            ViewBag.UserNames = userViewModels;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetUserMessages(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

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
            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");
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

        [HttpPost]
        public async Task<IActionResult> GetSuggestions([FromBody] List<string> messages)
        {
            if (messages == null)
                return Json(new { success = false, suggestions = new List<string>() });

            var suggestions = await _aiChatService.GenerateReplySuggestionsAsync(messages);

            return Json(new { success = true, suggestions });
        }

        [HttpGet]
        public async Task<IActionResult> GetAiReplySuggestions(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, suggestions = new List<string>() });

            // burada basitçe geçmiş mesajları alıyoruz (örnek)
            var messages = new List<string>
    {
        "Merhaba",
        "Toplantıyı yarına alalım mı?"
    };

            var suggestions = await _aiChatService.GenerateReplySuggestionsAsync(messages);

            return Json(new { success = true, suggestions });
        }

        [HttpPost]
        public async Task<IActionResult> GetAiReplySuggestionsLive(string userId, string draftMessage)
        {
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, suggestions = new List<string>() });

            var messages = new List<string>();

            if (!string.IsNullOrEmpty(draftMessage))
            {
                messages.Add(draftMessage);
            }

            var suggestions = await _aiChatService.GenerateReplySuggestionsAsync(messages);

            return Json(new { success = true, suggestions });
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAiReplySuggestions(string userId)
        //{
        //    var currentUser = await _userManager.GetUserAsync(User);
        //    ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");
        //    if (currentUser == null)
        //        return Json(new { success = false, message = "Kullanıcı bulunamadı." });

        //    if (string.IsNullOrEmpty(userId))
        //        return Json(new { success = false, message = "UserId boş olamaz." });

        //    var messagesSent = _context.ChatHistories
        //        .Where(c => c.SenderId == currentUser.Id && c.ReceiverId == userId)
        //        .OrderBy(c => c.MessageTime)
        //        .ToList();

        //    var messagesReceived = _context.ChatHistories
        //        .Where(c => c.SenderId == userId && c.ReceiverId == currentUser.Id)
        //        .OrderBy(c => c.MessageTime)
        //        .ToList();

        //    var allMessages = messagesSent
        //        .Concat(messagesReceived)
        //        .OrderBy(c => c.MessageTime)
        //        .TakeLast(10)
        //        .ToList();

        //    var suggestions = await _aiChatService.GenerateReplySuggestionsAsync(allMessages, currentUser.Id);

        //    return Json(new
        //    {
        //        success = true,
        //        suggestions = suggestions
        //    });
        //}

        //[HttpPost]
        //public async Task<IActionResult> GetAiReplySuggestionsLive(string userId, string draftMessage)
        //{
        //    var currentUser = await _userManager.GetUserAsync(User);
        //    if (currentUser == null)
        //        return Json(new { success = false, message = "Kullanıcı bulunamadı." });

        //    if (string.IsNullOrEmpty(userId))
        //        return Json(new { success = false, message = "UserId boş olamaz." });

        //    var messagesSent = _context.ChatHistories
        //        .Where(c => c.SenderId == currentUser.Id && c.ReceiverId == userId)
        //        .OrderBy(c => c.MessageTime)
        //        .ToList();

        //    var messagesReceived = _context.ChatHistories
        //        .Where(c => c.SenderId == userId && c.ReceiverId == currentUser.Id)
        //        .OrderBy(c => c.MessageTime)
        //        .ToList();

        //    var allMessages = messagesSent
        //        .Concat(messagesReceived)
        //        .OrderBy(c => c.MessageTime)
        //        .TakeLast(10)
        //        .ToList();

        //    if (!string.IsNullOrWhiteSpace(draftMessage))
        //    {
        //        allMessages.Add(new ChatHistory
        //        {
        //            SenderId = currentUser.Id,
        //            ReceiverId = userId,
        //            Message = draftMessage,
        //            MessageTime = DateTime.Now
        //        });
        //    }

        //    var suggestions = await _aiChatService.GenerateReplySuggestionsAsync(allMessages, currentUser.Id);

        //    return Json(new
        //    {
        //        success = true,
        //        suggestions = suggestions
        //    });
        //}
    }
}



