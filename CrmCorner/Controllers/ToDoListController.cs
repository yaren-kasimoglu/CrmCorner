using System;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text.RegularExpressions;
using CrmCorner.Models;
using CrmCorner.ViewModels;
using Independentsoft.Graph.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph;
using Microsoft.Office.Interop.Outlook;
using MySqlX.XDevAPI.Relational;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using Exception = System.Exception;

namespace CrmCorner.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,TeamLeader,TeamMember")]
    public class ToDoListController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly CrmCornerContext _context;
        public ToDoListController(CrmCornerContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        private bool isFirstLoad = true;
        public async Task<IActionResult> ToDoList(int? id)
        {
            string[] dataArray;
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

            if (id.HasValue && id > 0)
            {
                GetToDoList(id.Value);
            }
            else
            {
                ToDoListDay();
            }

            var todoList = _context.ToDoList
             .Where(e => e.UserId == currentUser.Id && e.Title != "Önemli" && e.Title!="Bana Atananlar")
             .ToList();
            int titleCounts = 0;

            foreach (var item in todoList)
            {
                var title = item.Title;
                item.Title = char.ToUpper(title[0]) + title.Substring(1).ToLower();
                if (title.Length > 20)
                {
                    item.Title = title.Substring(0, 7) + "...";
                }

                var unselected = todoList.FirstOrDefault(e => e.Id == item.Id)?.NotDoneList;
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
                dataArray = null;
            }
            ViewBag.ToDoList = todoList;
            #region Önemli listesi
            var importantlistId = _context.ToDoList.AsNoTracking()
              .Where(e => e.UserId == currentUser.Id && e.Title == "Önemli")
              .OrderByDescending(e => e.SystemDate).Select(e => e.Id).FirstOrDefault();

            if (importantlistId != null && importantlistId > 0)
            {
                ViewBag.ImportantId = importantlistId;
            }
            else
            {
                ToDoList toDolist = new ToDoList();
                toDolist.SystemDate = DateTime.Today;
                toDolist.UserId = currentUser.Id;
                toDolist.Title = "Önemli";
                _context.ToDoList.Add(toDolist);
                _context.SaveChanges();
            }
            #endregion
            #region Önemli listesi
            var assignlistId = _context.ToDoList.AsNoTracking()
              .Where(e => e.UserId == currentUser.Id && e.Title == "Bana Atananlar")
              .OrderByDescending(e => e.SystemDate).Select(e => e.Id).FirstOrDefault();

            if (assignlistId != null && assignlistId > 0)
            {
                ViewBag.AssignListId = assignlistId;
            }
            else
            {
                ToDoList toDolist = new ToDoList();
                toDolist.SystemDate = DateTime.Today;
                toDolist.UserId = currentUser.Id;
                toDolist.Title = "Bana Atananlar";
                _context.ToDoList.Add(toDolist);
                _context.SaveChanges();
            }
            #endregion
            return View();

            // Eğer istek AJAX isteği değilse, yönlendirme gerçekleştir
        }

        [HttpPost]
        public async Task<IActionResult> ToDoListAdd(string textValue, bool isChecked, int id,DateTime selectedDate)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var today = DateTime.Today;
            selectedDate= selectedDate == default(DateTime)? DateTime.Today: selectedDate;
            if (id == 0)
            {
                ToDo toDo = new ToDo();
                toDo.UpdateSystemDate = DateTime.Today;
                var toDoValue = _context.ToDos.AsNoTracking()
                      .Include(e => e.AppUser)
                      .Where(e => e.UserId == currentUser.Id && e.SystemDate == today)
                      .FirstOrDefault();
                string originalString = "";
                if (isChecked)
                {
                    originalString = toDoValue != null ? toDoValue.DoneList : null;
                    string duzeltilmisDeger = TrimCommas(originalString + "," + textValue);
                    toDo.DoneList = duzeltilmisDeger;
                    toDo.NotDoneList = toDoValue != null ? toDoValue.NotDoneList : null;
                }
                else
                {
                    originalString = toDoValue != null ? toDoValue.NotDoneList : null;
                    string duzeltilmisDeger = TrimCommas(originalString + "," + textValue);
                    toDo.NotDoneList = duzeltilmisDeger;
                    toDo.DoneList = toDoValue != null ? toDoValue.DoneList : null;
                }
                toDo.UserId = currentUser.Id;
                toDo.MainGoalTitle = "";
                toDo.Title = "Günüm";
                toDo.CreatedDate = selectedDate;
                if (currentUser != null)
                {

                    if (toDoValue == null || toDoValue.Id == 0)
                    {
                        toDo.SystemDate = today;
                        _context.ToDos.Add(toDo);
                        _context.SaveChanges();
                    }
                    else
                    {
                        toDo.SystemDate = toDoValue.SystemDate;
                        toDo.Id = toDoValue.Id;
                        _context.ToDos.Update(toDo);
                        _context.SaveChanges();
                    }
                }
            }
            else
            {
                var toDoValue = _context.ToDoList.AsNoTracking()
                   .Where(e => e.UserId == currentUser.Id && e.Id == id)
                   .FirstOrDefault();
                ToDoList toDo = new ToDoList();
                toDo.UpdateSystemDate = DateTime.Today;
                string originalString = "";
                if (isChecked)
                {
                    originalString = toDoValue != null ? toDoValue.DoneList : null;
                    string duzeltilmisDeger = TrimCommas(originalString + "," + textValue);
                    toDo.DoneList = duzeltilmisDeger;
                    toDo.NotDoneList = toDoValue != null ? toDoValue.NotDoneList : null;
                }
                else
                {
                    originalString = toDoValue != null ? toDoValue.NotDoneList : null;
                    string duzeltilmisDeger = TrimCommas(originalString + "," + textValue);
                    toDo.NotDoneList = duzeltilmisDeger;
                    toDo.DoneList = toDoValue != null ? toDoValue.DoneList : null;
                }
                toDo.UserId = currentUser.Id;
                toDo.MainGoalTitle = "";
                toDo.Title = toDoValue.Title;
                toDo.CreatedDate = selectedDate;

                if (currentUser != null)
                {
                    if (toDoValue.Id == 0)
                    {
                        toDo.SystemDate = today;
                        _context.ToDoList.Add(toDo);
                        _context.SaveChanges();
                    }
                    else
                    {
                        toDo.Id = id;
                        toDo.SystemDate = toDoValue.SystemDate;
                        _context.ToDoList.Update(toDo);
                        _context.SaveChanges();
                    }
                }
            }

            return RedirectToAction("ToDoList", new { id = id });
        }
        [HttpPost]
        public async Task<IActionResult> ToDoListAddList(string title, int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ToDoList toDolist = new ToDoList();
            toDolist.SystemDate = DateTime.Today;
            toDolist.UserId = currentUser.Id;
            toDolist.Title = title;
            _context.ToDoList.Add(toDolist);
            _context.SaveChanges();
            return RedirectToAction("ToDoList", new { id = id });
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
                var todo = _context.ToDos.AsNoTracking()
             .Where(e => e.UserId == currentUser.Id && e.SystemDate == today)
             .FirstOrDefault();
                if (todo != null)
                {
                    var selected = todo.DoneList != null ? todo.DoneList : null;
                    var unselected = todo.NotDoneList != null ? todo.NotDoneList : null;
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
                ViewBag.TitleValue = "Günüm";
                //seçili olanları ve seçili olmayanları ekle
                var jsonData = new
                {
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
        [HttpGet]
        public async Task<IActionResult> GetToDoList(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                var today = DateTime.Today;
                string[] dataArray;
                string[] dataArrays;
                var todo = _context.ToDoList.AsNoTracking()
                .Where(e => e.UserId == currentUser.Id && e.Id == id)
                .ToList();
                if (todo.Count > 0)
                {
                    var selected = todo.FirstOrDefault(e => e.UserId == currentUser.Id)?.DoneList;
                    var unselected = todo.FirstOrDefault(e => e.UserId == currentUser.Id)?.NotDoneList;
                    //var assigment = todo.FirstOrDefault(e => e.UserId == currentUser.Id)?.AssigmentTo;
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
                    //if (assigment != null)
                    //{
                    //    ViewBag.Assigment = assigment;
                    //}
                    ViewBag.TitleValue = todo.FirstOrDefault(e => e.UserId == currentUser.Id)?.Title;
                    ViewBag.Date = todo.FirstOrDefault(e => e.UserId == currentUser.Id)?.SystemDate.ToString();
                }
                var jsonData = new
                {
                    Title = ViewBag.TitleValue,
                    TaskData = ViewBag.TaskData,
                    NotTaskData = ViewBag.NotTaskData,
                    Count = todo.Count,
                   // Assigment= ViewBag.Assigment,
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
        public async Task<IActionResult> DeleteToDo(string textContent, int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var today = DateTime.Today;
            string[] splittedselected = null;
            string[] splittedunselected = null;
            string[] resultselected = null;
            string[] resultunselected = null;
            if (id == 0)
            {
                var toDoValue = _context.ToDos.AsNoTracking()
              .Include(e => e.AppUser)
              .Where(e => e.UserId == currentUser.Id && e.SystemDate == today).FirstOrDefault();
                if (toDoValue.DoneList != null)
                {
                    splittedselected = toDoValue.DoneList.Split(',');
                    resultselected = RemoveItemFromArray(splittedselected, textContent);
                }
                else
                    resultselected = new string[0];
                if (toDoValue.NotDoneList != null)
                {
                    splittedunselected = toDoValue.NotDoneList.Split(',');
                    resultunselected = RemoveItemFromArray(splittedunselected, textContent);
                }
                else
                    resultunselected = new string[0];

                ToDo toDo = new ToDo();
                toDo.SystemDate = DateTime.Today;
                toDo.DoneList = resultselected.Count() > 0 && resultselected != null ? string.Join(",", resultselected) : toDoValue.DoneList == "" || resultselected.Length == 0 ? null : toDoValue.DoneList;
                toDo.NotDoneList = resultunselected.Count() > 0 && resultunselected != null ? string.Join(",", resultunselected) : toDoValue.NotDoneList == "" || resultunselected.Length == 0 ? null : toDoValue.NotDoneList;
                toDo.UserId = currentUser.Id;
                toDo.MainGoalTitle = "";
                toDo.Title = toDoValue.Title;
                try
                {
                    toDo.Id = toDoValue.Id;
                    _context.ToDos.Update(toDo);
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                }
            }
            else
            {
                var toDoValue = _context.ToDoList.AsNoTracking()
                .Where(e => e.UserId == currentUser.Id && e.Id == id)
               .FirstOrDefault();
                splittedselected = toDoValue.DoneList != null ? toDoValue.DoneList.Split(',') : null;
                splittedunselected = toDoValue.NotDoneList != null ? toDoValue.NotDoneList.Split(',') : null;
                if (splittedselected != null)
                    resultselected = RemoveItemFromArray(splittedselected, textContent);
                else
                    resultselected = new string[0];
                if (splittedunselected != null)
                    resultunselected = RemoveItemFromArray(splittedunselected, textContent);
                else
                    resultunselected = new string[0];
                ToDoList toDo = new ToDoList();
                toDo.SystemDate = DateTime.Today;
                toDo.DoneList = resultselected.Count() > 0 && resultselected != null ? string.Join(",", resultselected) : toDoValue.DoneList == "" || resultselected.Length == 0 ? null : toDoValue.DoneList;
                toDo.NotDoneList = resultunselected.Count() > 0 && resultunselected != null ? string.Join(",", resultunselected) : toDoValue.NotDoneList == "" || resultunselected.Length == 0 ? null : toDoValue.NotDoneList;
                toDo.UserId = currentUser.Id;
                toDo.MainGoalTitle = "";
                toDo.Title = toDoValue.Title;
                toDo.Id = id;
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
            return RedirectToAction("ToDoList", new { id = id });
        }


        [HttpPost]
        public async Task<IActionResult> DeleteToDoList(int id)
        {
            ToDoList todoList = _context.ToDoList.Find(id);

            // Eğer müşteri bulunamazsa
            if (todoList == null)
            {
                return NotFound();
            }
            _context.ToDoList.Remove(todoList);
            _context.SaveChanges();
            return RedirectToAction("ToDoList", new { id = 0 });

        }


        [HttpPost]
        public async Task<IActionResult> UpdateToDo(int id, string textContent, bool checkbox)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var today = DateTime.Today;
            string[] splittedselected = null;
            string[] splittedunselected = null;
            string[] resultselected = null;
            string[] resultunselected = null;
            if (textContent != null)
            {
                if (checkbox)
                {
                    if (id > 0)
                    {
                        var toDoValue = _context.ToDoList.AsNoTracking()
                        .Where(e => e.UserId == currentUser.Id && e.Id == id)
                        .FirstOrDefault();

                        ToDoList toDo = new ToDoList();
                        toDo.SystemDate = toDoValue.SystemDate;
                        toDo.UpdateSystemDate= DateTime.Today;
                        toDo.UserId = currentUser.Id;
                        toDo.MainGoalTitle = "";
                        toDo.Title = toDoValue.Title;
                        toDo.Id = id;

                        splittedunselected = toDoValue.NotDoneList != null ? toDoValue.NotDoneList.Split(',') : null;
                        if (splittedunselected != null)
                            resultunselected = RemoveItemFromArray(splittedunselected, textContent);
                        else
                            resultunselected = new string[0];
                        toDo.DoneList = toDoValue.DoneList != null ? toDoValue.DoneList + "," + textContent : textContent;
                        toDo.NotDoneList = resultunselected.Count() > 0 && resultunselected != null ? string.Join(",", resultunselected) : toDoValue.NotDoneList == "" || resultunselected.Length == 0 ? null : toDoValue.NotDoneList;
                        _context.ToDoList.Update(toDo);
                        _context.SaveChanges();
                    }
                    else
                    {
                        var toDoValue = _context.ToDos.AsNoTracking()
                        .Include(e => e.AppUser)
                        .Where(e => e.UserId == currentUser.Id && e.SystemDate == today).FirstOrDefault();
                        ToDo toDo = new ToDo();
                        toDo.SystemDate = toDoValue.SystemDate;
                        toDo.UpdateSystemDate = DateTime.Today;
                        toDo.UserId = currentUser.Id;
                        toDo.MainGoalTitle = "";
                        toDo.Title = toDoValue.Title;
                        toDo.Id = toDoValue.Id;
                        splittedunselected = toDoValue.NotDoneList != null ? toDoValue.NotDoneList.Split(',') : null;
                        if (splittedunselected != null)
                            resultunselected = RemoveItemFromArray(splittedunselected, textContent);
                        else
                            resultunselected = new string[0];
                        toDo.DoneList = toDoValue.DoneList != null ? toDoValue.DoneList + "," + textContent : textContent;
                        toDo.NotDoneList = resultunselected.Count() > 0 && resultunselected != null ? string.Join(",", resultunselected) : toDoValue.NotDoneList == "" || resultunselected.Length == 0 ? null : toDoValue.NotDoneList;
                        _context.ToDos.Update(toDo);
                        _context.SaveChanges();
                    }

                }
                else
                {
                    if (id > 0)
                    {
                        var toDoValue = _context.ToDoList.AsNoTracking()
                        .Where(e => e.UserId == currentUser.Id && e.Id == id)
                        .FirstOrDefault();

                        ToDoList toDo = new ToDoList();
                        toDo.SystemDate = toDoValue.SystemDate;
                        toDo.UpdateSystemDate = DateTime.Today;
                        toDo.UserId = currentUser.Id;
                        toDo.MainGoalTitle = "";
                        toDo.Title = toDoValue.Title;
                        toDo.Id = id;

                        splittedselected = toDoValue.DoneList != null ? toDoValue.DoneList.Split(',') : null;
                        if (splittedselected != null)
                            resultselected = RemoveItemFromArray(splittedselected, textContent);
                        else
                            resultselected = new string[0];
                        toDo.DoneList = resultselected.Count() > 0 && resultselected != null ? string.Join(",", resultselected) : toDoValue.DoneList == "" || resultselected.Length == 0 ? null : toDoValue.DoneList;
                        toDo.NotDoneList = toDoValue.NotDoneList != null ? toDoValue.NotDoneList + "," + textContent : textContent;
                        _context.ToDoList.Update(toDo);
                        _context.SaveChanges();
                    }
                    else
                    {
                        var toDoValue = _context.ToDos.AsNoTracking()
                        .Include(e => e.AppUser)
                        .Where(e => e.UserId == currentUser.Id && e.SystemDate == today).FirstOrDefault();
                        ToDo toDo = new ToDo();
                        toDo.SystemDate = toDoValue.SystemDate;
                        toDo.UpdateSystemDate = DateTime.Today;
                        toDo.UserId = currentUser.Id;
                        toDo.MainGoalTitle = "";
                        toDo.Id = toDoValue.Id;
                        splittedselected = toDoValue.DoneList != null ? toDoValue.DoneList.Split(',') : null;
                        if (splittedselected != null)
                            resultselected = RemoveItemFromArray(splittedselected, textContent);
                        else
                            resultselected = new string[0];
                        toDo.DoneList = resultselected.Count() > 0 && resultselected != null ? string.Join(",", resultselected) : toDoValue.DoneList == "" || resultselected.Length == 0 ? null : toDoValue.DoneList;
                        toDo.NotDoneList = toDoValue.NotDoneList != null ? toDoValue.NotDoneList + "," + textContent : textContent;
                        _context.ToDos.Update(toDo);
                        _context.SaveChanges();
                    }
                }
            }

            return Json(new { Message = "success" });
        }

        

        [HttpPost]
        public async Task<IActionResult> UpdateToDoText(int id, string notselectedtext,string selectedtext)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var today = DateTime.Today;
            string[] splittedselected = null;
            string[] splittedunselected = null;
            string[] resultselected = null;
            string[] resultunselected = null;
            if (notselectedtext != null || selectedtext!=null)
            {
                    if (id > 0)
                    {
                        var toDoValue = _context.ToDoList.AsNoTracking()
                        .Where(e => e.UserId == currentUser.Id && e.Id == id)
                        .FirstOrDefault();

                        ToDoList toDo = new ToDoList();
                        toDo.SystemDate = DateTime.Today;
                        toDo.UserId = currentUser.Id;
                        toDo.MainGoalTitle = "";
                        toDo.Title = toDoValue.Title;
                        toDo.Id = id;
                  
                        toDo.DoneList = selectedtext;
                    
                        toDo.NotDoneList = notselectedtext;
                    
                    _context.ToDoList.Update(toDo);
                    _context.SaveChanges();
                    }
                    else
                    {
                        var toDoValue = _context.ToDos.AsNoTracking()
                        .Include(e => e.AppUser)
                        .Where(e => e.UserId == currentUser.Id && e.SystemDate == today).FirstOrDefault();
                        ToDo toDo = new ToDo();
                        toDo.SystemDate = DateTime.Today;
                        toDo.UserId = currentUser.Id;
                        toDo.MainGoalTitle = "";
                        toDo.Title = toDoValue.Title;
                        toDo.Id = toDoValue.Id;
                         toDo.DoneList = selectedtext;

                        toDo.NotDoneList = notselectedtext;
                    _context.ToDos.Update(toDo);
                        _context.SaveChanges();
                    }
            }
            return RedirectToAction("ToDoList", new { id = id });
        }
        static string[] RemoveItemFromArray(string[] array, string item)
        {
            item = item == null ? "" : item;
            if (!array.Contains(item))
                return array;

            // İlk bulunan öğenin indeksini bulun
            int indexToRemove = Array.IndexOf(array, item);

            // Eğer öğe dizide yoksa veya öğenin ilk bulunan indeksi -1 ise diziyi aynen döndürün
            if (indexToRemove == -1)
                return array;

            // Verilen öğeyi içermeyen yeni bir dizi oluştur
            string[] newArray = new string[array.Length - 1];
            int index = 0;

            // Yeni diziyi oluştururken, sadece ilk bulunan öğeyi atlayın
            for (int i = 0; i < array.Length; i++)
            {
                if (i != indexToRemove)
                {
                    newArray[index] = array[i];
                    index++;
                }
            }

            // Yeni dizide boş olmayan öğeleri seçerek temizlenmiş diziyi elde edin
            var clearNewArray = newArray.Where(e => !string.IsNullOrEmpty(e)).ToArray();
            return clearNewArray;
        }



        [HttpPost]
        public async Task<IActionResult> UpdateTitle(int itemId, string newTitle)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ToDoList todoList = _context.ToDoList.AsNoTracking().Where(m => m.Id == itemId).FirstOrDefault();

            // Eğer müşteri bulunamazsa
            if (todoList == null)
            {
                return NotFound();
            }
            ToDoList toDo = new ToDoList();
            toDo.SystemDate = DateTime.Today;
            toDo.DoneList = todoList.DoneList;
            toDo.NotDoneList = todoList.NotDoneList;
            toDo.UserId = currentUser.Id;
            toDo.MainGoalTitle = "";
            toDo.Title = newTitle;
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

            return Json(new { Message = "success" });
        }
        [HttpGet]
        public async Task<IActionResult> GetTitle(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ToDoList todoList = _context.ToDoList.AsNoTracking().Where(m => m.Id == id).FirstOrDefault();

            if (todoList == null)
            {
                return NotFound();
            }

            return Json(new { Message = todoList.Title });

        }
       
        [HttpPost]
        public async Task<IActionResult> AddTaskToImportant(int id, string textContent, bool check)
        {
            string[] splittedselected = null;
            string[] splittedunselected = null;
            string[] resultselected = null;
            string[] resultunselected = null;
            var currentUser = await _userManager.GetUserAsync(User);
            ToDoList todoList = _context.ToDoList.AsNoTracking().Where(m => m.Id == id).FirstOrDefault();
            var doneListControl=FindItemFromArray(todoList.DoneList, textContent);
            var notdoneListControl = FindItemFromArray(todoList.NotDoneList, textContent);
            // Eğer müşteri bulunamazsa
            if (todoList == null)
            {
                return NotFound();
            }
            if (!doneListControl && !notdoneListControl)
            {
                if (check)
                {
                    var toDoValue = _context.ToDoList.AsNoTracking()
                     .Where(e => e.UserId == currentUser.Id && e.Id == id).FirstOrDefault();
                    ToDoList toDo = new ToDoList();
                    toDo.SystemDate = DateTime.Today;
                    toDo.UserId = currentUser.Id;
                    toDo.MainGoalTitle = "";
                    toDo.Id = id;
                    toDo.Title = "Önemli";

                    splittedunselected = toDoValue.NotDoneList != null ? toDoValue.NotDoneList.Split(',') : null;
                    if (splittedunselected != null)
                        resultunselected = RemoveItemFromArray(splittedunselected, textContent);
                    else
                        resultunselected = new string[0];
                    toDo.DoneList = toDoValue.DoneList != null ? toDoValue.DoneList + "," + textContent : textContent;
                    toDo.NotDoneList = resultunselected.Count() > 0 && resultunselected != null ? string.Join(",", resultunselected) : toDoValue.NotDoneList == "" || resultunselected.Length == 0 ? null : toDoValue.NotDoneList;
                    _context.ToDoList.Update(toDo);
                    _context.SaveChanges();
                }
                else
                {
                    var toDoValue = _context.ToDoList.AsNoTracking()
                           .Where(e => e.UserId == currentUser.Id && e.Id == id)
                           .FirstOrDefault();

                    ToDoList toDo = new ToDoList();
                    toDo.SystemDate = DateTime.Today;
                    toDo.UserId = currentUser.Id;
                    toDo.MainGoalTitle = "";
                    toDo.Title = "Önemli";

                    toDo.Id = id;

                    splittedselected = toDoValue.DoneList != null ? toDoValue.DoneList.Split(',') : null;
                    if (splittedselected != null)
                        resultselected = RemoveItemFromArray(splittedselected, textContent);
                    else
                        resultselected = new string[0];
                    toDo.DoneList = resultselected.Count() > 0 && resultselected != null ? string.Join(",", resultselected) : toDoValue.DoneList == "" || resultselected.Length == 0 ? null : toDoValue.DoneList;
                    toDo.NotDoneList = toDoValue.NotDoneList != null ? toDoValue.NotDoneList + "," + textContent : textContent;
                    _context.ToDoList.Update(toDo);
                    _context.SaveChanges();
                }
            }
            return Json(new { Message = "success" });
        }
        public string TrimCommas(string str)
        {
            // Başta ve sonda virgül varsa kaldır
            while (str.StartsWith(","))
            {
                str = str.Substring(1);
            }

            // Sonda virgülleri kaldır
            while (str.EndsWith(","))
            {
                str = str.Substring(0, str.Length - 1);
            }

            return str;
        }
        static bool FindItemFromArray(string array, string item)
        {
            if (array != null && item != null)
            {
                string[] arraySplitted = array.Split(',');
                if (arraySplitted.Contains(item))
                    return true;
                else
                    return false;
            }
            return false;
        }
        [HttpGet]
        public async Task<IActionResult> GetPerson()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var searchPeople = _context.Users.
                Where(c => c.CompanyId == currentUser.CompanyId)
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
                        UserId = currentUsers.Id,
                    };
                    userViewModels.Add(userViewModel);
                }
            }

            return Json(new { Message = userViewModels });

        }
        [HttpPost]
        public async Task<IActionResult> AssignPerson(string person,string text)
        {
          
                // person JSON formatında geliyor, isteğinize göre deserialize edebilirsiniz
            var currentUsers = await _userManager.FindByNameAsync(person);
            var currentUser = await _userManager.GetUserAsync(User);
            ToDoList toDo = new ToDoList();
            toDo.UpdateSystemDate = DateTime.Today;
            toDo.SystemDate = DateTime.Today;
            var toDoValue = _context.ToDoList.AsNoTracking()
                 .Where(e => e.UserId == currentUsers.Id && e.Title == "Bana Atananlar")
                  .FirstOrDefault();
            string originalString = "";
                originalString = toDoValue != null ? toDoValue.NotDoneList : null;
                string duzeltilmisDeger = TrimCommas(originalString + "," + text);
                toDo.NotDoneList = duzeltilmisDeger;
                toDo.DoneList = toDoValue != null ? toDoValue.DoneList : null;

            toDo.UserId = currentUsers.Id;
            toDo.MainGoalTitle = "";
            toDo.Title = "Bana Atananlar";
            toDo.CreatedDate = DateTime.Today;
            //toDo.AssigmentTo = currentUser.NameSurname;
            if (currentUsers != null)
            {

                if (toDoValue == null || toDoValue.Id == 0)
                {
                    toDo.CreatedDate = DateTime.Today;
                    _context.ToDoList.Add(toDo);
                    _context.SaveChanges();
                }
                else
                {
                    toDo.Id = toDoValue.Id;
                    _context.ToDoList.Update(toDo);
                    _context.SaveChanges();
                }
            }
           

            return Ok(new { message = "Kişi başarıyla atanmıştır." });
           
        }
    }

}

