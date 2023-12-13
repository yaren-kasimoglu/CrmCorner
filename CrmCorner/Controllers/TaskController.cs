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
