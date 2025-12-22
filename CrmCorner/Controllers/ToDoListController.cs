using System;
using System.Linq;
using System.Threading.Tasks;
using CrmCorner.Models;
using CrmCorner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,TeamLeader,TeamMember")]
    public class ToDoListController : Controller
    {
        private readonly CrmCornerContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ToDoListController(CrmCornerContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================
        // ANA SAYFA
        // ==========================
        public async Task<IActionResult> ToDoList(int? id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            // Önemli ve Bana Atananlar yoksa oluştur
            int importantId = await EnsureBoardExists(currentUser.Id, "Önemli");
            int assignId = await EnsureBoardExists(currentUser.Id, "Bana Atananlar");

            if (id == null || id == 0)
            {
                return await LoadDayBoard(currentUser, importantId, assignId);
            }

            return await LoadCustomBoard(currentUser, id.Value, importantId, assignId);
        }

        // ==========================
        // Günüm (id = 0)
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
                .Where(x => x.TodoBoardId == board.Id)
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
            // 🟣 Eğer "Bana Atananlar" listesi açılıyorsa özel filtre çalışacak
            if (boardId == assignId)
            {
                var assignedTasks = await _context.TodoEntries
                    .Where(t => t.AssigneeId == user.Id)   // Sadece bana atananlar
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

                // Kullanıcı listesi (Assign popup için)
                var assignedUsers = await _context.Users
                    .Where(u => u.CompanyId == user.CompanyId)
                    .Select(u => new { u.Id, u.NameSurname })
                    .ToListAsync();

                ViewBag.Users = assignedUsers;

                return View("ToDoList", assignedModel);
            }

            // 🟢 Normal listeler (Günüm / Önemli / Diğer Boardlar)
            var board = await _context.TodoBoards
                .FirstOrDefaultAsync(x => x.Id == boardId && x.UserId == user.Id);

            if (board == null)
                return NotFound();

            var entries = await _context.TodoEntries
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

            // Kullanıcı listesi (Assign popup için)
            var users = await _context.Users
                .Where(u => u.CompanyId == user.CompanyId)
                .Select(u => new { u.Id, u.NameSurname })
                .ToListAsync();

            ViewBag.Users = users;

            return View("ToDoList", model);
        }


        // ==========================
        // Sayfa view modelini hazırla
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
                .Where(x => x.UserId == userId && x.Title != "Önemli" && x.Title != "Bana Atananlar")
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
                NotDoneItems = entries.Where(x => !x.IsDone).OrderBy(x => x.CreatedDate).ToList(),
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
            if (user == null) return Unauthorized();

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
        public async Task<IActionResult> AddEntry(int boardId, string text, DateTime? date)
        {
            if (string.IsNullOrWhiteSpace(text))
                return RedirectToAction("ToDoList", new { id = boardId });

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var entry = new TodoEntry
            {
                TodoBoardId = boardId,
                UserId = user.Id,
                Text = text.Trim(),
                IsDone = false,
                CreatedDate = date ?? DateTime.Now
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
            var entry = await _context.TodoEntries.FindAsync(id);
            if (entry == null) return NotFound();

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
            var entry = await _context.TodoEntries.FindAsync(id);
            if (entry != null)
            {
                _context.TodoEntries.Remove(entry);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ToDoList", new { id = boardId });
        }

        [HttpPost]
        public async Task<IActionResult> AssignTask(int taskId, string assigneeUserId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Unauthorized();

            // 1) Atanacak kişiyi bul
            var assignee = await _userManager.FindByIdAsync(assigneeUserId);
            if (assignee == null)
                return NotFound(new { message = "Kullanıcı bulunamadı." });

            // 2) Görevi bul
            var task = await _context.TodoEntries.FirstOrDefaultAsync(x => x.Id == taskId);
            if (task == null)
                return NotFound(new { message = "Görev bulunamadı." });

            // 3) Alanları doldur
            task.AssigneeId = assignee.Id;         // Kime atandı
            task.AssignedById = currentUser.Id;    // Kim atadı

            await _context.SaveChangesAsync();

            return Ok(new { message = "Görev başarıyla atandı." });
        }



    }
}
