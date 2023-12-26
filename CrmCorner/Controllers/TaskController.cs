using CrmCorner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    public class TaskController : Controller
    {
        private readonly CrmcornerContext _context;
        public TaskController(CrmcornerContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var tasks = _context.TaskComps
                                .Include(e => e.Customer)
                                .Include(e => e.Employee)
                                .Include(e => e.Status)
                                .ToList();
            return View(tasks);
        }
        [HttpGet]
        public IActionResult TaskAdd()
        {
            // Veritabanından tüm departmanları al
            var employee = _context.Employees.ToList();
            var customer = _context.Customers.ToList();
            var status = _context.Statuses.ToList();

            List<SelectListItem> employeeItems = employee
            .Select(d => new SelectListItem
            {
                Text = d.EmployeeName + " " + d.EmployeeSurname,
                Value = d.IdEmployee.ToString()
            }).ToList();

            List<SelectListItem> customerItems = customer
            .Select(d => new SelectListItem
            {
                Text = d.Name + " " + d.Surname,
                Value = d.Id.ToString()
            }).ToList();

            List<SelectListItem> statusItems = status
        .Select(d => new SelectListItem
        {
            Text = d.StatusName,
            Value = d.StatusId.ToString()
        }).ToList();

            // ViewBag'de SelectListItem'ları sakla
            ViewBag.Employee = employeeItems;
            ViewBag.Customer = customerItems;
            ViewBag.Status = statusItems;
            return View();
        }
        [HttpPost]
        public IActionResult TaskAdd(TaskComp task)
        {
            if (ModelState.IsValid)
            {
                task.CreatedDate = DateTime.Now;
                if (task.EmployeeId != 0 || task.CustomerId != 0 || task.StatusId != 0)
                {
                    // (Eager Loading) kullanarak Department'ı yükle
                    var tasks = _context.TaskComps
                              .Include(e => e.Employee)
                              .Include(e => e.Customer)
                              .Include(e => e.Status)
                              .ToList();

                    _context.TaskComps.Add(task);
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    // Hata mesajları ile birlikte hata sayfasını göster
                    var employee = _context.Departments.ToList();
                    var customer = _context.Positions.ToList();
                    var status = _context.Positions.ToList();
                    ViewBag.Employee = new SelectList(employee, "IdDepartment", "EmployeeName", task.EmployeeId);
                    ViewBag.Customer = new SelectList(customer, "IdPositions", "CustomerName", task.CustomerId);
                    ViewBag.Status = new SelectList(status, "IdPositions", "StatusName", task.StatusId);

                    return View("ErrorView", task);

                }
            }
            else
            {
                var employee = _context.Departments.ToList();
                var customer = _context.Positions.ToList();
                var status = _context.Positions.ToList();
                ViewBag.Employee = new SelectList(employee, "IdDepartment", "EmployeeName", task.EmployeeId);
                ViewBag.Customer = new SelectList(customer, "IdPositions", "CustomerName", task.CustomerId);
                ViewBag.Status = new SelectList(status, "IdPositions", "StatusName", task.StatusId);

                return View();
            }
        }
        [HttpGet]
        public IActionResult TaskEdit(int id)
        {
            var employee = _context.Employees.ToList();
            var customer = _context.Customers.Include(c => c.Company).ToList();
            var status = _context.Statuses.ToList();


            List<SelectListItem> employeeItems = employee
            .Select(d => new SelectListItem
            {
                Text = d.EmployeeName + " " + d.EmployeeSurname,
                Value = d.IdEmployee.ToString()
            }).ToList();

            List<SelectListItem> customerItems = customer
            .Select(d => new SelectListItem
            {
                Text = d.Name + " " + d.Surname + " / " + d.Company?.CompanyName,
                Value = d.Id.ToString()
            }).ToList();

            List<SelectListItem> statusItems = status
        .Select(d => new SelectListItem
        {
            Text = d.StatusName,
            Value = d.StatusId.ToString()
        }).ToList();

            // ViewBag'de SelectListItem'ları sakla
            ViewBag.Employee = employeeItems;
            ViewBag.Customer = customerItems;
            ViewBag.Status = statusItems;

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

                // Değişiklikleri kontrol et ve log tablosuna kaydet
                LogChanges(originalTask, editedTask);

                // Çalışanı güncelle
                _context.TaskComps.Update(editedTask);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                return View("ErrorView", editedTask);
            }
        }

        private void LogChanges(TaskComp originalTask, TaskComp editedTask)
        {
            foreach (var property in typeof(TaskComp).GetProperties())
            {
                var originalValue = property.GetValue(originalTask);
                var editedValue = property.GetValue(editedTask);

                if ((originalValue != null && editedValue != null) && !originalValue.Equals(editedValue))
                {
                    TaskCompLog log = new TaskCompLog
                    {
                        TaskId = editedTask.TaskId,
                        UpdatedField = property.Name,
                        OldValue = originalValue.ToString(),
                        NewValue = editedValue.ToString(),
                        UpdatedBy = editedTask.Employee?.IdEmployee, // ? işaretini kullandım, böylece null check yapılır.
                        UpdatedAt = DateTime.Now
                    };

                    _context.TaskCompLogs.Add(log);
                }
            }
            _context.SaveChanges();
        }

        public IActionResult TaskCompLog()
        {
            var logs = _context.TaskCompLogs.ToList();
            return View(logs);
        }

        public IActionResult Timeline()
        {
            var taskCompLogs = _context.TaskCompLogs
                .Include(t => t.Task)
                .Include(e => e.UpdatedByNavigation)
                .ToList();

            return View(taskCompLogs);
        }

        //public IActionResult TaskEdit(TaskComp editedTask)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        editedTask.ModifiedDate = DateTime.Now;
        //        // Çalışanı güncelle
        //        _context.TaskComps.Update(editedTask);
        //        _context.SaveChanges();

        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {
        //        return View("ErrorView", editedTask);
        //    }
        //}

        public IActionResult TaskDetail(int id)
        {
            var employee = _context.Employees.ToList();
            var customer = _context.Customers.Include(c => c.Company).ToList();
            var status = _context.Statuses.ToList();


            List<SelectListItem> employeeItems = employee
            .Select(d => new SelectListItem
            {
                Text = d.EmployeeName + " " + d.EmployeeSurname,
                Value = d.IdEmployee.ToString()
            }).ToList();

            List<SelectListItem> customerItems = customer
            .Select(d => new SelectListItem
            {
                Text = d.Name + " " + d.Surname + " / " + d.Company?.CompanyName,
                Value = d.Id.ToString()
            }).ToList();

            List<SelectListItem> statusItems = status
        .Select(d => new SelectListItem
        {
            Text = d.StatusName,
            Value = d.StatusId.ToString()
        }).ToList();

            // ViewBag'de SelectListItem'ları sakla
            ViewBag.Employee = employeeItems;
            ViewBag.Customer = customerItems;
            ViewBag.Status = statusItems;

            TaskComp task = _context.TaskComps.Find(id);
            if (task == null)
            {
                return NotFound();
            }
            return View("TaskDetail", task);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(int taskId, IFormFile uploadedFile)
        {
            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                try
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        await uploadedFile.CopyToAsync(memoryStream);
                        byte[] fileBytes = memoryStream.ToArray();

                        TaskComp task = await _context.TaskComps.FindAsync(taskId);

                        if (task != null)
                        {
                           task.UploadedFileName = uploadedFile.FileName;
                            task.UploadedFile = fileBytes;
                            await _context.SaveChangesAsync();
                            ViewBag.Message = "Dosya başarıyla yüklendi ve kaydedildi!";
                        }
                        else
                        {
                            ViewBag.Message = "Belirtilen görev bulunamadı!";
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "Dosya yüklenirken bir hata oluştu: " + ex.Message;
                }
            }
            else
            {
                ViewBag.Message = "Dosya yüklenemedi veya dosya boş!";
            }

            // İlgili view'i döndürürken ViewBag.Message'i kullanabilirsiniz
            return RedirectToAction("TaskDetail", new { id = taskId });
        }

        public IActionResult DownloadFile(int taskId)
        {
            var task = _context.TaskComps.FirstOrDefault(t => t.TaskId == taskId);

            if (task != null && task.UploadedFile != null)
            {
                // Örneğin, bir dosya adı belirtmek için dosyanın MIME türünü kullanabilirsiniz.
                var contentType = "application/octet-stream";
                var fileName = task.UploadedFileName;

                return File(task.UploadedFile, contentType, fileName);
            }

            return NotFound(); // Dosya bulunamazsa NotFound döndür
        }

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
