using CrmCorner.Models;
using CrmCorner.Services;
using CrmCorner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,TeamLeader,TeamMember")]
    public class ChatAppController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly CrmCornerContext _context;
        private readonly AiChatService _aiChatService;

        public ChatAppController(
            CrmCornerContext context,
            UserManager<AppUser> userManager,
            AiChatService aiChatService)
        {
            _context = context;
            _userManager = userManager;
            _aiChatService = aiChatService;
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
            ViewBag.UserNames = await GetUserListAsync(currentUser, search);

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetUserMessages(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || string.IsNullOrWhiteSpace(userId))
            {
                return Json(new { Message = new List<ChatHistory>() });
            }

            ViewBag.PictureUrl = string.IsNullOrWhiteSpace(currentUser.Picture)
                ? "/userprofilepicture/defaultpp.png"
                : "/userprofilepicture/" + currentUser.Picture;

            var allMessages = await _context.ChatHistories
                .Where(x =>
                    (x.SenderId == currentUser.Id && x.ReceiverId == userId) ||
                    (x.SenderId == userId && x.ReceiverId == currentUser.Id))
                .OrderBy(x => x.MessageTime)
                .ToListAsync();

            var notReadMessages = allMessages
                .Where(x => x.ReceiverId == currentUser.Id && !x.IsRead)
                .ToList();

            if (notReadMessages.Any())
            {
                foreach (var message in notReadMessages)
                {
                    message.IsRead = true;
                }

                _context.UpdateRange(notReadMessages);
                await _context.SaveChangesAsync();

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = false,
                    Expires = DateTime.Now.AddDays(1),
                    Path = "/"
                };

                Response.Cookies.Append("HasUnreadMessages", "false", cookieOptions);
            }

            ViewBag.GetUserMessage = allMessages;
            ViewBag.GetUserId = currentUser.Id;

            return Json(new { Message = allMessages });
        }

        [HttpPost]
        public async Task<IActionResult> GetUserName(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || string.IsNullOrWhiteSpace(userId))
            {
                return Json(new { Message = "User not found" });
            }

            var peopleName = await _context.Users
                .AsNoTracking()
                .Where(x => x.CompanyId == currentUser.CompanyId && x.Id == userId)
                .Select(x => x.NameSurname)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(peopleName))
            {
                return Json(new { Message = "User not found" });
            }

            return Json(new { Message = peopleName });
        }

        [HttpPost]
        public async Task<IActionResult> GetSuggestions([FromBody] List<string> messages)
        {
            if (messages == null || !messages.Any())
            {
                return Json(new { success = false, suggestions = new List<string>() });
            }

            var suggestions = await _aiChatService.GenerateReplySuggestionsAsync(messages);
            return Json(new { success = true, suggestions });
        }

        [HttpGet]
        public async Task<IActionResult> GetAiReplySuggestions(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null || string.IsNullOrWhiteSpace(userId))
            {
                return Json(new { success = false, suggestions = new List<string>() });
            }

            var messages = await _context.ChatHistories
                .AsNoTracking()
                .Where(x =>
                    (x.SenderId == currentUser.Id && x.ReceiverId == userId) ||
                    (x.SenderId == userId && x.ReceiverId == currentUser.Id))
                .OrderByDescending(x => x.MessageTime)
                .Take(10)
                .OrderBy(x => x.MessageTime)
                .Select(x => x.Message)
                .ToListAsync();

            var suggestions = await _aiChatService.GenerateReplySuggestionsAsync(messages);

            return Json(new { success = true, suggestions });
        }

        [HttpPost]
        public async Task<IActionResult> GetAiReplySuggestionsLive(string userId, string draftMessage)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null || string.IsNullOrWhiteSpace(userId))
            {
                return Json(new { success = false, suggestions = new List<string>() });
            }

            var messages = await _context.ChatHistories
                .AsNoTracking()
                .Where(x =>
                    (x.SenderId == currentUser.Id && x.ReceiverId == userId) ||
                    (x.SenderId == userId && x.ReceiverId == currentUser.Id))
                .OrderByDescending(x => x.MessageTime)
                .Take(10)
                .OrderBy(x => x.MessageTime)
                .Select(x => x.Message)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(draftMessage))
            {
                messages.Add(draftMessage.Trim());
            }

            var suggestions = await _aiChatService.GenerateReplySuggestionsAsync(messages);

            return Json(new { success = true, suggestions });
        }

        private async Task<List<UserViewModel>> GetUserListAsync(AppUser currentUser, string? search)
        {
            var query = _context.Users
                .AsNoTracking()
                .Where(x => x.CompanyId == currentUser.CompanyId && x.Id != currentUser.Id);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x => x.NameSurname.Contains(search));
            }

            var users = await query
                .OrderBy(x => x.NameSurname)
                .ToListAsync();

            var userIds = users.Select(x => x.Id).ToList();

            var unreadSenderIds = await _context.ChatHistories
                .AsNoTracking()
                .Where(x =>
                    x.ReceiverId == currentUser.Id &&
                    !x.IsRead &&
                    userIds.Contains(x.SenderId))
                .Select(x => x.SenderId)
                .Distinct()
                .ToListAsync();

            var result = users.Select(item => new UserViewModel
            {
                Email = item.Email,
                UserName = item.UserName,
                NameSurname = item.NameSurname,
                PhoneNumber = item.PhoneNumber,
                UserId = item.Id,
                PictureUrl = string.IsNullOrWhiteSpace(item.Picture)
                    ? "/userprofilepicture/defaultpp.png"
                    : "/userprofilepicture/" + item.Picture,
                HasUnreadMessages = unreadSenderIds.Contains(item.Id)
            }).ToList();

            return result;
        }
    }
}