using System;
using CrmCorner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Exchange.WebServices.Data;
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
            ToDoListDay();
            var todoList = _context.ToDoList
             .Where(e => e.UserId == currentUser.Id)
             .ToList();
             ViewBag.ToDoList = todoList;
             return View();
        }

        [HttpPost]
        public async Task<IActionResult> ToDoListAdd(string selected, string unselected,string maingoals,string title, int itemId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var today = DateTime.Today;
            if (itemId ==0)
            {
                var toDoValue = _context.ToDos
              .Include(e => e.AppUser)
              .Where(e => e.UserId == currentUser.Id && e.SystemDate == today)
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
                    if (toDoValue == 0 && (selected != null || unselected != null))
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
            }
            else
            {
                var toDoValue = _context.ToDoList
               .Where(e => e.UserId == currentUser.Id && e.Title == title)
               .Select(e => e.Id).FirstOrDefault();
                ToDoList toDo = new ToDoList();
                toDo.SystemDate = DateTime.Today;
                toDo.DoneList = selected;
                toDo.NotDoneList = unselected;
                toDo.UserId = currentUser.Id;
                toDo.MainGoalTitle = maingoals;
                toDo.Title = title;
                if (currentUser != null)
                {
                    if (toDoValue == 0 && (selected != null || unselected != null))
                    {
                        _context.ToDoList.Add(toDo);
                        _context.SaveChanges();
                    }
                    else
                    {
                        toDo.Id = toDoValue;
                        _context.ToDoList.Update(toDo);
                        _context.SaveChanges();
                    }
                }
            }
           return Json(new { Message = "success" });
        }
        [HttpPost]
        public async Task<IActionResult> ToDoListAddList(string title)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ToDoList toDolist = new ToDoList();
            toDolist.SystemDate = DateTime.Today;
            toDolist.UserId = currentUser.Id;
            toDolist.Title = title;
            try
            {
                _context.ToDoList.Add(toDolist);
                _context.SaveChanges();
            }
            catch(Exception ex)
            {
           
            }
            return Json(new { Message = "success" });
        }
            
        [HttpPost]
        public async Task<IActionResult> ToDoListDay()
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
                if (todo.Count > 0)
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
                    ViewBag.TitleValue = todo.FirstOrDefault(e => e.UserId == currentUser.Id)?.Title;
                    ViewBag.MainGoalTitle = todo.FirstOrDefault(e => e.UserId == currentUser.Id)?.MainGoalTitle;
                }
                //seçili olanları ve seçili olmayanları ekle
                var jsonData = new
                {
                    MainGoalTitle = ViewBag.MainGoalTitle,
                    Title = ViewBag.TitleValue,
                    TaskData = ViewBag.TaskData,
                    NotTaskData = ViewBag.NotTaskData
                };
                return Json(new { Message = jsonData });
            }
            else
            {
                ViewBag.ErrorMessage = "Geçerli kullanıcı bilgisi bulunamadı.";
                return View();
            }
        }
        [HttpPost]
        public async Task<IActionResult> GetToDoList(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if(currentUser != null)
            {
                var today = DateTime.Today;
                string[] dataArray;
                string[] dataArrays;
                var todo = _context.ToDoList
                .Where(e => e.UserId == currentUser.Id && e.Id == id)
                .ToList();
                if (todo.Count > 0)
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
                    ViewBag.TitleValue = todo.FirstOrDefault(e => e.UserId == currentUser.Id)?.Title;
                    ViewBag.MainGoalTitle = todo.FirstOrDefault(e => e.UserId == currentUser.Id)?.MainGoalTitle;
                }
                var jsonData = new
                {
                    MainGoalTitle = ViewBag.MainGoalTitle,
                    Title = ViewBag.TitleValue,
                    TaskData = ViewBag.TaskData,
                    NotTaskData = ViewBag.NotTaskData
                };
                return Json(new { Message = jsonData });

            }
            else
            {
                ViewBag.ErrorMessage = "Geçerli kullanıcı bilgisi bulunamadı.";
                return View();
            }
        }
        
    }
        
}

