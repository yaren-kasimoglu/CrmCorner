using CrmCorner.Models;
using CrmCorner.Services;
using CrmCorner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CrmCorner.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,TeamLeader,TeamMember")]
    public class ToDoListController : Controller
    {
        private readonly CrmCornerContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly EmailService _emailService;

        public ToDoListController(CrmCornerContext context, UserManager<AppUser> userManager, EmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        // ==========================
        // ANA SAYFA
        // ==========================
        public async Task<IActionResult> ToDoList(int? id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Unauthorized();

            int importantId = await EnsureBoardExists(currentUser.Id, "Önemli");
            int assignId = await EnsureBoardExists(currentUser.Id, "Bana Atananlar");

            if (id == null || id == 0)
            {
                return await LoadDayBoard(currentUser, importantId, assignId);
            }

            return await LoadCustomBoard(currentUser, id.Value, importantId, assignId);
        }

        // ==========================
        // Günüm
        // ==========================
        private async Task<IActionResult> LoadDayBoard(AppUser user, int importantId, int assignId)
        {
            var board = await _context.TodoBoards
                .FirstOrDefaultAsync(x => x.UserId == user.Id && x.Title == "Günüm");

            if (board == null)
            {
                board = new TodoBoard
                {
                    Title = "Günüm",
                    UserId = user.Id,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                _context.TodoBoards.Add(board);
                await _context.SaveChangesAsync();
            }

            var entries = await _context.TodoEntries
                .Include(x => x.AssigneeUser)
                .Include(x => x.AssignedByUser)
                .Where(x => x.TodoBoardId == board.Id)
                .OrderBy(x => x.IsDone)
                .ThenByDescending(x => x.CreatedDate)
                .ToListAsync();

            var model = await BuildPageModel(user.Id, board.Id, board.Title, entries, importantId, assignId);

            var users = await _context.Users
                .Where(u => u.CompanyId == user.CompanyId)
                .ToListAsync();

            ViewBag.Users = users;

            return View("ToDoList", model);
        }

        // ==========================
        // Diğer listeler
        // ==========================
        private async Task<IActionResult> LoadCustomBoard(AppUser user, int boardId, int importantId, int assignId)
        {
            // Bana Atananlar
            if (boardId == assignId)
            {
                var assignedTasks = await _context.TodoEntries
                    .Include(t => t.AssigneeUser)
                    .Include(t => t.AssignedByUser)
                    .Where(t => t.AssigneeId == user.Id)
                    .OrderBy(t => t.IsDone)
                    .ThenByDescending(t => t.CreatedDate)
                    .ToListAsync();

                var assignedModel = await BuildPageModel(
                    user.Id,
                    boardId,
                    "Bana Atananlar",
                    assignedTasks,
                    importantId,
                    assignId
                );

                var assignedUsers = await _context.Users
                    .Where(u => u.CompanyId == user.CompanyId)
                    .ToListAsync();

                ViewBag.Users = assignedUsers;

                return View("ToDoList", assignedModel);
            }

            // Önemli
            if (boardId == importantId)
            {
                var importantTasks = await _context.TodoEntries
                    .Include(t => t.AssigneeUser)
                    .Include(t => t.AssignedByUser)
                    .Where(t => t.UserId == user.Id && t.IsImportant)
                    .OrderBy(t => t.IsDone)
                    .ThenByDescending(t => t.CreatedDate)
                    .ToListAsync();

                var importantModel = await BuildPageModel(
                    user.Id,
                    boardId,
                    "Önemli",
                    importantTasks,
                    importantId,
                    assignId
                );

                var importantUsers = await _context.Users
                    .Where(u => u.CompanyId == user.CompanyId)
                    .ToListAsync();

                ViewBag.Users = importantUsers;

                return View("ToDoList", importantModel);
            }

            // Normal listeler
            var board = await _context.TodoBoards
                .FirstOrDefaultAsync(x => x.Id == boardId && x.UserId == user.Id);

            if (board == null)
                return NotFound();

            var entries = await _context.TodoEntries
                .Include(x => x.AssigneeUser)
                .Include(x => x.AssignedByUser)
                .Where(x => x.TodoBoardId == board.Id)
                .OrderBy(x => x.IsDone)
                .ThenByDescending(x => x.CreatedDate)
                .ToListAsync();

            var model = await BuildPageModel(
                user.Id,
                board.Id,
                board.Title,
                entries,
                importantId,
                assignId
            );

            var users = await _context.Users
                .Where(u => u.CompanyId == user.CompanyId)
                .ToListAsync();

            ViewBag.Users = users;

            return View("ToDoList", model);
        }

        // ==========================
        // ViewModel hazırla
        // ==========================
        private async Task<ToDoPageViewModel> BuildPageModel(
            string userId,
            int currentBoardId,
            string title,
            System.Collections.Generic.List<TodoEntry> entries,
            int importantId,
            int assignId)
        {
            var boards = await _context.TodoBoards
                .Where(x => x.UserId == userId && x.Title != "Önemli" && x.Title != "Bana Atananlar" && x.Title != "Günüm")
                .Select(x => new ToDoBoardViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Count = _context.TodoEntries.Count(e => e.TodoBoardId == x.Id && !e.IsDone)
                })
                .ToListAsync();

            return new ToDoPageViewModel
            {
                CurrentBoardId = currentBoardId,
                Title = title,
                Boards = boards,
                DoneItems = entries.Where(x => x.IsDone).OrderByDescending(x => x.CompletedDate).ToList(),
                NotDoneItems = entries
    .Where(x => !x.IsDone)
    .OrderBy(x => x.Deadline.HasValue ? 0 : 1)
    .ThenBy(x => x.Deadline)
    .ThenBy(x => x.CreatedDate)
    .ToList(),
                ImportantId = importantId,
                AssignListId = assignId
            };
        }

        // ==========================
        // Board yoksa oluştur
        // ==========================
        private async Task<int> EnsureBoardExists(string userId, string title)
        {
            var board = await _context.TodoBoards
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Title == title);

            if (board != null)
                return board.Id;

            board = new TodoBoard
            {
                Title = title,
                UserId = userId,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            _context.TodoBoards.Add(board);
            await _context.SaveChangesAsync();

            return board.Id;
        }

        // ==========================
        // Yeni liste oluştur
        // ==========================
        [HttpPost]
        public async Task<IActionResult> AddBoard(string title)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            if (!string.IsNullOrWhiteSpace(title))
            {
                var board = new TodoBoard
                {
                    Title = title.Trim(),
                    UserId = user.Id,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                _context.TodoBoards.Add(board);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ToDoList");
        }

        // ==========================
        // Görev ekle
        // ==========================
        [HttpPost]
        public async Task<IActionResult> AddEntry(int boardId, string text, DateTime? deadline)
        {
            if (string.IsNullOrWhiteSpace(text))
                return RedirectToAction("ToDoList", new { id = boardId });

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var board = await _context.TodoBoards
                .FirstOrDefaultAsync(x => x.Id == boardId && x.UserId == user.Id);

            if (board == null)
                return NotFound();

            var createdDate = DateTime.Now;
            bool isDayBoard = string.Equals(board.Title, "Günüm", StringComparison.OrdinalIgnoreCase);

            // Günüm dışındaki listelerde geçmiş tarih seçilmesin
            if (!isDayBoard && deadline.HasValue && deadline.Value < createdDate)
            {
                TempData["TodoError"] = "Bitiş tarihi geçmiş bir zaman olamaz.";
                return RedirectToAction("ToDoList", new { id = boardId });
            }

            var entry = new TodoEntry
            {
                TodoBoardId = boardId,
                UserId = user.Id,
                Text = text.Trim(),
                IsDone = false,
                CreatedDate = createdDate,
                IsDayBoardTask = isDayBoard,
                IsImportant = false,

                // Günüm için eski test mantığı devam
                ExpiresAt = isDayBoard ? createdDate.AddMinutes(2) : (DateTime?)null,
                ExpirationWarningSent = false,

                // Normal listeler için deadline
                Deadline = isDayBoard ? null : deadline,

                DeadlineReminderWeekSent = false,
                DeadlineReminder3DaysSent = false,
                DeadlineReminderLastDaySent = false,
                DeadlineReminder2HoursSent = false
            };

            _context.TodoEntries.Add(entry);
            await _context.SaveChangesAsync();

            return RedirectToAction("ToDoList", new { id = boardId });
        }

        // ==========================
        // Görev tamamlandı / geri al
        // ==========================

        [HttpPost]
        public async Task<IActionResult> ToggleEntry(int id, int boardId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Unauthorized();

            var entry = await _context.TodoEntries
                .Include(x => x.TodoBoard)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entry == null)
                return NotFound();

            bool canManage =
                entry.UserId == currentUser.Id ||
                (entry.TodoBoard != null && entry.TodoBoard.UserId == currentUser.Id) ||
                entry.AssigneeId == currentUser.Id ||
                entry.AssignedById == currentUser.Id;

            if (!canManage)
                return Unauthorized();

            entry.IsDone = !entry.IsDone;
            entry.CompletedDate = entry.IsDone ? DateTime.Now : null;

            _context.TodoEntries.Update(entry);
            await _context.SaveChangesAsync();

            return RedirectToAction("ToDoList", new { id = boardId });
        }

        // ==========================
        // Görev sil
        // ==========================

        [HttpPost]
        public async Task<IActionResult> DeleteEntry(int id, int boardId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Unauthorized();

            var entry = await _context.TodoEntries
                .Include(x => x.TodoBoard)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entry == null)
                return RedirectToAction("ToDoList", new { id = boardId });

            bool canManage =
                entry.UserId == currentUser.Id ||
                (entry.TodoBoard != null && entry.TodoBoard.UserId == currentUser.Id) ||
                entry.AssigneeId == currentUser.Id ||
                entry.AssignedById == currentUser.Id;

            if (!canManage)
                return Unauthorized();

            _context.TodoEntries.Remove(entry);
            await _context.SaveChangesAsync();

            return RedirectToAction("ToDoList", new { id = boardId });
        }

        // ==========================
        // Görev ata
        // ==========================

        [HttpPost]
        public async Task<IActionResult> AssignTask(int taskId, string assigneeUserId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Unauthorized();

            var assignee = await _userManager.FindByIdAsync(assigneeUserId);
            if (assignee == null)
                return NotFound(new { message = "Kullanıcı bulunamadı." });

            if (assignee.CompanyId != currentUser.CompanyId)
                return BadRequest(new { message = "Sadece aynı şirketteki kullanıcıya görev atanabilir." });

            var task = await _context.TodoEntries
                .Include(x => x.TodoBoard)
                .FirstOrDefaultAsync(x => x.Id == taskId);

            if (task == null)
                return NotFound(new { message = "Görev bulunamadı." });

            if (task.UserId != currentUser.Id && (task.TodoBoard == null || task.TodoBoard.UserId != currentUser.Id))
                return Unauthorized();

            task.AssigneeId = assignee.Id;
            task.AssignedById = currentUser.Id;

            await _context.SaveChangesAsync();

            var taskUrl = Url.Action(
                "ToDoList",
                "ToDoList",
                null,
                Request.Scheme);

            if (!string.IsNullOrWhiteSpace(assignee.Email))
            {
                var assignedByName = !string.IsNullOrWhiteSpace(currentUser.NameSurname)
                    ? currentUser.NameSurname
                    : currentUser.UserName;

                await _emailService.SendTaskAssignedEmailAsync(
                    assignee.Email,
                    assignedByName,
                    task.Text,
                    taskUrl
                );
            }

            return Ok(new { message = "Görev başarıyla atandı." });
        }

        // ==========================
        // Önemli işaretle / kaldır
        // ==========================


        [HttpPost]
        public async Task<IActionResult> ToggleImportant(int id, int boardId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Unauthorized();

            var entry = await _context.TodoEntries
                .Include(x => x.TodoBoard)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entry == null)
                return NotFound();

            bool canManage =
                entry.UserId == currentUser.Id ||
                (entry.TodoBoard != null && entry.TodoBoard.UserId == currentUser.Id) ||
                entry.AssigneeId == currentUser.Id ||
                entry.AssignedById == currentUser.Id;

            if (!canManage)
                return Unauthorized();

            entry.IsImportant = !entry.IsImportant;

            _context.TodoEntries.Update(entry);
            await _context.SaveChangesAsync();

            return RedirectToAction("ToDoList", new { id = boardId });
        }


        [HttpPost]
        public async Task<IActionResult> DeleteBoard(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var board = await _context.TodoBoards
                .Include(x => x.Entries)
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == user.Id);

            if (board == null)
                return NotFound();

            // Sistem listeleri silinmesin
            if (board.Title == "Günüm" || board.Title == "Önemli" || board.Title == "Bana Atananlar")
            {
                TempData["TodoError"] = "Bu liste silinemez.";
                return RedirectToAction("ToDoList");
            }

            if (board.Entries != null && board.Entries.Any())
            {
                _context.TodoEntries.RemoveRange(board.Entries);
            }

            _context.TodoBoards.Remove(board);
            await _context.SaveChangesAsync();

            return RedirectToAction("ToDoList");
        }
    }
}