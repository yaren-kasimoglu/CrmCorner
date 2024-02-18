
using CrmCorner.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace CrmCorner.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly CrmCornerContext _context;

        public TaskController(CrmCornerContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser != null)
            {
                var tasks = _context.TaskComps
                    .Include(e => e.Customer)
                       .Include(e => e.Status)
                       .Include(e => e.AppUser)
                       .Where(e => e.UserId == currentUser.Id)
                       .ToList();
                return View(tasks);
            }

            else
            {
                ViewBag.ErrorMessage = "Geçerli kullanıcı bilgisi bulunamadı.";
                return View();
            }


        }

        #region TaskEkleme
        [HttpGet]
        public async Task<IActionResult> TaskAdd()
        {

            var status = _context.Statuses.ToList();
            List<SelectListItem> statusItems = status
        .Select(d => new SelectListItem
        {
            Text = d.StatusName,
            Value = d.StatusId.ToString()
        }).ToList();

            var currentUser = await _userManager.GetUserAsync(User);

            var currentUserCompanyId = currentUser?.CompanyId;

            var users = _context.Users.Where(u => u.CompanyId == currentUserCompanyId!.Value).ToList();
            var userItems = users.Select(u => new SelectListItem
            {
                Text = u.UserName,
                Value = u.Id.ToString()
            }).ToList();


            var customer = _context.CustomerNs.Where(c=>c.AppUserId==currentUser.Id).ToList();

            List<SelectListItem> customerItems = customer
            .Select(d => new SelectListItem
            {
                Text = d.Name + " " + d.Surname +" / "+ d.CompanyName,
                Value = d.Id.ToString()
            }).ToList();


            ViewBag.Customer = customerItems;
            ViewBag.Status = statusItems;
            ViewBag.Users = userItems;
            return View();
        }

        [HttpPost]
        public IActionResult TaskAdd(TaskComp task)
        {
            if (ModelState.IsValid)
            {
                task.CreatedDate = DateTime.Now; // Oluşturulma tarihini şimdi olarak ayarla


                if (task.StatusId.HasValue && task.StatusId.Value != 0) // StatusId kontrolü
                {
                    _context.TaskComps.Add(task); // Task'ı ekleyin
                    _context.SaveChanges(); // Değişiklikleri kaydedin
                    return RedirectToAction("Index"); // Başarılıysa, Index sayfasına yönlendir
                }
                else
                {
                    ModelState.AddModelError("", "Lütfen geçerli bir durum seçiniz.");
                }
            }

            ViewBag.Status = new SelectList(_context.Statuses.ToList(), "StatusId", "StatusName", task.StatusId);
            ViewBag.Users = new SelectList(_context.Users.ToList(), "Id", "UserName", task.UserId);
            ViewBag.Customer = new SelectList(_context.Users.ToList(), "Id", "Name", task.CustomerId);
            return View(task); // Formu ModelState hataları ile geri gönder
        }
        #endregion

        #region TaskGüncelleme/Editmele
        [HttpGet]
        public async Task<IActionResult> TaskEdit(int id)
        {
            var status = _context.Statuses.ToList();
            List<SelectListItem> statusItems = status
        .Select(d => new SelectListItem
        {
            Text = d.StatusName,
            Value = d.StatusId.ToString()
        }).ToList();

            var currentUser = await _userManager.GetUserAsync(User);

            var currentUserCompanyId = currentUser?.CompanyId;

            var users = _context.Users.Where(u => u.CompanyId == currentUserCompanyId!.Value).ToList();
            var userItems = users.Select(u => new SelectListItem
            {
                Text = u.UserName,
                Value = u.Id.ToString()
            }).ToList();

            var customer = _context.CustomerNs.Where(c => c.AppUserId == currentUser.Id).ToList();

            List<SelectListItem> customerItems = customer
            .Select(d => new SelectListItem
            {
                Text = d.Name + " " + d.Surname + " / " + d.CompanyName,
                Value = d.Id.ToString()
            }).ToList();


            ViewBag.Users = userItems;

            ViewBag.Status = statusItems;

            ViewBag.Customer = customerItems;

            TaskComp task = _context.TaskComps.Find(id);
            if (task == null)
            {
                return NotFound();
            }
            return View("TaskEdit", task);
        }

        [HttpPost]
        public IActionResult TaskEdit(TaskComp editedTask)
        {
            if (ModelState.IsValid)
            {
                editedTask.ModifiedDate = DateTime.Now;

                var originalTask = _context.TaskComps.AsNoTracking().FirstOrDefault(t => t.TaskId == editedTask.TaskId);

                editedTask.CreatedDate = originalTask.CreatedDate; // createdDate değerini orijinal değeri ile güncelleme

                // Değişiklikleri kontrol et ve log tablosuna kaydet
                //  LogChanges(originalTask, editedTask);

                // task güncelle
                _context.TaskComps.Update(editedTask);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                return View("ErrorView", editedTask);
            }
        }
        #endregion

        public async Task<IActionResult> TaskDetail(int id)
        {
            var status = _context.Statuses.ToList();
            List<SelectListItem> statusItems = status
        .Select(d => new SelectListItem
        {
            Text = d.StatusName,
            Value = d.StatusId.ToString()
        }).ToList();

            var currentUser = await _userManager.GetUserAsync(User);

            var currentUserCompanyId = currentUser?.CompanyId;

            var users = _context.Users.Where(u => u.CompanyId == currentUserCompanyId!.Value).ToList();
            var userItems = users.Select(u => new SelectListItem
            {
                Text = u.UserName,
                Value = u.Id.ToString()
            }).ToList();


            var customer = _context.CustomerNs.Where(c => c.AppUserId == currentUser.Id).ToList();

            List<SelectListItem> customerItems = customer
            .Select(d => new SelectListItem
            {
                Text = d.Name + " " + d.Surname + " / " + d.CompanyName,
                Value = d.Id.ToString()
            }).ToList();


            ViewBag.Customer = customerItems;
            ViewBag.Status = statusItems;
            ViewBag.Users = userItems;

            // TaskComp task = _context.TaskComps
            //.Include(t => t.TaskCompLogs)
            ///*     .Include(t => t.Customer)*/ // TaskComp nesnesinin Customer ilişkisini yükle
            //.FirstOrDefault(t => t.TaskId == id);

            TaskComp task = _context.TaskComps
                .Include(e => e.Customer).
                Include(e => e.Status).
                Include(e => e.AppUser).
                FirstOrDefault(t => t.TaskId == id);

            if (task == null)
            {
                return NotFound();
            }
            return View("TaskDetail", task);
        }



        //private string GetCustomerNameById(int? customerId)
        //{
        //    if (customerId == null) return null;
        //    var customer = _context.Customers.FirstOrDefault(c => c.Id == customerId);
        //    return customer != null ? $"{customer.Name} {customer.Surname}" : "Unknown";
        //}
        //private string GetStatusNameById(int? statusId)
        //{
        //    if (statusId == null) return null;
        //    var status = _context.Statuses.FirstOrDefault(c => c.StatusId == statusId);
        //    return status != null ? $"{status.StatusName}" : "Unknown";
        //}
        //private string GetEmployeeNameById(int? employeeId)
        //{
        //    if (employeeId == null) return null;
        //    var employee = _context.Employees.FirstOrDefault(c => c.IdEmployee == employeeId);
        //    return employee != null ? $"{employee.EmployeeName} {employee.EmployeeSurname}" : "Unknown";
        //}
        //#endregion

        //private void LogChanges(TaskComp originalTask, TaskComp editedTask)
        //{
        //    foreach (var property in typeof(TaskComp).GetProperties())
        //    {
        //        var originalValue = property.GetValue(originalTask);
        //        var editedValue = property.GetValue(editedTask);

        //        if ((originalValue != null && editedValue != null) && !originalValue.Equals(editedValue))
        //        {
        //            string oldValueString = originalValue.ToString();
        //            string newValueString = editedValue.ToString();
        //            string fieldName = property.Name;

        //            //// CustomerId için özel bir işlem yap
        //            //if (property.Name == "CustomerId")
        //            //{
        //            //    oldValueString = GetCustomerNameById((int?)originalValue);
        //            //    newValueString = GetCustomerNameById((int?)editedValue);
        //            //    fieldName = "Müşteri Bilgisi";
        //            //}
        //            if (property.Name == "EmployeeId")
        //            {
        //                oldValueString = GetEmployeeNameById((int?)originalValue);
        //                newValueString = GetEmployeeNameById((int?)editedValue);
        //                fieldName = "Sorumlu Çalışan Bilgisi";
        //            }
        //            if (property.Name == "StatusId")
        //            {
        //                oldValueString = GetStatusNameById((int?)originalValue);
        //                newValueString = GetStatusNameById((int?)editedValue);
        //                fieldName = "Güncel Durum Bilgisi";
        //            }

        //            TaskCompLog log = new TaskCompLog
        //            {
        //                TaskId = editedTask.TaskId,
        //                UpdatedField = fieldName,
        //                OldValue = oldValueString,
        //                NewValue = newValueString,
        //                UpdatedBy = editedTask.Employee?.IdEmployee,
        //                UpdatedAt = DateTime.Now
        //            };

        //            _context.TaskCompLogs.Add(log);
        //        }
        //    }
        //    _context.SaveChanges();
        //}

        //public IActionResult TaskCompLog()
        //{
        //    var logs = _context.TaskCompLogs.ToList();
        //    return View(logs);
        //}

        //public IActionResult Timeline()
        //{
        //    var tasks = _context.TaskComps
        //        .Include(t => t.TaskCompLogs)
        //        .ToList();

        //    return View(tasks);
        //}
        //public IActionResult GetTaskTimeline(int taskId)
        //{
        //    // TaskComps tablosundan taskId'ye göre ilgili görevi çek
        //    var task = _context.TaskComps
        //                    .Include(t => t.TaskCompLogs)
        //                    .FirstOrDefault(t => t.TaskId == taskId);

        //    if (task != null)
        //    {
        //        return Json(task.TaskCompLogs);
        //    }

        //    return Json(null);
        //}

        //[HttpPost]
        //public async Task<IActionResult> UploadFile(int taskId, IFormFile uploadedFile)
        //{
        //    if (uploadedFile != null && uploadedFile.Length > 0)
        //    {
        //        try
        //        {
        //            using (MemoryStream memoryStream = new MemoryStream())
        //            {
        //                await uploadedFile.CopyToAsync(memoryStream);
        //                byte[] fileBytes = memoryStream.ToArray();

        //                TaskComp task = await _context.TaskComps.FindAsync(taskId);

        //                if (task != null)
        //                {
        //                    // Mevcut dosyaları al
        //                    string existingFileNames = task.UploadedFileName ?? "";
        //                    byte[] existingFiles = task.UploadedFile;

        //                    // Yeni dosya adını ve içeriğini alınan dosyalara ekle
        //                    existingFileNames += (string.IsNullOrEmpty(existingFileNames) ? "" : ",") + uploadedFile.FileName;

        //                    if (existingFiles == null)
        //                    {
        //                        // İlk dosya yükleniyorsa yeni bir byte[] oluştur
        //                        task.UploadedFile = fileBytes;
        //                    }
        //                    else
        //                    {
        //                        // Varolan byte[]'ın sonuna yeni dosya içeriğini eklenir
        //                        byte[] combinedFiles = new byte[existingFiles.Length + fileBytes.Length];
        //                        Array.Copy(existingFiles, combinedFiles, existingFiles.Length);
        //                        Array.Copy(fileBytes, 0, combinedFiles, existingFiles.Length, fileBytes.Length);
        //                        task.UploadedFile = combinedFiles;
        //                    }

        //                    // Yeni değerleri kaydet
        //                    task.UploadedFileName = existingFileNames;

        //                    await _context.SaveChangesAsync();
        //                    ViewBag.Message = "Dosya başarıyla yüklendi ve kaydedildi!";
        //                }
        //                else
        //                {
        //                    ViewBag.Message = "Belirtilen görev bulunamadı!";
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            ViewBag.Message = "Dosya yüklenirken bir hata oluştu: " + ex.Message;
        //        }
        //    }
        //    else
        //    {
        //        ViewBag.Message = "Dosya yüklenemedi veya dosya boş!";
        //    }

        //    // İlgili view'i döndürürken ViewBag.Message'i kullan
        //    return RedirectToAction("TaskDetail", new { id = taskId });
        //}


        //public async Task<IActionResult> DownloadFile(int taskId, string fileName)
        //{
        //    TaskComp task = await _context.TaskComps.FindAsync(taskId);

        //    if (task != null && !string.IsNullOrEmpty(fileName))
        //    {
        //        string[] fileNames = task.UploadedFileName?.Split(',');
        //        byte[] files = task.UploadedFile;

        //        if (fileNames != null && files != null)
        //        {
        //            int index = Array.IndexOf(fileNames, fileName);

        //            if (index >= 0 && index < fileNames.Length)
        //            {
        //                byte[] fileContent = files;

        //                return File(fileContent, "application/octet-stream", fileName);
        //            }
        //        }
        //    }

        //    return NotFound(); // Dosya bulunamadıysa 404 Not Found döndür
        //}

        //private byte[] GetFileContentAtIndex1(byte[] files, int index)
        //{
        //    if (files != null && index >= 0 && index < files.Length)
        //    {
        //        // İlgili indeksteki dosya içeriğini döndür
        //        byte[] fileContent = new byte[files.Length - index];
        //        Array.Copy(files, index, fileContent, 0, files.Length - index);
        //        return fileContent;
        //    }

        //    return null;
        //}

        //public async Task<IActionResult> DeleteFile(int taskId, string fileName)
        //{
        //    TaskComp task = await _context.TaskComps.FindAsync(taskId);

        //    if (task != null && !string.IsNullOrEmpty(fileName))
        //    {
        //        string[] fileNames = task.UploadedFileName?.Split(',');
        //        byte[] files = task.UploadedFile;

        //        if (fileNames != null && files != null)
        //        {
        //            int index = Array.IndexOf(fileNames, fileName);

        //            if (index >= 0 && index < fileNames.Length)
        //            {
        //                // İlgili dosyanın adını listeden çıkar
        //                List<string> fileList = new List<string>(fileNames);
        //                fileList.RemoveAt(index);
        //                fileNames = fileList.ToArray();

        //                // Dosya listesinden çıkarılan dosyanın indeksine karşılık gelen blobu da kaldır
        //                List<byte> fileBlob = new List<byte>(files);
        //                fileBlob.RemoveRange(index * sizeof(byte), sizeof(byte));
        //                files = fileBlob.ToArray();

        //                // Veritabanına güncellenmiş dosya listesi ve blob'u kaydet
        //                task.UploadedFileName = string.Join(",", fileNames);
        //                task.UploadedFile = files;

        //                await _context.SaveChangesAsync();

        //                ViewBag.Message = "Dosya başarıyla silindi!";
        //            }
        //            else
        //            {
        //                ViewBag.Message = "Dosya bulunamadı!";
        //            }
        //        }
        //    }
        //    else
        //    {
        //        ViewBag.Message = "Belirtilen görev veya dosya bulunamadı!";
        //    }

        //    return RedirectToAction("TaskDetail", new { id = taskId });
        //}



        public IActionResult TaskDelete(int id)
        {
            TaskComp task = _context.TaskComps.Find(id);

            if (task == null)
            {
                return NotFound();
            }

            _context.TaskComps.Remove(task);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }

}