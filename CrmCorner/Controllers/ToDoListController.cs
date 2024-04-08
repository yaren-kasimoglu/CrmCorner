using System;
using System.Linq;
using CrmCorner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph;
using MySqlX.XDevAPI.Relational;
using static System.Net.Mime.MediaTypeNames;

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
            string[] dataArray;

            var currentUser = await _userManager.GetUserAsync(User);
            ToDoListDay();
            var todoList = _context.ToDoList
             .Where(e => e.UserId == currentUser.Id)
             .ToList();
            int titleCounts=0;

            foreach (var item in todoList)
            {
                var title = item.Title;
                if(title.Length>7)
                    item.Title = title.Substring(0, 7)+"...";
                var unselected = todoList.FirstOrDefault(e => e.Title == title)?.NotDoneList;
                if (unselected != null)
                {
                    dataArray = unselected.Split(',');
                    var count = dataArray.Length;
                    if (count != null)
                    {
                        item.Count = (int)count;
                    }
                }
                else
                    item.Count = 0;
                dataArray =null; 
            }
            ViewBag.ToDoList = todoList;
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ToDoListAdd(string selected, string unselected,string maingoals,string title, int itemId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var today = DateTime.Today;
            if (itemId ==1)
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
            var toDoValue = _context.ToDoList
               .Where(e => e.UserId == currentUser.Id && e.Title == title)
               .Select(e => e.Id).FirstOrDefault();
            if (toDoValue == 0)
            {
               _context.ToDoList.Add(toDolist);
               _context.SaveChanges();
              
            }
            else
            {
                _context.ToDoList.Update(toDolist);
                _context.SaveChanges();
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
                    NotTaskData = ViewBag.NotTaskData,
                    Count= todo.Count
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
        public async Task<IActionResult> DeleteToDo(string selectedItem, string title,int itemId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var today = DateTime.Today;
            string[] splittedselected = null;
            string[] splittedunselected = null;
            string[] resultselected = null;
            string[] resultunselected = null;
            if (itemId == 1)
            {
                var toDoValue = _context.ToDos.AsNoTracking()
              .Include(e => e.AppUser)
              .Where(e => e.UserId == currentUser.Id && e.SystemDate == today).FirstOrDefault();
                if (toDoValue.DoneList != null)
                {
                    splittedselected = toDoValue.DoneList.Split(',');
                    resultselected = RemoveItemFromArray(splittedselected, selectedItem);
                }
                if (toDoValue.NotDoneList != null)
                {
                    splittedunselected = toDoValue.NotDoneList.Split(',');
                    resultunselected = RemoveItemFromArray(splittedunselected, selectedItem);
                }
                ToDo toDo = new ToDo();
                toDo.SystemDate = DateTime.Today;
                toDo.DoneList = resultselected!=null ? string.Join(",", resultselected) : toDoValue.DoneList;
                toDo.NotDoneList = resultunselected!=null ? string.Join(",", resultunselected) : toDoValue.NotDoneList;
                toDo.UserId = currentUser.Id;
                toDo.MainGoalTitle = toDoValue.MainGoalTitle;
                toDo.Title = title;
                try
                {
                    toDo.Id = toDoValue.Id;
                    _context.ToDos.Update(toDo);
                    _context.SaveChanges();
                }
                catch(Exception ex)
                {
                    Console.Write(ex.Message);
                }
            }
            else
            {
                var toDoValue = _context.ToDoList.AsNoTracking()
                .Where(e => e.UserId == currentUser.Id && e.Id == itemId)
               .FirstOrDefault();
                splittedselected = toDoValue.DoneList!=null? toDoValue.DoneList.Split(','): null;
                splittedunselected = toDoValue.NotDoneList!=null? toDoValue.NotDoneList.Split(','):null;
                if(splittedselected!=null)
                    resultselected = RemoveItemFromArray(splittedselected, selectedItem);
                if (splittedunselected != null)
                    resultunselected = RemoveItemFromArray(splittedunselected, selectedItem);
                ToDoList toDo = new ToDoList();
                toDo.SystemDate = DateTime.Today;
                toDo.DoneList = resultselected.Count() >0? string.Join(",", resultselected) : toDoValue.DoneList == "" ? null : toDoValue.DoneList; 
                toDo.NotDoneList = resultunselected.Count()>0 ? string.Join(",", resultunselected) : toDoValue.NotDoneList==""?null: toDoValue.NotDoneList; 
                toDo.UserId = currentUser.Id;
                toDo.MainGoalTitle = toDoValue.MainGoalTitle; 
                toDo.Title = title;
                toDo.Id = itemId;
                try
                {
                    _context.ToDoList.Update(toDo);
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                }

            }
                return Json(new { Message = "success" });
        }
        static string[] RemoveItemFromArray(string[] array, string item)
        {
            item = item == null ? "" : item;
            if (!array.Contains(item))
                return array;
            // Verilen öğeyi içermeyen yeni bir dizi oluştur
            string[] newArray = new string[array.Length - 1];
            int index = 0;

            foreach (string element in array)
            {
                if (element != item)
                {
                    newArray[index] = element;
                    index++;
                }
            }
            return newArray;
        }
    }
        
}

