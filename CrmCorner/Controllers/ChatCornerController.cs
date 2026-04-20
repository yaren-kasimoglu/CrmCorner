using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CrmCorner.Models.ChatCorner;
using CrmCorner.Services.ChatCorner;
using CrmCorner.Models;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    [Authorize]
    public class ChatCornerController : Controller
    {
        private readonly IChatCornerService _chatCornerService;
        private readonly CrmCornerContext _context;

        public ChatCornerController(IChatCornerService chatCornerService, CrmCornerContext context)
        {
            _chatCornerService = chatCornerService;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] ChatCornerQuestionRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Question))
            {
                return Json(new ChatCornerResponseDto
                {
                    Success = false,
                    ErrorMessage = "Soru boş olamaz."
                });
            }

            var result = await _chatCornerService.HandleQuestionAsync(request.Question, User);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string q)
        {
            q = (q ?? "").Trim().ToLower();

            if (string.IsNullOrWhiteSpace(q))
                return Json(new List<object>());

            var users = await _context.Users
                .Where(x =>
                    (x.Email != null && x.Email.ToLower().Contains(q)) ||
                    (x.NameSurname != null && x.NameSurname.ToLower().Contains(q)))
                .Select(x => new
                {
                    id = x.Id,
                    name = x.NameSurname,
                    email = x.Email
                })
                .Take(8)
                .ToListAsync();

            return Json(users);
        }
    }
}