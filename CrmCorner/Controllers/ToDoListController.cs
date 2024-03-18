using System;
using CrmCorner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph;

namespace CrmCorner.Controllers
{
    public class ToDoListController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly CrmCornerContext _context;
        public ToDoListController(CrmCornerContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> ToDoList()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var today = DateTime.Today;
            string[] dataArray;
            string[] dataArrays;
            if (currentUser != null)
            {
               var todo = _context.ToDos
            .Where(e => e.UserId == currentUser.Id && e.SystemDate == today)
            .ToList();
                if (todo.Count>0)
                {
                    var selected = todo.FirstOrDefault(e => e.UserId == currentUser.Id)?.DoneList;
                    var unselected = todo.FirstOrDefault(e => e.UserId == currentUser.Id)?.NotDoneList;
                    if (selected != null)
                    {
                        dataArray = selected.Split(',');
                        ViewBag.TaskData = dataArray;
                    }
                    if (unselected != null)
                    {
                        dataArrays = unselected.Split(',');
                        ViewBag.NotTaskData = dataArrays;
                    }
                }
                //seçili olanları ve seçili olmayanları ekle
                return View(todo);
            }
            else
            {
                ViewBag.ErrorMessage = "Geçerli kullanıcı bilgisi bulunamadı.";
                return View();
            }

        }

        [HttpPost]
        public async Task<IActionResult> ToDoListAdd(string selected, string unselected,string maingoals,string title)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var today = DateTime.Today;
            var toDoValue = _context.ToDos
           .Include(e => e.AppUser)
           .Where(e => e.UserId == currentUser.Id && e.SystemDate==today)
           .Select(e => e.Id).FirstOrDefault();
            ToDo toDo = new ToDo();
            toDo.SystemDate = DateTime.Today;
            toDo.DoneList = selected;
            toDo.NotDoneList = unselected;
            toDo.UserId = currentUser.Id;
            toDo.MainGoalTitle = maingoals;
            toDo.Title = title;
            if (currentUser != null)
            {
                if (toDoValue == 0)
                {
                     _context.ToDos.Add(toDo);
                     _context.SaveChanges();
                }
                else
                {
                    toDo.Id = toDoValue;
                    _context.ToDos.Update(toDo);
                    _context.SaveChanges();
                }
            }
           return Json(new { Message = "success" });
            
         
        }
    }
        
}

